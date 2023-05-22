using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRun : MonoBehaviour
{
    public float wallRunForce = 10f;
    public float wallRunDuration = 2f;
    public float wallRunCooldown = 2f;
    public float wallRunGravityMultiplier = 0.5f;
    public float wallCheckDistance = 0.5f;
    public Transform orientation;
    public Transform camOrientation;

    private CharacterController controller;
    private bool isWallRunning = false;
    public bool isOnWall = false;
    private Vector3 wallNormal;
    private float lastWallRunTime;
    public static WallRun instance;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    //void Update()
    //{
    //    CheckWall();
    //    HandleWallRunInput();
    //    ApplyWallRunGravity();
    //    HandleWallRun();
    //}

    public void CheckWall()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, orientation.forward, out hit, wallCheckDistance))
        {
            if (!hit.collider.isTrigger && !controller.isGrounded && hit.transform.tag == "Wall")
            {
                isOnWall = true;
                wallNormal = hit.normal;
            }
        }
        else
        {
            isOnWall = false;
        }
    }

    public void HandleWallRunInput()
    {
        if (isOnWall && Time.time > lastWallRunTime + wallRunCooldown)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                lastWallRunTime = Time.time;
                isWallRunning = true;
                Invoke("StopWallRun", wallRunDuration);
            }
            
        }
    }

    public void StopWallRun()
    {
        isWallRunning = false;
    }

    public void ApplyWallRunGravity()
    {
        if (isWallRunning)
        {
            controller.Move(-Physics.gravity * wallRunGravityMultiplier * Time.deltaTime);
        }
    }

    public void HandleWallRun()
    {
        if (isWallRunning)
        {
            Vector3 force = orientation.forward * wallRunForce;
            controller.Move(force * Time.deltaTime);

            if (Input.GetKey(KeyCode.S))
            {
                controller.Move(-force * Time.deltaTime);
            }
        }
    }
}
