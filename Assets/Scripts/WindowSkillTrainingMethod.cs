using TMPro;
using UnityEngine;

/// <summary>
///     Handles rendering skill training methods in window.
/// </summary>
public class WindowSkillTrainingMethod : MonoBehaviour 
{
    private SkillTrainingMethod method;
    private TMP_Text methodName;
    private TMP_Text field;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        methodName = transform.Find("Method Name").GetComponent<TMP_Text>();
        field = transform.Find("Method Values").GetComponent<TMP_Text>();
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        Clear();
    }

    /// <summary>
    ///     Sets the text.
    /// </summary>
    /// <param name="method">Training method of skill.</param>
    public void SetMethod(SkillTrainingMethod method)
    {
        Clear();
        this.method = method;
        this.method.count.OnChange += Draw;
        Draw();
    }

    /// <summary>
    ///     Clears event handlers.
    /// </summary>
    private void Clear()
    {
        if (method != null)
        {
            method.count.OnChange -= Draw;
            method = null;
        }
    }

    /// <summary>
    ///     Draws the window.
    /// </summary>
    private void Draw()
    {
        string sName = method.name;
        string sValue = string.Format("+{0:0.00} (<color=\"yellow\">{1}<color=\"white\">/{2})",
            method.xpGainEach, method.count.Value, method.countMax);

        if (method.IsComplete())
        {
            sName = "<color=\"grey\">" + sName;
            sValue = string.Format("<color=\"grey\">+{0:0.00} ({1}/{2})",
                method.xpGainEach, method.count.Value, method.countMax);
        }

        methodName.text = sName;
        field.text = sValue;
    }
}