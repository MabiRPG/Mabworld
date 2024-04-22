using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Window_Skill : MonoBehaviour
{
    public static Window_Skill instance = null;
    
    public GameObject skillPrefab;
    private List<GameObject> skillList = new List<GameObject>(); 
    
    void Awake() 
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        // Hides the object at start
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Populates the skill list once.
        if (skillList.Count > 0)
        {
            return;
        }

        // For every skill, create a new prefab to display skill info in window.
        foreach (KeyValuePair<int, Skill> skill in Player.instance.skills)
        {
            // Instantiates the prefab in the window. Parent window has
            // a vertical layout group to control children components.
            GameObject obj;
            obj = Instantiate(skillPrefab, transform);
            
            // Finds the skill name field and reassigns it.
            TMP_Text name = obj.transform.Find("Name").GetComponent<TMP_Text>();
            name.text = skill.Value.info["name"].ToString();

            // Finds the skill icon sprite and reassigns it.
            Image img = obj.GetComponentInChildren<Image>();
            string dir = "Sprites/Skill Icons/" + skill.Value.info["icon_name"].ToString();
            img.sprite = Resources.Load<Sprite>(dir);

            // Finds the skill rank field and reassigns it.
            TMP_Text rank = obj.transform.Find("Rank").GetComponent<TMP_Text>();
            rank.text = "Rank " + skill.Value.rank;

            // Adds it to the list.
            skillList.Add(obj);
        }
    }

    public void ToggleVisible()
    {
        // Changes visibilty of object.
        gameObject.SetActive(!gameObject.activeSelf);
        
        // Destroys all objects
        foreach (GameObject obj in skillList)
        {
            Destroy(obj);
        }

        // Clears the list for update
        skillList.Clear();
    }
}
