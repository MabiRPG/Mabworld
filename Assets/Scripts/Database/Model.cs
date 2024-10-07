using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

public abstract class BaseModel
{
    protected DatabaseManager database;
    protected Dictionary<string, ModelFieldReference> fieldMap = new Dictionary<string, ModelFieldReference>();
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
        DataRow row = table.Rows[0];
        database.ParseRow(row, fieldMap);

        return row;
    }

    public void Upsert()
    {
        if (writeString == default)
        {
            return;
        }
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
    private FieldInfo field;
    private Type fieldType;

    public ModelFieldReference(object model, string fieldName)
    {
        this.model = model;
        field = model.GetType().GetField(fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        fieldType = field.FieldType;
    }

    public dynamic Get()
    {
        return field.GetValue(model);
    }

    public void Set(dynamic value)
    {
        field.SetValue(model, value);
    }

    public Type Type()
    {
        return fieldType;
    }
}