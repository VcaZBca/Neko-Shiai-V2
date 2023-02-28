using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private SpriteRenderer sprite;
    private Animator anim;

    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;

    private bool crouch = false;
    private float crouchSpeed = 0.5f;
    private bool isCrouching = false;
    private float ceilingCheckRadius = 0.2f;
    [SerializeField] Collider2D standingCollider;


    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private bool canRoll = true;
    private bool isRolling;
    private float rollingPower = 24f;
    private float rollingTime = 0.2f;
    private float rollingCooldown = 1f;

    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);


    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform ceilingCheck;

    [SerializeField] private TrailRenderer tr;

    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    private enum MovementState { idle, running, jumping, falling, crouching, climbing, roll, hurt }



    // Update is called once per frame
    void Update()
    {
        if (isRolling)
        {
            return;
        }

        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        { 
        coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        horizontal = Input.GetAxisRaw("Horizontal");

        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f && (isCrouching == false))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);

            jumpBufferCounter = 0f;
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f && (isCrouching == false))
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

            coyoteTimeCounter = 0f;
        }

        if (Input.GetButtonDown("Roll") && canRoll && IsGrounded())
        {
            StartCoroutine(Roll());
        }

        if (IsGrounded() && CeilingCheck() || (Input.GetAxis("Crouch") != 0f))
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }

        if (isCrouching == true)
        {
            Debug.Log($"rb.velocity: {rb.velocity}");
            standingCollider.enabled = false;
            rb.velocity = new Vector2(horizontal * speed * crouchSpeed, rb.velocity.y);
            Debug.Log($"rb.velocity: {rb.velocity}");
        }
        else if (isCrouching == false)
        {
            standingCollider.enabled = true;
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }



        WallSlide();
        WallJump();


        if (!isWallJumping)
        {
            Flip();
        }

        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        if (isRolling)
        {
            return;
        }

        if (!isWallJumping && (isCrouching == false))
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }



    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private bool CeilingCheck()
    {
        return Physics2D.OverlapCircle(ceilingCheck.position, 0.2f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f) 
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if (transform.lossyScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
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

    private void UpdateAnimationState()
    {
        MovementState state = 0;

        if (rb.velocity.x == 0f)
        {
            state = MovementState.idle;
        }

        if (isCrouching == true)
        {
            state = MovementState.crouching;
        }

        anim.SetInteger("state", (int)state);
    }

}
