using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class PerKeySerializingService : IService
{
    readonly IService _inner;
    readonly Dictionary<string, UniTask> _inFlight = new();

    public PerKeySerializingService(IService inner) => _inner = inner;

    public UniTask<SystemResult> SaveDataAsync(string key, string data) => Enqueue(key, () => _inner.SaveDataAsync(key, data));
    public UniTask<SystemResult> LoadDataAsync(string key) => Enqueue(key, () => _inner.LoadDataAsync(key));

    async UniTask<SystemResult> Enqueue(string key, Func<UniTask<SystemResult>> op)
    {
        var previous = _inFlight.TryGetValue(key, out var t) ? t : UniTask.CompletedTask;
        var gate = new UniTaskCompletionSource();
        _inFlight[key] = gate.Task;
        await previous;
        try { return await op(); }
        finally { gate.TrySetResult(); }
    }
}