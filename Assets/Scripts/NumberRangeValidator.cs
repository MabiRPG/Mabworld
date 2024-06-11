using System;
using TMPro;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "NumberRangeValidator.asset", menuName = "TextMeshPro/Input Validators/Number Range", order = 100)]
public class NumberRangeValidator : TMP_InputValidator
{
    private float minimum = 1;
    private float maximum = 1;

    public void SetRange(float minimum, float maximum)
    {
        this.minimum = minimum;
        this.maximum = maximum;
    }

    /// <summary>
    /// Override Validate method to implement your own validation
    /// </summary>
    /// <param name="text">This is a reference pointer to the actual text in the input field; 
    /// changes made to this text argument will also result in changes made to text shown 
    /// in the input field</param>
    /// <param name="pos">This is a reference pointer to the input field's text insertion 
    /// index position (your blinking caret cursor); changing this value will also 
    /// change the index of the input field's insertion position</param>
    /// <param name="ch">This is the character being typed into the input field</param>
    /// <returns>Return the character you'd allow into </returns>
    public override char Validate(ref string text, ref int pos, char ch)
    {
        if (!char.IsNumber(ch))
        {
            return (char)0;
        }

        float value = float.Parse(text.Insert(pos, ch.ToString()));

        if (value < minimum)
        {
            text = minimum.ToString();
            pos = text.Length;
            return text[^1];
        }
        else if (value > maximum)
        {
            text = maximum.ToString();
            pos = text.Length;
            return text[^1];
        }
        else
        {
            text = text.Insert(pos, ch.ToString());
            pos += 1;
            return ch;
        }
    }
}