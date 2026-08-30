
using Cysharp.Threading.Tasks;

public class InstantSave : ISaveService
{
    InstantBackend _backend = new();
    public async UniTask<SaveResult> SaveDataAsync(string key, string data)
    {
        _backend.Save(key, data);
        await UniTask.Yield();       // uniform timing — see README
        return SaveResult.OK;
    }
}
