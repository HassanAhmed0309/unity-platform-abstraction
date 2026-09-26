using System;
using TMPro;
using UnityEngine.UI;

public class LoadSystemScreen : Screen
{
    public TMP_Dropdown loadType_DD;
    public TMP_InputField loadKey_if;
    public TextMeshProUGUI resultArea_txt;

    public Button loadData_btn;
    public Button back_btn;

    string key = "";
    string content = "";

    IService selectedService;

    public void Awake()
    {
        loadData_btn.onClick.AddListener(OnLoadClicked);

        back_btn.onClick.AddListener(() => UIManager.activateScreen?.Invoke(ScreenTitle.MainMenu, true));
    }
    void OnDestroy()
    {
        loadData_btn.onClick.RemoveAllListeners();
        back_btn.onClick.RemoveAllListeners();
    }

    public override void Start()
    {
        base.Start();
    }
    async void OnLoadClicked()
    {
        LogResult("Load Called!");
        key = loadKey_if.text;
        string selectedOption = loadType_DD.options[loadType_DD.value].text;
        if (SetSaveType(selectedOption))
        {
            SystemResult result = await selectedService.LoadDataAsync(key);
            LogResult(result);
        }
    }

    bool SetSaveType(string selectedOption)
    {
        if (Enum.TryParse(selectedOption, ignoreCase: true, out ServiceType correctType))
        {
            selectedService = UIManager.serviceInitializer.GetService(correctType);
            return true;
        }
        else
        {
            SystemResult currentResult = new()
            {
                Result = SystemResult.Status.Failed,
                Data = "",
                Reason = $"Save Type Received is not available!"
            };
            LogResult(currentResult);
            return false;
        }
    }
    void LogResult(SystemResult result)
    {
        string msg = result.Reason;
        resultArea_txt.text += $"[{DateTime.Now.ToLocalTime()}] {msg}\n";
    }
    void LogResult(string result)
    {
        string msg = result;
        resultArea_txt.text += $"[{DateTime.Now.ToLocalTime()}] {msg}\n";
    }
}
