using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }

    private Light2D globalLight;

    private readonly List<Color32> lightingColors = new List<Color32>{
        new Color32(91, 46, 0, 255),
        new Color32(255, 255, 255, 255),
        new Color32(91, 46, 0, 255),
        new Color32(4, 4, 5, 255)
    };
    private readonly int dawnDurationInSeconds = 10;
    private readonly int dayDurationInSeconds = 100;
    private readonly int duskDurationInSeconds = 10;
    private readonly int nightDurationInSeconds = 100;

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
            StartCoroutine(BeginLightingCycle(dawnDurationInSeconds, lightingColors[0]));
            yield return new WaitForSeconds(dawnDurationInSeconds);
            StartCoroutine(BeginLightingCycle(dayDurationInSeconds, lightingColors[1]));
            yield return new WaitForSeconds(dayDurationInSeconds);
            StartCoroutine(BeginLightingCycle(duskDurationInSeconds, lightingColors[2]));
            yield return new WaitForSeconds(duskDurationInSeconds);
            StartCoroutine(BeginLightingCycle(nightDurationInSeconds, lightingColors[3]));
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