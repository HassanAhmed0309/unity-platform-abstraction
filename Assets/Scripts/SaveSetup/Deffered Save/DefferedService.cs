using System;
using Cysharp.Threading.Tasks;
public class DefferedService : IService
{
    readonly DefferedBackend defferedSaveLoad;

    public DefferedService(Func<bool> shouldFail = null)
    {
        defferedSaveLoad = new DefferedBackend(shouldFail);
    }

    public async UniTask<SystemResult> LoadDataAsync(string key)
    {
        var tcs = new UniTaskCompletionSource<SystemResult>();
        _ = defferedSaveLoad.Load(key, result => tcs.TrySetResult(result));
        return await tcs.Task;
    }

    public async UniTask<SystemResult> SaveDataAsync(string key, string data)
    {
        var tcs = new UniTaskCompletionSource<SystemResult>();
        _ = defferedSaveLoad.Save(key, data, ok => tcs.TrySetResult(ok ? new SystemResult() { Result = SystemResult.Status.Success, Data = data, Reason = $"Successfully saved data against key {key}" }
        : new SystemResult() { Result = SystemResult.Status.Failed, Data = "", Reason = $"Data couldn't be saved against key {key}" }));
        return await tcs.Task;
    }
}

public struct SystemResult
{
    public enum Status { Success, NotFound, Failed };
    public Status Result { get; set; }
    public string Data;
    public string Reason;
}