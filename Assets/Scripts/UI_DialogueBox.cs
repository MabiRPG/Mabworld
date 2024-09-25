using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_DialogueBox : MonoBehaviour
{
    public static UI_DialogueBox Instance { get; private set; }

    public int ID;
    public int questID;
    public int npcID;
    public string text;
    public int nextID;
    public Sprite icon; 

    private Image npc1Image;
    private Image npc2Image;
    private TMP_Text npc1Name;
    private TMP_Text npc2Name;
    private TMP_Text mainText;

    private string overflowText;

    private const string dialogueQuery = @"SELECT * FROM quest_dialogue WHERE id = @id
        AND quest_id = @questID LIMIT 1;";
    private const string npcQuery = @"SELECT * FROM npc WHERE id = @id LIMIT 1;";

    private void Awake()
    {
        // Singleton recipe so only one instance is active at a time.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        npc1Image = transform.Find("NPC 1 Image").GetComponent<Image>();
        npc2Image = transform.Find("NPC 2 Image").GetComponent<Image>();
        npc1Name = transform.Find("NPC 1 Image/Image").GetComponentInChildren<TMP_Text>();
        npc2Name = transform.Find("NPC 2 Image/Image").GetComponentInChildren<TMP_Text>();
        mainText = transform.Find("Text Box").GetComponent<TMP_Text>();

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (overflowText != null)
            {
                mainText.text = overflowText;
            }
            else if (nextID != -1)
            {
                Load(nextID);
            }
            else
            {
                gameObject.SetActive(false);
                return;
            }
        }
    }

    public void SetDialogue(int questID)
    {
        this.questID = questID;
        gameObject.SetActive(true);
        Load(1);
    }

    private void Load(int ID)
    {
        this.ID = ID;

        DataTable dt = GameManager.Instance.QueryDatabase(dialogueQuery, 
            ("@id", ID), ("@questID", questID));
        DataRow row = dt.Rows[0];

        int prevNPC = npcID; 

        GameManager.Instance.ParseDatabaseRow(row, this,
            ("npc_id", "npcID"), ("next_id", "nextID"));

        if (prevNPC != npcID)
        {
            dt = GameManager.Instance.QueryDatabase(npcQuery, ("@id", npcID));
            row = dt.Rows[0];
            icon = GameManager.Instance.LoadAsset<Sprite>(row["icon"].ToString());

            npc1Name.text = row["name"].ToString();
            npc1Image.sprite = icon;
        }

        mainText.text = text;
        mainText.ForceMeshUpdate();
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
}
