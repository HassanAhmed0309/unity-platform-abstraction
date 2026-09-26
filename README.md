# Unity Platform Abstraction — Save Service

## What this is

This repo is a design exercise around one interface: `IService`, a fully asynchronous save/load contract, and three fake backends that each violate a different assumption a game-side caller might otherwise make about how platform save APIs behave. It exists to show how that interface absorbs those violations so the caller never has to know which backend it's talking to.

## What this is not

- **Not a library.** It isn't meant to be imported into another project.
- **Not a game.** There's no gameplay behind the Save/Load screens — they exist to drive the system, not to be played.
- **Not integrated with any real platform SDK.** All three backends are in-memory fakes. No vendor code, no vendor names, no API keys.
- **Not a WebGL build.** Editor/PC only. WebGL is the platform that motivates the design (single-threaded, no blocking, disagreeing SDKs), but shipping a build isn't the point here — demonstrating the abstraction is.

---

## The problem this models

Real platform save APIs disagree with each other in ways that are individually minor but collectively make "just call Save and await it" impossible to write once and reuse. Some APIs return synchronously. Some are callback-based with real latency. Some return `null` for both "key not found" and "call failed" — the same value, two unrelated meanings. Some fire their completion callback more than once. None of this is hypothetical; it's the ordinary experience of shipping the same game across several browser-based portals from one codebase.

The three backends here each model one class of that misbehavior, not any specific vendor:

| Backend | What it models |
|---|---|
| `InstantBackend` / `InstantService` | A synchronous API with no error channel — `Load` returns `null` for a missing key, which is structurally identical to "the call failed," until the adapter disambiguates it. |
| `DefferedBackend` / `DefferedService` | A callback-based API with real latency and a real chance of failure. |
| `FlakyBackend` / `FlakyService` | An API whose completion callback can fire more than once for a single call, and whose completions can land out of order relative to when the calls were issued. |

---

## Design decisions

### Why the interface is uniformly async

```csharp
public interface IService
{
    UniTask<SystemResult> SaveDataAsync(string key, string data);
    UniTask<SystemResult> LoadDataAsync(string key);
}
```

There's no synchronous variant and no `bool TryGetSync`. This is a one-way door, and it's deliberate: a synchronous backend can always be *adapted upward* into something async — `InstantService` does exactly that, wrapping a synchronous dictionary lookup in a method that still returns a `UniTask`. The reverse isn't safely possible. An asynchronous, callback-based backend cannot be forced into a synchronous call without blocking the calling thread until the callback fires — and on a single-threaded platform like WebGL, blocking the one thread that's supposed to be running the callback *is* the deadlock. So the interface commits to the lowest common denominator every backend can honestly support, rather than the one that's most convenient for the easiest backend.

### Why `SystemResult` instead of `null` or `bool`

```csharp
public struct SystemResult
{
    public enum Status { Success, NotFound, Failed }
    public Status Result { get; set; }
    public string Data;
    public string Reason;
}
```

The concrete problem this solves is sitting in `InstantBackend.Load`:

```csharp
public string Load(string key) => savedData.TryGetValue(key, out var val) ? val : null;
```

