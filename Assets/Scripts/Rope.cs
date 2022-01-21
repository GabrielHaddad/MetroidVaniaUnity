using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{

    Rigidbody2D rb2d;
    SpringJoint2D joint2D;
    private LineRenderer lineRenderer;
    private List<Vector2> ropeSegments = new List<Vector2>();
    private float ropeSegLen = 0.25f;
    private int segmentLength = 35;
    private float lineWidth = 0.1f;
    Vector2 currentGrapplePosition;
    Vector2 worldPoint;
    Vector2 grapplePoint;
    [SerializeField] Transform gunPoint;
    BoxCollider2D boxCollider2D;
    [SerializeField] LayerMask whatIsGrappable;
    [SerializeField] LayerMask whatIsWallJumpable;
    [SerializeField] LayerMask isGround;
    [SerializeField] float runSpeed;
    [SerializeField] float dashStrenght;
    [SerializeField] float jumpForce;
    [SerializeField] float distanceGrapple;
    bool isGrapling = false;
    float moveInput;
    bool canMove = true;
    bool isWallSliding = false;
    [SerializeField] float dashDistance = 15f;
    bool isDashing = false;
    [SerializeField] float dashCoolDown = 0.5f;
    [SerializeField] ParticleSystem dashEffect;
    [SerializeField] float wallSlideSpeed = 15f;
    [SerializeField] float wallJumpForce = 15f;
    [SerializeField] float stopMovementWallJump = 0.3f;


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
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        // Vector3 ropeStartPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // for (int i = 0; i < segmentLength; i++)
        // {
        //     this.ropeSegments.Add(ropeStartPoint);
        //     ropeStartPoint.y -= ropeSegLen;
        // }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartGrapple();
        }

        if (boxCollider2D.IsTouchingLayers(whatIsWallJumpable) && !boxCollider2D.IsTouchingLayers(isGround) && Mathf.Abs(moveInput) > Mathf.Epsilon)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    void FixedUpdate()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (!isDashing && canMove)
        {

            //rb2d.AddForce(new Vector2(moveInput * runSpeed * 10 * Time.fixedDeltaTime, 0f), ForceMode2D.Impulse);
            float xMovement = moveInput * runSpeed * 10 * Time.fixedDeltaTime;
            Vector2 playerVelocity = new Vector2(xMovement, rb2d.velocity.y);

            if (Mathf.Abs(xMovement) < Mathf.Epsilon && isGrapling)
            {
                playerVelocity.x = rb2d.velocity.x;
            }

            if (isWallSliding)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, -wallSlideSpeed * 10 * Time.fixedDeltaTime);
            }
            else
            {
                rb2d.velocity = playerVelocity;
            }

        }


        if (!isWallSliding && !isDashing && Input.GetKey(KeyCode.Space) && (boxCollider2D.IsTouchingLayers(isGround) || isGrapling))
        {
            rb2d.velocity = Vector2.zero;
            rb2d.AddForce(new Vector2(0f, jumpForce * 10 * Time.deltaTime), ForceMode2D.Impulse);
            isGrapling = false;
            joint2D.enabled = false;
            lineRenderer.enabled = false;
        }
        
        if (isWallSliding && Input.GetKey(KeyCode.Space))
        {
            rb2d.velocity = Vector2.zero;
            rb2d.AddForce(new Vector2(wallJumpForce * 10 * -moveInput * Time.fixedDeltaTime, wallJumpForce * 10 * Time.fixedDeltaTime), ForceMode2D.Impulse);
            StartCoroutine(StopMovementPlayer());
        }

        if (Input.GetKey(KeyCode.E) && !isDashing)
        {
            StartCoroutine(Dash());
            //moveInput = Input.GetAxisRaw("Horizontal");
            // rb2d.AddForce(new Vector2(moveInput * dashStrenght * 10 * Time.fixedDeltaTime, 0f), ForceMode2D.Impulse);    
        }

    }

    IEnumerator StopMovementPlayer()
    {
        canMove = false;
        yield return new WaitForSeconds(stopMovementWallJump);
        canMove = true;
    }

    IEnumerator Dash()
    {
        isDashing = true;
        if (Mathf.Abs(moveInput) < Mathf.Epsilon)
        {
            rb2d.velocity = Vector2.zero;
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
            rb2d.AddForce(new Vector2(dashDistance * 10 * Time.fixedDeltaTime, 0f), ForceMode2D.Impulse);
        }
        else
        {
            rb2d.velocity = Vector2.zero;
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
            rb2d.AddForce(new Vector2(dashDistance * moveInput * Time.fixedDeltaTime, 0f), ForceMode2D.Impulse);
        }

        ParticleSystem instance = Instantiate(dashEffect, transform.position, Quaternion.Euler(0f, 0f, -90f), transform);
        Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);

        float gravity = rb2d.gravityScale;
        rb2d.gravityScale = 0;

        yield return new WaitForSeconds(dashCoolDown);
        isDashing = false;
        rb2d.gravityScale = gravity;
    }

    void LateUpdate()
    {
        if (isGrapling)
        {
            DrawRope();
            lineRenderer.enabled = true;
        }
    }

    void StartGrapple()
    {
        worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, 0f, whatIsGrappable);

        if (hit.collider != null)
        {
            isGrapling = true;
            grapplePoint = hit.point;
            joint2D.autoConfigureConnectedAnchor = false;
            joint2D.connectedAnchor = hit.point;

            float distanceFromPoint = Vector2.Distance(gunPoint.position, grapplePoint);

            joint2D.autoConfigureDistance = false;
            joint2D.distance = distanceFromPoint * distanceGrapple;
            joint2D.enabled = true;

            lineRenderer.positionCount = 2;
        }
    }

    void Run()
    {
        rb2d.AddForce(new Vector2(Input.GetAxisRaw("Horizontal") * runSpeed * Time.deltaTime, 0f), ForceMode2D.Impulse);
    }

    private void DrawRope()
    {
        currentGrapplePosition = Vector2.Lerp(worldPoint, gunPoint.position, Time.deltaTime * 8f);

        lineRenderer.SetPosition(0, gunPoint.position);
        lineRenderer.SetPosition(1, currentGrapplePosition);
    }

    void OnJointBreak2D(Joint2D brokenJoint)
    {
        lineRenderer.enabled = false;
        isGrapling = false;
        joint2D = gameObject.AddComponent(joint2D.GetType()) as SpringJoint2D;
        joint2D.enabled = false;
        joint2D.enableCollision = true;
        joint2D.breakForce = 5000f;
    }
}