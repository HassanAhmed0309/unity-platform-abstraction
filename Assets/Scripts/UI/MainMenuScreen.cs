using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : Screen
{
    [SerializeField] Button SaveSystem_btn;
    [SerializeField] Button LoadSystem_btn;

    void Awake()
    {
        SaveSystem_btn.onClick.AddListener(() => UIManager.activateScreen?.Invoke(ScreenTitle.SaveScreen, true));
        LoadSystem_btn.onClick.AddListener(() => UIManager.activateScreen?.Invoke(ScreenTitle.LoadScreen, true));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
    }

    void OnDestroy()
    {
        SaveSystem_btn.onClick.RemoveAllListeners();
        LoadSystem_btn.onClick.RemoveAllListeners();
    }
}
