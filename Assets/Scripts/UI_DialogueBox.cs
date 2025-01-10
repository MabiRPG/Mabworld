using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DialogueBox : MonoBehaviour, IOverlay, IInputHandler
{
    public int ID;
    public int questID;
    public int npcID;
    public string text;
    public int nextID;
    public Sprite icon;

    [SerializeField]
    private Image npc1Image;
    [SerializeField]
    private TMP_Text npc1Name;
    [SerializeField]
    private TMP_Text npc1Cultivation;
    [SerializeField]
    private TMP_Text mainText;

    private string overflowText;

    private List<QuestDialogueModel> dialogues;
    private int index = 0;

    public bool isFullscreenFocus { get; set; }

    private void Awake()
    {
        isFullscreenFocus = true;
        AddOverlayCaller();
        InputController.Instance.SetActiveDialogueBox(this);
    }

    public void SetDialogue(List<QuestDialogueModel> dialogues)
    {
        this.dialogues = dialogues;
        gameObject.SetActive(true);
        index = 0;
        SetText();
    }

    private void SetText()
    {
        if (index == dialogues.Count)
        {
            RemoveOverlayCaller();
            InputController.Instance.SetActiveDialogueBox(null);
            Destroy(gameObject);
            return;
        }

        QuestDialogueModel dialogue = dialogues[index];
        NPCModel npc = new NPCModel(GameManager.Instance.Database, dialogue.npcID);
        CultivationStageModel stage = CultivationStageModel.stages
            [(npc.cultivationStageID, npc.cultivationSubstageID)];

        npc1Name.text = npc.name;
        npc1Cultivation.text = $"{stage.name} Stage ({stage.substageName})";
        npc1Image.sprite = npc.icon;
        mainText.text = dialogue.text;
        mainText.ForceMeshUpdate();
        CheckTextOverflow();
    }

    private void CheckTextOverflow()
    {
        if (mainText.isTextOverflowing)
        {
            mainText.text = text[..mainText.firstOverflowCharacterIndex];
            overflowText = text[mainText.firstOverflowCharacterIndex..];
        }
        else
        {
            overflowText = null;
        }
    }

    public void AddOverlayCaller()
    {
        if (!isFullscreenFocus)
        {
            return;
        }

        GameObject obj = GameManager.Instance.overlay;
        Overlay overlay = obj.GetComponent<Overlay>();
        overlay.AddCaller(gameObject);
    }

    public void RemoveOverlayCaller()
    {
        if (!isFullscreenFocus)
        {
            return;
        }

        GameObject obj = GameManager.Instance.overlay;
        Overlay overlay = obj.GetComponent<Overlay>();
        overlay.RemoveCaller(gameObject);
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (overflowText == null)
            {
                index++;
                SetText();
            }
            else
            {
                mainText.text = overflowText;
                mainText.ForceMeshUpdate();
                CheckTextOverflow();
            }
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (overflowText == null)
            {
                index++;
                SetText();
            }
            else
            {
                mainText.text = overflowText;
                mainText.ForceMeshUpdate();
                CheckTextOverflow();
            }
        }
    }
}
