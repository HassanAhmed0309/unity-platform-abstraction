
using System;
using Cysharp.Threading.Tasks;

public class InstantService : IService
{
    InstantBackend _backend = new();

    public async UniTask<SystemResult> LoadDataAsync(string key)
    {
        string data = _backend.Load(key);
        await UniTask.Yield();       // uniform timing — see README
        SystemResult result = new();
        if (data == null)
        {
            result = new()
            {
                Result = SystemResult.Status.NotFound,
                Data = null,
                Reason = $"No key {key} found"
            };
        }
        else
        {
            result = new()
            {
                Result = SystemResult.Status.Success,
                Data = data,
                Reason = $"Data {data} found for {key}"
            };
        }
        return result;
    }

    public async UniTask<SystemResult> SaveDataAsync(string key, string data)
    {
        _backend.Save(key, data);
        await UniTask.Yield();       // uniform timing — see README
        return new SystemResult()
        {
            Result = SystemResult.Status.Success,
            Data = data,
            Reason = $"Successfully saved data against key {key}"
        };
    }

    // public static implicit operator InstantService(PerKeySerializingService v)
    // {
    //     throw new NotImplementedException();
    // }
}
