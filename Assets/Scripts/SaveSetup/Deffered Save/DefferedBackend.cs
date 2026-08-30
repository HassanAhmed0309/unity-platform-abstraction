using System;
using System.Collections.Generic;

public class DefferedBackend
{
    Dictionary<string,string> savedData = new();
    public void Save(string key, string val, Action<bool> actionCallback)
    {
        savedData[key] = val;
        actionCallback?.Invoke(true);    
    }
    public string Load(string key) => savedData[key];
}
