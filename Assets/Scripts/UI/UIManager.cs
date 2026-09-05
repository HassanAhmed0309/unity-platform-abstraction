using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static Action<ScreenTitle, Screen> assignToScreenList;
    public static Action<ScreenTitle, bool> activateScreen;
    public static Action deactivateAll;

    Dictionary<ScreenTitle, Screen> allScreens = new();

    void Awake()
    {
        assignToScreenList += AssignToScreenList;
        activateScreen += ActivateScreen;
        deactivateAll += DeactivateAllScreens;
    }
    void OnDestroy()
    {
        assignToScreenList -= AssignToScreenList;
        activateScreen -= ActivateScreen;
        deactivateAll -= DeactivateAllScreens;
    }
    void AssignToScreenList(ScreenTitle title, Screen screen)
    {
        allScreens.Add(title, screen);
    }

    public void ActivateScreen(ScreenTitle screenToActivateTitle, bool deactivatePreviousScreens = false)
    {
        if (deactivatePreviousScreens)
        {
            DeactivateAllScreens();
        }

        if (allScreens.Count > 0)
        {
            allScreens[screenToActivateTitle].Activate();
        }
    }

    void DeactivateAllScreens()
    {
        if (allScreens.Count > 0)
        {
            foreach (ScreenTitle title in allScreens.Keys)
            {
                allScreens[title].Deactivate();
            }
        }
    }

}

public enum SaveType
{
    Immediate,
    Deffered
}

public enum ScreenTitle
{
    SaveScreen,
    LoadScreen
}