using Cysharp.Threading.Tasks;
public class DefferedSave : ISaveService
{
    DefferedBackend defferedSaveLoad = new();
    public UniTask<SaveResult> SaveDataAsync(string key, string data)
    {
        var tcs = new UniTaskCompletionSource<SaveResult>();
        defferedSaveLoad.Save(key, data, ok => tcs.TrySetResult(ok ? SaveResult.OK : SaveResult.Failed));
        return tcs.Task;
    }
}

public enum SaveResult
{
    OK, Failed
}

public enum LoadResult
{
    Ok, Failed
}