using System;
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

    private TextField nameSearch;
    private int index;
    private QuestModel selectedQuest;
    private MultiColumnListView questView;
    private Button questAddButton;

    private TextField selectedName;
    private MultiColumnListView prerequisiteView;
    private MultiColumnListView stepView;
    private MultiColumnListView rewardView;
    private MultiColumnListView dialogueView;

    private Button prerequisiteAddButton;
    private Button stepAddButton;
    private Button rewardAddButton;

    private DatabaseManager database;
    private List<QuestModel> quests;
    private List<SkillModel> skills;
    private List<NPCModel> npcs;
    private List<ItemModel> items;
    private int conversationID;
    private int questCounter;

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
        questCounter = dt.Rows.Count;

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            QuestModel quest = new QuestModel(database, ID);
            quests.Add(quest);            
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
    }

    public void CreateGUI()
    {
        Initialize();

        m_VisualTreeAsset.CloneTree(rootVisualElement);

        refreshButton = rootVisualElement.Q<Button>("refreshButton");
        refreshButton.RegisterCallback<ClickEvent>(e =>
        {
            Initialize();
            questView.itemsSource = quests;
            DisplayQuestInfo(index);
            questView.RefreshItems();
        });
        commitButton = rootVisualElement.Q<Button>("commitButton");
        commitButton.RegisterCallback<ClickEvent>(e => SaveQuests());

        nameSearch = rootVisualElement.Q<TextField>("nameSearch");
        nameSearch.RegisterValueChangedCallback(e =>
        {
            questView.itemsSource = quests
                .Where(v => v.name.Contains(e.newValue, StringComparison.OrdinalIgnoreCase))
                .ToList();

            questView.RefreshItems();
        });

        questView = rootVisualElement.Q<MultiColumnListView>("questView");
        CreateQuestView();

        selectedName = rootVisualElement.Q<TextField>("selectedName");
        selectedName.RegisterValueChangedCallback(e =>
        {
            selectedQuest.name = e.newValue;
            questView.RefreshItems();
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

        questAddButton = rootVisualElement.Q<Button>("questAddButton");
        questAddButton.RegisterCallback<ClickEvent>(e =>
        {
            questCounter += 1;
            QuestModel newQuest = new QuestModel(database, questCounter);
            newQuest.name = $"Placeholder ID {questCounter}";
            quests.Add(newQuest);
            questView.selectedIndex = questView.itemsSource.Count - 1;
            questView.RefreshItems();
        });

        questView.itemsSource = quests;
        questView.selectedIndicesChanged += OnQuestSelectionChange;
        questView.columnSortingChanged += () => SortQuestColumns();
        questView.RefreshItems();
    }

    private void SortQuestColumns()
    {
        List<QuestModel> questList = (List<QuestModel>)questView.itemsSource;

        foreach (var column in questView.sortedColumns)
        {
            switch (column.columnName)
            {
                case "quest":
                    if (column.direction == SortDirection.Ascending)
                    {
                        questList = questList.OrderBy(v => v.name).ToList();
                    }
                    else
                    {
                        questList = questList.OrderByDescending(v => v.name).ToList();
                    }

                    break;
                default:
                    break;
            }
        }

        questView.itemsSource = questList;
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
                    dropdown.choices.Add("Add new dialogue");
                    dropdown.RegisterValueChangedCallback(e =>
                    {
                        if (e.newValue == "Add new dialogue")
                        {
                            int max = 0;

                            if (selectedQuest.dialogues.Count > 0)
                            {
                                max = selectedQuest.dialogues.Max(v => v.conversationID);
                            }

                            QuestDialogueModel dialogue = new QuestDialogueModel(database,
                                1, max + 1, selectedQuest.ID);
                            dialogue.text = "";
                            dialogue.npcID = 1;
                            selectedQuest.dialogues.Add(dialogue);

                            dropdown.value = (max + 1).ToString();
                            condition.param1 = (max + 1).ToString();
                            conversationID = max + 1;
                        }
                        else
                        {
                            condition.param1 = e.newValue;
                            conversationID = int.Parse(e.newValue);
                        }

                        listView.RefreshItems();
                        dialogueView.RefreshItems();
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
                    dialogueView = CreateDialogueView();
                    item.Add(dialogueView);

                    Button button = new Button();
                    button.text = "Add new line";
                    button.RegisterCallback<ClickEvent>(e =>
                    {
                        int max = selectedQuest.dialogues
                            .Where(v => v.conversationID == conversationID)
                            .Max(v => v.ID);
                        QuestDialogueModel dialogue = new QuestDialogueModel(database, max + 1,
                            conversationID, selectedQuest.ID);
                        dialogue.text = "";
                        dialogue.npcID = 1;

                        selectedQuest.dialogues.Add(dialogue);
                        dialogueView.itemsSource = selectedQuest.dialogues
                            .Where(v => v.conversationID == conversationID)
                            .ToList();
                        dialogueView.RefreshItems();
                    });

                    item.Add(button);
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
        Column moveColumn = new Column { name = "move", stretchable = true };

        newView.columns.Add(stepColumn);
        newView.columns.Add(npcColumn);
        newView.columns.Add(textColumn);
        newView.columns.Add(moveColumn);

        newView.columns["step"].makeCell = () =>
        {
            Label label = new Label();
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            return label;
        };
        newView.columns["step"].bindCell = (item, index) =>
        {
            QuestDialogueModel dialogue = (QuestDialogueModel)newView.itemsSource[index];
            (item as Label).text = dialogue.ID.ToString();
        };

        newView.columns["npc"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.choices = npcs.Select(v => v.name).ToList();
            dropdown.RegisterValueChangedCallback(e =>
            {
                int index = (int)dropdown.userData;
                QuestDialogueModel dialogue = (QuestDialogueModel)newView.itemsSource[index];
                dialogue.npcID = npcs
                    .Where(v => v.name == e.newValue)
                    .Select(v => v.ID)
                    .First();
            });

            return dropdown;
        };
        newView.columns["npc"].bindCell = (item, index) =>
        {
            QuestDialogueModel dialogue = (QuestDialogueModel)newView.itemsSource[index];
            (item as DropdownField).SetValueWithoutNotify(npcs
                .Where(v => v.ID == dialogue.npcID)
                .Select(v => v.name)
                .First());
            (item as DropdownField).userData = index;
        };

        newView.columns["text"].makeCell = () =>
        {
            TextField textField = new TextField();
            textField.multiline = true;
            textField.style.flexWrap = Wrap.Wrap;
            textField.style.whiteSpace = WhiteSpace.Normal;
            textField.RegisterValueChangedCallback(e =>
            {
                int index = (int)textField.userData;
                QuestDialogueModel dialogue = (QuestDialogueModel)newView.itemsSource[index];
                dialogue.text = e.newValue;
            });

            return textField;
        };
        newView.columns["text"].bindCell = (item, index) =>
        {
            QuestDialogueModel dialogue = (QuestDialogueModel)newView.itemsSource[index];
            (item as TextField).SetValueWithoutNotify(dialogue.text);
            (item as TextField).userData = index;
        };

        newView.columns["move"].makeCell = () =>
        {
            VisualElement element = new VisualElement();

            Button moveUpButton = new Button();
            moveUpButton.text = "↑";
            moveUpButton.clicked += () =>
            {
                int index = (int)element.userData;
                List<QuestDialogueModel> dialogues = (List<QuestDialogueModel>)newView.itemsSource;

                if (index >= 1)
                {
                    (newView.itemsSource[index] as QuestDialogueModel).ID--;
                    (newView.itemsSource[index - 1] as QuestDialogueModel).ID++;

                    dialogues = dialogues.OrderBy(v => v.ID).ToList();
                    newView.itemsSource = dialogues;
                    newView.RefreshItems();
                }
            };

            Button moveDownButton = new Button();
            moveDownButton.text = "↓";
            moveDownButton.clicked += () =>
            {
                int index = (int)element.userData;
                List<QuestDialogueModel> dialogues = (List<QuestDialogueModel>)newView.itemsSource;

                if (index <= dialogues.Count - 2)
                {
                    (newView.itemsSource[index] as QuestDialogueModel).ID++;
                    (newView.itemsSource[index + 1] as QuestDialogueModel).ID--;

                    dialogues = dialogues.OrderBy(v => v.ID).ToList();
                    newView.itemsSource = dialogues;
                    newView.RefreshItems();
                    stepView.RefreshItems();
                }
            };

            Button deleteButton = new Button();
            deleteButton.text = "X";
            deleteButton.clicked += () =>
            {
                int index = (int)element.userData;
                QuestDialogueModel dialogue = (QuestDialogueModel)newView.itemsSource[index];
                selectedQuest.dialogues.Remove(dialogue);

                foreach (QuestDialogueModel line in selectedQuest.dialogues)
                {
                    if (line.conversationID == dialogue.conversationID && line.ID > dialogue.ID)
                    {
                        line.ID--;
                    }
                }

                newView.itemsSource = selectedQuest.dialogues
                    .Where(v => v.conversationID == conversationID)
                    .ToList();
                newView.RefreshItems();
                stepView.RefreshItems();
            };

            element.Add(moveUpButton);
            element.Add(moveDownButton);
            element.Add(deleteButton);
            element.style.flexDirection = FlexDirection.Row;

            return element;
        };
        newView.columns["move"].bindCell = (item, index) =>
        {
            item.userData = index;
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

        prerequisiteView.columns["delete"].makeCell = () =>
        {
            Button button = new Button();
            button.text = "X";
            button.clicked += () =>
            {
                int index = (int)button.userData;
                QuestConditionModel condition = (QuestConditionModel)prerequisiteView.itemsSource[index];
                selectedQuest.prerequisites.Remove(condition);
                prerequisiteView.RefreshItems();
            };

            return button;
        };
        prerequisiteView.columns["delete"].bindCell = (item, index) =>
        {
            (item as Button).userData = index;
        };

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

        stepView.columns["step"].makeCell = () =>
        {
            Label label = new Label();
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            return label;
        };
        stepView.columns["step"].bindCell = (item, index) =>
        {
            QuestConditionModel step = (QuestConditionModel)stepView.itemsSource[index];
            (item as Label).text = step.stepID.ToString();
        };

        stepView.columns["move"].makeCell = () =>
        {
            VisualElement element = new VisualElement();

            Button moveUpButton = new Button();
            moveUpButton.text = "↑";
            moveUpButton.clicked += () =>
            {
                int index = (int)element.userData;
                List<QuestConditionModel> conditions = (List<QuestConditionModel>)stepView.itemsSource;

                if (index >= 1)
                {
                    (stepView.itemsSource[index] as QuestConditionModel).stepID--;
                    (stepView.itemsSource[index - 1] as QuestConditionModel).stepID++;

                    conditions = conditions.OrderBy(v => v.stepID).ToList();
                    stepView.itemsSource = conditions;
                    stepView.RefreshItems();
                }
            };

            Button moveDownButton = new Button();
            moveDownButton.text = "↓";
            moveDownButton.clicked += () =>
            {
                int index = (int)element.userData;
                List<QuestConditionModel> conditions = (List<QuestConditionModel>)stepView.itemsSource;

                if (index <= conditions.Count - 2)
                {
                    (stepView.itemsSource[index] as QuestConditionModel).stepID++;
                    (stepView.itemsSource[index + 1] as QuestConditionModel).stepID--;

                    conditions = conditions.OrderBy(v => v.stepID).ToList();
                    stepView.itemsSource = conditions;
                    stepView.RefreshItems();
                }
            };

            Button deleteButton = new Button();
            deleteButton.text = "X";
            deleteButton.clicked += () =>
            {
                int index = (int)element.userData;
                QuestConditionModel condition = (QuestConditionModel)stepView.itemsSource[index];
                selectedQuest.steps.Remove(condition);

                foreach (QuestConditionModel con in selectedQuest.steps)
                {
                    if (con.stepID > condition.stepID)
                    {
                        con.stepID--;
                    }
                }

                stepView.RefreshItems();
            };

            element.Add(moveUpButton);
            element.Add(moveDownButton);
            element.Add(deleteButton);
            element.style.flexDirection = FlexDirection.Row;

            return element;
        };
        stepView.columns["move"].bindCell = (item, index) =>
        {
            item.userData = index;
        };

        CreateQuestInfoView(stepView);

        stepAddButton.clicked += () =>
        {
            QuestConditionModel condition = new QuestConditionModel(database,
                selectedQuest.ID, QuestModel.stepsTableName);

            int max = 0;

            if (stepView.itemsSource.Count > 0)
            {
                max = (stepView.itemsSource as List<QuestConditionModel>).Max(v => v.stepID);
            }

            condition.stepID = max + 1;

            stepView.itemsSource.Add(condition);
            stepView.RefreshItems();
        };
    }

    private void CreateRewardView()
    {
        rewardView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

        rewardView.columns["delete"].makeCell = () =>
        {
            Button button = new Button();
            button.text = "X";
            button.clicked += () =>
            {
                int index = (int)button.userData;
                QuestConditionModel condition = (QuestConditionModel)rewardView.itemsSource[index];
                selectedQuest.rewards.Remove(condition);
                rewardView.RefreshItems();
            };

            return button;
        };
        rewardView.columns["delete"].bindCell = (item, index) =>
        {
            (item as Button).userData = index;
        };
        
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

    private void SaveQuests()
    {
        database.Write(@"DELETE FROM quest; DELETE FROM quest_prerequisite;
            DELETE FROM quest_step; DELETE FROM quest_reward; DELETE FROM quest_dialogue;", 
            new Dictionary<string, ModelFieldReference>());

        foreach (QuestModel quest in quests)
        {
            quest.Upsert();

            foreach (QuestConditionModel condition in quest.prerequisites)
            {
                condition.Upsert();
            }

            foreach (QuestConditionModel condition in quest.steps)
            {
                condition.Upsert();
            }

            foreach (QuestConditionModel condition in quest.rewards)
            {
                condition.Upsert();
            }

            foreach (QuestDialogueModel dialogue in quest.dialogues)
            {
                dialogue.Upsert();
            }
        }
    }
}
