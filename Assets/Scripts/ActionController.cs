using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionHandler
{
    protected Player player;
    protected object caller;
    public event Action OnSuccess;
    public event Action OnFailure;
    protected Vector3 destination;

    public ActionHandler(Player player, object caller)
    {
        this.player = player;
        this.caller = caller;
    }

    public void RaiseOnSuccess()
    {
        OnSuccess?.Invoke();
    }

    public void RaiseOnFailure()
    {
        OnFailure?.Invoke();
    }

    public abstract void Handle();
}

public class ActionMoveController : ActionHandler
{
    public ActionMoveController(Player player, object caller, Vector3 destination)
        : base(player, caller)
    {
        this.destination = destination;
    }

    public override void Handle()
    {
        ResultMoveController resultMove = new ResultMoveController(player, caller, this);
        IEnumerator task = player.controller.AttemptMove(destination, resultMove);
        player.controller.SetTask(task);
    }
}

public class ActionGatherController : ActionHandler
{
    private Skill skill;

    public ActionGatherController(Player player, object caller, Skill skill)
        : base(player, caller)
    {
        this.skill = skill;
    }

    public override void Handle()
    {
        ResultGatherController result = new ResultGatherController(player, caller, skill, this);
        IEnumerator task = player.controller.AttemptSkill(skill, result);
        player.controller.SetTask(task);
    }
}

public class ActionSkillController : ActionHandler
{
    private Skill skill;
    private int skillID;
    public ActionType type;

    public enum ActionType
    {
        RankUp,
        RankDown,
        Learn,
    }

    public ActionSkillController(
        Player player,
        object caller,
        int skillID,
        ActionType type = ActionType.Learn
    )
        : base(player, caller)
    {
        this.skillID = skillID;
        this.type = type;
    }

    public ActionSkillController(Player player, object caller, Skill skill, ActionType type)
        : base(player, caller)
    {
        this.skill = skill;
        this.type = type;
    }

    public override void Handle()
    {
        ResultSkillController result = new ResultSkillController(player, caller, skill, this);

        switch (type)
        {
            case ActionType.Learn:
                player.skillManager.Learn(skillID);
                skill = player.skillManager.Get(skillID);
                result.skill = skill;
                result.Handle(true);

                break;
            case ActionType.RankUp:
                if (
                    !skill.CanRankUp()
                    || skill.xp.Value < 100
                    || !player.skillManager.IsLearned(skill)
                )
                {
                    result.Handle(false);
                }
                else
                {
                    skill.RankUp();
                    result.Handle(true);
                }

                break;
            default:
                break;
        }
    }
}

public class ActionItemController : ActionHandler
{
    public int lootTableID;
    public int itemID;
    public int itemPreviousQuantity;
    public int itemCurrentQuantity;
    public ActionType type;

    public enum ActionType
    {
        ItemAdd,
        ItemRemove,
    }

    private int itemQuantity;

    public ActionItemController(
        Player player,
        object caller,
        int lootTableID,
        ActionType type = ActionType.ItemAdd
    )
        : base(player, caller)
    {
        this.type = type;
        this.lootTableID = lootTableID;
    }

    public ActionItemController(
        Player player,
        object caller,
        int itemID,
        int itemQuantity,
        ActionType type = ActionType.ItemAdd
    )
        : base(player, caller)
    {
        this.itemID = itemID;
        this.itemQuantity = itemQuantity;
        this.type = type;
    }

    public override void Handle()
    {
        switch (type)
        {
            case ActionType.ItemAdd:
            {
                if (lootTableID != default)
                {
                    LootGenerator lootGen = new LootGenerator(lootTableID);
                    (int newItemID, int newItemGain) = lootGen.Generate();

                    itemID = newItemID;
                    itemPreviousQuantity = player.inventoryManager.GetQuantity(itemID);
                    itemCurrentQuantity = itemPreviousQuantity;
                    itemCurrentQuantity += player.inventoryManager.AddItem(itemID, newItemGain);
                }
                else
                {
                    itemPreviousQuantity = player.inventoryManager.GetQuantity(itemID);
                    itemCurrentQuantity = itemPreviousQuantity;
                    itemCurrentQuantity += player.inventoryManager.AddItem(itemID, itemQuantity);
                }

                ResultItemController result = new ResultItemController(player, caller, this);
                result.Handle(true);

                break;
            }
            case ActionType.ItemRemove:
            {
                itemPreviousQuantity = player.inventoryManager.GetQuantity(itemID);
                itemCurrentQuantity = player.inventoryManager.RemoveItem(itemID, itemQuantity);
                ResultItemController result = new ResultItemController(player, caller, this);
                result.Handle(true);

                break;
            }
            default:
                break;
        }
    }
}

public class ActionNPCInteractController : ActionHandler
{
    public int NPCID;

    public ActionNPCInteractController(Player player, object caller, int NPCID, Vector3 destination)
        : base(player, caller)
    {
        this.NPCID = NPCID;
        this.destination = destination;
    }

    public override void Handle()
    {
        ActionMoveController moveAction = new ActionMoveController(
            player,
            caller,
            destination
        );
        moveAction.OnSuccess += () =>
        {
            ResultNPCInteractController result = new ResultNPCInteractController(player, caller, this);
            result.Handle(true);
        };
        moveAction.Handle();
    }
}

public class ActionCraftController : ActionHandler
{
    private Skill skill;
    private List<CraftingRecipeIngredientModel> ingredients;
    private List<CraftingRecipeProductModel> products;
    private int quantity;

    public ActionCraftController(
        Player player,
        object caller,
        Vector3 destination,
        Skill skill,
        List<CraftingRecipeIngredientModel> ingredients,
        List<CraftingRecipeProductModel> products,
        int quantity
    )
        : base(player, caller)
    {
        this.destination = destination;
        this.skill = skill;
        this.ingredients = ingredients;
        this.products = products;
        this.quantity = quantity;
    }

    public override void Handle()
    {
        ResultCraftController result = new ResultCraftController(
            player,
            caller,
            ingredients,
            products,
            quantity,
            this
        );
        IEnumerator task = player.controller.AttemptSkill(skill, result);
        player.controller.SetTask(task);
    }
}
