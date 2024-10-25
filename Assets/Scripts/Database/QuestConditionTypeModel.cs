using System.Collections.Generic;

public class QuestConditionTypeModel : TypeModel<QuestConditionTypeModel>
{
    private int categoryID;
    public static Dictionary<int, int> category = new Dictionary<int, int>();

    public QuestConditionTypeModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "quest_condition_type";

        fieldMap.Add("category_id", new ModelFieldReference(this, nameof(categoryID)));

        CreateReadQuery();
        ReadRow();

        if (!category.ContainsKey(ID))
        {
            category.Add(ID, categoryID);
        }
    }

    public static int GetCategory(int ID)
    {
        if (category.ContainsKey(ID))
        {
            return category[ID];
        }

        return -1;
    }
}