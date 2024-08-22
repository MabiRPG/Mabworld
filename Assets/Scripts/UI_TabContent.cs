using UnityEngine;
using UnityEngine.UI;

public class UI_TabContent : MonoBehaviour
{
    public int tabIndex;
    private UI_TabController controller;

    public void SetController(UI_TabController controller)
    {
        this.controller = controller;
        controller.currentIndex.OnChange += () => { CheckIndex(controller.currentIndex.Value); };
    }

    private void CheckIndex(int index)
    {
        if (index == tabIndex)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)controller.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)controller.transform.parent.transform);
    }
}