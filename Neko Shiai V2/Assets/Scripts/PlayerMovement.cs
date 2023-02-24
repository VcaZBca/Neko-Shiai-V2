using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;

    private bool canRoll = true;
    private bool isRolling;
    private float rollingPower = 24f;
    private float rollingTime = 0.2f;
    private float rollingCooldown = 1f;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private TrailRenderer tr;



    // Update is called once per frame
    void Update()
    {
        if (isRolling)
        {
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        if (Input.GetButtonDown("Roll") && canRoll)
        {
            StartCoroutine(Roll());
        }

        Flip();
    }

    private void FixedUpdate()
    {
        if (isRolling)
        {
            return;
        }

        rb.velocity = new Vector2 (horizontal * speed, rb.velocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private IEnumerator Roll()
    {
        canRoll = false;
        isRolling = true;
        rb.velocity = new Vector2(transform.localScale.x * rollingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(rollingTime);
        tr.emitting = false;
        isRolling = false;
        yield return new WaitForSeconds(rollingCooldown);
        canRoll = true;
    }
}
