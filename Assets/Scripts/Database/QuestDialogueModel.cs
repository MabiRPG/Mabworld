public class QuestDialogueModel : Model
{
    public int ID;
    public int conversationID;
    public int questID;
    public int npcID;
    public string text;

    public QuestDialogueModel(DatabaseManager database, int ID, int conversationID, 
        int questID) : base(database)
    {
        this.ID = ID;
        this.conversationID = conversationID;
        this.questID = questID;
        tableName = "quest_dialogue";

        primaryKeys.Add("id");
        primaryKeys.Add("conversation_id");
        primaryKeys.Add("quest_id");

        fieldMap.Add("id", new ModelFieldReference(this, nameof(ID)));
        fieldMap.Add("conversation_id", new ModelFieldReference(this, nameof(conversationID)));
        fieldMap.Add("quest_id", new ModelFieldReference(this, nameof(questID)));
        fieldMap.Add("npc_id", new ModelFieldReference(this, nameof(npcID)));
        fieldMap.Add("text", new ModelFieldReference(this, nameof(text)));

        CreateReadQuery();
        CreateWriteQuery();

        ReadRow();
    }
}