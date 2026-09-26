using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

public class HostileConditionTests
{
    [UnityTest]
    public IEnumerator DefferedService_Save_ReportsFailure_WhenBackendFails() => UniTask.ToCoroutine(async () =>
    {
        var service = new DefferedService(() => true); // force every save to fail

        var result = await service.SaveDataAsync("key", "value");

        Assert.AreEqual(SystemResult.Status.Failed, result.Result);
    });

    [UnityTest]
    public IEnumerator DefferedService_Save_DoesNotPersistData_WhenBackendFails() => UniTask.ToCoroutine(async () =>
    {
        var service = new DefferedService(() => true); // force every save to fail

        await service.SaveDataAsync("key", "value");
        var loadResult = await service.LoadDataAsync("key");

        Assert.AreEqual(SystemResult.Status.NotFound, loadResult.Result);
    });

    [UnityTest]
    public IEnumerator FlakyService_Save_DuplicateCallback_ResolvesOnceWithoutThrowing() => UniTask.ToCoroutine(async () =>
    {
        var service = new FlakyService();

        var result = await service.SaveDataAsync("key", "value");

        Assert.AreEqual(SystemResult.Status.Success, result.Result);
    });

    [UnityTest]
    public IEnumerator FlakyService_Load_DuplicateCallback_ResolvesOnceWithoutThrowing() => UniTask.ToCoroutine(async () =>
    {
        var service = new FlakyService();
        await service.SaveDataAsync("key", "value");

        var result = await service.LoadDataAsync("key");

        Assert.AreEqual(SystemResult.Status.Success, result.Result);
        Assert.AreEqual("value", result.Data);
    });

    [UnityTest]
    public IEnumerator PerKeySerializingService_ConcurrentSavesToSameKey_ApplyInCallOrder() => UniTask.ToCoroutine(async () =>
    {
        var service = new PerKeySerializingService(new FlakyService());

        // Fired back-to-back with no await between them, so without the
        // per-key serialization the two operations could complete out of
        // order and "first" could silently win even though it was issued
        // before "second".
        var firstTask = service.SaveDataAsync("race-key", "first");
        var secondTask = service.SaveDataAsync("race-key", "second");

        await UniTask.WhenAll(firstTask, secondTask);

        var loadResult = await service.LoadDataAsync("race-key");
        Assert.AreEqual("second", loadResult.Data);
    });
}
