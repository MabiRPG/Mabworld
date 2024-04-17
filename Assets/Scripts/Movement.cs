using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private Animator animator;

    private bool isMoving = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>(); 
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected void AttemptMove (float xDir, float yDir)
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;
        RaycastHit2D hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        if (hit.transform == null & !isMoving)
        {
            StartCoroutine(Move(end));
            Animate(xDir, yDir);
        }
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
            // Debug.Log(sqdRemainingDistance);
            sqdRemainingDistance = (transform.position - end).sqrMagnitude;

            yield return null;
        }

        rb2D.MovePosition(end);
        isMoving = false;
        animator.SetBool("isMoving", false);
    }

    private void Animate (float xDir, float yDir)
    {
        animator.SetFloat("MovementX", xDir);
        animator.SetFloat("MovementY", yDir);
    }
}
