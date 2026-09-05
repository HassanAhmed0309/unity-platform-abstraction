using System;
using TMPro;
using UnityEngine.UI;

public class SaveSystemScreen : Screen
{
    public TMP_Dropdown saveType_DD;
    public TMP_InputField saveKey_if;
    public TMP_InputField saveData_if;
    public TextMeshProUGUI resultArea_txt;

    public Button saveSubmit_btn;

    string key = "";
    string content = "";

    ISaveService selectedSaveService;
    InstantSave instant;
    DefferedSave deffered;

    public void Awake()
    {
        saveSubmit_btn.onClick.AddListener(OnSaveClicked);
        instant = new InstantSave();
        deffered = new DefferedSave();
    }
    async void OnSaveClicked()
    {
        LogResult("Saved Called!");
        key = saveKey_if.text;
        content = saveData_if.text;
        string selectedOption = saveType_DD.options[saveType_DD.value].text;
        if (SetSaveType(selectedOption))
        {
            SaveResult result = await selectedSaveService.SaveDataAsync(key, content);
            LogResult(result);
        }
    }

    bool SetSaveType(string selectedOption)
    {
        if (Enum.TryParse(selectedOption, ignoreCase: true, out SaveType correctType))
        {
            switch (correctType)
            {
                case SaveType.Immediate:
                    selectedSaveService = instant;
                    break;
                case SaveType.Deffered:
                    selectedSaveService = deffered;
                    break;
                default:
                    selectedSaveService = instant;
                    break;
            }
            return true;
        }
        else
        {
            SaveResult currentResult = new()
            {
                Result = SaveResult.Status.Failed,
                Data = "",
                Reason = $"Save Type Received is not available!"
            };
            LogResult(currentResult);
            return false;
        }
    }
    void LogResult(SaveResult result)
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
