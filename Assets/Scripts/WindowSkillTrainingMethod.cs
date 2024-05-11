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
        methodName = gameObject.transform.Find("Method Name").GetComponent<TMP_Text>();
        field = gameObject.transform.Find("Method Values").GetComponent<TMP_Text>();
    }

    private void OnDisable()
    {
        Clear();
    }

    /// <summary>
    ///     Sets the text.
    /// </summary>
    /// <param name="newMethod">Training method of skill.</param>
    public void SetMethod(SkillTrainingMethod newMethod)
    {
        Clear();
        method = newMethod;
        method.countEvent.OnChange += Draw;
        Draw();
    }

    private void Clear()
    {
        if (method != null)
        {
            method.countEvent.OnChange -= Draw;
            method = null;
        }
    }

    private void Draw()
    {
        string sName = method.name;
        string sValue = string.Format("+{0:0.00} (<color=\"yellow\">{1}<color=\"white\">/{2})",
            method.xpGainEach, method.count, method.countMax);

        if (method.IsComplete())
        {
            sName = "<color=\"grey\">" + sName;
            sValue = string.Format("<color=\"grey\">+{0:0.00} ({1}/{2})",
                method.xpGainEach, method.count, method.countMax);
        }

        methodName.text = sName;
        field.text = sValue;
    }
}