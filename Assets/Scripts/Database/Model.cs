using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using UnityEngine;

public abstract class BaseModel
{
    protected DatabaseManager database;

    protected List<string> primaryKeys = new List<string>();
    protected Dictionary<string, ModelFieldReference> fieldMap = 
        new Dictionary<string, ModelFieldReference>();

    protected string tableName;
    protected string readString;
    protected string writeString;

    public BaseModel(DatabaseManager database)
    {
        this.database = database;
    }

    protected virtual DataRow ReadRow()
    {
        if (readString == default)
        {
            return null;
        }

        DataTable table = database.ReadTable(readString, fieldMap);

        if (table.Rows.Count == 0)
        {
            return null;
        }

        DataRow row = table.Rows[0];
        database.ParseRow(row, fieldMap);

        return row;
    }

    public virtual bool Upsert()
    {
        if (writeString == default)
        {
            return false;
        }

        database.Write(writeString, fieldMap);
        return true;
    }

    protected virtual void CreateReadQuery()
    {
        StringBuilder read = new StringBuilder();

        read.AppendFormat("SELECT * FROM {0} WHERE ", tableName);

        foreach (string key in primaryKeys)
        {
            read.AppendFormat("{0} = @{0} AND ", key);
        }

        read.Remove(read.Length - 4, 4);
        read.Append(";");
        readString = read.ToString();
    }

    protected virtual void CreateWriteQuery()
    {
        StringBuilder write = new StringBuilder();
        write.AppendFormat("INSERT INTO {0} (", tableName);

        foreach ((string name, ModelFieldReference _) in fieldMap)
        {
            write.AppendFormat("{0},", name);
        }

        // Removes extra comma to prevent error...
        write.Remove(write.Length - 1, 1);
        write.Append(") Values (");

        foreach ((string name, ModelFieldReference _) in fieldMap)
        {
            write.AppendFormat("@{0},", name);
        }

        // Removes extra comma to prevent error...
        write.Remove(write.Length - 1, 1);
        write.Append(") ON CONFLICT(");

        foreach (string key in primaryKeys)
        {
            write.AppendFormat("{0},", key);
        }

        // Removes extra comma to prevent error...
        write.Remove(write.Length - 1, 1);
        write.Append(") DO UPDATE SET ");

        foreach ((string name, ModelFieldReference _) in fieldMap)
        {
            write.AppendFormat("{0}=@{0},", name);
        }

        // Removes extra comma to prevent error...
        write.Remove(write.Length - 1, 1);
        write.Append(";");

        writeString = write.ToString();
    }
}

// Generic class to enable each static dictionary type to be inherited by specific
// subclass implementations.
public class TypeModel<T> : BaseModel
{
    public int ID;
    public string name;
    public static Dictionary<int, string> types = new Dictionary<int, string>();

    public TypeModel(DatabaseManager database) : base(database)
    {
        primaryKeys.Add("id");
        
        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("name", new ModelFieldReference(this, nameof(name)));
    }

    protected override DataRow ReadRow()
    {
        DataRow row = base.ReadRow();

        if (!types.ContainsKey(ID))
        {
            types.Add(ID, name);
        }

        return row;
    }
}

public class ModelFieldReference
{
    private object model;
    private int index;
    private FieldInfo field;
    private Type fieldType;

    public ModelFieldReference(object model, string fieldName)
    {
        this.model = model;
        field = model.GetType().GetField(fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        fieldType = field.FieldType;
    }

    public ModelFieldReference(object model, string fieldName, int index)
    {
        this.model = model;
        this.index = index;
        field = model.GetType().GetField(fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        fieldType = field.FieldType;
    }

    public dynamic Get()
    {
        if (fieldType == typeof(List<float>))
        {
            List<float> values = (List<float>)field.GetValue(model);
            return values[index];
        }
        else
        {
            return field.GetValue(model);
        }
    }

    public void Set(dynamic value)
    {
        if (fieldType == typeof(List<float>))
        {
            List<float> values = (List<float>)field.GetValue(model);
            values[index] = value;
        }
        else
        {
            field.SetValue(model, value);
        }
    }

    public Type Type()
    {
        return fieldType;
    }
}