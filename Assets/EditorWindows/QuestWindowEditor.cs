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

    private MultiColumnListView stepView;

    private DatabaseManager database;
    private List<QuestModel> quests;
    private List<SkillModel> skills;

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
        questView.selectedIndicesChanged += OnQuestSelectionChange;
        questView.RefreshItems();
    }

    private void CreateStepView()
    {
        stepView.columns["step"].makeCell = () => new Label();
        stepView.columns["step"].bindCell = (item, index) =>
        {
            QuestConditionModel step = (QuestConditionModel)stepView.itemsSource[index];
            (item as Label).text = step.stepID.ToString();
        };

        stepView.columns["condition"].makeCell = () =>
        {
            DropdownField dropdown = new DropdownField();
            dropdown.choices = QuestConditionTypeModel.types.Values.ToList();
            return dropdown;
        };
        stepView.columns["condition"].bindCell = (item, index) =>
        {
            QuestConditionModel condition = (QuestConditionModel)stepView.itemsSource[index];
            (item as DropdownField).SetValueWithoutNotify(
                QuestConditionTypeModel.FindByID(condition.conditionID));
        };

        stepView.columns["param1"].makeCell = () => new VisualElement();
        stepView.columns["param1"].bindCell = (item, index) =>
        {
            item.Clear();

            QuestConditionModel condition = (QuestConditionModel)stepView.itemsSource[index];
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

                    item.Add(dropdown);

                    break;
                }
                default:
                    break;
            }
        };

        stepView.columns["param2"].makeCell = () => new VisualElement();
        stepView.columns["param2"].bindCell = (item, index) =>
        {
            item.Clear();

            QuestConditionModel condition = (QuestConditionModel)stepView.itemsSource[index];
            int categoryID = QuestConditionTypeModel.GetCategory(condition.conditionID);

            switch (QuestConditionCategoryTypeModel.FindByID(categoryID))
            {
                case "Skill":
                {
                    DropdownField dropdown = new DropdownField();

                    dropdown.SetValueWithoutNotify(condition.param2);
                    dropdown.choices = SkillModel.ranks;

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

                    item.Add(dropdown);

                    break;
                }
                default:
                    break;
            }
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

        stepView.itemsSource = selectedQuest.steps;
        stepView.RefreshItems();
    }
}
