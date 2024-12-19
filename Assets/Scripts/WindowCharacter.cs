using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Handles processing the character window for the player.
/// </summary>
public class WindowCharacter : Window
{
    public static WindowCharacter Instance = null;

    private TMP_Text actorNameText;
    private TMP_Text actorStageText;
    private TMP_Text actorSubstageText;

    private UI_ProgressBar actorHPBar;
    private UI_ProgressBar actorMPBar;
    private UI_ProgressBar actorXPBar;

    // private TMP_Text actorDamageText;
    // private TMP_Text actorInjuryText;
    // private TMP_Text actorCriticalText;
    // private TMP_Text actorBalanceText;

    private TMP_Text actorStrText;
    private TMP_Text actorIntText;
    private TMP_Text actorDexText;
    private TMP_Text actorLuckText;

    private TMP_Text actorDefenseText;
    // private TMP_Text actorProtectionText;
    // private TMP_Text actorMagicDefenseText;
    // private TMP_Text actorMagicProtectionText;

    private GameObject basicInfoLeft;
    private GameObject equipmentSlots;
    private GameObject basicInfoRight;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
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

        // Creates a hashmap of components to easily reference by name.
        basicInfoLeft = body.transform.Find("Basic Info (L)").gameObject;
        Dictionary<string, TMP_Text> dict = CreateComponentMap(basicInfoLeft.transform);
        actorNameText = dict["name"];
        actorStageText = dict["stage"];
        actorSubstageText = dict["substage"];
        actorStrText = dict["str"];
        actorIntText = dict["int"];
        actorDexText = dict["dex"];
        actorLuckText = dict["luck"];
        actorDefenseText = dict["defense"];

        // HP and MP
        actorHPBar = body.transform.Find("Basic Info (L)/Bars Parent/HP Bar Parent/HP Bar").GetComponent<UI_ProgressBar>();
        actorMPBar = body.transform.Find("Basic Info (L)/Bars Parent/MP Bar Parent/MP Bar").GetComponent<UI_ProgressBar>();
        actorXPBar = body.transform.Find("Basic Info (L)/Bars Parent/XP Bar Parent/XP Bar").GetComponent<UI_ProgressBar>();
    }

    /// <summary>
    ///     Creates a dictionary of TMP_Text components in the transform that end with "Parent".
    /// </summary>
    /// <param name="transform">Transform of the GameObject to search under.</param>
    /// <returns>
    ///     Dictionary of string and TMP_Text components where the keys are the parent
    ///     GameObject's field names, and the values are their corresponding TMP_Text components.
    /// </returns>
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

    /// <summary>
    ///     Called after all Awakes.
    /// </summary>
    private void Start()
    {
        Draw();
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        // Basic Details
        Player.Instance.actorName.OnChange += Draw;
        Player.Instance.actorStage.OnChange += Draw;
        Player.Instance.actorSubstage.OnChange += Draw;

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

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        // Basic Details
        Player.Instance.actorName.OnChange -= Draw;
        Player.Instance.actorStage.OnChange -= Draw;
        Player.Instance.actorSubstage.OnChange -= Draw;

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

    /// <summary>
    ///     Draws the window.
    /// </summary>
    private void Draw()
    {
        // Basic Details
        actorNameText.text = Player.Instance.actorName.Value.ToString();

        CultivationStageModel stage = CultivationStageModel.stages
            [((int)Player.Instance.actorStage.Value, (int)Player.Instance.actorSubstage.Value)];

        actorStageText.text = stage.name;
        actorSubstageText.text = stage.substageName;

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
        actorDefenseText.text = Player.Instance.actorDefense.Value.ToString();
        // actorProtectionText.text = Player.Instance.actorProt.Value.ToString();
        // actorMagicDefenseText.text = Player.Instance.actorMDefense.Value.ToString();
        // actorMagicProtectionText.text = Player.Instance.actorMProt.Value.ToString();

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gameObject.transform);
    }
}