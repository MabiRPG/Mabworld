using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     This class handles progress bar rendering.
/// </summary>
public class UI_ProgressBar : MonoBehaviour
{
    public float current;
    public float maximum;
    public Image filledBar;

    /// <summary>
    ///     Sets the current value of the bar.
    /// </summary>
    /// <param name="value">The value to set the bar to.</param>
    public void SetCurrent(float value)
    {
        if (value >= 0)
        {
            current = value;
            Draw();
        }
    }

    /// <summary>
    ///     Sets the maximum value of the bar.
    /// </summary>
    /// <param name="value">The value to set the bar to.</param>
    public void SetMaximum(float value)
    {
        if (value >= 0)
        {
            maximum = value;
            Draw();
        }
    }

    /// <summary>
    ///     Draws the window.
    /// </summary>
    private void Draw() 
    {
        float amount = current / maximum;
        
        if (amount > 1)
        {
            amount = 1;
        }
        
        filledBar.fillAmount = amount;
    }
}
