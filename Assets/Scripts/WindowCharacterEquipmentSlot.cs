using System.Collections.Generic;
using System.Linq;
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
    public static Dictionary<string, float> statAccumulator = new Dictionary<string, float>
    {
        {"HP", 0f },
        {"MP", 0f },
        {"STR", 0f },
        {"INT", 0f },
        {"DEX", 0f },
        {"Luck", 0f },
        {"Attack", 0f },
        {"Defense", 0f }
    };

    public void HandleItemPickup(List<RaycastResult> graphicHits, RaycastHit2D sceneHits, UI_Item item)
    {
        foreach ((int statID, float roll) in uiItem.item.stats)
        {
            string statName = ItemStatTypeModel.FindByID(statID);

            if (statAccumulator.ContainsKey(statName))
            {
                statAccumulator[statName] -= roll;
            }
        }

        Player.Instance.UpdateStats();

        uiItem.StartPickup();
    }

    public void HandleItemDrop(List<RaycastResult> graphicHits, RaycastHit2D sceneHits, UI_Item uiItem)
    {
        string itemCategory = ItemTypeModel.FindByID(uiItem.item.model.CategoryID);

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

                if (statAccumulator.ContainsKey(statName))
                {
                    statAccumulator[statName] += roll;
                }

                Debug.Log($"{statName} {roll}");
            }

            Player.Instance.UpdateStats();
        }
    }
}