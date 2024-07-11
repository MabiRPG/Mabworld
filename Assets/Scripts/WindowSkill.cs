using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// TODO : Switch draggable skill icons to proper monobehaviors without cursor.
// public class MovableSkillIcon : MonoBehaviour
// {
//     public Skill skill;
//     private Transform rectTransform;
//     private Image image;

//     private void Awake()
//     {
//         rectTransform = GetComponent<RectTransform>();
//         image = GetComponent<Image>();
//     }

//     public void SetSkill(Skill skill)
//     {
//         this.skill = skill;
//         image.sprite = skill.icon;
//     }
// }

/// <summary>
///     This class handles the skill window processing.
/// </summary>
public class WindowSkill : Window
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
    private PrefabFactory skillPrefabs;
    private PrefabFactory detailedPrefabs;
    private PrefabFactory advancePrefabs;

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

        skillPrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        skillPrefabs.SetPrefab(skillPrefab);
        detailedPrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        detailedPrefabs.SetPrefab(windowSkillDetailedPrefab);
        advancePrefabs = ScriptableObject.CreateInstance<PrefabFactory>();
        advancePrefabs.SetPrefab(windowSkillAdvancePrefab);
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

        skillPrefabs.SetActiveAll(false);
        Draw();
    }

    private void OnDisable()
    {
        Player.Instance.actorAP.OnChange -= Draw;
    }

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         // Stores all the results of our raycasts
    //         List<RaycastResult> hits = new List<RaycastResult>();
    //         // Create a new pointer data for our raycast manipulation
    //         PointerEventData pointerData = new PointerEventData(GetComponent<EventSystem>());
    //         pointerData.position = Input.mousePosition;
    //         // Raycast for any windows underneath
    //         GameManager.Instance.raycaster.Raycast(pointerData, hits);

    //         if (hits.Count == 0)
    //         {
    //             return;
    //         }

    //         if (hits[0].gameObject.name == "Icon")
    //         {
    //             Debug.Log("hit icon");
    //         }
    //     }
    // }

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
            GameObject obj = skillPrefabs.GetFree(skill.Key, body.transform);
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
        GameObject obj = advancePrefabs.GetFree(skill, transform.parent);
        WindowSkillAdvance window = obj.GetComponent<WindowSkillAdvance>();
        window.SetSkill(skill);
        window.Focus();
    }

    /// <summary>
    ///     Creates a new detailed skill window.
    /// </summary>
    /// <param name="skill">Skill to populate window.</param>
    private void CreateDetailedSkillWindow(Skill skill)
    {
        GameObject obj = detailedPrefabs.GetFree(skill, transform.parent);
        WindowSkillDetailed window = obj.GetComponent<WindowSkillDetailed>();
        window.SetSkill(skill, () => CreateAdvanceSkillWindow(skill));
        window.Focus();
    }
}
