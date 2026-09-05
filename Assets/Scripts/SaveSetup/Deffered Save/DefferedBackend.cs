using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class DefferedBackend
{
    Dictionary<string, string> savedData = new();
    public async UniTaskVoid Save(string key, string val, Action<bool> actionCallback)
    {
        savedData[key] = val;
        await UniTask.Delay(UnityEngine.Random.Range(50, 300));
        actionCallback?.Invoke(true);
    }
    public async UniTaskVoid Load(string key, Action<SaveResult> actionCallback)
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

        SaveResult.Status status;
        string reason = "";
        if (resultStatus)
        {
            status = SaveResult.Status.Success;
            reason = $"Found Data against key {key}";
        }
        else
        {
            status = SaveResult.Status.NotFound;
            reason = $"No data against key {key}";
        }

        actionCallback?.Invoke(new SaveResult()
        {
            Result = status,
            Data = data,
            Reason = reason
        });
    }
}
