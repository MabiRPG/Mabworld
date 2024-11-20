using TMPro;
using UnityEngine;

public class WindowQuestCondition : MonoBehaviour 
{
    private TMP_Text cName;
    private TMP_Text cState;

    private void Awake()
    {
        cName = transform.Find("Name").GetComponent<TMP_Text>();
        cState = transform.Find("State").GetComponent<TMP_Text>();
    }

    public void SetCondition(QuestCondition condition)
    {
        cName.text = QuestConditionTypeModel.FindByID(condition.model.conditionID);
        cState.text = condition.State.ToString();
    }
}