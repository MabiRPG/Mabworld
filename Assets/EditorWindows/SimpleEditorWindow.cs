using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleEditorWindow : EditorWindow
{
    private Button refreshButton;
    private Button commitButton;

    private MultiColumnListView leftPane;

    private int index;
    private SkillModel selectedSkill;
    // private Button selectedIcon;
    private TextField selectedName;
    private ObjectField selectedIcon;
    private ObjectField selectedSFX;
    private DropdownField selectedCategory;
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

    private MultiColumnListView statView;
    private Button statAddButton;
    private MultiColumnListView trainingView;
    private Button methodAddButton;

    private string firstAvailableRank;
    private string lastAvailableRank;

    private DatabaseManager database;
    private List<SkillModel> skills;
    private List<int> usedStatIDs;
    private List<int> usedTrainingMethodIDs;

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
        database = new DatabaseManager("mabinogi.db");

        DataTable dt = database.Read("SELECT id FROM skill;");
        skills = new List<SkillModel>();

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            SkillModel skill = new SkillModel(database, ID);
            skills.Add(skill);
        }

        dt = database.Read("SELECT id FROM skill_category_type;");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            new SkillTypeModel(database, ID);
        }
        
        dt = database.Read("SELECT id FROM skill_stat_type;");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            new SkillStatTypeModel(database, ID);
        }

        dt = database.Read("SELECT id FROM training_method_type;");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            new TrainingMethodTypeModel(database, ID);
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
            SetSelectedSkill(skills[index]);
            leftPane.RefreshItems();
        });

        commitButton = rootVisualElement.Q<Button>("commitButton");
        commitButton.RegisterCallback<ClickEvent>(e =>
        {
            SaveSkills();
        });

        leftPane = rootVisualElement.Q<MultiColumnListView>();
        leftPane.itemsSource = skills;

        // leftPane.columns.Add(new Column { name = "icon", title = "Icon", width = 40 });
        leftPane.columns["icon"].makeCell = () => new Image();
        leftPane.columns["icon"].bindCell = 
            (item, index) => { (item as Image).sprite = skills[index].icon; };

        // leftPane.columns.Add(new Column { name = "name", title = "Name", width = 150 });
        leftPane.columns["name"].makeCell = () => new Label();
        leftPane.columns["name"].bindCell = 
            (item, index) => { (item as Label).text = skills[index].name; };
        
        leftPane.selectedIndicesChanged += OnSkillSelectionChange;

        // selectedIcon = rootVisualElement.Q<Button>("selectedIcon");
        selectedName = rootVisualElement.Q<TextField>("selectedName");
        selectedName.RegisterValueChangedCallback(e => 
        {
            selectedSkill.name = e.newValue;
            leftPane.RefreshItems();
        });
        selectedIcon = rootVisualElement.Q<ObjectField>("selectedIcon");
        selectedIcon.RegisterValueChangedCallback(e =>
        {
            selectedSkill.icon = (Sprite)e.newValue;
        });
        selectedSFX = rootVisualElement.Q<ObjectField>("selectedSFX");
        selectedSFX.RegisterValueChangedCallback(e =>
        {
            selectedSkill.sfx = (AudioClip)e.newValue;
        });
        selectedCategory = rootVisualElement.Q<DropdownField>("selectedCategory");
        selectedCategory.RegisterValueChangedCallback(e =>
        {
            selectedSkill.categoryID = ConvertSkillCategoryNameToID(e.newValue);
        });
        selectedDescription = rootVisualElement.Q<TextField>("selectedDescription");
        selectedDescription.RegisterValueChangedCallback(e => 
        {
            selectedSkill.description = e.newValue;
        });
        selectedDetails = rootVisualElement.Q<TextField>("selectedDetails");
        selectedDetails.RegisterValueChangedCallback(e =>
        {
            selectedSkill.details = e.newValue;
        });
        selectedFirstRank = rootVisualElement.Q<DropdownField>("selectedFirstRank");
        selectedFirstRank.RegisterValueChangedCallback(e =>
        {
            selectedSkill.firstAvailableRank = e.newValue;
        });
        selectedStartRank = rootVisualElement.Q<DropdownField>("selectedStartRank");
        selectedStartRank.RegisterValueChangedCallback(e =>
        {
            selectedSkill.startingRank = e.newValue;
        });
        selectedLastRank = rootVisualElement.Q<DropdownField>("selectedLastRank");
        selectedLastRank.RegisterValueChangedCallback(e =>
        {
            selectedSkill.lastAvailableRank = e.newValue;
        });
        selectedBaseLoadTime = rootVisualElement.Q<FloatField>("selectedBaseLoadTime");
        selectedBaseLoadTime.RegisterValueChangedCallback(e =>
        {
            selectedSkill.baseLoadTime = e.newValue;
        });
        selectedBaseUseTime = rootVisualElement.Q<FloatField>("selectedBaseUseTime");
        selectedBaseUseTime.RegisterValueChangedCallback(e =>
        {
            selectedSkill.baseUseTime = e.newValue;
        });
        selectedBaseCooldownTime = rootVisualElement.Q<FloatField>("selectedBaseCooldownTime");
        selectedBaseCooldownTime.RegisterValueChangedCallback(e =>
        {
            selectedSkill.baseCooldown = e.newValue;
        });
        selectedIsStartingWith = rootVisualElement.Q<Toggle>("selectedIsStartingWith");
        selectedIsStartingWith.RegisterValueChangedCallback(e =>
        {
            selectedSkill.isStartingWith = e.newValue;
        });
        selectedIsLearnable = rootVisualElement.Q<Toggle>("selectedIsLearnable");
        selectedIsLearnable.RegisterValueChangedCallback(e =>
        {
            selectedSkill.isLearnable = e.newValue;
        });
        selectedIsPassive = rootVisualElement.Q<Toggle>("selectedIsPassive");
        selectedIsPassive.RegisterValueChangedCallback(e =>
        {
            selectedSkill.isPassive = e.newValue;
        });

        statView = rootVisualElement.Q<MultiColumnListView>("selectedStats");
        trainingView = rootVisualElement.Q<MultiColumnListView>("selectedTraining");

        CreateStatView();
        CreateTrainingView();
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
        statView.columns["stat"].bindCell = (item, j) => {
            List<string> names = new List<string>();
            int statID = (statView.itemsSource[j] as SkillStatModel).statID;
            string statName = SkillStatTypeModel.types[statID];
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

        for (int i = 1; i <= 15; i++)
        {
            int j = i; // For local purposes.
            string hex = j.ToString("X");

            statView.columns[hex].makeCell = () =>
            {
                FloatField floatField = new FloatField();
                floatField.RegisterValueChangedCallback(e =>
                {
                    (statView.itemsSource[(int)floatField.userData] as SkillStatModel)
                        .values[15 - j] = e.newValue;
                });

                return floatField;
            };
            statView.columns[hex].bindCell = (item, k) => {
                statView.columns[hex].visible = true;
                (item as FloatField).SetValueWithoutNotify( 
                    (statView.itemsSource[k] as SkillStatModel).values[15 - j]);
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
        usedStatIDs.Remove((statView.itemsSource[index] as SkillStatModel).statID);

        foreach ((int ID, string name) in SkillStatTypeModel.types)
        {
            if (name == newType)
            {
                (statView.itemsSource[index] as SkillStatModel).statID = ID;
                usedStatIDs.Add(ID);
                break;
            }
        }

        statView.RefreshItems();
    }

    private void RemoveStatAt(MultiColumnListView statView, int index)
    {
        usedStatIDs.Remove((statView.itemsSource[index] as SkillStatModel).statID);
        statView.itemsSource.RemoveAt(index);
        statView.Rebuild();
    }

    private void CreateTrainingView()
    {
        trainingView.columnSortingChanged += () => SortMethodColumns();

        trainingView.columns["name"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.RegisterValueChangedCallback(e =>
            {
                foreach ((int ID, string name) in TrainingMethodTypeModel.types)
                {
                    if (name == e.newValue)
                    {
                        (trainingView.itemsSource[(int)dropdown.userData] as TrainingMethodModel)
                            .trainingMethodID = ID;
                        break;
                    }
                }
            });

            return dropdown;
        };
        trainingView.columns["name"].bindCell = (item, j) =>
        {
            List<string> names = new List<string>(TrainingMethodTypeModel.types.Values);
            int ID = (trainingView.itemsSource[j] as TrainingMethodModel).trainingMethodID;
            string name = TrainingMethodTypeModel.types[ID];
            (item as DropdownField).SetValueWithoutNotify(name);
            (item as DropdownField).choices = names;
            (item as DropdownField).userData = j;
        };

        trainingView.columns["rank"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.RegisterValueChangedCallback(e =>
            {
                (trainingView.itemsSource[(int)dropdown.userData] as TrainingMethodModel)
                    .rank = e.newValue;
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

        trainingView.columns["xpGainEach"].makeCell = () =>
        {
            FloatField floatField = new FloatField();
            floatField.RegisterValueChangedCallback(e =>
            {
                (trainingView.itemsSource[(int)floatField.userData] as TrainingMethodModel).xpGainEach = 
                    e.newValue;
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
                (trainingView.itemsSource[(int)integerField.userData] as TrainingMethodModel).countMax =
                    e.newValue;
            });

            return integerField;
        };
        trainingView.columns["countMax"].bindCell = (item, j) =>
        {
            (item as IntegerField).value = 
                (trainingView.itemsSource[j] as TrainingMethodModel).countMax;
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
            button.RegisterCallback<ClickEvent>(e => RemoveMethodAt(trainingView, button));
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

    private void RemoveMethodAt(MultiColumnListView trainingView, Button button)
    {
        trainingView.itemsSource.RemoveAt((int)button.userData);
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
        // selectedIcon.style.backgroundImage = new StyleBackground(selectedSkill.icon);
        //selectedIcon.clicked += ChangeIcon;
        selectedName.value = selectedSkill.name;
        
        List<string> names = new List<string>();

        foreach ((int ID, string name) in SkillTypeModel.types)
        {
            if (ID == selectedSkill.categoryID)
            {
                selectedCategory.value = name;
            }

            names.Add(name);
        }

        selectedCategory.choices = names;
        selectedIcon.value = selectedSkill.icon;
        selectedSFX.value = selectedSkill.sfx;
        selectedDescription.value = selectedSkill.description;
        selectedDetails.value = selectedSkill.details;

        selectedFirstRank.value = selectedSkill.firstAvailableRank;
        firstAvailableRank = selectedSkill.firstAvailableRank;
        selectedFirstRank.choices = SkillModel.ranks;

        selectedStartRank.value = selectedSkill.startingRank;
        selectedStartRank.choices = SkillModel.ranks;  

        selectedLastRank.value = selectedSkill.lastAvailableRank;
        lastAvailableRank = selectedSkill.lastAvailableRank;
        selectedLastRank.choices = SkillModel.ranks;

        selectedBaseLoadTime.value = selectedSkill.baseLoadTime;
        selectedBaseUseTime.value = selectedSkill.baseUseTime;
        selectedBaseCooldownTime.value = selectedSkill.baseCooldown;

        selectedIsStartingWith.value = selectedSkill.isStartingWith;
        selectedIsLearnable.value = selectedSkill.isLearnable;
        selectedIsPassive.value = selectedSkill.isPassive;

        statView.itemsSource = selectedSkill.stats.Values.ToList();
        statView.RefreshItems();

        trainingView.itemsSource = selectedSkill.trainingMethods.Values.ToList();
        trainingView.RefreshItems();
    }

    private string AddToAddressables(UnityEngine.Object asset)
    {
        if (asset == default)
        {
            return "";
        }

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        string assetPath = AssetDatabase.GetAssetPath(asset);
        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
        settings.CreateAssetReference(assetGUID);

        return settings.FindAssetEntry(assetGUID).address;
    }

    private int ConvertSkillCategoryNameToID(string name)
    {
        foreach ((int ID, string categoryName) in SkillTypeModel.types)
        {
            if (categoryName == name)
            {
                return ID;
            }
        }

        // Default to Foundation if cannot be found.
        return 1;
    }

    private int SaveSkills()
    {
        int rowsChanged = 0;

        foreach (SkillModel skill in skills)
        {
            string iconAddress = AddToAddressables(skill.icon);
            string sfxAddress = AddToAddressables(skill.sfx);

            string commit = @"INSERT INTO skill
                (
                    id, 
                    name, 
                    category_id,
                    description,
                    details,
                    icon,
                    sfx,
                    starting_rank,
                    first_available_rank,
                    last_available_rank,
                    base_load_time,
                    base_use_time,
                    base_cooldown,
                    is_starting_with,
                    is_learnable,
                    is_passive
                ) 
                Values
                (
                    @ID, 
                    @name, 
                    @categoryID,
                    @description,
                    @details,
                    @icon,
                    @sfx,
                    @startingRank,
                    @firstAvailableRank,
                    @lastAvailableRank,
                    @baseLoadTime,
                    @baseUseTime,
                    @baseCooldown,
                    @isStartingWith,
                    @isLearnable,
                    @isPassive
                )
                ON CONFLICT(id) DO UPDATE 
                SET 
                    name=@name,
                    category_id=@categoryID,
                    description=@description,
                    details=@details,
                    icon=@icon,
                    sfx=@sfx,
                    starting_rank=@startingRank,
                    first_available_rank=@firstAvailableRank,
                    last_available_rank=@lastAvailableRank,
                    base_load_time=@baseLoadTime,
                    base_use_time=@baseUseTime,
                    base_cooldown=@baseCooldown,
                    is_starting_with=@isStartingWith,
                    is_learnable=@isLearnable,
                    is_passive=@isPassive
                ;";

            rowsChanged += database.Write(commit, 
                ("@ID", skill.ID), 
                ("@name", skill.name),
                ("@categoryID", skill.categoryID),
                ("@description", skill.description),
                ("@details", skill.details),
                ("@icon", iconAddress),
                ("@sfx", sfxAddress),
                ("@firstAvailableRank", skill.firstAvailableRank),
                ("@startingRank", skill.startingRank),
                ("@lastAvailableRank", skill.lastAvailableRank),
                ("@baseLoadTime", skill.baseLoadTime),
                ("@baseUseTime", skill.baseUseTime),
                ("@baseCooldown", skill.baseCooldown),
                ("@isStartingWith", Convert.ToInt32(skill.isStartingWith)),
                ("@isLearnable", Convert.ToInt32(skill.isLearnable)),
                ("@isPassive", Convert.ToInt32(skill.isPassive))
            );
        }

        return rowsChanged;
    }
}
