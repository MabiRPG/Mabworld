using System;
using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    private UI_ProgressBar progressBar;
    private TMP_Text percentageText;

    [SerializeField]
    private GameObject percentageTextObject;

    private void Awake()
    {
        progressBar = GetComponentInChildren<UI_ProgressBar>();
        percentageText = percentageTextObject.GetComponent<TMP_Text>();

        progressBar.SetMaximum(100);
    }

    public void SetProgress(float value)
    {
        progressBar.SetCurrent(value * 100);
        percentageText.text = $"Loading... {Math.Round(value * 100, 2)}%";
    }
}