using UnityEngine;

public class Screen : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] ScreenTitle screenTitle;

    public virtual void Start()
    {
        UIManager.assignToScreenList?.Invoke(screenTitle, this);
    }

    public virtual void Activate()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
    }
    public virtual void Deactivate()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = true;
    }
}
