using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class betaController : MonoBehaviour
{
    public float speed = 10.0f;
    public float jumpForce = 500.0f;
    public float distToGround = 1.0f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private bool isGrounded;
    private bool canWallRun = false;
    private Vector3 wallDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.AddForce(movement * speed);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce);
        }

        if (canWallRun && Input.GetKey(KeyCode.LeftShift))
        {
            rb.useGravity = false;
            rb.velocity = new Vector3(wallDirection.x * speed, wallDirection.y * speed, wallDirection.z * speed);
        }
        else
        {
            rb.useGravity = true;
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position, -Vector3.up, distToGround, groundMask);

        if (isGrounded)
        {
            canWallRun = false;
        }
        else
        {
            CheckWall();
        }
    }

    void CheckWall()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 2.0f))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                canWallRun = true;
                wallDirection = hit.normal;
            }
        }
        else
        {
            canWallRun = false;
        }
    }
}
