using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    public static LightController Instance { get; private set; }

    private Light2D globalLight;

    [Header("Colors")]
    [SerializeField]
    private Color32 dawnColor = new Color32(91, 46, 0, 255);
    [SerializeField]
    private Color32 dayColor = new Color32(255, 255, 255, 255);
    [SerializeField]
    private Color32 duskColor = new Color32(91, 46, 0, 255);
    [SerializeField]
    private Color32 nightColor = new Color32(4, 4, 5, 255);

    [Header("Timers")]
    [SerializeField]
    private int dawnDurationInSeconds = 10;
    [SerializeField]
    private int dayDurationInSeconds = 100;
    [SerializeField]
    private int duskDurationInSeconds = 10;
    [SerializeField]
    private int nightDurationInSeconds = 100;

    private void Awake()
    {
        // Singleton recipe so only one instance is active at a time.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        globalLight = GetComponent<Light2D>();

        StartCoroutine(Clock());
    }

    private IEnumerator Clock()
    {
        while (true)
        {
            StartCoroutine(BeginLightingCycle(dawnDurationInSeconds, dawnColor));
            yield return new WaitForSeconds(dawnDurationInSeconds);
            StartCoroutine(BeginLightingCycle(dayDurationInSeconds, dayColor));
            yield return new WaitForSeconds(dayDurationInSeconds);
            StartCoroutine(BeginLightingCycle(duskDurationInSeconds, duskColor));
            yield return new WaitForSeconds(duskDurationInSeconds);
            StartCoroutine(BeginLightingCycle(nightDurationInSeconds, nightColor));
            yield return new WaitForSeconds(nightDurationInSeconds);
        }
    }

    private IEnumerator BeginLightingCycle(int duration, Color32 targetColor)
    {
        Color currentColor = globalLight.color;
        Color stepColor = (targetColor - currentColor) / duration;

        for (int i = 0; i < duration; i++)
        {
            globalLight.color += stepColor;
            yield return new WaitForSeconds(1);
        }
    }
}