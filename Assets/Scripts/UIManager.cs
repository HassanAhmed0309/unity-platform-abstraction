using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button saveSubmit;
    public TextMeshProUGUI saveData;
    public TextMeshProUGUI saveKey;
    public Dropdown saveType;

    string key = "";
    string content = "";

    ISaveService seelctedSaveService;

    public void Awake()
    {
        saveSubmit.onClick.AddListener(OnSaveClicked);
    }

    void OnSaveClicked()
    {
        key = saveKey.text;
        content = saveData.text;
        //if()
    }
}
