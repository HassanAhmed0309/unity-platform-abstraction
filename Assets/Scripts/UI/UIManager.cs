using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static Action<ScreenTitle, Screen> assignToScreenList;
    public static Action<ScreenTitle, bool> activateScreen;
    public static Action deactivateAll;

    Dictionary<ScreenTitle, Screen> allScreens = new();

    public static ServiceInitializer serviceInitializer;

    void Awake()
    {
        assignToScreenList += AssignToScreenList;
        activateScreen += ActivateScreen;
        deactivateAll += DeactivateAllScreens;

        serviceInitializer = new();
        serviceInitializer.InitializeServices();
    }

    IEnumerator Start()
    {
        yield return null;
        ActivateScreen(ScreenTitle.MainMenu, true);
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

public enum ServiceType
{
    Immediate,
    Deffered,
    Flaky
}

public enum ScreenTitle
{
    SaveScreen,
    LoadScreen,
    MainMenu
}