using System;
using System.Collections.Generic;

public class InstantBackend
{
    Dictionary<string,string> savedData = new();
    public void Save(string key, string val) => savedData[key] = val;
    public string Load(string key) => savedData[key];
}
