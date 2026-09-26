using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class FlakyBackend
{
    Dictionary<string, string> savedData = new();

    public async UniTaskVoid Save(string key, string val, Action<bool> actionCallback)
    {
        await UniTask.Delay(UnityEngine.Random.Range(50, 150));
        savedData[key] = val;
        actionCallback?.Invoke(true);

        await UniTask.Delay(UnityEngine.Random.Range(50, 150));
        actionCallback?.Invoke(true);
    }
    public async UniTaskVoid Load(string key, Action<SystemResult> actionCallback)
    {
        string data = "";
        bool resultStatus = true;
        if (savedData.ContainsKey(key))
        {
            data = savedData[key];
            resultStatus = true;
        }
        else
        {
            data = StaticConstants.EMPTYSTRING;
            resultStatus = false;
        }
        await UniTask.Delay(UnityEngine.Random.Range(50, 300));

        SystemResult.Status status;
        string reason = "";
        if (resultStatus)
        {
            status = SystemResult.Status.Success;
            reason = $"Found Data against key {key}";
        }
        else
        {
            status = SystemResult.Status.NotFound;
            reason = $"No data against key {key}";
        }

        var result = new SystemResult()
        {
            Result = status,
            Data = data,
            Reason = reason
        };
        actionCallback?.Invoke(result);

        await UniTask.Delay(UnityEngine.Random.Range(50, 150));
        actionCallback?.Invoke(result);
    }
}
