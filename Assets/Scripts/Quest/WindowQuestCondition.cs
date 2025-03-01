using System.Linq;
using System.Text;
using TMPro;
using TypeExtension;
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
        StringBuilder nameBuilder = new StringBuilder();
        StringBuilder stateBuilder = new StringBuilder();
        int categoryID = condition.model.conditionID;
        int conditionCategoryID = QuestConditionTypeModel.GetCategory(categoryID);

        nameBuilder.Append($"{QuestConditionTypeModel.FindByID(categoryID)} ");

        switch (QuestConditionCategoryTypeModel.FindByID(conditionCategoryID))
        {
            case "Skill":
                SkillModel skill = new SkillModel(
                    GameManager.Instance.Database,
                    int.Parse(condition.model.param1)
                );

                string skillName = skill.Name;
                string rank = condition.model.param2;

                // nameBuilder.Replace(" skill", "");
                stateBuilder.AppendFormat($"{skillName} Rank {rank}");

                // Skill playerSkill = Player.Instance.skillManager.Get(skill.ID);

                break;
            case "Item":
                ItemModel item = new ItemModel(
                    GameManager.Instance.Database,
                    int.Parse(condition.model.param1)
                );

                string itemName = item.Name;
                int quantity = int.Parse(condition.model.param2);

                nameBuilder.Replace(" item", "");
                nameBuilder.AppendFormat($"{itemName}");

                int playerQuantity = Player.Instance.inventoryManager.GetQuantity(item.ID);

                stateBuilder.AppendFormat($"({playerQuantity} / {quantity})");

                break;
            case "Cultivation stage":
                CultivationStageModel stage = CultivationStageModel.stages[
                    (int.Parse(condition.model.param1), int.Parse(condition.model.param2))
                ];
                string stageName = stage.name;
                string substageName = stage.substageName;

                // nameBuilder.Replace(" cultivation stage", "");
                stateBuilder.AppendFormat($"{stageName} - {substageName}");
                stateBuilder.AppendFormat(
                    $" ({int.Parse(condition.model.param2).DisplayWithSuffix()} stage)"
                );

                break;
            case "NPC":
                NPCModel npc = new NPCModel(
                    GameManager.Instance.Database,
                    int.Parse(condition.model.param1)
                );
                string npcName = npc.name;

                nameBuilder.Replace(" someone", "");
                nameBuilder.AppendFormat(npcName);

                break;
            case "Quest":
                QuestModel quest = new QuestModel(
                    GameManager.Instance.Database,
                    int.Parse(condition.model.param1));

                stateBuilder.AppendFormat(quest.name);

                break;
            default:
                break;
        }

        cName.text = nameBuilder.ToString();
        cState.text = stateBuilder.ToString();

        if (condition.state.Value)
        {
            cName.alpha = 0.1f;
            cState.alpha = 0.1f;
        }
        else
        {
            cName.alpha = 1f;
            cState.alpha = 1f;
        }
    }
}
