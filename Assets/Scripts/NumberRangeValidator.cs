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