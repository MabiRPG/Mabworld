using TMPro;
using UnityEngine;

/// <summary>
///     Handles rendering skill training methods in window.
/// </summary>
public class WindowSkillTrainingMethod : MonoBehaviour 
{
    private TMP_Text name;
    private TMP_Text field;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        name = gameObject.transform.Find("Method Name").GetComponent<TMP_Text>();
        field = gameObject.transform.Find("Method Values").GetComponent<TMP_Text>();
    }

    /// <summary>
    ///     Sets the text.
    /// </summary>
    /// <param name="method">Training method of skill.</param>
    public void SetText(TrainingMethod method)
    {
        string sName = method.method["name"].ToString();
        float xpGainEach = float.Parse(method.method["xp_gain_each"].ToString());
        object count = method.method["count"];
        object countMax = method.method["count_max"];
        string sValue = string.Format("+{0:0.00} (<color=\"yellow\">{1}<color=\"white\">/{2})",
            xpGainEach, count, countMax);

        name.text = sName;
        field.text = sValue;
    }
}