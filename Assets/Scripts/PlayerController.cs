using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Player")]
    Rigidbody2D rb2d;
    BoxCollider2D boxCollider2D;
    SpringJoint2D joint2D;
    LineRenderer lineRenderer;

    [Header("Grappling Hook")]
    [SerializeField] float lineWidth = 0.1f;
    [SerializeField] float grapplingDistance = 0f;
    [SerializeField] Transform grappleGunStartPoint;
    [SerializeField] LayerMask isGrappable;
    [SerializeField] float distanceBetweenAtachedBodies;
    Vector3 clickedWorldPoint;
    Vector2 grapplePoint;
    bool isGrapling = false;

    [Header("Movement")]
    [SerializeField] LayerMask isGround;
    [SerializeField] float runSpeed;
    [SerializeField] float jumpForce;
    float moveInput;
    bool canMove = true;
    bool canJump = false;

    [Header("Dash")]
    [SerializeField] float dashDistance = 15f;
    [SerializeField] float dashCoolDown = 0.5f;
    [SerializeField] ParticleSystem dashEffect;
    bool isDashing = false;
    bool canDash = false;

    [Header("Wall Jump")]
    [SerializeField] LayerMask isWallJumpable;
    [SerializeField] float wallSlideSpeed = 15f;
    [SerializeField] float wallJumpForce = 15f;
    [SerializeField] float stopMovementWallJump = 0.3f;
    bool isWallSliding = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        rb2d = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        joint2D = GetComponent<SpringJoint2D>();
    }

    void Start()
    {
        joint2D.enabled = false;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetMouseButtonDown(0))
        {
            StartGrapple();
        }

        CheckIfIsWallSliding();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            canJump = true;
        }

        if (Input.GetMouseButtonDown(1))
        {
            canDash = true;
        }
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            Run();

            if (canJump)
            {
                Jump();
                canJump = false;
            }

            if (canDash && !isDashing)
            {
                StartCoroutine(Dash());
                canDash = false;
            }
        }

    }

    void LateUpdate()
    {
        if (isGrapling)
        {
            DrawRope();
        }
    }

    void Run()
    {
        float xMovement = moveInput * runSpeed;
        bool isPlayerNotMoving = Mathf.Abs(xMovement) < Mathf.Epsilon;

        Vector2 playerVelocity = new Vector2(xMovement, rb2d.velocity.y);

        if (isPlayerNotMoving && isGrapling)
        {
            playerVelocity.x = rb2d.velocity.x;
        }
        else if (isWallSliding)
        {
            playerVelocity.y = -wallSlideSpeed;
        }

        rb2d.velocity = playerVelocity;
    }

    void Jump()
    {

        bool isTouchingGround = boxCollider2D.IsTouchingLayers(isGround);
        rb2d.velocity = Vector2.zero;

        if (!isDashing && (isTouchingGround || isGrapling))
        {
            rb2d.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);

            isGrapling = false;
            joint2D.enabled = false;
            lineRenderer.enabled = false;
        }
        else if (isWallSliding)
        {
            float movementX = wallJumpForce * -moveInput;
            rb2d.AddForce(new Vector2(movementX, wallJumpForce), ForceMode2D.Impulse);
            StartCoroutine(StopMovementPlayer(stopMovementWallJump));
        }

    }

    void CheckIfIsWallSliding()
    {
        bool isTouchingWall = boxCollider2D.IsTouchingLayers(isWallJumpable);
        bool isTouchingGround = boxCollider2D.IsTouchingLayers(isGround);
        bool hasHorizontalSpeed = Mathf.Abs(moveInput) > Mathf.Epsilon;

        if (isTouchingWall && !isTouchingGround && hasHorizontalSpeed)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }


    IEnumerator StopMovementPlayer(float stopTime)
    {
        canMove = false;
        yield return new WaitForSeconds(stopTime);
        canMove = true;
    }

    IEnumerator Dash()
    {   
        isDashing = true;
        bool isPlayerNotMoving = Mathf.Abs(moveInput) < Mathf.Epsilon;

        rb2d.velocity = Vector2.zero;
        rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);

        Vector2 dashForce;

        if (isPlayerNotMoving)
        {
            dashForce = new Vector2(dashDistance, 0f);
        }
        else
        {
            dashForce = new Vector2(dashDistance * moveInput, 0f);
        }

        rb2d.AddForce(dashForce, ForceMode2D.Impulse);

        PlayDashParticleEffect();

        float gravity = rb2d.gravityScale;
        rb2d.gravityScale = 0;
        canMove = false;

        yield return new WaitForSeconds(dashCoolDown);

        canMove = true;
        isDashing = false;
        rb2d.gravityScale = gravity;
    }

    void PlayDashParticleEffect()
    {
        ParticleSystem instance = Instantiate(dashEffect, transform.position, Quaternion.Euler(0f, 0f, -90f), transform);
        Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
    }

    void StartGrapple()
    {
        clickedWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 difference = transform.position - clickedWorldPoint;
        difference.Normalize();

        RaycastHit2D hit = Physics2D.Raycast(clickedWorldPoint, difference, -grapplingDistance, isGrappable);

        if (hit.collider != null)
        {
            isGrapling = true;
            grapplePoint = hit.point;
            ConfigureSpringJoint();
            joint2D.enabled = true;
        }
    }

    void ConfigureSpringJoint()
    {
        joint2D.autoConfigureConnectedAnchor = false;
        joint2D.autoConfigureDistance = false;
        joint2D.connectedAnchor = grapplePoint;

        float distanceFromPoint = Vector2.Distance(grappleGunStartPoint.position, grapplePoint);
        joint2D.distance = distanceFromPoint * distanceBetweenAtachedBodies;
    }

    private void DrawRope()
    {
        Vector2 currentGrapplePosition = Vector2.Lerp(grapplePoint, grappleGunStartPoint.position, Time.deltaTime * 8f);

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, grappleGunStartPoint.position);
        lineRenderer.SetPosition(1, currentGrapplePosition);

        lineRenderer.enabled = true;
    }

    void AddNewSpringJoint()
    {
        joint2D = gameObject.AddComponent(joint2D.GetType()) as SpringJoint2D;
        joint2D.enabled = false;
        joint2D.enableCollision = true;
        joint2D.breakForce = 5000f;
    }

    void OnJointBreak2D(Joint2D brokenJoint)
    {
        lineRenderer.enabled = false;
        isGrapling = false;

        AddNewSpringJoint();
    }
}