using System;
using System.Data;
using UnityEngine;

/// <summary>
///     Handles the individual item processing.
/// </summary>
[Serializable]
public class Item : ItemModel
{
    public int quantity;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="ID">Item ID in database.</param>
    public Item(int ID) : base(GameManager.Instance.Database, ID)
    {
    }
}