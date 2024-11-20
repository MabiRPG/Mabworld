using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowQuestRow : MonoBehaviour, IInputHandler
{
    private Quest quest;
    private TMP_Text qName;
    private TMP_Text qProgress;

    private void Awake()
    {
        qName = transform.Find("Name").GetComponent<TMP_Text>();
        qProgress = transform.Find("Progress").GetComponent<TMP_Text>();
    }

    public void SetQuest(Quest quest)
    {
        this.quest = quest;
        qName.text = quest.name;
        qProgress.text = quest.questState.ToString();
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            WindowQuest.Instance.SetQuestDetailed(quest);
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
    }
}