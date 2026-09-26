using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
public interface IService
{
    //UniTask<string> --> string is the result of the function (Success, Fail, WaitForCallback,...)
    public UniTask<SystemResult> SaveDataAsync(string key, string data);
    public UniTask<SystemResult> LoadDataAsync(string key);
}
