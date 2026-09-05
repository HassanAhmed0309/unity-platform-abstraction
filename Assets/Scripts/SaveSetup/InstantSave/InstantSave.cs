
using Cysharp.Threading.Tasks;

public class InstantSave : ISaveService
{
    InstantBackend _backend = new();

    public async UniTask<SaveResult> LoadDataAsync(string key)
    {
        string data = _backend.Load(key);
        await UniTask.Yield();       // uniform timing — see README
        SaveResult result = new();
        if (data == StaticConstants.EMPTYSTRING)
        {
            result = new()
            {
                Result = SaveResult.Status.NotFound,
                Data = StaticConstants.EMPTYSTRING,
                Reason = $"No key {key} found"
            };
        }
        else
        {
            result = new()
            {
                Result = SaveResult.Status.Success,
                Data = data,
                Reason = $"Data {data} found for {key}"
            };
        }
        return result;
    }

    public async UniTask<SaveResult> SaveDataAsync(string key, string data)
    {
        _backend.Save(key, data);
        await UniTask.Yield();       // uniform timing — see README
        return new SaveResult()
        {
            Result = SaveResult.Status.Success,
            Data = data,
            Reason = $"Successfully saved data against key {key}"
        };
    }
}
