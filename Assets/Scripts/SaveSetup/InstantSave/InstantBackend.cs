using System;
using System.Collections.Generic;

public class InstantBackend
{
    Dictionary<string, string> savedData = new();
    public void Save(string key, string val) => savedData[key] = val;
    public string Load(string key)
    {
        if (savedData.ContainsKey(key))
            return savedData[key];
        return StaticConstants.EMPTYSTRING;
    }
}


public static class StaticConstants
{
    public const string EMPTYSTRING = ">empty<";
}