using UnityEngine;
using UnityEngine.UI;

public class UI_TabButton : MonoBehaviour
{
    public int tabIndex;
    private Button button;
    private UI_TabController controller;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void SetController(UI_TabController controller)
    {
        button.onClick.RemoveAllListeners();
        this.controller = controller;
        button.onClick.AddListener(SetIndex);
    }

    private void SetIndex()
    {
        if (controller == null)
        {
            return;
        }
        else if (controller.currentIndex.Value == tabIndex)
        {
            return;
        }

        controller.currentIndex.Value = tabIndex;
    }
}