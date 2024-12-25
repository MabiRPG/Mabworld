using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemWorldDrop : MonoBehaviour, IMouseInputHandler
{
    private UI_Item uiItem;

    private Button button;
    private TMP_Text text;
    private RectTransform rectTransform;

    private void Awake()
    {
        button = GetComponent<Button>();
        text = GetComponentInChildren<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetItem(UI_Item uiItem, Vector2 localPosition)
    {
        this.uiItem = uiItem;
        button.image.color = Color.yellow;
        text.text = uiItem.item.model.name;
        rectTransform.localPosition = localPosition;

        // Resets the content size fitter.
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gameObject.transform);
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            ActionMoveController action = new ActionMoveController(
                Player.Instance,
                this,
                graphicHits[0].worldPosition);

            action.OnSuccess += () =>
            {
                uiItem.gameObject.SetActive(true);
                uiItem.StartPickup();
                Destroy(gameObject);
            };

            action.Handle();
        }
    }
}