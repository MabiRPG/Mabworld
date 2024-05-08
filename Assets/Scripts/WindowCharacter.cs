using TMPro;
using UnityEngine;

public class WindowCharacter : Window
{
    public static WindowCharacter Instance = null;

    private ProgressBar actorHPBar;
    private ProgressBar actorMPBar;
    private TMP_Text actorStrText;
    private TMP_Text actorIntText;
    private TMP_Text actorDexText;
    private TMP_Text actorLuckText;

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
        actorHPBar = character.transform.Find("HP & MP Bar Parent").Find("HP Bar Parent").Find("HP Bar").GetComponent<ProgressBar>();
        actorMPBar = character.transform.Find("HP & MP Bar Parent").Find("MP Bar Parent").Find("MP Bar").GetComponent<ProgressBar>();
        actorStrText = character.transform.Find("Stats Parent").Find("Str Parent").Find("Value").GetComponent<TMP_Text>();
        actorIntText = character.transform.Find("Stats Parent").Find("Int Parent").Find("Value").GetComponent<TMP_Text>();
        actorDexText = character.transform.Find("Stats Parent").Find("Dex Parent").Find("Value").GetComponent<TMP_Text>();
        actorLuckText = character.transform.Find("Stats Parent").Find("Luck Parent").Find("Value").GetComponent<TMP_Text>();
    }

    private void Start()
    {
        Draw();
    }

    private void Draw()
    {
        actorHPBar.SetCurrent(Player.Instance.actorHP.current);
        actorHPBar.SetMaximum(Player.Instance.actorHP.maximum);
        actorMPBar.SetCurrent(Player.Instance.actorMP.current);
        actorMPBar.SetMaximum(Player.Instance.actorMP.maximum);
        actorStrText.text = Player.Instance.actorStr.current.ToString();
        actorIntText.text = Player.Instance.actorInt.current.ToString();
        actorDexText.text = Player.Instance.actorDex.current.ToString();
        actorLuckText.text = Player.Instance.actorLuck.current.ToString();
    }
}
