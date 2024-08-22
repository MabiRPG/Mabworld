using System.Collections.Generic;
using UnityEngine;

public class UI_TabController : MonoBehaviour
{
    [SerializeField]
    private GameObject tabButtonsParent;
    [SerializeField]
    private GameObject tabContentsParent;

    public IntManager currentIndex = new IntManager();

    private Dictionary<int, UI_TabButton> tabButtons = new Dictionary<int, UI_TabButton>();
    private Dictionary<int, UI_TabContent> tabContents = new Dictionary<int, UI_TabContent>();

    private void Start()
    {
        UI_TabButton[] buttons = tabButtonsParent.GetComponentsInChildren<UI_TabButton>(true);

        foreach (UI_TabButton button in buttons)
        {
            if (!tabButtons.ContainsKey(button.tabIndex))
            {
                tabButtons.Add(button.tabIndex, button);
                button.SetController(this);
            }
        }

        UI_TabContent[] contents = tabContentsParent.GetComponentsInChildren<UI_TabContent>(true);

        foreach (UI_TabContent content in contents)
        {
            if (!tabContents.ContainsKey(content.tabIndex))
            {
                tabContents.Add(content.tabIndex, content);
                content.SetController(this);
            }
        }

        currentIndex.Value = 0;
    }
}