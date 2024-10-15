using System;
using System.Collections.Generic;
using System.Data;
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
    }

    private void SaveItems()
    {
        database.Write("DELETE FROM item;", new Dictionary<string, ModelFieldReference>());

        foreach (ItemModel item in items)
        {
            item.Upsert();
        }
    }
}
