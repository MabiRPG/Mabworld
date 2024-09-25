using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleEditorWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private const string skillQuery = @"SELECT * FROM skill;";

    [MenuItem("Window/UI Toolkit/SimpleEditorWindow")]
    public static void ShowExample()
    {
        SimpleEditorWindow wnd = GetWindow<SimpleEditorWindow>();
        wnd.titleContent = new GUIContent("SimpleEditorWindow");
    }

    public void CreateGUI()
    {
        DatabaseManager database = new DatabaseManager("mabinogi.db");
        DataTable skillTable = database.QueryDatabase(skillQuery);
        List<string> names = new List<string>();

        foreach (DataRow row in skillTable.Rows)
        {
            names.Add(row["name"].ToString());
        }

        // Create a two-pane view with the left pane being fixed.
        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

        // Add the view to the visual tree by adding it as a child to the root element.
        rootVisualElement.Add(splitView);

        // A TwoPaneSplitView needs exactly two child elements.
        var leftPane = new ListView();
        splitView.Add(leftPane);
        var rightPane = new VisualElement();
        splitView.Add(rightPane);

        // Initialize the list view with all sprites' names
        leftPane.makeItem = () => new Label();
        leftPane.bindItem = (item, index) => { (item as Label).text = names[index]; };
        leftPane.itemsSource = names;
    }
}
