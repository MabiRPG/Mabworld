using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResultHandler
{
    public Player player;
    public object caller;
    public Skill skill;
    protected ActionHandler nextAction;

    public ResultHandler(Player player, object caller, ActionHandler nextAction = null)
    {
        this.player = player;
        this.caller = caller;
        this.nextAction = nextAction;
    }

    public virtual void Handle(bool isSuccess)
    {
        if (skill != null)
        {
            foreach (SkillTrainingMethod method in skill.methods)
            {
                method.Update(this, caller, isSuccess);
            }
        }

        foreach (Quest quest in player.quests.Values)
        {
            quest.Update(this);
        }

        if (nextAction != null && isSuccess)
        {
            nextAction.HandleNext();
        }
    }
}

public class ResultMoveController : ResultHandler
{
    public ResultMoveController(Player player, object caller, ActionHandler nextAction)
        : base(player, caller, nextAction) { }
}

public class ResultGatherController : ResultHandler
{
    public int resourceID;
    public int resourceGain;

    public ResultGatherController(Player player, object caller, Skill skill)
        : base(player, caller)
    {
        this.skill = skill;
    }

    public override void Handle(bool isSuccess)
    {
        MapResource resource = (MapResource)caller;

        if (isSuccess)
        {
            ActionItemController action = new ActionItemController(
                player,
                caller,
                resource.model.lootTableID
            );
            action.Handle();
            resourceID = action.itemID;
            resourceGain = action.itemCurrentQuantity - action.itemPreviousQuantity;
            resource.UpdateResource();
            Player.Instance.AddXP(100);
        }

        AudioController.Instance.PlayGatherResultSFX(isSuccess);

        base.Handle(isSuccess);
    }
}

public class ResultSkillController : ResultHandler
{
    public readonly ActionSkillController action;

    public ResultSkillController(
        Player player,
        object caller,
        Skill skill,
        ActionSkillController action
    )
        : base(player, caller)
    {
        this.skill = skill;
        this.action = action;
    }
}

public class ResultItemController : ResultHandler
{
    public readonly ActionItemController action;

    public ResultItemController(Player player, object caller, ActionItemController action)
        : base(player, caller)
    {
        this.action = action;
    }
}

public class ResultNPCInteractController : ResultHandler
{
    public readonly ActionNPCInteractController action;

    public ResultNPCInteractController(
        Player player,
        object caller,
        ActionNPCInteractController action
    )
        : base(player, caller)
    {
        this.action = action;
    }
}

public class ResultCraftController : ResultHandler
{
    public readonly List<CraftingRecipeIngredientModel> ingredients;
    public readonly List<CraftingRecipeProductModel> products;
    public readonly int quantity;

    public ResultCraftController(
        Player player,
        object caller,
        List<CraftingRecipeIngredientModel> ingredients,
        List<CraftingRecipeProductModel> products,
        int quantity
    )
        : base(player, caller)
    {
        this.ingredients = ingredients;
        this.products = products;
        this.quantity = quantity;
    }

    public override void Handle(bool isSuccess)
    {
        if (!isSuccess)
        {
            return;
        }

        foreach (CraftingRecipeProductModel product in products)
        {
            ActionItemController action = new ActionItemController(
                Player.Instance,
                this,
                product.itemID,
                product.quantity * quantity,
                ActionItemController.ActionType.ItemAdd
            );
            action.Handle();
        }

        foreach (CraftingRecipeIngredientModel ingredient in ingredients)
        {
            ActionItemController action = new ActionItemController(
                Player.Instance,
                this,
                ingredient.itemID,
                ingredient.quantity * quantity,
                ActionItemController.ActionType.ItemRemove
            );
            action.Handle();
        }        
    }
}
