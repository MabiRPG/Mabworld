using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Handles all mob (non-NPC and non-Player) processing.
/// </summary>
public class Mob : Actor
{
    public Vector2 origin;

    // Traversal radius is the maximum range in world units that 
    // a mob can wander around its origin.
    public float traversalRadius = 3;
    // Minimum and maximum idle times after a movement is complete to delay movement again.
    public float minimumIdleTime = 3;
    public float maximumIdleTime = 15;

    public MobController controller;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        origin = transform.position;

        controller = gameObject.AddComponent<MobController>();
        controller.Init(this);
    }
}