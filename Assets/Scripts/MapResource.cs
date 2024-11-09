using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     Handles all interactable objects in the world.
/// </summary>
public class MapResource : MonoBehaviour, IInputHandler
{
    public MapResourceModel model;
    private SpriteRenderer spriteRenderer;
    public IntManager resource = new IntManager();
    private bool isRegening;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        string name = gameObject.name;

        foreach (MapResourceModel model in GameManager.Instance.Database.mapResourceModels)
        {
            if (name.StartsWith(model.name))
            {
                this.model = model;
                resource.Value = model.resource;
                break;
            }
        }

        if (model == null)
        {
            Destroy(this);
        }
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        // Add event hook
        resource.OnChange += ChangeSpriteState;
        // Set up initial state.
        ChangeSpriteState();
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        resource.Clear();
    }

    /// <summary>
    ///     Changes the sprite state depending on the resource amount.
    /// </summary>
    private void ChangeSpriteState()
    {
        // If resource is full, set sprite, stop regeneration
        if (resource.Value == model.resourceMax)
        {
            spriteRenderer.sprite = model.fullSprite;
            StopCoroutine(Regenerate());
            return;
        }
        // Otherwise, start regen coroutine
        else if (!isRegening && resource.Value == 0)
        {
            StartCoroutine(Regenerate());
        }

        // If empty, set sprite.
        if (resource.Value == 0)
        {
            spriteRenderer.sprite = model.emptySprite;
        }
        // If partial full sprite exists, use it, otherwise default to full.
        else
        {
            if (model.partialFullSprite != null)
            {
                spriteRenderer.sprite = model.partialFullSprite;
            }
            else
            {
                spriteRenderer.sprite = model.fullSprite;
            }
        }
    }

    /// <summary>
    ///     Begins regenerating the resource if necessary through a coroutine.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Regenerate()
    {
        isRegening = true;

        while (resource.Value < model.resourceMax)
        {
            yield return new WaitForSeconds(model.resourceRegenInterval);
            resource.Value = Math.Min(model.resourceMax, 
                resource.Value + model.resourceRegenPerInterval);
        }

        isRegening = false;
    }

    public void UpdateResource()
    {
        // if (resultHandler.isSuccess)
        // {
            resource.Value--;
        // }
    }

    public void HandleMouseInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (resource.Value == 0)
            {
                return;
            }

            Skill playerSkill = Player.Instance.skillManager.Get(model.skillID);

            if (model.rankRequired == null || !playerSkill.IsRankOrGreater(model.rankRequired))
            {
                Player.Instance.HandleMouseInput(graphicHits, sceneHits);
                return;
            }

            ActionGatherController actionController = 
                new ActionGatherController(Player.Instance, this);
            actionController.Handle();
        }
    }

    public void HandleKeyboardInput(List<RaycastResult> graphicHits, RaycastHit2D sceneHits)
    {
        throw new NotImplementedException();
    }
}
