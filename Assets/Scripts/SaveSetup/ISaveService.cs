using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
public interface ISaveService
{
    //UniTask<string> --> string is the result of the function (Success, Fail, WaitForCallback,...)
    public UniTask<SaveResult> SaveDataAsync(string key, string data);
    public UniTask<SaveResult> LoadDataAsync(string key);
}
