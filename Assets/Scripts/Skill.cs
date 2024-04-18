using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

public class Skill 
{
    public int id;
    public string name;
    public string description;
    public string iconName;
    public string details;
    public string startRank;
    public string endRank;
    public float baseUseTime;
    public float baseCooldown;

    public int index = 0;
    public string rank;

    private float xp;
    private float xpMax;

    private TrainingMethod[] methods;

    private string[] ranks = {"F", "E", "D", "C", "B", "A", "9", "8", "7", "6", "5", "4", "3", "2", "1"};

    public Skill(int id)
    {
        LoadSkillInfo(id);

        rank = ranks[index];
    }

    public void LoadSkillInfo(int id) 
    {
        // Creates the database uri location
        string dbUri = string.Format("URI=file:Assets/Database/{0}", GameManager.instance.databaseName);
        
        // Opens a connection
        IDbConnection dbConnection = new SqliteConnection(dbUri);
        dbConnection.Open();

        // Creates a sql query command
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = @"SELECT * FROM skills WHERE skill_id = @id";

        // Adds a parameterized field of id
        var parameter = dbCommand.CreateParameter();
        parameter.ParameterName = "@id";
        parameter.Value = id;

        dbCommand.Parameters.Add(parameter);

        // Executes the command, loads the table, and closes the reader.
        IDataReader reader = dbCommand.ExecuteReader();

        while (reader.Read()) 
        {
            id = reader.GetInt32(0);
            name = reader.GetString(1);
            description = reader.GetString(2);
            iconName = reader.GetString(3);
            details = reader.GetString(4);
            startRank = reader.GetString(5);
            endRank = reader.GetString(6);
            baseUseTime = reader.GetFloat(7);
            baseCooldown = reader.GetFloat(8);
        }

        int startIndex = Array.IndexOf(ranks, startRank);
        int endIndex = Array.IndexOf(ranks, endRank);

        ranks = ranks.Skip(startIndex).Take(endIndex - startIndex).ToArray();

        reader.Close();
    }

    public void RankUp() 
    {
        index++;
        rank = ranks[index];
    }

    public void RankDown()
    {
        index--;
        rank = ranks[index];
    }

    public void CreateTrainingMethods()
    {
    }
}

public class TrainingMethod
{
    public int id;
    public string name;
    public int count = 0;
    public float xpGainEach;
    public int countMax;

    private Dictionary<string, object> status = new Dictionary<string, object>();

    public TrainingMethod(int skill_id, string rank, int method_id)
    {
        LoadMethodInfo(skill_id, rank, method_id);
    }

    public void LoadMethodInfo(int skill_id, string rank, int method_id)
    {
        // Creates the database uri location
        string dbUri = string.Format("URI=file:Assets/Database/{0}", GameManager.instance.databaseName);
        
        // Opens a connection
        IDbConnection dbConnection = new SqliteConnection(dbUri);
        dbConnection.Open();

        // Creates a sql query command
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = @"SELECT training_methods_type.method, training_methods.xp_gain_each, training_methods.count_max 
            FROM training_methods
            JOIN training_methods_type
            ON training_methods.method_id = training_methods_type.method_id
            JOIN skills
            ON training_methods.skill_id = skills.skill_id
            WHERE training_methods.skill_id = @skill_id AND training_methods.rank = @rank AND training_methods.method_id = @method_id";

        // Adds a parameterized field of id
        var parameter = dbCommand.CreateParameter();
        parameter.ParameterName = "@skill_id";
        parameter.Value = skill_id;

        dbCommand.Parameters.Add(parameter);

        parameter = dbCommand.CreateParameter();
        parameter.ParameterName = "@rank";
        parameter.Value = rank;

        dbCommand.Parameters.Add(parameter);

        parameter = dbCommand.CreateParameter();
        parameter.ParameterName = "@method_id";
        parameter.Value = method_id;

        dbCommand.Parameters.Add(parameter);

        // Executes the command, loads the table, and closes the reader.
        IDataReader reader = dbCommand.ExecuteReader();

        while (reader.Read()) 
        {
            name = reader.GetString(0);
            xpGainEach = reader.GetFloat(1);
            countMax = reader.GetInt32(2);
        }

        id = method_id;

        reader.Close();
    }

    public void update(Dictionary<string, object> statusIncoming)
    {
        status = statusIncoming;
    }

    public bool checkTraining()
    {
        switch(id)
        {
            case 1:
                return IsSuccess();
            case 2:
                return IsFail();
            case 3:
                return GatherTwoOrMore();
        }

        return false;
    }

    public bool IsSuccess()
    {
        if ((bool)status["success"] == true)
        {
            return true;
        }
        
        return false;
    }

    public bool IsFail()
    {
        return !IsSuccess();
    }

    public bool GatherTwoOrMore()
    {
        if (IsSuccess() && (string)status["action"] == "gather" && (int)status["resourceGain"] > 1)
        {
            return true;
        }

        return false;
    }

}