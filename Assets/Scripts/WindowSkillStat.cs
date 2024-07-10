using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

/// <summary>
///     Handles rendering skill stats in window.
/// </summary>
public class WindowSkillStat : MonoBehaviour 
{
    private TMP_Text field;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    private void Awake()
    {
        field = GetComponent<TMP_Text>();
    }

    /// <summary>
    ///     Sets the text.
    /// </summary>
    /// <param name="name">Name of the stat.</param>
    /// <param name="value">Value of the stat.</param>
    public void SetText(string name, float value)
    {
        string sName = name;
        string sValue = value.ToString();
        string s;

        if (name.Contains("rate"))
        {
            sValue = "+" + sValue + "%";
        }
        else if (name.Contains("time"))
        {
            sValue += "s";
        }

        sName = ToCapitalize(sName.Replace("_", " "));
        s = sName + ": " + sValue;
        field.text = s;
    }

    /// <summary>
    ///     Capitalizes every word in string for formatting.
    /// </summary>
    /// <param name="x">String to be formatted.</param>
    /// <returns>Formatted string.</returns>
    protected string ToCapitalize(string x)
    {
        return Regex.Replace(x, @"\b([a-z])", m => m.Value.ToUpper());
    }
}