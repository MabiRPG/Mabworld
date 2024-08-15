using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
///     This class handles the skill window processing.
/// </summary>
public class WindowSkill : Window, IInputHandler
{
    // Global reference.
    public static WindowSkill Instance = null;
    
    // Prefab for every skill row in window.
    [SerializeField]
    private GameObject skillPrefab;
    // Prefab for extra skill detail window.
    [SerializeField]
    private GameObject windowSkillDetailedPrefab;
    // Prefab for skill advancement.
    [SerializeField]
    private GameObject windowSkillAdvancePrefab;
    
    // Prefab manager instances.
    public PrefabFactory skillPrefabFactory;
    public PrefabFactory detailedPrefabFactory;
    public PrefabFactory advancePrefabFactory;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake() 
    {
        base.Awake();

        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        skillPrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        skillPrefabFactory.SetPrefab(skillPrefab);
        detailedPrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        detailedPrefabFactory.SetPrefab(windowSkillDetailedPrefab);
        advancePrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        advancePrefabFactory.SetPrefab(windowSkillAdvancePrefab);
    }

    /// <summary>
    ///     Called after all Awakes.
    /// </summary>
    private void Start()
    {
        Draw();
        // Hides the object at start
        gameObject.SetActive(false);
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        Player.Instance.actorAP.OnChange += Draw;
        skillPrefabFactory.SetActiveAll(false);
        Draw();
    }

    private void OnDisable()
    {
        Player.Instance.actorAP.OnChange -= Draw;
        skillPrefabFactory.SetActiveAll(false);
    }

    /// <summary>
    ///     Draws the window.
    /// </summary>
    private void Draw()
    {
        // For every skill, create a new prefab to display skill info in window.
        foreach (KeyValuePair<int, Skill> skill in Player.Instance.skillManager.Skills)
        {
            // Instantiates the prefab in the window. Parent window has
            // a vertical layout group to control children components.
            GameObject obj = skillPrefabFactory.GetFree(skill.Key, body.transform.Find("Scroll View").Find("Viewport").Find("Content"));
            // Gets the script, sets the skill and button call functions.
            WindowSkillRow row = obj.GetComponent<WindowSkillRow>();
            row.SetSkill(
                skill.Value, 
                () => CreateDetailedSkillWindow(skill.Value), 
                () => CreateAdvanceSkillWindow(skill.Value)
            );
        }
        
        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gameObject.transform);
    }

    /// <summary>
    ///     Creates a new advance skill window.
    /// </summary>
    /// <param name="skill"></param>
    private void CreateAdvanceSkillWindow(Skill skill)
    {
        GameObject obj = advancePrefabFactory.GetFree(skill, transform.parent);
        WindowSkillAdvance window = obj.GetComponent<WindowSkillAdvance>();
        window.SetSkill(skill);
        WindowManager.Instance.SetActive(window);
    }

    /// <summary>
    ///     Creates a new detailed skill window.
    /// </summary>
    /// <param name="skill">Skill to populate window.</param>
    private void CreateDetailedSkillWindow(Skill skill)
    {
        GameObject obj = detailedPrefabFactory.GetFree(skill, transform.parent);
        WindowSkillDetailed window = obj.GetComponent<WindowSkillDetailed>();
        window.SetSkill(skill, () => CreateAdvanceSkillWindow(skill));
        WindowManager.Instance.SetActive(window);
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     foreach (RaycastResult hit in graphicHits)
        //     {
        //         Debug.Log(hit.gameObject.name);

        //         if (hit.gameObject.TryGetComponent(out IInputHandler handler))
        //         {
        //             handler.HandleMouseInput(graphicHits, sceneHits);
        //             break;
        //         }
        //     }
        // }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        throw new System.NotImplementedException();
    }
}
