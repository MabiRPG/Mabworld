using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowCharacter : Window
{
    public static WindowCharacter Instance = null;

    private TMP_Text actorNameText;
    // private TMP_Text actorTitleText;
    // private TMP_Text actorAgeText;
    // private TMP_Text actorRaceText;
    private TMP_Text actorLevelText;
    private TMP_Text actorAPText;

    private ProgressBar actorHPBar;
    private ProgressBar actorMPBar;
    private ProgressBar actorXPBar;

    // private TMP_Text actorDamageText;
    // private TMP_Text actorInjuryText;
    // private TMP_Text actorCriticalText;
    // private TMP_Text actorBalanceText;

    private TMP_Text actorStrText;
    private TMP_Text actorIntText;
    private TMP_Text actorDexText;
    private TMP_Text actorLuckText;

    // private TMP_Text actorDefenseText;
    // private TMP_Text actorProtectionText;
    // private TMP_Text actorMagicDefenseText;
    // private TMP_Text actorMagicProtectionText;

    private GameObject basicInfoLeft;
    private GameObject equipmentSlots;
    private GameObject basicInfoRight;

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

        basicInfoLeft = body.transform.Find("Basic Info (L)").gameObject;
        Dictionary<string, TMP_Text> dict = CreateComponentMap(basicInfoLeft.transform);
        actorNameText = dict["name"];
        actorLevelText = dict["level"];
        actorAPText = dict["ap"];
        actorStrText = dict["str"];
        actorIntText = dict["int"];
        actorDexText = dict["dex"];
        actorLuckText = dict["luck"];

        // HP and MP
        actorHPBar = body.transform.Find("Basic Info (L)").Find("Bars Parent").Find("HP Bar Parent").Find("HP Bar").GetComponent<ProgressBar>();
        actorMPBar = body.transform.Find("Basic Info (L)").Find("Bars Parent").Find("MP Bar Parent").Find("MP Bar").GetComponent<ProgressBar>();
        actorXPBar = body.transform.Find("Basic Info (L)").Find("Bars Parent").Find("XP Bar Parent").Find("XP Bar").GetComponent<ProgressBar>();

        equipmentSlots = body.transform.Find("Equipment Slots").gameObject;
        //dict = CreateComponentMap(equipmentSlots.transform);

        basicInfoRight = body.transform.Find("Basic Info (R)").gameObject;
        //dict = CreateComponentMap(basicInfoRight.transform);

        // Hides the object at start
        gameObject.SetActive(false);

        // Defensive Stats
        // actorDefenseText = body.transform.Find("Basic Information (R)").Find("Defensive Stats Parent").Find("Defense Parent").Find("Value").GetComponent<TMP_Text>();
        // actorProtectionText = body.transform.Find("Basic Information (R)").Find("Defensive Stats Parent").Find("Protection Parent").Find("Value").GetComponent<TMP_Text>();
        // actorMagicDefenseText = body.transform.Find("Basic Information (R)").Find("Defensive Stats Parent").Find("Magic Defense Parent").Find("Value").GetComponent<TMP_Text>();
        // actorMagicProtectionText = body.transform.Find("Basic Information (R)").Find("Defensive Stats Parent").Find("Magic Protection Parent").Find("Value").GetComponent<TMP_Text>();
    }

    private Dictionary<string, TMP_Text> CreateComponentMap(Transform transform)
    {
        Dictionary<string, TMP_Text> dict = new Dictionary<string, TMP_Text>();
        List<TMP_Text> texts = new List<TMP_Text>();
        transform.GetComponentsInChildren<TMP_Text>(false, texts);

        foreach (TMP_Text text in texts)
        {
            GameObject parent = text.gameObject.transform.parent.gameObject;

            if (text.name == "Value" && parent.name.EndsWith("Parent"))
            {
                string key = parent.name.Split(" ")[0];
                key = key.ToLower().Trim();
                dict.Add(key, text);
            }
        }

        return dict;
    }

    private void Start()
    {
        Draw();
    }

    private void OnEnable()
    {
        // Basic Details
        Player.Instance.actorName.OnChange += Draw;
        Player.Instance.actorLevel.OnChange += Draw;
        Player.Instance.actorAP.OnChange += Draw;

        // HP and MP
        Player.Instance.actorHP.OnChange += Draw;
        Player.Instance.actorMP.OnChange += Draw;
        Player.Instance.actorXP.OnChange += Draw;

        // Regular Stats
        Player.Instance.actorStr.OnChange += Draw;
        Player.Instance.actorInt.OnChange += Draw;
        Player.Instance.actorDex.OnChange += Draw;
        Player.Instance.actorLuck.OnChange += Draw;

        // Defensive Stats
        Player.Instance.actorDefense.OnChange += Draw;
        Player.Instance.actorProt.OnChange += Draw;
        Player.Instance.actorMDefense.OnChange += Draw;
        Player.Instance.actorMProt.OnChange += Draw;

        Draw();
    }

    private void OnDisable()
    {
        // Basic Details
        Player.Instance.actorName.OnChange -= Draw;
        Player.Instance.actorLevel.OnChange -= Draw;
        Player.Instance.actorAP.OnChange -= Draw;

        // HP and MP
        Player.Instance.actorHP.OnChange -= Draw;
        Player.Instance.actorMP.OnChange -= Draw;
        Player.Instance.actorXP.OnChange -= Draw;

        // Regular Stats
        Player.Instance.actorStr.OnChange -= Draw;
        Player.Instance.actorInt.OnChange -= Draw;
        Player.Instance.actorDex.OnChange -= Draw;
        Player.Instance.actorLuck.OnChange -= Draw;

        // Defensive Stats
        Player.Instance.actorDefense.OnChange -= Draw;
        Player.Instance.actorProt.OnChange -= Draw;
        Player.Instance.actorMDefense.OnChange -= Draw;
        Player.Instance.actorMProt.OnChange -= Draw;
    }

    private void Draw()
    {
        // Basic Details
        actorNameText.text = Player.Instance.actorName.Value.ToString();
        actorLevelText.text = Player.Instance.actorLevel.Value.ToString();
        actorAPText.text = Player.Instance.actorAP.Value.ToString();

        // HP and MP
        actorHPBar.SetCurrent(Player.Instance.actorHP.Value);
        actorHPBar.SetMaximum(Player.Instance.actorHP.Maximum);
        actorMPBar.SetCurrent(Player.Instance.actorMP.Value);
        actorMPBar.SetMaximum(Player.Instance.actorMP.Maximum);
        actorXPBar.SetCurrent(Player.Instance.actorXP.Value);
        actorXPBar.SetMaximum(Player.Instance.actorXP.Maximum);

        // Regular Stats
        actorStrText.text = Player.Instance.actorStr.Value.ToString();
        actorIntText.text = Player.Instance.actorInt.Value.ToString();
        actorDexText.text = Player.Instance.actorDex.Value.ToString();
        actorLuckText.text = Player.Instance.actorLuck.Value.ToString();

        // Defensive Stats
        // actorDefenseText.text = Player.Instance.actorDefense.Value.ToString();
        // actorProtectionText.text = Player.Instance.actorProt.Value.ToString();
        // actorMagicDefenseText.text = Player.Instance.actorMDefense.Value.ToString();
        // actorMagicProtectionText.text = Player.Instance.actorMProt.Value.ToString();
    }
}
