using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowSkillCategoryButton : MonoBehaviour
{
    private TMP_Text categoryText;

    private void Awake()
    {
        categoryText = GetComponentInChildren<TMP_Text>();
    }

    public void SetCategory(string name)
    {
        categoryText.text = name;
    }
}