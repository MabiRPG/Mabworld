#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillEditorWindow : EditorWindow
{
    private Button refreshButton;
    private Button commitButton;

    private MultiColumnListView skillView;

    private int index;
    private SkillModel selectedSkill;
    // private Button selectedIcon;
    private TextField selectedName;
    private ObjectField selectedIcon;
    private ObjectField selectedSFX;
    private DropdownField selectedCultivationStage;
    private TextField selectedDescription;
    private TextField selectedDetails;
    private DropdownField selectedFirstRank;
    private DropdownField selectedStartRank;
    private DropdownField selectedLastRank;
    private FloatField selectedBaseLoadTime;
    private FloatField selectedBaseUseTime;
    private FloatField selectedBaseCooldownTime;
    private Toggle selectedIsStartingWith;
    private Toggle selectedIsLearnable;
    private Toggle selectedIsPassive;

    private TextField nameSearch;
    private MultiColumnListView statView;
    private Button statAddButton;
    private MultiColumnListView trainingView;
    private Button methodAddButton;

    private string firstAvailableRank;
    private string lastAvailableRank;

    private DatabaseManager database;
    private List<SkillModel> skills;
    private int skillCounter;
    private List<int> usedStatIDs;
    private List<int> usedTrainingMethodIDs;

    private List<MapResourceModel> mapResources;
    private List<ItemModel> items;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("MabWorld/Skill Editor")]
    public static void ShowExample()
    {
        SkillEditorWindow wnd = GetWindow<SkillEditorWindow>();
        wnd.titleContent = new GUIContent("Skill Editor");
    }

    private void Initialize()
    {
        database = new DatabaseManager("mabinogi.db");

        DataTable dt = database.Read("SELECT id FROM skill;");
        skills = new List<SkillModel>();

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            SkillModel skill = new SkillModel(database, ID);
            skills.Add(skill);
        }

        skillCounter = skills.Max(v => v.ID);

        dt = database.Read("SELECT id FROM map_resource;");
        mapResources = new List<MapResourceModel>();

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            MapResourceModel mapResource = new MapResourceModel(database, ID);
            mapResources.Add(mapResource);
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
            skillView.itemsSource = skills;
            SetSelectedSkill(skills[index]);
            skillView.RefreshItems();
        });

        commitButton = rootVisualElement.Q<Button>("commitButton");
        commitButton.RegisterCallback<ClickEvent>(e => SaveSkills());

        nameSearch = rootVisualElement.Q<TextField>("nameSearch");
        nameSearch.RegisterValueChangedCallback(e =>
        {
            skillView.itemsSource = skills
                .Where(v => v.Name.Contains(e.newValue, StringComparison.OrdinalIgnoreCase))
                .ToList();

            skillView.RefreshItems();
        });

        skillView = rootVisualElement.Q<MultiColumnListView>();
        skillView.columnSortingChanged += () => SortSkillColumns();
        skillView.itemsSource = skills;

        skillView.columns["icon"].makeCell = () => new Image();
        skillView.columns["icon"].bindCell =
            (item, index) => { (item as Image).sprite = skills[index].Icon; };

        skillView.columns["name"].makeCell = () => new Label();
        skillView.columns["name"].bindCell =
            (item, index) => { (item as Label).text = skills[index].Name; };

        skillView.selectedIndicesChanged += OnSkillSelectionChange;

        Button skillAddButton = rootVisualElement.Q<Button>("skillAddButton");
        skillAddButton.RegisterCallback<ClickEvent>(e =>
        {
            skillCounter += 1;

            SkillModel newSkill = new SkillModel(database, skillCounter);
            newSkill.Name = $"Placeholder ID {skillCounter}";
            skills.Add(newSkill);

            skillView.RefreshItems();
        });

        selectedName = rootVisualElement.Q<TextField>("selectedName");
        selectedName.RegisterValueChangedCallback(e =>
        {
            selectedSkill.Name = e.newValue;
            skillView.RefreshItems();
        });
        selectedIcon = rootVisualElement.Q<ObjectField>("selectedIcon");
        selectedIcon.RegisterValueChangedCallback(e =>
        {
            selectedSkill.Icon = (Sprite)e.newValue;
            skillView.RefreshItems();
        });
        selectedSFX = rootVisualElement.Q<ObjectField>("selectedSFX");
        selectedSFX.RegisterValueChangedCallback(e =>
        {
            selectedSkill.Sfx = (AudioClip)e.newValue;
        });
        selectedCultivationStage = rootVisualElement.Q<DropdownField>("selectedCultivationStage");
        selectedCultivationStage.RegisterValueChangedCallback(e =>
        {
            selectedSkill.CultivationStageID = CultivationStageModel.stages
                .Where(v => v.Value.name == e.newValue)
                .Select(v => v.Value.ID)
                .First();
        });
        selectedDescription = rootVisualElement.Q<TextField>("selectedDescription");
        selectedDescription.RegisterValueChangedCallback(e =>
        {
            selectedSkill.Description = e.newValue;
        });
        selectedDetails = rootVisualElement.Q<TextField>("selectedDetails");
        selectedDetails.RegisterValueChangedCallback(e =>
        {
            selectedSkill.Details = e.newValue;
        });
        selectedFirstRank = rootVisualElement.Q<DropdownField>("selectedFirstRank");
        selectedFirstRank.RegisterValueChangedCallback(e =>
        {
            selectedSkill.FirstAvailableRank = e.newValue;
        });
        selectedStartRank = rootVisualElement.Q<DropdownField>("selectedStartRank");
        selectedStartRank.RegisterValueChangedCallback(e =>
        {
            selectedSkill.StartingRank = e.newValue;
        });
        selectedLastRank = rootVisualElement.Q<DropdownField>("selectedLastRank");
        selectedLastRank.RegisterValueChangedCallback(e =>
        {
            selectedSkill.LastAvailableRank = e.newValue;
        });
        selectedBaseLoadTime = rootVisualElement.Q<FloatField>("selectedBaseLoadTime");
        selectedBaseLoadTime.RegisterValueChangedCallback(e =>
        {
            selectedSkill.BaseLoadTime = e.newValue;
        });
        selectedBaseUseTime = rootVisualElement.Q<FloatField>("selectedBaseUseTime");
        selectedBaseUseTime.RegisterValueChangedCallback(e =>
        {
            selectedSkill.BaseUseTime = e.newValue;
        });
        selectedBaseCooldownTime = rootVisualElement.Q<FloatField>("selectedBaseCooldownTime");
        selectedBaseCooldownTime.RegisterValueChangedCallback(e =>
        {
            selectedSkill.BaseCooldown = e.newValue;
        });
        selectedIsStartingWith = rootVisualElement.Q<Toggle>("selectedIsStartingWith");
        selectedIsStartingWith.RegisterValueChangedCallback(e =>
        {
            selectedSkill.IsStartingWith = e.newValue;
        });
        selectedIsLearnable = rootVisualElement.Q<Toggle>("selectedIsLearnable");
        selectedIsLearnable.RegisterValueChangedCallback(e =>
        {
            selectedSkill.IsLearnable = e.newValue;
        });
        selectedIsPassive = rootVisualElement.Q<Toggle>("selectedIsPassive");
        selectedIsPassive.RegisterValueChangedCallback(e =>
        {
            selectedSkill.IsPassive = e.newValue;
        });

        statView = rootVisualElement.Q<MultiColumnListView>("selectedStats");
        trainingView = rootVisualElement.Q<MultiColumnListView>("selectedTraining");

        CreateStatView();
        CreateTrainingView();
    }

    private void SortSkillColumns()
    {
        foreach (var column in skillView.sortedColumns)
        {
            switch (column.columnName)
            {
                case "name":
                    if (column.direction == SortDirection.Ascending)
                    {
                        skills = skills.OrderBy(v => v.Name).ToList();
                    }
                    else
                    {
                        skills = skills.OrderByDescending(v => v.Name).ToList();
                    }

                    break;
                default:
                    break;
            }
        }

        skillView.itemsSource = skills;
        skillView.RefreshItems();
    }

    private void CreateStatView()
    {
        statView.columnSortingChanged += () => SortStatColumns();

        statView.columns["stat"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.RegisterValueChangedCallback(e =>
                ChangeStatType(statView, (int)dropdown.userData, e.newValue));

            return dropdown;
        };
        statView.columns["stat"].bindCell = (item, j) =>
        {
            List<string> names = new List<string>();

            int statID = (statView.itemsSource[j] as SkillStatModel).statID;
            string statName = SkillStatTypeModel.FindByID(statID);
            (item as DropdownField).SetValueWithoutNotify(statName);

            foreach ((int ID, string name) in SkillStatTypeModel.types)
            {
                if (!usedStatIDs.Contains(ID))
                {
                    names.Add(name);
                }
            }

            (item as DropdownField).userData = j;
            (item as DropdownField).choices = names;
        };

        for (int i = 1; i <= SkillModel.ranks.Count; i++)
        {
            int j = i; // For local purposes.
            string hex = j.ToString("X");

            statView.columns[hex].makeCell = () =>
            {
                FloatField floatField = new FloatField();
                floatField.RegisterValueChangedCallback(e =>
                {
                    (statView.itemsSource[(int)floatField.userData] as SkillStatModel)
                        .values[SkillModel.ranks.Count - j] = e.newValue;
                });

                return floatField;
            };
            statView.columns[hex].bindCell = (item, k) =>
            {
                statView.columns[hex].visible = true;
                (item as FloatField).SetValueWithoutNotify(
                    (statView.itemsSource[k] as SkillStatModel).values[SkillModel.ranks.Count - j]);
                (item as FloatField).userData = k;
            };
        }

        statView.columns["delete"].makeCell = () =>
        {
            Button button = new Button();
            // Must use a callback here to prevent duplicating click events in bind.
            button.RegisterCallback<ClickEvent>(e =>
                RemoveStatAt(statView, (int)button.userData)
            );
            return button;
        };

        statView.columns["delete"].bindCell = (item, j) =>
        {
            (item as Button).text = "X";
            (item as Button).userData = j;
        };

        statAddButton = rootVisualElement.Q<Button>("statAddButton");
        statAddButton.clicked += () =>
        {
            statView.itemsSource.Add(new SkillStatModel(database, selectedSkill.ID));
            statView.RefreshItems();
        };
    }

    private void SortStatColumns()
    {
        List<SkillStatModel> stats = (List<SkillStatModel>)statView.itemsSource;

        foreach (var column in statView.sortedColumns)
        {
            switch (column.columnName)
            {
                case "stat":
                    if (column.direction == SortDirection.Ascending)
                    {
                        stats = stats.OrderBy(v => SkillStatTypeModel.types[v.statID]).ToList();
                    }
                    else
                    {
                        stats = stats.OrderByDescending(v => SkillStatTypeModel.types[v.statID]).ToList();
                    }

                    break;
                default:
                    if (int.TryParse(column.columnName, NumberStyles.HexNumber,
                        CultureInfo.CurrentCulture, out int index))
                    {
                        index = SkillModel.ranks.Count - index;

                        if (index < 0 || index > SkillModel.ranks.Count - 1)
                        {
                            break;
                        }

                        if (column.direction == SortDirection.Ascending)
                        {
                            stats = stats.OrderBy(v => v.values[index]).ToList();
                        }
                        else
                        {
                            stats = stats.OrderByDescending(v => v.values[index]).ToList();
                        }
                    }

                    break;
            }
        }

        statView.itemsSource = stats;
        statView.RefreshItems();
    }

    private void ChangeStatType(MultiColumnListView statView, int index, string newType)
    {
        SkillStatModel oldStat = (SkillStatModel)statView.itemsSource[index];
        int oldID = oldStat.statID;

        if (selectedSkill.stats.ContainsKey(oldID))
        {
            selectedSkill.stats.Remove(oldID);
        }

        usedStatIDs.Remove(oldID);

        foreach ((int ID, string name) in SkillStatTypeModel.types)
        {
            if (name == newType)
            {
                oldStat.statID = ID;
                selectedSkill.stats.Add(ID, oldStat);
                usedStatIDs.Add(ID);
                break;
            }
        }

        statView.itemsSource = selectedSkill.stats.Values.ToList();
        statView.RefreshItems();
    }

    private void RemoveStatAt(MultiColumnListView statView, int index)
    {
        SkillStatModel oldStat = (SkillStatModel)statView.itemsSource[index];
        int oldID = oldStat.statID;

        if (selectedSkill.stats.ContainsKey(oldID))
        {
            selectedSkill.stats.Remove(oldID);
        }

        usedStatIDs.Remove(oldID);
        statView.itemsSource = selectedSkill.stats.Values.ToList();
        statView.RefreshItems();
    }

    private void CreateTrainingView()
    {
        trainingView.columnSortingChanged += () => SortMethodColumns();

        trainingView.columns["rank"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.RegisterValueChangedCallback(e =>
            {
                int index = (int)dropdown.userData;
                TrainingMethodModel method = (TrainingMethodModel)trainingView.itemsSource[index];
                // method.rank = e.newValue;
                // int ID = (trainingView.itemsSource[index] as TrainingMethodModel).trainingMethodID;
                // string name = TrainingMethodTypeModel.FindByID(ID);
                // bool success = ChangeMethodType(trainingView, index, name, e.newValue);

                // if (!success)
                // {
                //     dropdown.SetValueWithoutNotify(e.previousValue);
                // }

                ChangeMethodType(trainingView, index,
                    method.trainingMethodID, e.newValue, method.param1, method.param2);
            });

            return dropdown;
        };
        trainingView.columns["rank"].bindCell = (item, j) =>
        {
            (item as DropdownField).SetValueWithoutNotify(
                (trainingView.itemsSource[j] as TrainingMethodModel).rank);
            (item as DropdownField).choices = SkillModel.ranks;
            (item as DropdownField).userData = j;
        };

        trainingView.columns["type"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.RegisterValueChangedCallback(e =>
            {
                int index = (int)dropdown.userData;
                // string rank = (trainingView.itemsSource[index] as TrainingMethodModel).rank;
                // bool success = ChangeMethodType(trainingView, index, e.newValue, rank);

                // if (!success)
                // {
                //     dropdown.SetValueWithoutNotify(e.previousValue);
                // }
                TrainingMethodModel method = (TrainingMethodModel)trainingView.itemsSource[index];
                int trainingMethodID = TrainingMethodTypeModel.FindByName(e.newValue);

                ChangeMethodType(trainingView, index,
                    trainingMethodID, method.rank, method.param1, method.param2);
            });

            return dropdown;
        };
        trainingView.columns["type"].bindCell = (item, j) =>
        {
            List<string> types = new List<string>(TrainingMethodTypeModel.types.Values);

            int ID = (trainingView.itemsSource[j] as TrainingMethodModel).trainingMethodID;
            string type = TrainingMethodTypeModel.FindByID(ID);

            (item as DropdownField).SetValueWithoutNotify(type);
            (item as DropdownField).choices = types;
            (item as DropdownField).userData = j;
        };

        trainingView.columns["name"].makeCell = () =>
        {
            TextField text = new TextField();
            text.RegisterValueChangedCallback(e =>
            {
                int index = (int)text.userData;
                TrainingMethodModel method = (TrainingMethodModel)trainingView.itemsSource[index];
                method.name = e.newValue;
            });

            return text;
        };
        trainingView.columns["name"].bindCell = (item, index) =>
        {
            (item as TextField).userData = index;
            (item as TextField).SetValueWithoutNotify(
                (trainingView.itemsSource[index] as TrainingMethodModel).name);
        };

        trainingView.columns["param1"].makeCell = () => new VisualElement();
        trainingView.columns["param1"].bindCell = (item, index) =>
        {
            item.Clear();

            TrainingMethodModel method = (TrainingMethodModel)trainingView.itemsSource[index];


            switch (TrainingMethodTypeModel.FindByID(method.trainingMethodID))
            {
                case "Success":
                    break;
                case "Fail":
                    break;
                case "Gather":
                    {
                        DropdownField dropdown = new DropdownField();

                        dropdown.SetValueWithoutNotify(mapResources
                            .Where(v => v.ID == int.Parse(method.param1))
                            .Select(v => v.name)
                            .First());
                        dropdown.choices = mapResources
                            .Select(v => v.name)
                            .OrderBy(v => v)
                            .ToList();
                        dropdown.RegisterValueChangedCallback(e =>
                        {
                            // method.param1 = mapResources
                            //     .Where(v => v.name == e.newValue)
                            //     .Select(v => v.ID)
                            //     .First()
                            //     .ToString();
                            string value = mapResources
                                .Where(v => v.name == e.newValue)
                                .Select(v => v.ID)
                                .First()
                                .ToString();

                            ChangeMethodType(trainingView, index,
                                method.trainingMethodID, method.rank, value, method.param2);
                        });

                        item.Add(dropdown);

                        break;
                    }
                case "Fully gather":
                    {
                        DropdownField dropdown = new DropdownField();

                        dropdown.SetValueWithoutNotify(mapResources
                            .Where(v => v.ID == int.Parse(method.param1))
                            .Select(v => v.name)
                            .First());
                        dropdown.choices = mapResources
                            .Select(v => v.name)
                            .OrderBy(v => v)
                            .ToList();
                        dropdown.RegisterValueChangedCallback(e =>
                        {
                            string value = mapResources
                                .Where(v => v.name == e.newValue)
                                .Select(v => v.ID)
                                .First()
                                .ToString();

                            ChangeMethodType(trainingView, index,
                                method.trainingMethodID, method.rank, value, method.param2);
                        });

                        item.Add(dropdown);

                        break;
                    }
                case "Craft":
                    {
                        DropdownField dropdown = new DropdownField();

                        dropdown.SetValueWithoutNotify(items
                            .Where(v => v.ID == int.Parse(method.param1))
                            .Select(v => v.Name)
                            .First());
                        dropdown.choices = items
                            .Select(v => v.Name)
                            .OrderBy(v => v)
                            .ToList();
                        dropdown.RegisterValueChangedCallback(e =>
                        {
                            string value = items
                                .Where(v => v.Name == e.newValue)
                                .Select(v => v.ID)
                                .First()
                                .ToString();

                            ChangeMethodType(trainingView, index,
                                method.trainingMethodID, method.rank, value, method.param2);
                        });

                        item.Add(dropdown);

                        break;
                    }
                default:
                    break;
            }
        };

        trainingView.columns["param2"].makeCell = () => new VisualElement();
        trainingView.columns["param2"].bindCell = (item, index) =>
        {
            item.Clear();

            TrainingMethodModel method = (TrainingMethodModel)trainingView.itemsSource[index];

            switch (TrainingMethodTypeModel.FindByID(method.trainingMethodID))
            {
                case "Success":
                    break;
                case "Fail":
                    break;
                case "Gather":
                    {
                        DropdownField dropdown = new DropdownField();

                        dropdown.SetValueWithoutNotify(items
                            .Where(v => v.ID == int.Parse(method.param2))
                            .Select(v => v.Name)
                            .First());
                        dropdown.choices = items
                            .Select(v => v.Name)
                            .OrderBy(v => v)
                            .ToList();
                        dropdown.RegisterValueChangedCallback(e =>
                        {
                            string value = items
                                .Where(v => v.Name == e.newValue)
                                .Select(v => v.ID)
                                .First()
                                .ToString();

                            ChangeMethodType(trainingView, index,
                                method.trainingMethodID, method.rank, method.param1, value);
                        });

                        item.Add(dropdown);

                        break;
                    }
                case "Fully gather":
                    {
                        DropdownField dropdown = new DropdownField();

                        dropdown.SetValueWithoutNotify(items
                            .Where(v => v.ID == int.Parse(method.param2))
                            .Select(v => v.Name)
                            .First());
                        dropdown.choices = items
                            .Select(v => v.Name)
                            .OrderBy(v => v)
                            .ToList();
                        dropdown.RegisterValueChangedCallback(e =>
                        {
                            string value = items
                                .Where(v => v.Name == e.newValue)
                                .Select(v => v.ID)
                                .First()
                                .ToString();

                            ChangeMethodType(trainingView, index,
                                method.trainingMethodID, method.rank, method.param1, value);
                        });

                        item.Add(dropdown);

                        break;
                    }
                case "Craft":
                    break;
                default:
                    break;
            }
        };

        trainingView.columns["xpGainEach"].makeCell = () =>
        {
            FloatField floatField = new FloatField();
            floatField.RegisterValueChangedCallback(e =>
            {
                (trainingView.itemsSource[(int)floatField.userData] as TrainingMethodModel)
                    .xpGainEach = e.newValue;
                trainingView.RefreshItems();
            });

            return floatField;
        };
        trainingView.columns["xpGainEach"].bindCell = (item, j) =>
        {
            (item as FloatField).SetValueWithoutNotify(
                (trainingView.itemsSource[j] as TrainingMethodModel).xpGainEach);
            (item as FloatField).userData = j;
        };

        trainingView.columns["countMax"].makeCell = () =>
        {
            IntegerField integerField = new IntegerField();
            integerField.RegisterValueChangedCallback(e =>
            {
                (trainingView.itemsSource[(int)integerField.userData] as TrainingMethodModel)
                    .countMax = e.newValue;
                trainingView.RefreshItems();
            });

            return integerField;
        };
        trainingView.columns["countMax"].bindCell = (item, j) =>
        {
            (item as IntegerField).SetValueWithoutNotify(
                (trainingView.itemsSource[j] as TrainingMethodModel).countMax);
            (item as IntegerField).userData = j;
        };

        trainingView.columns["total"].makeCell = () => new Label();
        trainingView.columns["total"].bindCell = (item, j) =>
        {
            float totalXP = (trainingView.itemsSource[j] as TrainingMethodModel).xpGainEach;
            totalXP *= (trainingView.itemsSource[j] as TrainingMethodModel).countMax;
            (item as Label).text = totalXP.ToString();
        };

        trainingView.columns["delete"].makeCell = () =>
        {
            Button button = new Button();
            button.RegisterCallback<ClickEvent>(e =>
                RemoveMethodAt(trainingView, (int)button.userData));
            return button;
        };
        trainingView.columns["delete"].bindCell = (item, j) =>
        {
            (item as Button).text = "X";
            (item as Button).userData = j;
        };

        methodAddButton = rootVisualElement.Q<Button>("methodAddButton");
        methodAddButton.clicked += () =>
        {
            trainingView.itemsSource.Add(new TrainingMethodModel(database, selectedSkill.ID));
            trainingView.RefreshItems();
        };
    }

    private void SortMethodColumns()
    {
        List<TrainingMethodModel> trainingMethods =
            (List<TrainingMethodModel>)trainingView.itemsSource;

        foreach (var column in trainingView.sortedColumns)
        {
            switch (column.columnName)
            {
                case "name":
                    if (column.direction == SortDirection.Ascending)
                    {
                        trainingMethods = trainingMethods
                            .OrderBy(v => TrainingMethodTypeModel.types[v.trainingMethodID])
                            .ToList();
                    }
                    else
                    {
                        trainingMethods = trainingMethods
                            .OrderByDescending(v => TrainingMethodTypeModel.types[v.trainingMethodID])
                            .ToList();
                    }

                    break;
                case "rank":
                    if (column.direction == SortDirection.Ascending)
                    {
                        trainingMethods =
                            trainingMethods.OrderBy(v => v.rank).ToList();
                    }
                    else
                    {
                        trainingMethods =
                            trainingMethods.OrderByDescending(v => v.rank).ToList();
                    }

                    break;
                case "xpGainEach":
                    if (column.direction == SortDirection.Ascending)
                    {
                        trainingMethods =
                            trainingMethods.OrderBy(v => v.xpGainEach).ToList();
                    }
                    else
                    {
                        trainingMethods =
                            trainingMethods.OrderByDescending(v => v.xpGainEach).ToList();
                    }

                    break;
                case "countMax":
                    if (column.direction == SortDirection.Ascending)
                    {
                        trainingMethods =
                            trainingMethods.OrderBy(v => v.countMax).ToList();
                    }
                    else
                    {
                        trainingMethods =
                            trainingMethods.OrderByDescending(v => v.countMax).ToList();
                    }

                    break;
                default:
                    break;
            }
        }

        trainingView.itemsSource = trainingMethods;
        trainingView.RefreshItems();
    }

    private bool ChangeMethodType(MultiColumnListView trainingView, int index,
        int newMethodID, string newRank, string newParam1, string newParam2)
    {
        TrainingMethodModel oldMethod = (TrainingMethodModel)trainingView.itemsSource[index];
        int oldID = oldMethod.trainingMethodID;
        string oldRank = oldMethod.rank;
        string oldParam1 = oldMethod.param1;
        string oldParam2 = oldMethod.param2;

        // if (selectedSkill.trainingMethods.ContainsKey((oldID, oldRank)))
        // {
        //     selectedSkill.trainingMethods.Remove((oldID, oldRank));
        // }

        if (!selectedSkill.trainingMethods.ContainsKey((newMethodID, newRank, newParam1, newParam2)))
        {
            selectedSkill.trainingMethods.Remove((oldID, oldRank, oldParam1, oldParam2));
            selectedSkill.trainingMethods.Add((newMethodID, newRank, newParam1, newParam2), oldMethod);

            oldMethod.trainingMethodID = newMethodID;
            oldMethod.rank = newRank;
            oldMethod.param1 = newParam1;
            oldMethod.param2 = newParam2;
        }

        // foreach ((int ID, string name) in TrainingMethodTypeModel.types)
        // {
        //     if (name == newMethodName)
        //     {
        //         if (selectedSkill.trainingMethods.ContainsKey((ID, newRank)))
        //         {
        //             Debug.Log("Found duplicate...");
        //             selectedSkill.trainingMethods.Add((oldID, oldRank), oldMethod);
        //             return false;
        //         }
        //         else
        //         {
        //             oldMethod.trainingMethodID = ID;
        //             oldMethod.rank = newRank;
        //             selectedSkill.trainingMethods.Add((ID, newRank), oldMethod);
        //             break;
        //         }
        //     }
        // }

        trainingView.itemsSource = selectedSkill.trainingMethods.Values.ToList();
        trainingView.RefreshItems();
        return true;
    }

    private void RemoveMethodAt(MultiColumnListView trainingView, int index)
    {
        TrainingMethodModel oldMethod = (TrainingMethodModel)trainingView.itemsSource[index];
        int oldID = oldMethod.trainingMethodID;
        string oldRank = oldMethod.rank;
        string oldParam1 = oldMethod.param1;
        string oldParam2 = oldMethod.param2;

        if (selectedSkill.trainingMethods.ContainsKey((oldID, oldRank, oldParam1, oldParam2)))
        {
            selectedSkill.trainingMethods.Remove((oldID, oldRank, oldParam1, oldParam2));
        }

        trainingView.itemsSource = selectedSkill.trainingMethods.Values.ToList();
        trainingView.RefreshItems();
    }

    private void OnSkillSelectionChange(IEnumerable<int> selectedIndex)
    {
        var enumerator = selectedIndex.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return;
        }

        index = enumerator.Current;
        SetSelectedSkill(skills[index]);
    }

    private void SetSelectedSkill(SkillModel skill)
    {
        selectedSkill = skill;
        usedStatIDs = new List<int>(selectedSkill.stats.Keys);
        usedTrainingMethodIDs = new List<int>(selectedSkill.trainingMethods.Keys.Select(x => x.Item1));
        DisplaySkillInfo();
    }

    private void DisplaySkillInfo()
    {
        selectedName.value = selectedSkill.Name;

        selectedCultivationStage.value = CultivationStageModel.stages
            .Where(v => v.Value.ID == selectedSkill.CultivationStageID)
            .Select(v => v.Value.name)
            .First();

        selectedCultivationStage.choices = CultivationStageModel.stages
            .Select(v => v.Value.name)
            .Distinct()
            .ToList();

        selectedIcon.value = selectedSkill.Icon;
        selectedSFX.value = selectedSkill.Sfx;
        selectedDescription.value = selectedSkill.Description;
        selectedDetails.value = selectedSkill.Details;

        selectedFirstRank.value = selectedSkill.FirstAvailableRank;
        firstAvailableRank = selectedSkill.FirstAvailableRank;
        selectedFirstRank.choices = SkillModel.ranks;

        selectedStartRank.value = selectedSkill.StartingRank;
        selectedStartRank.choices = SkillModel.ranks;

        selectedLastRank.value = selectedSkill.LastAvailableRank;
        lastAvailableRank = selectedSkill.LastAvailableRank;
        selectedLastRank.choices = SkillModel.ranks;

        selectedBaseLoadTime.value = selectedSkill.BaseLoadTime;
        selectedBaseUseTime.value = selectedSkill.BaseUseTime;
        selectedBaseCooldownTime.value = selectedSkill.BaseCooldown;

        selectedIsStartingWith.value = selectedSkill.IsStartingWith;
        selectedIsLearnable.value = selectedSkill.IsLearnable;
        selectedIsPassive.value = selectedSkill.IsPassive;

        statView.itemsSource = selectedSkill.stats.Values.ToList();
        statView.RefreshItems();

        trainingView.itemsSource = selectedSkill.trainingMethods.Values.ToList();
        trainingView.RefreshItems();
    }

    // private int ConvertSkillCategoryNameToID(string name)
    // {
    //     foreach ((int ID, string categoryName) in SkillTypeModel.types)
    //     {
    //         if (categoryName == name)
    //         {
    //             return ID;
    //         }
    //     }

    //     // Default to Foundation if cannot be found.
    //     return 1;
    // }

    private void SaveSkills()
    {
        database.Write("DELETE FROM skill; DELETE FROM skill_stat; DELETE FROM training_method;",
            new Dictionary<string, ModelFieldReference>());

        foreach (SkillModel skill in skills)
        {
            skill.Upsert();

            foreach (SkillStatModel stat in skill.stats.Values)
            {
                stat.Upsert();
            }

            foreach (TrainingMethodModel trainingMethod in skill.trainingMethods.Values)
            {
                trainingMethod.Upsert();
            }
        }
    }
}

#endif