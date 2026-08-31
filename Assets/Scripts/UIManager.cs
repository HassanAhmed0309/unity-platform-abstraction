using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Dropdown saveType_DD;
    public TMP_InputField saveKey_if;
    public TMP_InputField saveData_if;
    public TextMeshProUGUI resultArea_txt;
    public Button saveSubmit_btn;

    string key = "";
    string content = "";

    ISaveService selectedSaveService;

    public void Awake()
    {
        saveSubmit_btn.onClick.AddListener(OnSaveClicked);
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
            SetResult(result);
        }
    }

    bool SetSaveType(string selectedOption)
    {
        if (Enum.TryParse(selectedOption, ignoreCase: true, out SaveType correctType))
        {
            switch (correctType)
            {
                case SaveType.Immediate:
                    selectedSaveService = new InstantSave();
                    break;
                case SaveType.Deffered:
                    selectedSaveService = new DefferedSave();
                    break;
                default:
                    selectedSaveService = new InstantSave();
                    break;
            }
            return true;
        }
        else
        {
            SetResult(SaveResult.Failed);
            return false;
        }
    }

    void SetResult(SaveResult result)
    {
        string msg = "";
        switch (result)
        {
            case SaveResult.OK:
                msg = "Key and Data Saved Successfully!";
                break;
            case SaveResult.Failed:
                msg = "Failure Saving Key and Data!";
                break;
        }
        LogResult(msg);
    }

    void LogResult(string msg)
    {
        resultArea_txt.text += $"[{DateTime.Now.ToLocalTime()}]{msg}\n";
    }

}

public enum SaveType
{
    Immediate,
    Deffered
}