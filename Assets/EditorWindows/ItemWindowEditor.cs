using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemWindowEditor : EditorWindow
{
    private Button refreshButton;
    private Button commitButton;

    private int index;
    private ItemModel selectedItem;
    private TextField selectedName;
    private DropdownField selectedCategory;
    private TextField selectedDescription;
    private ObjectField selectedIcon;
    private IntegerField selectedStackSizeLimit;
    private IntegerField selectedWidthInGrid;
    private IntegerField selectedHeightInGrid;

    private MultiColumnListView statView;
    private Button statAddButton;
    private List<int> usedStatIDs;

    private DatabaseManager database;
    private List<ItemModel> items;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/ItemEditor")]
    public static void ShowExample()
    {
        ItemWindowEditor wnd = GetWindow<ItemWindowEditor>();
        wnd.titleContent = new GUIContent("Item Editor");
    }

    private void Initialize()
    {
        database = new DatabaseManager("mabinogi.db");

        DataTable dt = database.Read("SELECT id FROM item;");
        items = new List<ItemModel>();

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            ItemModel item = new ItemModel(database, ID);
            items.Add(item);
        }

        dt = database.Read("SELECT id FROM item_category_type;");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            new ItemTypeModel(database, ID);
        }

        dt = database.Read("SELECT id from item_stat_type");

        foreach (DataRow row in dt.Rows)
        {
            int ID = int.Parse(row["id"].ToString());
            new ItemStatTypeModel(database, ID);
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
            selectedItem = items[index];
            DisplayItemInfo(index);
        });

        commitButton = rootVisualElement.Q<Button>("commitButton");
        commitButton.RegisterCallback<ClickEvent>(e => SaveItems());

        ListView itemPane = rootVisualElement.Q<ListView>("itemPane");
        itemPane.makeItem = () => new Label();
        itemPane.bindItem = (item, index) =>
        {
            (item as Label).text = items[index].name;
        };

        itemPane.itemsSource = items;
        itemPane.selectedIndicesChanged += OnItemSelectionChange;
        itemPane.RefreshItems();

        selectedName = rootVisualElement.Q<TextField>("selectedName");
        selectedName.RegisterValueChangedCallback(e =>
        {
            selectedItem.name = e.newValue;
            itemPane.RefreshItems();
        });
        selectedCategory = rootVisualElement.Q<DropdownField>("selectedCategory");
        selectedCategory.RegisterValueChangedCallback(e =>
        {
            selectedItem.categoryID = ItemTypeModel.FindByName(e.newValue);
        });
        selectedDescription = rootVisualElement.Q<TextField>("selectedDescription");
        selectedDescription.RegisterValueChangedCallback(e =>
        {
            selectedItem.description = e.newValue;
        });
        selectedIcon = rootVisualElement.Q<ObjectField>("selectedIcon");
        selectedIcon.RegisterValueChangedCallback(e =>
        {
            selectedItem.icon = (Sprite)e.newValue;
        });
        selectedStackSizeLimit = rootVisualElement.Q<IntegerField>("selectedStackSizeLimit");
        selectedStackSizeLimit.RegisterValueChangedCallback(e =>
        {
            selectedItem.stackSizeLimit = e.newValue;
        });
        selectedWidthInGrid = rootVisualElement.Q<IntegerField>("selectedWidthInGrid");
        selectedWidthInGrid.RegisterValueChangedCallback(e =>
        {
            selectedItem.widthInGrid = e.newValue;
        });
        selectedHeightInGrid = rootVisualElement.Q<IntegerField>("selectedHeightInGrid");
        selectedHeightInGrid.RegisterValueChangedCallback(e =>
        {
            selectedItem.heightInGrid = e.newValue;
        });

        statView = rootVisualElement.Q<MultiColumnListView>("selectedStats");
        CreateStatView();
    }

    private void CreateStatView()
    {
        statView.columns["stat"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.RegisterValueChangedCallback(e =>
                ChangeStatType(statView, (int)dropdown.userData, e.newValue));

            return dropdown;
        };
        statView.columns["stat"].bindCell = (item, index) =>
        {
            List<string> names = new List<string>();

            foreach ((int ID, string name) in ItemStatTypeModel.types)
            {
                if (!usedStatIDs.Contains(ID))
                {
                    names.Add(name);
                }
            }

            int statID = (statView.itemsSource[index] as ItemStatModel).statID;
            string statName = ItemStatTypeModel.FindByID(statID);

            (item as DropdownField).SetValueWithoutNotify(statName);
            (item as DropdownField).choices = names;
            (item as DropdownField).userData = index;
        };

        statView.columns["min"].makeCell = () =>
        {
            FloatField floatField = new FloatField();
            floatField.isDelayed = true;

            floatField.RegisterValueChangedCallback(e =>
            {
                int index = (int)floatField.userData;
                ItemStatModel stat = statView.itemsSource[index] as ItemStatModel;
                bool isRange = ItemStatTypeModel.IsRange(stat.statID);

                if (isRange)
                {
                    float value = Mathf.Min(e.newValue, stat.max);
                    stat.min = value;
                }
                else
                {
                    stat.min = e.newValue;
                    stat.max = e.newValue;    
                }

                statView.RefreshItems();
            });

            return floatField;
        };
        statView.columns["min"].bindCell = (item, index) =>
        {
            (item as FloatField).SetValueWithoutNotify(
                (statView.itemsSource[index] as ItemStatModel).min);
            (item as FloatField).userData = index;
        };

        statView.columns["max"].makeCell = () =>
        {
            FloatField floatField = new FloatField();
            floatField.isDelayed = true;

            floatField.RegisterValueChangedCallback(e =>
            {
                int index = (int)floatField.userData;
                ItemStatModel stat = statView.itemsSource[index] as ItemStatModel;
                float value = Mathf.Max(e.newValue, stat.min);
                stat.max = value;

                statView.RefreshItems();
            });

            return floatField;
        };
        statView.columns["max"].bindCell = (item, index) =>
        {
            int statID = (statView.itemsSource[index] as ItemStatModel).statID;
            bool isRange = ItemStatTypeModel.IsRange(statID);

            if (isRange)
            {
                (item as FloatField).SetValueWithoutNotify(
                    (statView.itemsSource[index] as ItemStatModel).max);
                (item as FloatField).SetEnabled(true);
            }
            else
            {
                (item as FloatField).SetValueWithoutNotify(
                    (statView.itemsSource[index] as ItemStatModel).min);
                (item as FloatField).SetEnabled(false);
            }

            (item as FloatField).userData = index;
        };

        statView.columns["delete"].makeCell = () =>
        {
            Button button = new Button();
            button.RegisterCallback<ClickEvent>(e => RemoveStatAt(statView, (int)button.userData));
            button.text = "X";
            return button;
        };
        statView.columns["delete"].bindCell = (item, index) =>
        {
            (item as Button).userData = index;
        };

        statAddButton = rootVisualElement.Q<Button>("statAddButton");
        statAddButton.clicked += () =>
        {
            statView.itemsSource.Add(new ItemStatModel(database, selectedItem.ID));
            statView.RefreshItems();
        };
    }

    private void ChangeStatType(MultiColumnListView statView, int index,
        string newStatName)
    {
        ItemStatModel oldStat = (ItemStatModel)statView.itemsSource[index];
        int oldID = oldStat.statID;

        if (selectedItem.stats.ContainsKey(oldID))
        {
            selectedItem.stats.Remove(oldID);
        }

        usedStatIDs.Remove(oldID);

        int newID = ItemStatTypeModel.FindByName(newStatName);

        if (selectedItem.stats.ContainsKey(newID))
        {
            selectedItem.stats.Add(oldID, oldStat);
            usedStatIDs.Add(oldID);
        }
        else
        {
            oldStat.statID = newID;
            selectedItem.stats.Add(newID, oldStat);
            usedStatIDs.Add(newID);
        }

        statView.RefreshItems();
    }

    private void RemoveStatAt(MultiColumnListView statView, int index)
    {
        ItemStatModel oldStat = (ItemStatModel)statView.itemsSource[index];
        int oldID = oldStat.statID;

        if (selectedItem.stats.ContainsKey(oldID))
        {
            selectedItem.stats.Remove(oldID);
        }

        usedStatIDs.Remove(oldID);

        statView.itemsSource = selectedItem.stats.Values.ToList();
        statView.RefreshItems();   
    }

    private void OnItemSelectionChange(IEnumerable<int> selectedIndex)
    {
        var enumerator = selectedIndex.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return;
        }

        index = enumerator.Current;
        DisplayItemInfo(index);
    }

    private void DisplayItemInfo(int index)
    {
        selectedItem = items[index];
        usedStatIDs = new List<int>(selectedItem.stats.Keys);

        selectedName.SetValueWithoutNotify(selectedItem.name);

        List<string> names = new List<string>(ItemTypeModel.types.Values);
        string name = ItemTypeModel.FindByID(selectedItem.categoryID);
        selectedCategory.SetValueWithoutNotify(name);
        selectedCategory.choices = names;

        selectedDescription.SetValueWithoutNotify(selectedItem.description);
        selectedIcon.SetValueWithoutNotify(selectedItem.icon);
        selectedStackSizeLimit.SetValueWithoutNotify(selectedItem.stackSizeLimit);
        selectedWidthInGrid.SetValueWithoutNotify(selectedItem.widthInGrid);
        selectedHeightInGrid.SetValueWithoutNotify(selectedItem.heightInGrid);

        statView.itemsSource = selectedItem.stats.Values.ToList();
        statView.RefreshItems();
    }

    private void SaveItems()
    {
        database.Write("DELETE FROM item; DELETE FROM item_stat;", 
            new Dictionary<string, ModelFieldReference>());

        foreach (ItemModel item in items)
        {
            item.Upsert();

            foreach (ItemStatModel stat in item.stats.Values)
            {
                stat.Upsert();
            }
        }
    }
}