A real synchronous save API that returns `null` this way gives the caller no way to distinguish "this key was never saved" from "the call itself failed." Collapsing those two into one `null` is a real ambiguity a real backend has, not a hypothetical — so the backend is left to keep producing it (that's what a real SDK would do), and the *adapter*, not the backend, is where the ambiguity gets resolved: `InstantService.LoadDataAsync` is the one place that turns `null` into an explicit `Status.NotFound`. Every other backend goes through the same disambiguation, so the caller only ever sees `Success`, `NotFound`, or `Failed` — never a bare `null` and never a `bool` that can't say why something failed.

### Why the synchronous backend still yields a frame

This is the single most important decision in the codebase, and the easiest one to get wrong.

```csharp
// InstantService.cs
_backend.Save(key, data);
await UniTask.Yield();       // deliberate — see below
return new SystemResult { ... };
```

If `InstantService.SaveDataAsync` completed entirely on the calling frame, its continuation would run **synchronously, on the caller's own stack**, the instant `await` is reached — because there'd be nothing to actually suspend on. `DefferedService` and `FlakyService` can never do that; their continuations always resume later, on a different point in the frame, because a real callback genuinely hasn't fired yet. That difference is invisible in almost every normal use of the API, and completely visible the moment a caller does something re-entrant in its continuation — mutates a collection it's mid-iterating, re-triggers a state transition, touches UI state that assumes it's not already mid-callback. That kind of bug reproduces reliably against the *synchronous* backend and reliably fails to reproduce against the *async* ones (or the other way around), which makes it look like a platform-specific bug when it's actually a timing assumption the caller never should have been allowed to make. Forcing `InstantService` to yield at least once removes that hidden asymmetry: every backend's continuation resumes later than the call site, so a caller can't accidentally depend on same-frame completion just because they happened to test against the fast backend.

### Concurrency policy: serialize per key, once, at the boundary

```csharp
instant  = new PerKeySerializingService(new InstantService());
deffered = new PerKeySerializingService(new DefferedService());
flaky    = new PerKeySerializingService(new FlakyService());
```

Two calls to the same key issued close together (an autosave firing while a level-complete save is still in flight, for example) can legitimately race: each backend's `Save` decides its own random delay, computes its result, and only *then* writes — so whichever call's delay happens to finish first would silently win, regardless of which call was actually issued last. That's a real bug class, not a corner case, and it applies equally to all three backends — it isn't specific to Flaky.

Rather than reimplementing a lock inside each backend (three copies of the same fix, with three chances to drift), `PerKeySerializingService` is a decorator that wraps *any* `IService` and, per key, chains operations so a new call to a key awaits the previous in-flight call to that same key before it starts. It's applied uniformly to all three backends at the one place they're constructed. The guarantee it provides: operations against the same key are applied in the order they were **issued**, not the order their internal delays happen to finish.

### Surviving a callback that fires twice

`FlakyBackend` deliberately invokes its callback twice for both `Save` and `Load`, with a real delay between the two firings — modeling an SDK backed by a message-passing bridge (postMessage / iframe) that has, in practice, occasionally delivered the same completion event twice. The fix for this lives in the bridge between callback and `UniTask`, not in `FlakyService` specifically:

```csharp
var tcs = new UniTaskCompletionSource<SystemResult>();
_ = flakySaveLoad.Save(key, data, result => tcs.TrySetResult(result));
return await tcs.Task;
```

`TrySetResult` — not `SetResult` — makes the second, duplicate delivery a silent no-op instead of an exception. This is the same bridging pattern `DefferedService` uses; `FlakyBackend` is the backend that actually exercises it under a duplicate delivery.

### Modeling failure without writing a flaky test suite

`DefferedBackend` can fail a save, but the failure trigger is injected rather than hardcoded as a fixed random chance:

```csharp
public DefferedBackend(Func<bool> shouldFail = null)
{
    _shouldFail = shouldFail ?? DefaultShouldFail;   // ~15% by default
}
```

Production code (`ServiceInitializer`) gets a realistic, randomly-occurring failure rate. Tests construct `new DefferedService(() => true)` or `(() => false)` to force a deterministic outcome. This matters because "Flaky" should describe the backend's *behavior class*, not the reliability of the test suite verifying it — a test that only fails a save 15% of the time it runs is a flaky test, which is a different and worse problem than the one this repo is trying to demonstrate.

---

## Project structure

```
Assets/Scripts/SaveSetup/
  IService.cs                    — the interface + SystemResult (actually declared in DefferedService.cs)
  ServiceInitializer.cs          — constructs and hands out the three backends
  PerKeySerializingService.cs    — the concurrency decorator
  InstantSave/                   — synchronous, ambiguous-null backend
  Deffered Save/                 — callback-based, latency + failure
  Flaky Save/                    — duplicate-callback backend

Assets/Scripts/UI/
  UIManager.cs, Screen.cs, MainMenuScreen.cs,
  SaveSystemScreen.cs, LoadSystemScreen.cs
                                  — demo scene; selects a backend via ServiceInitializer.GetService(),
                                    then calls IService the same way regardless of selection

Assets/Tests/PlayMode/
  SharedServiceContractTests.cs  — one suite, parameterized over all three backends
  HostileConditionTests.cs       — forced failure, duplicate callback, same-key race
```

## Running it

Open `Assets/Scenes/SampleScene.unity` and press Play. From the main menu, open either the Save or Load screen, pick a backend from the dropdown (Immediate / Deffered / Flaky), and use it — the caller code behind both buttons is identical regardless of which backend is selected.

## Running the tests

**Window → General → Test Runner → PlayMode tab → Run All.** `SharedServiceContractTests` runs the same three tests (round-trip save/load, missing-key returns `NotFound`, overwrite-by-key) once per backend via `[TestFixtureSource]`. `HostileConditionTests` covers the cases that matter more: a forced save failure leaves no data behind, a duplicated callback resolves once without throwing (on both Save and Load), and two saves fired at the same key without awaiting between them still apply in the order they were issued.

---

## Known limitations / what I'd change

- **`UniTask.Delay` simulates asynchronous, callback-driven timing — it is not the same mechanism as a real browser SDK bridge.** It reproduces the ordering and duplicate-delivery problems faithfully, but not real-world jitter, tab-backgrounding throttling, or the serialization boundary of an actual postMessage/iframe round-trip. A backend that's "arbitrary delay + can fail" via `UniTask.Delay` is a reasonable stand-in for reasoning about the *shape* of the problem, not a faithful timing replica.
- **`FlakyBackend.Save` never actually fails**, only duplicates its callback. It would be straightforward to give it the same injectable `shouldFail` trigger `DefferedBackend` has, for full symmetry — left out to keep Flaky's demonstrated behavior narrowly scoped to duplication and ordering, which is what it exists to prove.
- **Nothing persists across a session.** All three backends are in-memory dictionaries that reset on domain reload or app restart. That's intentional — this repo isn't meant to model real persistence — but worth stating so it doesn't read as an oversight.
- **`PerKeySerializingService`'s per-key bookkeeping is never evicted.** Every key that's ever been touched keeps a small entry in its internal dictionary for the process lifetime. Fine at demo scale; a long-running production version would need to clean up completed, no-longer-referenced entries.
- **The other four interfaces this project's broader scope originally named** (`IPlatformSdk`, `IAdService`, `IIapService`, `IPlatformLifecycle`) are intentionally absent. This repo is scoped to one interface done rigorously rather than five done shallowly.
