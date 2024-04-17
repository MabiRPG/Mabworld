using System.Data;
using UnityEngine;
using Mono.Data.Sqlite;

// public class GameManager : MonoBehaviour 
// {
//     public string databaseName;
    
//     private void Start() 
//     {
//         // Debug.Log(GetSkillInfo(1));
//     }

//     // private DataTable GetSkillInfo(int id) 
//     // {
//     //     string dbUri = string.Format("URI=file:Assets/Database/{0}", databaseName);
//     //     DataTable dt = new DataTable();
        
//     //     IDbConnection dbConnection = new SqliteConnection(dbUri);
//     //     dbConnection.Open();

//     //     IDbCommand dbCommand = dbConnection.CreateCommand();
//     //     dbCommand.CommandText = "SELECT * FROM skills WHERE skill_id = 2";
        
//     //     // var parameter = dbCommand.CreateParameter();
//     //     // parameter.ParameterName = "@id";
//     //     // parameter.Value = id;

//     //     // dbCommand.Parameters.Add(parameter);

//     //     IDataReader reader = dbCommand.ExecuteReader();
//     //     dt.Load(reader);

//     //     reader.Close();

//     //     return dt;
//     // }
// }