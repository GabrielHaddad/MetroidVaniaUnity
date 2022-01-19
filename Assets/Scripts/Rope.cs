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
    [SerializeField] LayerMask isGround;
    [SerializeField] float runSpeed;
    [SerializeField] float dashStrenght;
    [SerializeField] float jumpForce;
    [SerializeField] float distanceGrapple;
    bool isGrapling = false;
    float moveInput;

    
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

        
    }

    void FixedUpdate() 
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        //rb2d.AddForce(new Vector2(moveInput * runSpeed * 10 * Time.fixedDeltaTime, 0f), ForceMode2D.Impulse);
        rb2d.velocity += new Vector2(moveInput * runSpeed * 10 * Time.fixedDeltaTime, 0f);
        

        if (Input.GetKey(KeyCode.Space) && (boxCollider2D.IsTouchingLayers(isGround) || isGrapling))
        {
            rb2d.AddForce(new Vector2 (0f, jumpForce * 10 * Time.deltaTime), ForceMode2D.Impulse);
            isGrapling = false;
            joint2D.enabled = false;
            lineRenderer.enabled = false;
        }

        if (Input.GetMouseButton(1))
        {
            Debug.Log("sfsd");
            moveInput = Input.GetAxisRaw("Horizontal");
            rb2d.AddForce(new Vector2(moveInput * dashStrenght * 10 * Time.fixedDeltaTime, 0f), ForceMode2D.Impulse);
        }
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
        rb2d.AddForce( new Vector2(Input.GetAxisRaw("Horizontal") * runSpeed * Time.deltaTime, 0f), ForceMode2D.Impulse);
    }

    private void DrawRope()
    {
        currentGrapplePosition = Vector2.Lerp(worldPoint, gunPoint.position, Time.deltaTime * 8f);

        lineRenderer.SetPosition(0, gunPoint.position);
        lineRenderer.SetPosition(1, currentGrapplePosition);
    }
}