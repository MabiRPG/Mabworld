using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleEditorWindow : EditorWindow
{
    private MultiColumnListView leftPane;
    private VisualElement rightPane;

    private List<SkillModel> skills;
    private List<SkillCategoryModel> skillCategories;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/SimpleEditorWindow")]
    public static void ShowExample()
    {
        SimpleEditorWindow wnd = GetWindow<SimpleEditorWindow>();
        wnd.titleContent = new GUIContent("SimpleEditorWindow");
    }

    public void CreateGUI()
    {
        DatabaseManager database = new DatabaseManager("mabinogi.db");
        DataTable dt = database.QueryDatabase("SELECT * FROM skill;");
        skills = new List<SkillModel>();

        foreach (DataRow row in dt.Rows)
        {
            SkillModel skill = new SkillModel(database, row);
            skills.Add(skill);
        }

        dt = database.QueryDatabase("SELECT * FROM skill_category_type;");
        skillCategories = new List<SkillCategoryModel>();

        foreach (DataRow row in dt.Rows)
        {
            SkillCategoryModel skillCategory = new SkillCategoryModel(database, row);
            skillCategories.Add(skillCategory);
        }

        // // Create a two-pane view with the left pane being fixed.
        // var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

        // // Add the view to the visual tree by adding it as a child to the root element.
        // rootVisualElement.Add(splitView);

        // // A TwoPaneSplitView needs exactly two child elements.
        // leftPane = new MultiColumnListView();
        // splitView.Add(leftPane);
        // rightPane = new VisualElement();
        // splitView.Add(rightPane);

        m_VisualTreeAsset.CloneTree(rootVisualElement);

        var splitView = rootVisualElement.Q<TwoPaneSplitView>();
        leftPane = rootVisualElement.Q<MultiColumnListView>();

        leftPane.itemsSource = skills;

        // leftPane.columns.Add(new Column { name = "icon", title = "Icon", width = 40 });
        leftPane.columns["icon"].makeCell = () => new Image();
        leftPane.columns["icon"].bindCell = (item, index) => { (item as Image).sprite = skills[index].icon; };

        // leftPane.columns.Add(new Column { name = "name", title = "Name", width = 150 });
        leftPane.columns["name"].makeCell = () => new Label();
        leftPane.columns["name"].bindCell = (item, index) => { (item as Label).text = skills[index].name; };

        
        leftPane.selectedIndicesChanged += OnSkillSelectionChange;
    }

    private void OnSkillSelectionChange(IEnumerable<int> selectedIndex)
    {
        var enumerator = selectedIndex.GetEnumerator();

        if (enumerator.MoveNext())
        {
            int index = enumerator.Current;
            Button selectedIcon = rootVisualElement.Q<Button>("selectedIcon");
            selectedIcon.style.backgroundImage = new StyleBackground(skills[index].icon);
            //selectedIcon.clicked += ChangeIcon;

            TextField selectedName = rootVisualElement.Q<TextField>("selectedName");
            selectedName.value = skills[index].name;

            DropdownField selectedCategory = rootVisualElement.Q<DropdownField>("selectedCategory");
            List<string> names = new List<string>();

            foreach (SkillCategoryModel category in skillCategories)
            {
                names.Add(category.name);

                if (category.ID == skills[index].categoryID)
                {
                    selectedCategory.value = category.name;
                }
            }

            selectedCategory.choices = names;

            TextField selectedDescription = rootVisualElement.Q<TextField>("selectedDescription");
            selectedDescription.value = skills[index].description;

            TextField selectedDetails = rootVisualElement.Q<TextField>("selectedDetails");
            selectedDetails.value = skills[index].details;

            DropdownField selectedFirstRank = rootVisualElement.Q<DropdownField>("selectedFirstRank");
            selectedFirstRank.value = skills[index].firstAvailableRank;
            selectedFirstRank.choices = SkillModel.ranks;

            DropdownField selectedStartRank = rootVisualElement.Q<DropdownField>("selectedStartRank");
            selectedStartRank.value = skills[index].startingRank;
            selectedStartRank.choices = SkillModel.ranks;       
            
            DropdownField selectedLastRank = rootVisualElement.Q<DropdownField>("selectedLastRank");
            selectedLastRank.value = skills[index].lastAvailableRank;
            selectedLastRank.choices = SkillModel.ranks;

            FloatField selectedBaseLoadTime = rootVisualElement.Q<FloatField>("selectedBaseLoadTime");
            selectedBaseLoadTime.value = skills[index].baseLoadTime;           

            FloatField selectedBaseUseTime = rootVisualElement.Q<FloatField>("selectedBaseUseTime");
            selectedBaseUseTime.value = skills[index].baseLoadTime;    

            FloatField selectedBaseCooldownTime = rootVisualElement.Q<FloatField>("selectedBaseCooldownTime");
            selectedBaseCooldownTime.value = skills[index].baseLoadTime;

            Toggle selectedIsStartingWith = rootVisualElement.Q<Toggle>("selectedIsStartingWith");
            selectedIsStartingWith.value = skills[index].isStartingWith; 

            Toggle selectedIsLearnable = rootVisualElement.Q<Toggle>("selectedIsLearnable");
            selectedIsLearnable.value = skills[index].isLearnable; 

            Toggle selectedIsPassive = rootVisualElement.Q<Toggle>("selectedIsPassive");
            selectedIsPassive.value = skills[index].isPassive; 
        }
    }

    private void ChangeIcon()
    {
        string path = EditorUtility.OpenFilePanel("Select new icon", "Assets/Addressables/Sprites", "png");

        if (path.Length != 0)
        {
            Debug.Log(path);
        }
    }
}
