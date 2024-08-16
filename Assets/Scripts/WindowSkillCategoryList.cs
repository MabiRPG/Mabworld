using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowSkillCategoryList : MonoBehaviour, IInputHandler
{
    [SerializeField]
    private GameObject categoryButtonPrefab;
    private PrefabFactory buttonPrefabFactory;

    private List<WindowSkillCategoryButton> buttons = new List<WindowSkillCategoryButton>();
    private int index;

    private void Awake()
    {
        buttonPrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        buttonPrefabFactory.SetPrefab(categoryButtonPrefab);
    }

    private void Start()
    {
        foreach (var pair in Player.Instance.skillManager.Categories.OrderBy(p => p.Key))
        {
            GameObject obj = buttonPrefabFactory.GetFree(pair.Value, transform);
            WindowSkillCategoryButton button = obj.GetComponent<WindowSkillCategoryButton>();
            button.SetCategory(pair.Value);
            buttons.Add(button);
        }

        Draw();
        ChangeIndex(0);
    }

    private void OnEnable()
    {
        Player.Instance.skillManager.learnEvent.OnChange += Draw;
        Draw();
    }

    private void OnDisable()
    {
        Player.Instance.skillManager.learnEvent.OnChange -= Draw;
    }

    private void Draw()
    {
        // buttonPrefabFactory.SetActiveAll(false);

        // foreach (string name in Player.Instance.skillManager.GetLearnedCategories())
        // {
        //     buttonPrefabFactory.GetFree(name, transform);
        // }
    }

    private void ChangeIndex(int index)
    {
        this.index = index;

        for (int i = 0; i < buttons.Count; i++)
        {
            if (i != index)
            {
                buttons[i].GetComponent<Button>().interactable = false;
            }
            else
            {
                buttons[i].GetComponent<Button>().interactable = true;
            }
        }
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (RaycastResult hit in graphicHits)
            {
                if (hit.gameObject.TryGetComponent(out WindowSkillCategoryButton button))
                {
                    ChangeIndex(buttons.IndexOf(button));
                    break;
                }
            }
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        throw new System.NotImplementedException();
    }
}