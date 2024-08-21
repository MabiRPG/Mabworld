using System;
using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    private ProgressBar progressBar;
    private TMP_Text percentageText;


    private void Awake()
    {
        progressBar = GetComponentInChildren<ProgressBar>();
        percentageText = GetComponentInChildren<TMP_Text>();

        progressBar.SetMaximum(100);
    }

    public void SetProgress(float value)
    {
        progressBar.SetCurrent(value * 100);
        percentageText.text = $"Loading... {Math.Round(value * 100, 2)}%"; 
    }
}