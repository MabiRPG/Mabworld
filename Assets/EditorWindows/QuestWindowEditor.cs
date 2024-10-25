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

    private MultiColumnListView questView;

    private MultiColumnListView stepView;

    private DatabaseManager database;
    private List<QuestModel> quests;

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
    }

    public void CreateGUI()
    {
        Initialize();

        m_VisualTreeAsset.CloneTree(rootVisualElement);

        refreshButton = rootVisualElement.Q<Button>("refreshButton");
        commitButton = rootVisualElement.Q<Button>("commitButton");

        questView = rootVisualElement.Q<MultiColumnListView>("questView");
        CreateQuestView();

        stepView = rootVisualElement.Q<MultiColumnListView>("stepView");
        CreateStepView();
    }

    private void CreateQuestView()
    {
        questView.columns["quest"].makeCell = () => new Label();
        questView.columns["quest"].bindCell = (item, index) =>
        {
            (item as Label).text = (questView.itemsSource[index] as QuestModel).name;
        };

        questView.itemsSource = quests;
        questView.RefreshItems();
    }

    private void CreateStepView()
    {
        stepView.columns["step"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.choices = QuestConditionTypeModel.types.Values.ToList();
            return dropdown;
        };
        stepView.columns["step"].bindCell = (item, index) =>
        {
        };
    }
}
