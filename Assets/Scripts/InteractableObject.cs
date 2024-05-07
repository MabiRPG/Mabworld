using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public int eventID;
    public Dictionary<string, object> info = new Dictionary<string, object>();

    private const string eventQuery = @"SELECT * FROM map_resources WHERE event_id = @id LIMIT 1;";

    private void Start()
    {
        DataTable dt = GameManager.Instance.QueryDatabase(eventQuery, ("@id", eventID));

        foreach (DataRow row in dt.Rows)
        {
            foreach (DataColumn column in dt.Columns)
            {
                info.Add(column.ColumnName, row[column]);
            }
        }

        dt.Clear();
    }
}
