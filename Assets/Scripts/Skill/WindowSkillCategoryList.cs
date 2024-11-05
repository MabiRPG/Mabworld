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

    private Dictionary<Button, int> buttons = new Dictionary<Button, int>();
    public IntManager categoryIndex = new IntManager();

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
            
            WindowSkillCategoryButton script = obj.GetComponent<WindowSkillCategoryButton>();
            script.SetCategory(pair.Value);

            Button button = obj.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { WindowSkill.Instance.categoryIndex.Value = pair.Key; });

            buttons.Add(button, pair.Key);
        }

        Draw();
        ChangeCategory(1);
    }

    private void OnEnable()
    {
        Player.Instance.skillManager.learnEvent.OnChange += Draw;
        WindowSkill.Instance.categoryIndex.OnChange += 
            delegate { ChangeCategory(WindowSkill.Instance.categoryIndex.Value); };
        Draw();
    }

    private void OnDisable()
    {
        Player.Instance.skillManager.learnEvent.OnChange -= Draw;
        WindowSkill.Instance.categoryIndex.OnChange -=
            delegate { ChangeCategory(WindowSkill.Instance.categoryIndex.Value); };
    }

    private void Draw()
    {
        buttonPrefabFactory.SetActiveAll(false);

        foreach (string name in Player.Instance.skillManager.GetLearnedCategories())
        {
            buttonPrefabFactory.GetFree(name, transform);
        }
    }

    private void ChangeCategory(int index)
    {
        foreach (var button in buttons)
        {
            if (!button.Key.isActiveAndEnabled)
            {
                continue;
            }

            if (button.Value != index)
            {
                button.Key.interactable = false;
            }
            else
            {
                button.Key.interactable = true;
            }
        }
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (RaycastResult hit in graphicHits)
            {
                if (hit.gameObject.TryGetComponent(out WindowSkillCategoryButton script))
                {
                    Button button = script.gameObject.GetComponent<Button>();
                    
                    if (buttons.ContainsKey(button))
                    {
                        WindowSkill.Instance.categoryIndex.Value = buttons[button];
                        break;
                    }
                }
            }
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        throw new System.NotImplementedException();
    }
}