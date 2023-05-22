using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourManager : MonoBehaviour
{
    public static ParkourManager instance;

    #region Parkour Var
    [Header("PARKOUR")]
    public Vector3 rayOffset = new Vector3(0, 0, 0);
    public float rayLength = .9f;
    public LayerMask ObstaclesLayer;
    public float heightRayLength = 6f;
    public bool playerInaction;
    public List<NewParkourAction> NewParkourActions;
    public new CapsuleCollider collider;
    public bool inFrontOfObstacle;
    [Header("-----------")]
    [Header("PARKOUR LEDGE")]
    [SerializeField] float ledgeRayLength = 11f;
    [SerializeField] float ledgeRayHeightThreshold = .76f;
    public bool playerOnLedge;
    [Header("-----------")]
    [Header("WALL INTERACTION")]
    public Transform wallCheck;
    public LayerMask wallLayer;
    public float wallDistance = .4f;
    public bool isOnWall = false;
    #endregion

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

    #region Parkour
    public ObstacleInfo CheckObstacle()
    {
        var hitData = new ObstacleInfo();
        var rayOrigin = transform.position + rayOffset;
        hitData.hitFound = Physics.Raycast(rayOrigin, transform.forward, out hitData.hit, rayLength, ObstaclesLayer);

        Debug.DrawRay(rayOrigin, transform.forward * rayLength, (hitData.hitFound) ? Color.green : Color.red);

        if (hitData.hitFound)
        {
            var heightOrigin = hitData.hit.point + Vector3.up * heightRayLength;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down, out hitData.heightInfo, heightRayLength, ObstaclesLayer);
            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength, (hitData.heightHitFound) ? Color.blue : Color.red);
        }
        return hitData;
    }

    public void Parkour()
    {

        if (Input.GetKeyDown(KeyCode.Space) && !playerInaction)
        {
            var hitData = CheckObstacle();
            if (hitData.hitFound)
            {
                foreach (var action in NewParkourActions)
                {
                    if (action.CheckIfAvailable(hitData, transform))
                    {
                        StartCoroutine(PerformParkourAction(action));
                        break;
                    }
                }

            }
        }

    }

    public void SetControl(bool hasControl)
    {
       nahinController.instance.playerControl = hasControl;
        nahinController.instance.controller.enabled = hasControl;
        collider.enabled = hasControl;

        if (!hasControl)
        {
            nahinController.instance.anim.SetFloat("movementValue", 0f);
            nahinController.instance.requiredRotation = transform.rotation;
        }
    }
    IEnumerator PerformParkourAction(NewParkourAction action)
    {
        playerInaction = true;
        SetControl(false);
        nahinController.instance.anim.CrossFade(action.AnimationName, 0.2f);
        yield return null;

        var animationState = nahinController.instance.anim.GetNextAnimatorStateInfo(0);

        float timerCounter = 0f;

        while (timerCounter <= animationState.length)
        {
            timerCounter += Time.deltaTime;

            if (action.LookAtObstacle)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.RequiredRotation, 450f);
            }

            if (action.AllowTargetMatching)
            {
                CompareTarget(action);
            }

            yield return null;
        }

        yield return new WaitForSeconds(action.ParkourActionDelay);

        nahinController.instance.anim.SetBool("Running", false);
        SetControl(true);
        playerInaction = false;
    }

    void CompareTarget(NewParkourAction action)
    {
        nahinController.instance.anim.MatchTarget(action.ComparePos, transform.rotation, action.CompareBodyPart, new MatchTargetWeightMask(action.ComparePosWeight, 0), action.CompareStartTime, action.CompareEndTime);
    }
    #endregion
}
public struct ObstacleInfo
{
    public bool hitFound;
    public bool heightHitFound;
    public RaycastHit hit;
    public RaycastHit heightInfo;
}
