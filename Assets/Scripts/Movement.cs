using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private Animator animator;

    [SerializeField]
    private bool isMoving = false;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>(); 
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected void AttemptMove (float xDir, float yDir)
    {
        if (isMoving)
        {
            return;
        }

        Vector2 direction = new Vector2(xDir, yDir);
        // Must be initialized with some length or it won't detect anything!
        RaycastHit2D[] results = new RaycastHit2D[10];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(blockingLayer);
        boxCollider.Cast(direction, filter, results);

        Vector2 closestPoint = boxCollider.ClosestPoint(results[0].point);
        Vector2 target = (results[0].point - closestPoint) * (direction * direction);

        // Find the smallest distance movable by a magnitude of 1.
        if (target.sqrMagnitude > direction.sqrMagnitude)
        {
            target = direction;
        }

        StartCoroutine(Move(transform.position + (Vector3)target));
        Animate(xDir, yDir);
    }

    private IEnumerator Move (Vector3 end)
    {
        isMoving = true;
        animator.SetBool("isMoving", true);

        float sqdRemainingDistance = (transform.position - end).sqrMagnitude;

        while (sqdRemainingDistance > 0.1)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, 0.1f);
            rb2D.MovePosition(newPosition);
            sqdRemainingDistance = (transform.position - end).sqrMagnitude;

            yield return null;
        }

        rb2D.MovePosition(end);
        isMoving = false;
        animator.SetBool("isMoving", false);
    }

    private void Animate (float xDir, float yDir)
    {
        animator.SetFloat("moveX", xDir);
        animator.SetFloat("moveY", yDir);
    }
}