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
        Melee_Weapon,
        Ranged_Weapon,
        Magic_Weapon,
    }

    [SerializeField]
    private Slot slot;

    private UI_Item uiItem;

    public void HandleItemPickup(List<RaycastResult> graphicHits, RaycastHit2D sceneHits, UI_Item item)
    {
        foreach ((int statID, float roll) in uiItem.item.stats)
        {
            if (ItemStatTypeModel.FindByID(statID) == "Defense")
            {
                Player.Instance.actorDefense.Value -= roll;
            }
        }

        uiItem.StartPickup();
    }

    public void HandleItemDrop(List<RaycastResult> graphicHits, RaycastHit2D sceneHits, UI_Item uiItem)
    {
        if (ItemTypeModel.FindByID(uiItem.item.model.categoryID) == slot.ToString())
        {
            this.uiItem = uiItem;
            uiItem.SetParent(transform);

            RectTransform rectTransform = uiItem.gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;

            uiItem.EndPickup();

            foreach ((int statID, float roll) in uiItem.item.stats)
            {
                if (ItemStatTypeModel.FindByID(statID) == "Defense")
                {
                    Player.Instance.actorDefense.Value += roll;
                }
            }
        }
    }
}