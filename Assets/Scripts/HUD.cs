using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public Minimap minimap;

    private UI_ProgressBar hpBar;

    [SerializeField]
    private GameObject hpBarObject;

    private void Awake()
    {
        hpBar = hpBarObject.GetComponent<UI_ProgressBar>();
    }

    private void OnEnable()
    {
        minimap.gameObject.SetActive(true);

        Player.Instance.actorHP.OnChange += Draw;
        Player.Instance.actorHP.OnMaximumValueChange += Draw;

        Draw();
    }

    private void OnDisable()
    {
        minimap.gameObject.SetActive(false);

        Player.Instance.actorHP.OnChange -= Draw;
        Player.Instance.actorHP.OnMaximumValueChange -= Draw;
    }

    private void Draw()
    {
        hpBar.SetCurrent(Player.Instance.actorHP.Value);
        hpBar.SetMaximum(Player.Instance.actorHP.Maximum);
    }
}