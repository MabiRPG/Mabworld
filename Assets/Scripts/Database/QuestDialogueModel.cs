public class QuestDialogueModel : Model
{
    public int ID;
    public int questID;
    public int npcID;
    public string text;
    private int nextID;
    public QuestDialogueModel nextText;

    public QuestDialogueModel(DatabaseManager database, int ID, int questID) : base(database)
    {
        this.ID = ID;
        this.questID = questID;
        tableName = "quest_dialogue";

        primaryKeys.Add("id");
        primaryKeys.Add("quest_id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("quest_id", new ModelFieldReference(this, nameof(questID)));
        fieldMap.Add("npc_id", new ModelFieldReference(this, nameof(npcID)));
        fieldMap.Add("text", new ModelFieldReference(this, nameof(text)));
        fieldMap.Add("next_id", new ModelFieldReference(this, nameof(nextID)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();

        // if (nextID != -1)
        // {
        //     nextText = new QuestDialogueModel(database, nextID, questID);
        // }
        // else
        // {
        //     nextText = null;
        // }
    }
}