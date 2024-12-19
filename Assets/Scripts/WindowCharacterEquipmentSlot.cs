using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowCharacterEquipmentSlot : MonoBehaviour, IItemPickupHandler, IItemDropHandler
{
    public enum Slot
    {
        Helmet,
        Shoulders,
        Body,
        Pants,
        Shoes,
        Gloves,
        Necklace,
        Earring,
        Belt,
        Ring,
        Weapon
    }

    [SerializeField]
    private Slot slot;

    private UI_Item uiItem;

    public void HandleItemPickup(List<RaycastResult> graphicHits, RaycastHit2D sceneHits, UI_Item item)
    {
        foreach ((int statID, float roll) in uiItem.item.stats)
        {
            string statName = ItemStatTypeModel.FindByID(statID);

            if (Player.Instance.primaryStats.ContainsKey(statName))
            {
                Player.Instance.primaryStats[statName].Value -= roll;
                Player.Instance.primaryStats[statName].BaseMaximum -= roll;
            }
            else if (Player.Instance.secondaryStats.ContainsKey(statName))
            {
                Player.Instance.secondaryStats[statName].Value -= roll;
                Player.Instance.secondaryStats[statName].BaseMaximum -= roll;
            }
        }

        uiItem.StartPickup();
    }

    public void HandleItemDrop(List<RaycastResult> graphicHits, RaycastHit2D sceneHits, UI_Item uiItem)
    {
        string itemCategory = ItemTypeModel.FindByID(uiItem.item.model.categoryID);

        if (itemCategory == slot.ToString() ||
            (itemCategory.Contains("Weapon") && slot == Slot.Weapon))
        {
            this.uiItem = uiItem;
            uiItem.SetParent(transform);

            RectTransform rectTransform = uiItem.gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;

            uiItem.EndPickup();

            foreach ((int statID, float roll) in uiItem.item.stats)
            {
                string statName = ItemStatTypeModel.FindByID(statID);

                if (Player.Instance.primaryStats.ContainsKey(statName))
                {
                    Player.Instance.primaryStats[statName].Value += roll;
                    Player.Instance.primaryStats[statName].BaseMaximum += roll;
                }
                else if (Player.Instance.secondaryStats.ContainsKey(statName))
                {
                    Player.Instance.secondaryStats[statName].Value += roll;
                    Player.Instance.secondaryStats[statName].BaseMaximum += roll;
                }
            }
        }
    }
}