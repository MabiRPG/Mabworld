using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemWindowEditor : EditorWindow
{
    private int index;
    private ItemModel selectedItem;
    private TextField selectedName;
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
    }

    public void CreateGUI()
    {
        Initialize();

        m_VisualTreeAsset.CloneTree(rootVisualElement);

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
        selectedDescription = rootVisualElement.Q<TextField>("selectedDescription");
        selectedIcon = rootVisualElement.Q<ObjectField>("selectedIcon");
        selectedStackSizeLimit = rootVisualElement.Q<IntegerField>("selectedStackSizeLimit");
        selectedWidthInGrid = rootVisualElement.Q<IntegerField>("selectedWidthInGrid");
        selectedHeightInGrid = rootVisualElement.Q<IntegerField>("selectedHeightInGrid");
    }

    private void OnItemSelectionChange(IEnumerable<int> selectedIndex)
    {
        var enumerator = selectedIndex.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return;
        }

        index = enumerator.Current;
        selectedItem = items[index];
        selectedName.value = selectedItem.name;
        selectedDescription.value = selectedItem.description;
        selectedIcon.value = selectedItem.icon;
        selectedStackSizeLimit.value = selectedItem.stackSizeLimit;
        selectedWidthInGrid.value = selectedItem.widthInGrid;
        selectedHeightInGrid.value = selectedItem.heightInGrid;
    }
}
