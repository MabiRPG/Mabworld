using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class WindowQuestCondition : MonoBehaviour 
{
    private QuestCondition condition;
    private TMP_Text cName;
    private TMP_Text cState;

    private void Awake()
    {
        cName = transform.Find("Name").GetComponent<TMP_Text>();
        cState = transform.Find("State").GetComponent<TMP_Text>();
    }

    public void SetCondition(QuestCondition condition)
    {
        if (this.condition != null)
        {
            this.condition.state.OnChange -= Draw;
        }

        this.condition = condition;
        this.condition.state.OnChange += Draw;
        Draw();
    }

    private void Draw()
    {
        StringBuilder builder = new StringBuilder();
        int categoryID = condition.model.conditionID;
        int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

        builder.Append($"{QuestConditionTypeModel.FindByID(categoryID)} ");

        switch (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID))
        {
            case "Skill":
                SkillModel skill = new SkillModel(
                    GameManager.Instance.Database, 
                    int.Parse(condition.model.param1)
                );

                string skillName = skill.name;
                string rank = condition.model.param2;

                builder.Replace(" skill", "");
                builder.AppendFormat($"{skillName} Rank {rank}");

                break;
            case "Item":
                ItemModel item = new ItemModel(
                    GameManager.Instance.Database,
                    int.Parse(condition.model.param1)
                );

                string itemName = item.name;
                int quantity = int.Parse(condition.model.param2);

                builder.Replace(" item", "");
                builder.AppendFormat($"{itemName} x{quantity}");

                break;
            case "Cultivation stage":
                CultivationStageModel stage = CultivationStageModel.stages
                    [(int.Parse(condition.model.param1), int.Parse(condition.model.param2))];
                string stageName = stage.name;
                string substageName = stage.substageName;
                
                builder.Replace(" cultivation stage", "");
                builder.AppendFormat($"{stageName} {substageName}");

                break;
            case "NPC":
                NPCModel npc = new NPCModel(
                    GameManager.Instance.Database,
                    int.Parse(condition.model.param1)
                );
                string npcName = npc.name;

                builder.Replace(" someone", "");
                builder.AppendFormat(npcName);

                break;
            default:
                break;
        }

        cName.text = builder.ToString();
        cState.text = condition.state.Value.ToString();
    }
}