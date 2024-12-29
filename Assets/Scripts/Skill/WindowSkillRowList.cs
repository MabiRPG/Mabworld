using UnityEngine;

public class WindowSkillRowList : MonoBehaviour
{
    [SerializeField]
    private GameObject skillRowPrefab;
    private PrefabFactory skillRowPrefabFactory;

    private void Awake()
    {
        skillRowPrefabFactory = ScriptableObject.CreateInstance<PrefabFactory>();
        skillRowPrefabFactory.SetPrefab(skillRowPrefab);
    }

    private void OnEnable()
    {
        WindowSkill.Instance.categoryIndex.OnChange += ChangeCategory;
        Player.Instance.skillManager.learnEvent.OnChange += ChangeCategory;

        ChangeCategory();
    }

    private void OnDisable()
    {
        WindowSkill.Instance.categoryIndex.OnChange -= ChangeCategory;
        Player.Instance.skillManager.learnEvent.OnChange -= ChangeCategory;
    }

    private void ChangeCategory()
    {
        int index = WindowSkill.Instance.categoryIndex.Value;

        skillRowPrefabFactory.SetActiveAll(false);

        foreach (Skill skill in Player.Instance.skillManager.GetLearnedSkillsByCategory(index))
        {
            GameObject obj = skillRowPrefabFactory.GetFree(skill,
                transform.Find("Viewport/Content"));
            WindowSkillRow row = obj.GetComponent<WindowSkillRow>();
            row.SetSkill(skill, () => { }, () => { });
        }
    }
}