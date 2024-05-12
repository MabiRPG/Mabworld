using TMPro;
using UnityEngine;

public class WindowCharacter : Window
{
    public static WindowCharacter Instance = null;

    // HP and MP
    private ProgressBar actorHPBar;
    private ProgressBar actorMPBar;

    // Regular Stats
    private TMP_Text actorStrText;
    private TMP_Text actorIntText;
    private TMP_Text actorDexText;
    private TMP_Text actorLuckText;

    // Defensive Stats
    private TMP_Text actorDefenseText;
    private TMP_Text actorProtectionText;
    private TMP_Text actorMagicDefenseText;
    private TMP_Text actorMagicProtectionText;

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

        // Hides the object at start
        gameObject.SetActive(false);

        GameObject character = body.transform.Find("Character").gameObject;

        // HP and MP
        actorHPBar = character.transform.Find("HP & MP Bar Parent").Find("HP Bar Parent").Find("HP Bar").GetComponent<ProgressBar>();
        actorMPBar = character.transform.Find("HP & MP Bar Parent").Find("MP Bar Parent").Find("MP Bar").GetComponent<ProgressBar>();

        // Regular Stats
        actorStrText = character.transform.Find("Stats Parent").Find("Str Parent").Find("Value").GetComponent<TMP_Text>();
        actorIntText = character.transform.Find("Stats Parent").Find("Int Parent").Find("Value").GetComponent<TMP_Text>();
        actorDexText = character.transform.Find("Stats Parent").Find("Dex Parent").Find("Value").GetComponent<TMP_Text>();
        actorLuckText = character.transform.Find("Stats Parent").Find("Luck Parent").Find("Value").GetComponent<TMP_Text>();

        // Defensive Stats
        actorDefenseText = character.transform.Find("Defensive Stats Parent").Find("Defense Parent").Find("Value").GetComponent<TMP_Text>();
        actorProtectionText = character.transform.Find("Defensive Stats Parent").Find("Protection Parent").Find("Value").GetComponent<TMP_Text>();
        actorMagicDefenseText = character.transform.Find("Defensive Stats Parent").Find("Magic Defense Parent").Find("Value").GetComponent<TMP_Text>();
        actorMagicProtectionText = character.transform.Find("Defensive Stats Parent").Find("Magic Protection Parent").Find("Value").GetComponent<TMP_Text>();
    }

    private void Start()
    {
        Draw();
    }

    private void OnEnable()
    {
        // HP and MP
        Player.Instance.actorHP.OnChange += Draw;
        Player.Instance.actorMP.OnChange += Draw;

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
    }

    private void OnDisable()
    {
        // HP and MP
        Player.Instance.actorHP.OnChange -= Draw;
        Player.Instance.actorMP.OnChange -= Draw;

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
        // HP and MP
        actorHPBar.SetCurrent(Player.Instance.actorHP.Value);
        actorHPBar.SetMaximum(Player.Instance.actorHP.Maximum);
        actorMPBar.SetCurrent(Player.Instance.actorMP.Value);
        actorMPBar.SetMaximum(Player.Instance.actorMP.Maximum);

        // Regular Stats
        actorStrText.text = Player.Instance.actorStr.Value.ToString();
        actorIntText.text = Player.Instance.actorInt.Value.ToString();
        actorDexText.text = Player.Instance.actorDex.Value.ToString();
        actorLuckText.text = Player.Instance.actorLuck.Value.ToString();

        // Defensive Stats
        actorDefenseText.text = Player.Instance.actorDefense.Value.ToString();
        actorProtectionText.text = Player.Instance.actorProt.Value.ToString();
        actorMagicDefenseText.text = Player.Instance.actorMDefense.Value.ToString();
        actorMagicProtectionText.text = Player.Instance.actorMProt.Value.ToString();
    }
}
