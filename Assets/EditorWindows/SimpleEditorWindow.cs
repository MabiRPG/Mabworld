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

        MultiColumnListView statView = rootVisualElement.Q<MultiColumnListView>("selectedStats");

        statView.columns["stat"].makeCell = () => new DropdownField();
        statView.columns["stat"].bindCell = (item, j) => {
            List<string> names = new List<string>();

            foreach (SkillStatTypeModel statType in skillStatTypes)
            {
                if (statType.ID == (statView.itemsSource[j] as SkillStatModel).statID)
                {
                    // string statName = statType.name.Replace("_", " ");
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
                (item as FloatField).value = (statView.itemsSource[k] as SkillStatModel).values[15 - j]; 
            };
        }

        // statView.columns["delete"].makeCell = () => new Button();
        // statView.columns["delete"].bindCell = (item, j) =>
        // {
        //     (item as Button).text = "X";
        //     (item as Button).clicked += () =>
        //     {
        //         statView.itemsSource.RemoveAt(j);
        //         statView.Rebuild();
        //     };
        // };

        MultiColumnListView trainingView = rootVisualElement.Q<MultiColumnListView>("selectedTraining");

        trainingView.columns["name"].makeCell = () => new Label();
        trainingView.columns["name"].bindCell = (item, j) =>
        {
            List<string> names = new List<string>();

            foreach (TrainingMethodTypeModel trainingMethod in trainingMethodTypes)
            {
                if (trainingMethod.ID == (trainingView.itemsSource[j] as TrainingMethodModel).trainingMethodID)
                {
                    (item as Label).text = trainingMethod.name;
                    break;
                }
            }
        };

        trainingView.columns["rank"].makeCell = () => new Label();
        trainingView.columns["rank"].bindCell = (item, j) =>
        {
            (item as Label).text = (trainingView.itemsSource[j] as TrainingMethodModel).rank;
        };

        trainingView.columns["xpGainEach"].makeCell = () => new Label();
        trainingView.columns["xpGainEach"].bindCell = (item, j) =>
        {
            (item as Label).text = (trainingView.itemsSource[j] as TrainingMethodModel).xpGainEach.ToString();
        };

        trainingView.columns["countMax"].makeCell = () => new Label();
        trainingView.columns["countMax"].bindCell = (item, j) =>
        {
            (item as Label).text = (trainingView.itemsSource[j] as TrainingMethodModel).countMax.ToString();
        };
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

        // var items = new List<TreeViewItemData<string>>(110);
        // for (var i = 0; i < 10; i++)
        // {
        //     var itemIndex = i * 10 + i;

        //     var treeViewSubItemsData = new List<TreeViewItemData<string>>(10);

        //     for (var j = 0; j < 10; j++)
        //         treeViewSubItemsData.Add(new TreeViewItemData<string>(itemIndex + j + 1, (j+1).ToString()));

        //     var treeViewItemData = new TreeViewItemData<string>(itemIndex, (i+1).ToString(), treeViewSubItemsData);
        //     items.Add(treeViewItemData);
        // };

        // List<TreeViewItemData<string>> items = new List<TreeViewItemData<string>>();

        // for (int i = 0; i < SkillModel.ranks.Count; i++)
        // {
        //     int itemIndex = i * 100;

        //     List<TreeViewItemData<string>> subData = new List<TreeViewItemData<string>>(100);

        //     for (int j = 0; j < skills[index].stats.Count; j++)
        //     {
        //         SkillStatModel stat = skills[index].stats[j];
        //         string s_stat = stat.values[i].ToString();

        //         foreach (SkillStatTypeModel statType in skillStatTypes)
        //         {
        //             if (statType.ID == stat.statID)
        //             {
        //                 s_stat = statType.name + " " + s_stat;
        //                 break;
        //             }
        //         }

        //         subData.Add(new TreeViewItemData<string>(itemIndex + j + 1, s_stat));
        //     }

        //     items.Add(new TreeViewItemData<string>(itemIndex, SkillModel.ranks[i], subData));
        // }

        // TreeView selectedRanks = rootVisualElement.Q<TreeView>("selectedRanks");
        // selectedRanks.SetRootItems(items);
        // selectedRanks.makeItem = () => new Label();
        // selectedRanks.bindItem = (item, index) =>
        // {
        //     var i = selectedRanks.GetItemDataForIndex<string>(index);
        //     (item as Label).text = i;
        // };

        // selectedRanks.Rebuild();

        MultiColumnListView statView = rootVisualElement.Q<MultiColumnListView>("selectedStats");
        List<SkillStatModel> stats = skills[index].stats;
        statView.itemsSource = stats;
        statView.Rebuild();

        MultiColumnListView trainingView = rootVisualElement.Q<MultiColumnListView>("selectedTraining");
        List<TrainingMethodModel> trainingMethods = skills[index].trainingMethods;
        trainingView.itemsSource = trainingMethods;
        trainingView.Rebuild();
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
