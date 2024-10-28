using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestWindowEditor : EditorWindow
{
    private Button refreshButton;
    private Button commitButton;

    private int index;
    private QuestModel selectedQuest;
    private MultiColumnListView questView;

    private TextField selectedName;
    private MultiColumnListView prerequisiteView;
    private MultiColumnListView stepView;
    private MultiColumnListView rewardView;

    private Button prerequisiteAddButton;
    private Button stepAddButton;
    private Button rewardAddButton;

    private DatabaseManager database;
    private List<QuestModel> quests;
    private List<SkillModel> skills;
    private List<NPCModel> npcs;
    private List<ItemModel> items;
    private int conversationID;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("MabWorld/Quest Editor")]
    public static void ShowExample()
    {
        QuestWindowEditor wnd = GetWindow<QuestWindowEditor>();
        wnd.titleContent = new GUIContent("QuestWindowEditor");
    }

    private void Initialize()
    {
        database = new DatabaseManager("mabinogi.db");
        
        DataTable dt = database.Read("SELECT id FROM quest;");
        quests = new List<QuestModel>();

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            QuestModel quest = new QuestModel(database, ID);
            quests.Add(quest);            
        }

        dt = database.Read("SELECT id FROM quest_condition_type;");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            new QuestConditionTypeModel(database, ID);
        }

        dt = database.Read("SELECT id FROM quest_condition_category_type;");

         foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            new QuestConditionCategoryTypeModel(database, ID);
        }       

        dt = database.Read("SELECT id FROM skill;");
        skills = new List<SkillModel>();

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            SkillModel skill = new SkillModel(database, ID);
            skills.Add(skill);
        }

        dt = database.Read("SELECT id FROM npc;");
        npcs = new List<NPCModel>();

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            NPCModel npc = new NPCModel(database, ID);
            npcs.Add(npc);
        }

        dt = database.Read("SELECT id FROM item;");
        items = new List<ItemModel>();

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            ItemModel item = new ItemModel(database, ID);
            items.Add(item);
        }

        dt = database.Read("SELECT id FROM cultivation_stage;");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            new CultivationStageModel(database, ID);
        }

        dt = database.Read("SELECT id FROM cultivation_substage;");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            new CultivationSubstageModel(database, ID);
        }        
    }

    public void CreateGUI()
    {
        Initialize();

        m_VisualTreeAsset.CloneTree(rootVisualElement);

        refreshButton = rootVisualElement.Q<Button>("refreshButton");
        commitButton = rootVisualElement.Q<Button>("commitButton");

        questView = rootVisualElement.Q<MultiColumnListView>("questView");
        CreateQuestView();

        selectedName = rootVisualElement.Q<TextField>("selectedName");
        selectedName.RegisterValueChangedCallback(e =>
        {
            selectedQuest.name = e.newValue;
        });

        prerequisiteAddButton = rootVisualElement.Q<Button>("prerequisiteAddButton");
        stepAddButton = rootVisualElement.Q<Button>("stepAddButton");
        rewardAddButton = rootVisualElement.Q<Button>("rewardAddButton");

        prerequisiteView = rootVisualElement.Q<MultiColumnListView>("prerequisiteView");
        CreatePrerequisiteView();

        stepView = rootVisualElement.Q<MultiColumnListView>("stepView");
        CreateStepView();

        rewardView = rootVisualElement.Q<MultiColumnListView>("rewardView");
        CreateRewardView();
    }

    private void CreateQuestView()
    {
        questView.columns["quest"].makeCell = () => new Label();
        questView.columns["quest"].bindCell = (item, index) =>
        {
            (item as Label).text = (questView.itemsSource[index] as QuestModel).name;
        };

        questView.itemsSource = quests;
        questView.selectedIndicesChanged += OnQuestSelectionChange;
        questView.RefreshItems();
    }

    private void CreateQuestInfoView(MultiColumnListView listView)
    {
        listView.columns["condition"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.choices = QuestConditionTypeModel.types.Values.ToList();
            dropdown.RegisterValueChangedCallback(e =>
            {
                QuestConditionModel condition = 
                    (QuestConditionModel)listView.itemsSource[(int)dropdown.userData];
                condition.conditionID = QuestConditionTypeModel.FindByName(e.newValue);
                listView.RefreshItems();
            });

            return dropdown;
        };
        listView.columns["condition"].bindCell = (item, index) =>
        {
            QuestConditionModel condition = (QuestConditionModel)listView.itemsSource[index];
            (item as DropdownField).SetValueWithoutNotify(
                QuestConditionTypeModel.FindByID(condition.conditionID));
            (item as DropdownField).userData = index;
        };       

        listView.columns["condition"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.choices = QuestConditionTypeModel.types.Values.ToList();
            dropdown.RegisterValueChangedCallback(e =>
            {
                QuestConditionModel condition = 
                    (QuestConditionModel)listView.itemsSource[(int)dropdown.userData];
                condition.conditionID = QuestConditionTypeModel.FindByName(e.newValue);
                listView.RefreshItems();
            });

            return dropdown;
        };
        listView.columns["condition"].bindCell = (item, index) =>
        {
            QuestConditionModel condition = (QuestConditionModel)listView.itemsSource[index];
            (item as DropdownField).SetValueWithoutNotify(
                QuestConditionTypeModel.FindByID(condition.conditionID));
            (item as DropdownField).userData = index;
        }; 

        listView.columns["param1"].makeCell = () => new VisualElement();
        listView.columns["param1"].bindCell = (item, index) =>
        {
            item.Clear();

            QuestConditionModel condition = (QuestConditionModel)listView.itemsSource[index];
            int categoryID = QuestConditionTypeModel.GetCategory(condition.conditionID);

            switch (QuestConditionCategoryTypeModel.FindByID(categoryID))
            {
                case "Skill":
                {
                    DropdownField dropdown = new DropdownField();

                    dropdown.SetValueWithoutNotify(skills
                            .Where(v => v.ID == int.Parse(condition.param1))
                            .Select(v => v.name)
                            .First());
                    dropdown.choices = skills
                        .Select(v => v.name)
                        .OrderBy(v => v)
                        .ToList();
                    dropdown.RegisterValueChangedCallback(e =>
                    {
                        condition.param1 = skills
                            .Where(v => v.name == e.newValue)
                            .Select(v => v.ID)
                            .First()
                            .ToString();
                    });

                    item.Add(dropdown);

                    break;
                }
                case "NPC":
                {
                    DropdownField dropdown = new DropdownField();

                    dropdown.SetValueWithoutNotify(npcs
                        .Where(v => v.ID == int.Parse(condition.param1))
                        .Select(v => v.name)
                        .First());
                    dropdown.choices = npcs
                        .Select(v => v.name)
                        .OrderBy(v => v)
                        .ToList();
                    dropdown.RegisterValueChangedCallback(e =>
                    {
                        condition.param1 = npcs
                            .Where(v => v.name == e.newValue)
                            .Select(v => v.ID)
                            .First()
                            .ToString();
                    });

                    item.Add(dropdown);

                    break;
                }
                case "Cultivation stage":
                {
                    DropdownField dropdown = new DropdownField();

                    dropdown.SetValueWithoutNotify(
                        CultivationStageModel.FindByID(int.Parse(condition.param1)));
                    dropdown.choices = CultivationStageModel.types
                        .OrderBy(v => v.Key)
                        .Select(v => v.Value)
                        .ToList();
                    dropdown.RegisterValueChangedCallback(e =>
                    {
                        condition.param1 = CultivationStageModel.FindByName(e.newValue).ToString();
                        listView.RefreshItems();
                    });

                    item.Add(dropdown);

                    break;
                }
                case "Item":
                {
                    DropdownField dropdown = new DropdownField();

                    dropdown.SetValueWithoutNotify(items
                        .Where(v => v.ID == int.Parse(condition.param1))
                        .Select(v => v.name)
                        .First());
                    dropdown.choices = items
                        .Select(v => v.name)
                        .OrderBy(v => v)
                        .ToList();
                    dropdown.RegisterValueChangedCallback(e =>
                    {
                        condition.param1 = items
                            .Where(v => v.name == e.newValue)
                            .Select(v => v.ID)
                            .First()
                            .ToString();
                    });

                    item.Add(dropdown);

                    break;
                }
                case "Dialogue":
                {
                    DropdownField dropdown = new DropdownField();

                    dropdown.SetValueWithoutNotify(condition.param1.ToString());
                    conversationID = int.Parse(condition.param1);
                    dropdown.choices = selectedQuest.dialogues
                        .Select(v => v.conversationID.ToString())
                        .Distinct()
                        .ToList();
                    dropdown.RegisterValueChangedCallback(e =>
                    {
                        condition.param1 = e.newValue;
                        conversationID = int.Parse(e.newValue);
                        listView.RefreshItems();
                    });

                    item.Add(dropdown);

                    break;
                }
                case "Quest":
                {
                    DropdownField dropdown = new DropdownField();

                    dropdown.SetValueWithoutNotify(quests
                        .Where(v => v.ID == int.Parse(condition.param1))
                        .Select(v => v.name)
                        .First());
                    dropdown.choices = quests.Select(v => v.name).ToList();
                    dropdown.RegisterValueChangedCallback(e =>
                    {
                        condition.param1 = quests
                            .Where(v => v.name == e.newValue)
                            .Select(v => v.ID)
                            .First()
                            .ToString();
                    });

                    item.Add(dropdown);

                    break;
                }
                default:
                    break;
            }
        };

        listView.columns["param2"].makeCell = () => new VisualElement();
        listView.columns["param2"].bindCell = (item, index) =>
        {
            item.Clear();

            QuestConditionModel condition = (QuestConditionModel)listView.itemsSource[index];
            int categoryID = QuestConditionTypeModel.GetCategory(condition.conditionID);

            switch (QuestConditionCategoryTypeModel.FindByID(categoryID))
            {
                case "Skill":
                {
                    DropdownField dropdown = new DropdownField();

                    dropdown.SetValueWithoutNotify(condition.param2);
                    dropdown.choices = SkillModel.ranks;
                    dropdown.RegisterValueChangedCallback(e =>
                    {
                        condition.param2 = e.newValue;
                    });

                    item.Add(dropdown);

                    break;
                }
                case "Cultivation stage":
                {
                    DropdownField dropdown = new DropdownField();

                    dropdown.SetValueWithoutNotify(CultivationSubstageModel
                        .substages[(int.Parse(condition.param1), int.Parse(condition.param2))]);
                    dropdown.choices = CultivationSubstageModel.substages
                        .Where(v => v.Key.Item1 == int.Parse(condition.param1))
                        .OrderBy(v => v.Key.Item2)
                        .Select(v => v.Value)
                        .ToList();
                    dropdown.RegisterValueChangedCallback(e =>
                    {
                        condition.param2 = CultivationSubstageModel.substages
                            .Where(v => v.Value == e.newValue)
                            .Select(v => v.Key.Item2)
                            .First()
                            .ToString();
                    });

                    item.Add(dropdown);

                    break;
                }
                case "Item":
                {
                    IntegerField field = new IntegerField();

                    field.SetValueWithoutNotify(int.Parse(condition.param2));
                    field.RegisterValueChangedCallback(e =>
                    {
                        condition.param2 = e.newValue.ToString();
                    });

                    item.Add(field);

                    break;
                }
                case "Dialogue":
                {
                    MultiColumnListView dialogueView = CreateDialogueView();
                    item.Add(dialogueView);
                    dialogueView.RefreshItems();

                    break;
                }
                default:
                    break;
            }
        };
    }

    private MultiColumnListView CreateDialogueView()
    {
        MultiColumnListView newView = new MultiColumnListView();
        Column stepColumn = new Column { name = "step", title = "Step #", stretchable = true };
        Column npcColumn = new Column { name = "npc", title = "NPC", stretchable = true };
        Column textColumn = new Column { name = "text", title = "Text", stretchable = true };

        newView.columns.Add(stepColumn);
        newView.columns.Add(npcColumn);
        newView.columns.Add(textColumn);

        newView.columns["step"].makeCell = () => new Label();
        newView.columns["step"].bindCell = (item, index) =>
        {
            QuestDialogueModel dialogue = (QuestDialogueModel)newView.itemsSource[index];
            (item as Label).text = dialogue.ID.ToString();
        };

        newView.columns["npc"].makeCell = () => new DropdownField();
        newView.columns["npc"].bindCell = (item, index) =>
        {
            QuestDialogueModel dialogue = (QuestDialogueModel)newView.itemsSource[index];
            (item as DropdownField).SetValueWithoutNotify(npcs
                .Where(v => v.ID == dialogue.npcID)
                .Select(v => v.name)
                .First());
        };

        newView.columns["text"].makeCell = () =>
        {
            TextField textField = new TextField();
            textField.multiline = true;
            textField.style.flexWrap = Wrap.Wrap;
            textField.style.whiteSpace = WhiteSpace.Normal;
            return textField;
        };
        newView.columns["text"].bindCell = (item, index) =>
        {
            QuestDialogueModel dialogue = (QuestDialogueModel)newView.itemsSource[index];
            (item as TextField).SetValueWithoutNotify(dialogue.text);
        };

        newView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        newView.selectionType = SelectionType.None;
        newView.itemsSource = selectedQuest.dialogues
            .Where(v => v.conversationID == conversationID)
            .ToList();

        return newView;
    }

    private void CreatePrerequisiteView()
    {
        prerequisiteView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

        CreateQuestInfoView(prerequisiteView);

        prerequisiteAddButton.clicked += () =>
        {
            QuestConditionModel condition = new QuestConditionModel(database,
                selectedQuest.ID, QuestModel.prerequisitesTableName);
            prerequisiteView.itemsSource.Add(condition);
            prerequisiteView.RefreshItems();
        };
    }

    private void CreateStepView()
    {
        stepView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

        stepView.columns["step"].makeCell = () => new Label();
        stepView.columns["step"].bindCell = (item, index) =>
        {
            QuestConditionModel step = (QuestConditionModel)stepView.itemsSource[index];
            (item as Label).text = step.stepID.ToString();
        };

        CreateQuestInfoView(stepView);

        stepAddButton.clicked += () =>
        {
            QuestConditionModel condition = new QuestConditionModel(database,
                selectedQuest.ID, QuestModel.stepsTableName);
            stepView.itemsSource.Add(condition);
            stepView.RefreshItems();
        };
    }

    private void CreateRewardView()
    {
        rewardView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        
        CreateQuestInfoView(rewardView);

        rewardAddButton.clicked += () =>
        {
            QuestConditionModel condition = new QuestConditionModel(database,
                selectedQuest.ID, QuestModel.rewardsTableName);
            rewardView.itemsSource.Add(condition);
            rewardView.RefreshItems();
        };
    }

    private void OnQuestSelectionChange(IEnumerable<int> selectedIndex)
    {
        var enumerator = selectedIndex.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return;
        }

        index = enumerator.Current;
        DisplayQuestInfo(index);
    }

    private void DisplayQuestInfo(int index)
    {
        selectedQuest = (QuestModel)questView.itemsSource[index];

        selectedName.SetValueWithoutNotify(selectedQuest.name);

        prerequisiteView.itemsSource = selectedQuest.prerequisites;
        prerequisiteView.RefreshItems();

        stepView.itemsSource = selectedQuest.steps;
        stepView.RefreshItems();

        rewardView.itemsSource = selectedQuest.rewards;
        rewardView.RefreshItems();
    }
}
