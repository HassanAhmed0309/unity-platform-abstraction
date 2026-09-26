using UnityEngine;

public class Screen : MonoBehaviour
{
    [SerializeField] ScreenTitle screenTitle;

    public virtual void Start()
    {
        UIManager.assignToScreenList?.Invoke(screenTitle, this);
    }

    public virtual void Activate()
    {
        gameObject.SetActive(true);
        // canvasGroup.alpha = 1;
        // canvasGroup.interactable = true;
    }
    public virtual void Deactivate()
    {
        gameObject.SetActive(false);
        // canvasGroup.alpha = 0;
        // canvasGroup.interactable = false;
    }
}
