using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Progress_Bar : MonoBehaviour
{
    public float current;
    public float maximum;
    public Image filledBar;

    private void Update()
    {
        Refresh();
    }

    public void Refresh() 
    {
        float amount = current / maximum;
        
        if (amount > 1)
        {
            amount = 1;
        }
        
        filledBar.fillAmount = amount;
    }
}
