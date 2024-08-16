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
        WindowSkill.Instance.categoryIndex.OnChange += 
            delegate { ChangeCategory(WindowSkill.Instance.categoryIndex.Value); };
    }

    private void OnDisable()
    {
        WindowSkill.Instance.categoryIndex.OnChange -= 
            delegate { ChangeCategory(WindowSkill.Instance.categoryIndex.Value); };
    }

    private void ChangeCategory(int index)
    {
        skillRowPrefabFactory.SetActiveAll(false);

        foreach (Skill skill in Player.Instance.skillManager.GetLearnedSkillsByCategory(index))
        {
            GameObject obj = skillRowPrefabFactory.GetFree(skill, transform.Find("Viewport/Content"));
            WindowSkillRow row = obj.GetComponent<WindowSkillRow>();
            row.SetSkill(skill, () => { }, () => { });
        }
    }
}