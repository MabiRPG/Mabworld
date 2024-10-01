using System;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleEditorWindow : EditorWindow
{
    private MultiColumnListView leftPane;
    private VisualElement rightPane;

    private string firstAvailableRank;
    private string lastAvailableRank;

    private List<SkillModel> skills;
    private List<SkillTypeModel> skillTypes;
    private List<SkillStatTypeModel> skillStatTypes;
    private List<TrainingMethodTypeModel> trainingMethodTypes;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/SimpleEditorWindow")]
    public static void ShowExample()
    {
        SimpleEditorWindow wnd = GetWindow<SimpleEditorWindow>();
        wnd.titleContent = new GUIContent("SimpleEditorWindow");
    }

    private void Initialize()
    {
        DatabaseManager database = new DatabaseManager("mabinogi.db");
        DataTable dt = database.Query("SELECT * FROM skill;");
        skills = new List<SkillModel>();

        foreach (DataRow row in dt.Rows)
        {
            SkillModel skill = new SkillModel(database, row);
            skills.Add(skill);
        }

        dt = database.Query("SELECT * FROM skill_category_type;");
        skillTypes = new List<SkillTypeModel>();

        foreach (DataRow row in dt.Rows)
        {
            SkillTypeModel skillType = new SkillTypeModel(database, row);
            skillTypes.Add(skillType);
        }

        dt = database.Query("SELECT * FROM skill_stat_type;");
        skillStatTypes = new List<SkillStatTypeModel>();

        foreach (DataRow row in dt.Rows)
        {
            SkillStatTypeModel skillStatType = new SkillStatTypeModel(database, row);
            skillStatTypes.Add(skillStatType);
        }

        dt = database.Query("SELECT * FROM training_method_type;");
        trainingMethodTypes = new List<TrainingMethodTypeModel>();

        foreach (DataRow row in dt.Rows)
        {
            TrainingMethodTypeModel trainingMethodType = new TrainingMethodTypeModel(database, row);
            trainingMethodTypes.Add(trainingMethodType);
        }
    }

    public void CreateGUI()
    {
        Initialize();

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

        MultiColumnListView statView = rootVisualElement.Q<MultiColumnListView>("selectedStats");

        statView.columns["stat"].makeCell = () => new DropdownField();
        statView.columns["stat"].bindCell = (item, j) => {
            List<string> names = new List<string>();

            foreach (SkillStatTypeModel statType in skillStatTypes)
            {
                if (statType.ID == (statView.itemsSource[j] as SkillStatModel).statID)
                {
                    (item as DropdownField).value = statType.name;
                }

                names.Add(statType.name);
            }

            (item as DropdownField).choices = names;
        };

        for (int i = 1; i <= 15; i++)
        {
            int j = i; // For local purposes.
            string hex = j.ToString("X");

            statView.columns[hex].makeCell = () => new FloatField();
            statView.columns[hex].bindCell = (item, k) => {
                // Truncate ranks that are not available in skill.
                if (j > int.Parse(firstAvailableRank, System.Globalization.NumberStyles.HexNumber) ||
                    j < int.Parse(lastAvailableRank, System.Globalization.NumberStyles.HexNumber))
                {
                    statView.columns[hex].visible = false;
                    return;
                }

                statView.columns[hex].visible = true;
                (item as FloatField).value = (statView.itemsSource[k] as SkillStatModel).values[15 - j]; 
            };
        }

        statView.columns["delete"].makeCell = () =>
        {
            Button button = new Button();
            // Must use a callback here to prevent duplicating click events in bind.
            button.RegisterCallback<ClickEvent>(e => RemoveStatAt(statView, button));
            return button;
        };

        statView.columns["delete"].bindCell = (item, j) =>
        {
            (item as Button).text = "X";
            (item as Button).userData = j;
        };

        Button statAddButton = rootVisualElement.Q<Button>("statAddButton");
        statAddButton.clicked += () =>
        {
            statView.itemsSource.Add(new SkillStatModel());
            statView.RefreshItems();
        };

        MultiColumnListView trainingView = rootVisualElement.Q<MultiColumnListView>("selectedTraining");

        trainingView.columns["name"].makeCell = () => new DropdownField();
        trainingView.columns["name"].bindCell = (item, j) =>
        {
            List<string> names = new List<string>();

            foreach (TrainingMethodTypeModel trainingMethod in trainingMethodTypes)
            {
                if (trainingMethod.ID == (trainingView.itemsSource[j] as TrainingMethodModel).trainingMethodID)
                {
                    (item as DropdownField).value = trainingMethod.name;
                }

                names.Add(trainingMethod.name);
            }

            (item as DropdownField).choices = names;
        };

        trainingView.columns["rank"].makeCell = () => new DropdownField();
        trainingView.columns["rank"].bindCell = (item, j) =>
        {
            (item as DropdownField).value = (trainingView.itemsSource[j] as TrainingMethodModel).rank;
            (item as DropdownField).choices = SkillModel.ranks;
        };

        trainingView.columns["xpGainEach"].makeCell = () => new FloatField();
        trainingView.columns["xpGainEach"].bindCell = (item, j) =>
        {
            (item as FloatField).value = (trainingView.itemsSource[j] as TrainingMethodModel).xpGainEach;
        };

        trainingView.columns["countMax"].makeCell = () => new IntegerField();
        trainingView.columns["countMax"].bindCell = (item, j) =>
        {
            (item as IntegerField).value = (trainingView.itemsSource[j] as TrainingMethodModel).countMax;
        };

        trainingView.columns["total"].makeCell = () => new Label();
        trainingView.columns["total"].bindCell = (item, j) =>
        {
            float totalXP = (trainingView.itemsSource[j] as TrainingMethodModel).xpGainEach;
            totalXP *= (trainingView.itemsSource[j] as TrainingMethodModel).countMax;
            (item as Label).text = totalXP.ToString();
        };
    }

    private void RemoveStatAt(MultiColumnListView statView, Button button)
    {
        statView.itemsSource.RemoveAt((int)button.userData);
        statView.RefreshItems();
    }

    private void OnSkillSelectionChange(IEnumerable<int> selectedIndex)
    {
        var enumerator = selectedIndex.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return;
        }

        int index = enumerator.Current;

        Button selectedIcon = rootVisualElement.Q<Button>("selectedIcon");
        selectedIcon.style.backgroundImage = new StyleBackground(skills[index].icon);
        //selectedIcon.clicked += ChangeIcon;

        TextField selectedName = rootVisualElement.Q<TextField>("selectedName");
        selectedName.value = skills[index].name;

        DropdownField selectedCategory = rootVisualElement.Q<DropdownField>("selectedCategory");
        List<string> names = new List<string>();

        foreach (SkillTypeModel category in skillTypes)
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
        firstAvailableRank = skills[index].firstAvailableRank;
        selectedFirstRank.choices = SkillModel.ranks;

        DropdownField selectedStartRank = rootVisualElement.Q<DropdownField>("selectedStartRank");
        selectedStartRank.value = skills[index].startingRank;
        selectedStartRank.choices = SkillModel.ranks;

        DropdownField selectedLastRank = rootVisualElement.Q<DropdownField>("selectedLastRank");
        selectedLastRank.value = skills[index].lastAvailableRank;
        lastAvailableRank = skills[index].lastAvailableRank;
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

        MultiColumnListView statView = rootVisualElement.Q<MultiColumnListView>("selectedStats");
        List<SkillStatModel> stats = new List<SkillStatModel>(skills[index].stats);
        statView.itemsSource = stats;
        statView.RefreshItems();

        MultiColumnListView trainingView = rootVisualElement.Q<MultiColumnListView>("selectedTraining");
        List<TrainingMethodModel> trainingMethods = new List<TrainingMethodModel>(skills[index].trainingMethods);
        trainingView.itemsSource = trainingMethods;
        trainingView.RefreshItems();
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
