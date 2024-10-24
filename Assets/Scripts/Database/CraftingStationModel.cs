using System.Collections.Generic;

public class CraftingStationModel : TypeModel<CraftingStationModel>
{
    public CraftingStationModel(DatabaseManager database, int ID) : base(database)
    {
        this.ID = ID;
        tableName = "crafting_station";
        CreateReadQuery();
        ReadRow();
    }
}