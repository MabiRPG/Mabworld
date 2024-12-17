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

    private Image npc1Image;
    // private Image npc2Image;
    private TMP_Text npc1Name;
    // private TMP_Text npc2Name;
    private TMP_Text mainText;

    private string overflowText;

    private List<QuestDialogueModel> dialogues;
    private int index = 0;

    public bool isFullscreenFocus { get; set; }

    private void Awake()
    {
        npc1Image = transform.Find("NPC 1 Image").GetComponent<Image>();
        // npc2Image = transform.Find("NPC 2 Image").GetComponent<Image>();
        npc1Name = transform.Find("NPC 1 Image/Image").GetComponentInChildren<TMP_Text>();
        // npc2Name = transform.Find("NPC 2 Image/Image").GetComponentInChildren<TMP_Text>();
        mainText = transform.Find("Text Box").GetComponent<TMP_Text>();

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
        if (dialogues.Count - 1 <= index)
        {
            RemoveOverlayCaller();
            InputController.Instance.SetActiveDialogueBox(null);
            Destroy(gameObject);
        }

        QuestDialogueModel dialogue = dialogues[index];
        NPCModel npc = new NPCModel(GameManager.Instance.Database, dialogue.npcID);

        npc1Name.text = npc.name;
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
