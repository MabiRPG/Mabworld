using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Minimap minimap;

    [SerializeField]
    private GameObject hpBarObject;
    [SerializeField]
    private GameObject windowCharacterButtonObject;
    [SerializeField]
    private GameObject windowSkillButtonObject;
    [SerializeField]
    private GameObject windowInventoryButtonObject;
    [SerializeField]
    private GameObject windowQuestButtonObject;
    [SerializeField]
    private GameObject windowMapButtonObject;

    private UI_ProgressBar hpBar;
    private Button windowCharacterButton;
    private Button windowSkillButton;
    private Button windowInventoryButton;
    private Button windowQuestButton;
    private Button windowMapButton;

    private void Awake()
    {
        hpBar = hpBarObject.GetComponent<UI_ProgressBar>();
        windowCharacterButton = windowCharacterButtonObject.GetComponent<Button>();
        windowSkillButton = windowSkillButtonObject.GetComponent<Button>();
        windowInventoryButton = windowInventoryButtonObject.GetComponent<Button>();
        windowQuestButton = windowQuestButtonObject.GetComponent<Button>();
        windowMapButton = windowMapButtonObject.GetComponent<Button>();
    }

    private void OnEnable()
    {
        minimap.gameObject.SetActive(true);

        Player.Instance.actorHP.OnChange += Draw;
        Player.Instance.actorHP.OnMaximumValueChange += Draw;

        windowCharacterButton.onClick.AddListener(
            InputController.Instance.OpenWindow<WindowCharacter>
        );
        windowSkillButton.onClick.AddListener(
            InputController.Instance.OpenWindow<WindowSkill>
        );
        windowInventoryButton.onClick.AddListener(
            InputController.Instance.OpenWindow<WindowInventory>
        );
        windowQuestButton.onClick.AddListener(
            InputController.Instance.OpenWindow<WindowQuest>
        );

        Draw();
    }

    private void OnDisable()
    {
        minimap.gameObject.SetActive(false);

        Player.Instance.actorHP.OnChange -= Draw;
        Player.Instance.actorHP.OnMaximumValueChange -= Draw;

        windowCharacterButton.onClick.RemoveAllListeners();
        windowSkillButton.onClick.RemoveAllListeners();
        windowInventoryButton.onClick.RemoveAllListeners();
        windowQuestButton.onClick.RemoveAllListeners();
    }

    private void Draw()
    {
        hpBar.SetCurrent(Player.Instance.actorHP.Value);
        hpBar.SetMaximum(Player.Instance.actorHP.Maximum);
    }
}