using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

/// <summary>
///     Handles all skill bubble processing above the actor head.
/// </summary>
public class SkillBubble : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    // Limits by which we scale our sprite's local scale (Growing and shrinking effect)
    [SerializeField]
    private float scaleLowerLimit = 0.5f;
    [SerializeField]
    private float scaleUpperLimit = 1f;
    // How quickly we change the scale and at what interval in seconds
    [SerializeField]
    private float scaleChangePerInterval = 0.05f;
    [SerializeField]
    private float scaleChangeInterval = 0.05f;
    // How quickly we fade the skill bubble on cancel
    [SerializeField]
    private float cancelAlphaChangeInterval = 0.1f;
    [SerializeField]
    private float cancelDuration = 0.5f;

    // Addressable filenames to the skill sound effects
    private const string loadFilename = "Skill_Load";
    private const string readyFilename = "Skill_Ready";
    private const string cancelFilename = "Skill_Cancel";
    private AudioClip loadClip;
    private AudioClip readyClip;
    private AudioClip cancelClip;

    public EventManager readyEvent = new EventManager();
    public EventManager cancelEvent = new EventManager();

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        loadClip = Addressables.LoadAssetAsync<AudioClip>(loadFilename).WaitForCompletion();
        readyClip = Addressables.LoadAssetAsync<AudioClip>(readyFilename).WaitForCompletion();
        cancelClip = Addressables.LoadAssetAsync<AudioClip>(cancelFilename).WaitForCompletion();

        // Hides the sprite at the start.
        Hide();
    }

    /// <summary>
    ///     Called when mouse clicked on collider.
    /// </summary>
    private void OnMouseDown()
    {
        // If the skill bubble is visible, interrupt action.
        if (spriteRenderer.color.a == 1)
        {
            Player.Instance.controller.Interrupt();
        }
    }

    /// <summary>
    ///     Shows the skill bubble.
    /// </summary>
    public void Show()
    {
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    /// <summary>
    ///     Hides the skill bubble.
    /// </summary>
    public void Hide()
    {
        spriteRenderer.color = new Color(1, 1, 1, 0);
    }

    /// <summary>
    ///     Gets the coroutine to show the skill readying (loading)
    /// </summary>
    /// <param name="sprite">Skill sprite icon to show.</param>
    /// <param name="duration">Duration in seconds to load the skill.</param>
    /// <returns>Coroutine to be run.</returns>
    public IEnumerator Pulse(Sprite sprite, float duration)
    {
        spriteRenderer.sprite = sprite;
        Show();
        float timer = 0f;

        if (scaleLowerLimit > scaleUpperLimit)
        {
            // Break out of coroutine early
            yield break;
        }

        audioSource.clip = loadClip;
        audioSource.Play();

        // Sets our local scale to the upper limit, and finds the scale change required to
        // change per interval
        Vector3 scale = transform.localScale = new Vector3(scaleUpperLimit, scaleUpperLimit, scaleUpperLimit);
        Vector3 scaleChange = new Vector3(scaleChangePerInterval, scaleChangePerInterval, scaleChangePerInterval);

        // Loop over time, first shrinking the sprite then growing.
        while (timer < duration)
        {
            float interval = Math.Min(duration - timer, scaleChangeInterval);

            while (timer < duration && scale.x > scaleLowerLimit)
            {
                transform.localScale -= scaleChange;
                scale = transform.localScale;
                timer += interval;
                yield return new WaitForSeconds(interval);
            }

            while (timer < duration && scale.x < scaleUpperLimit)
            {
                transform.localScale += scaleChange;
                scale = transform.localScale;
                timer += interval;
                yield return new WaitForSeconds(interval);                
            }
        }

        transform.localScale = new Vector3(scaleUpperLimit, scaleUpperLimit, scaleUpperLimit);
        audioSource.clip = readyClip;
        audioSource.Play();
        readyEvent.RaiseOnChange(); 
    }

    /// <summary>
    ///     Gets the coroutine to show the skill cancelling.
    /// </summary>
    /// <returns>Coroutine to be run.</returns>
    public IEnumerator Fade()
    {
        audioSource.clip = cancelClip;
        audioSource.Play();

        float timer = 0f;
        // Finds the alpha to be changed at every interval
        float alphaInterval = spriteRenderer.color.a / (cancelDuration / cancelAlphaChangeInterval);

        // Start fading the sprite over time
        while (timer <= cancelDuration)
        {
            spriteRenderer.color = new Color(1, 1, 1, spriteRenderer.color.a - alphaInterval);
            timer += cancelAlphaChangeInterval;
            yield return new WaitForSeconds(cancelAlphaChangeInterval);
        }

        Hide();
        cancelEvent.RaiseOnChange();
    }
}
