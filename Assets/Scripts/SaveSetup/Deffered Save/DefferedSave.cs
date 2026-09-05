using Cysharp.Threading.Tasks;
public class DefferedSave : ISaveService
{
    DefferedBackend defferedSaveLoad = new();

    public async UniTask<SaveResult> LoadDataAsync(string key)
    {
        var tcs = new UniTaskCompletionSource<SaveResult>();
        _ = defferedSaveLoad.Load(key, result => tcs.TrySetResult(result));
        return await tcs.Task;
    }

    public async UniTask<SaveResult> SaveDataAsync(string key, string data)
    {
        var tcs = new UniTaskCompletionSource<SaveResult>();
        _ = defferedSaveLoad.Save(key, data, ok => tcs.TrySetResult(ok ? new SaveResult() { Result = SaveResult.Status.Success, Data = data, Reason = $"Successfully saved data against key {key}" }
        : new SaveResult() { Result = SaveResult.Status.Failed, Data = "", Reason = $"Data couldn't be saved against key {key}" }));
        return await tcs.Task;
    }
}

public struct SaveResult
{
    public enum Status { Success, NotFound, Failed };
    public Status Result { get; set; }
    public string Data;
    public string Reason;
}