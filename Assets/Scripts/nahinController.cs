using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using static States;

public class nahinController : MonoBehaviour
{
    #region To Assign
    [Header("-----------")]
    [Header("TO ASSIGN")]
    public Animator anim;
    public CharacterController controller;
    public Transform Cam;
    public CinemachineFreeLook freeLook;
    public Transform rayPos;

    public Transform enemy;

    public Transform LookAt;

    public bool playerControl = true;
    #endregion
    #region Stats
    [Header("-----------")]
    [Header("STATS")]
    public float speed = 6f;
    public float HP = 10f;
    public float moonEnergy;
    #endregion

    #region Neutralize
    [Header("-----------")]
    [Header("NEUTRALIZE")]

    public LineRenderer connection;
    public LayerMask targetLayer;
    public LayerMask nodesLayer;
    public Camera TPCam;
    public Transform camPos;
    public Transform target;
    public GameObject NodesPrefab;
    public Vector3 NodesOffSet;    
    GameObject nodeParent;
    public GameObject pointer;

    public float Range;
    public float startFOV;
    public float endFOV = 20f;
    public float transitionSpeed;

    public bool instantiated = false;
    public bool NodesActivated;
    #endregion

    #region Potentiation Var
    [Header("-----------")]
    [Header("POTENTIATION")]
    public bool PotentiationUnlocked;
    public bool buffed;

    public Slider moonBar;

    public float jumpSH;
    public float buffedSpeed = 12f;

    public List<AttackObject> poweredCombo;

    int pComboCounter;

    public float pTimeBetweenCombos;
    public float pCooldown;
    float pLastClickedTime;
    float pLastComboEnd;
    [SerializeField] Weapon[] pWeapon;
    [Header("-----------")]
    #endregion

    #region M&R
    [Header("-----------")]
    [Header("MOVEMENT & ROTATION")]
    public float horizontal;
    public float vertical;
    public Vector3 moveDir;
    public float gravity = -9.10f;
    public float jumpH = 1f;
    public float turnSmoothTime = .1f;
    private float turnSmoothVelocity;
   public Quaternion requiredRotation;

    [Header("-----------")]
    [Header("Sneaking")]
    public float sneakSpeed = 3f;
    public bool isSneaking;
    #endregion

    #region Ground Check
    [Header("-----------")]
    [Header("GROUND CHECK")]
    public Transform ground_Check;
    public LayerMask ground_Mask;
    public float ground_Distance = 0.4f;
    public Vector3 velocity;
    public bool isGrounded;
    #endregion

    #region Combat Var
    public List<AttackObject> combo;
    float lastClickedTime;
    float lastComboEnd;
    int comboCounter;
    public float timeBetweenCombos;
    public float cooldown;
    [SerializeField] Weapon[] weapon;
    #endregion

    private bool wallRunning = false;
    private bool isWallRunning = false;
    public static nahinController instance;

    public States currentState;

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
        startFOV = freeLook.m_Lens.FieldOfView;
        camPos = TPCam.gameObject.transform;
        currentState = new IdleNahin();
        isGrounded = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        currentState = currentState.Process();
        connection.SetPosition(0, new Vector3(0, 0, 0));
        moonBar.value = moonEnergy;

        
    }

    #region Movement, Rotation, Sneaking and Jumping
    public void Movement()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;


        if (direction.magnitude >= .1f)
        {
            anim.SetBool("Running", true);
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            requiredRotation = Quaternion.Euler(0f, angle, 0f);
            transform.rotation = requiredRotation;

            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("Running", false);
        }
        controller.Move(velocity * Time.deltaTime);
    }
    public void Grounded()
    {
        isGrounded = Physics.CheckSphere(ground_Check.position, ground_Distance, ground_Mask);
        velocity.y += gravity * Time.deltaTime;
        
            if (isGrounded && velocity.y < 0)
            {
                anim.ResetTrigger("Jump");
                anim.ResetTrigger("IdleJump");
                speed = 6f;
                velocity.y = 0f;
                gravity = -9.1f;
            }
        
        
    }

    public void IdleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !buffed && !ParkourManager.instance.playerInaction)
        {
            anim.SetTrigger("IdleJump");
            StartCoroutine(delayedIdleJump());
        }
    }

    public void RunJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            anim.SetTrigger("Jump");
            StartCoroutine(delayedRunJump());
        }
    }

    public void Sneak()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        isGrounded = Physics.CheckSphere(ground_Check.position, ground_Distance, ground_Mask);

        if (direction.magnitude >= .1f)
        {
            anim.SetBool("Sneaking", true);
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * sneakSpeed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("Sneaking", false);
        }
        controller.Move(velocity * Time.deltaTime);
    }

    IEnumerator delayedRunJump()
    {
        yield return new WaitForSeconds(.5f);
        velocity.y = Mathf.Sqrt(jumpH * -2f * gravity);
    }

    IEnumerator delayedIdleJump()
    {
        yield return new WaitForSeconds(.65f);
        velocity.y = Mathf.Sqrt(jumpH * -2f * gravity);
        speed = 0f;
    }


    #endregion

    #region Potentiation

    public void BuffedMovement()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        isGrounded = Physics.CheckSphere(ground_Check.position, ground_Distance, ground_Mask);
        if (isGrounded)
        {
            anim.SetBool("Falling", true);
        }
        else
        {
            anim.SetBool("Falling", false);
        }

        if (direction.magnitude >= .1f)
        {
            anim.SetBool("BuffedRunning", true);
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * buffedSpeed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("BuffedRunning", false);
        }
        controller.Move(velocity * Time.deltaTime);
    }
    public void StrongJump()
    {

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            anim.SetTrigger("StrongJump");
            StartCoroutine(delayedStrongJump());
        }

    }
    IEnumerator delayedStrongJump()
    {
        yield return new WaitForSeconds(.3f);
        Debug.Log("Jump");
        velocity.y = Mathf.Sqrt(jumpSH * -2f * gravity);
    }
    #endregion

    #region Neutralize
    public void Neutralize()
    {
        if (moonEnergy >= 1f)
        {
            RaycastHit hit;
            NodesSystem();

            if (Input.GetMouseButton(1) && !NodesActivated)
            {
                
                pointer.SetActive(true);
                freeLook.m_LookAt = rayPos;
                freeLook.m_Lens.FieldOfView = Mathf.Clamp(Mathf.Lerp(freeLook.m_Lens.FieldOfView, 20, transitionSpeed * Time.deltaTime), endFOV, startFOV);
                Time.timeScale = .5f;
                if (Physics.Raycast(camPos.position, camPos.forward, out hit, Range, targetLayer) && hit.collider.tag == "NeutralizeTarget")
                {
                    NodesPrefab.GetComponent<LookAtPlayer>().player = transform;
                    NodesActivated = true;
                    target = hit.collider.gameObject.transform;
                    Debug.DrawRay(camPos.position, camPos.forward * Range, Color.red);
                }
            }
            else if (NodesActivated == false)
            {
                pointer.SetActive(false);
                Time.timeScale = 1f;
                target = null;
                freeLook.m_LookAt = LookAt;
                freeLook.m_Lens.FieldOfView = Mathf.Clamp(Mathf.Lerp(freeLook.m_Lens.FieldOfView, 20, transitionSpeed * Time.deltaTime), startFOV, endFOV);
            }
        }
        

    }

    public void NodesSystem()
    {

        if (NodesActivated)
        {
            if (!instantiated)
            {
                nodeParent = Instantiate(NodesPrefab, target.position + NodesOffSet, target.rotation);
                instantiated = true;
            }
            if (Input.GetMouseButtonUp(1))
            {
                NodesActivated = false;
                Destroy(nodeParent);
                instantiated = false;
            }
            RaycastHit hit;
            if (Physics.Raycast(camPos.position, camPos.forward, out hit, Range, nodesLayer))
            {

                connection.SetPosition(1, hit.collider.transform.position);
                Debug.DrawRay(camPos.position, camPos.forward * Range, Color.red);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (hit.collider.tag == "Vision")
                    {
                        moonEnergy -= 20f;
                        target.transform.GetComponent<EnemyStateManager>().isBlinded = true;
                        Destroy(hit.collider.gameObject);
                        target.tag = "Neutralized";
                    }

                    if (hit.collider.tag == "Hearing")
                    {
                        moonEnergy -= 20f;
                        target.transform.GetComponent<EnemyStateManager>().isDeafened = true;
                        Destroy(hit.collider.gameObject);
                        target.tag = "Neutralized";
                    }

                    NodesActivated = false;
                    instantiated = false;
                    StartCoroutine(destroyNodes());
                    connection.SetPosition(1, new Vector3(0, 0, 0));
                    connection.SetPosition(0, new Vector3(0, 0, 0));
                }
            }

        }
    }

    IEnumerator destroyNodes()
    {
        yield return new WaitForSeconds(1f);
        Destroy(nodeParent);
    }
    #endregion

    #region Combat System

    public void Attack()
    {
        if (Time.time - lastComboEnd > timeBetweenCombos && comboCounter <= combo.Count)
        {
            CancelInvoke("EndCombo");

            if (Time.time - lastClickedTime >= cooldown)
            {
                weapon[0].EnableTriggerBox();
                weapon[1].EnableTriggerBox();
                anim.runtimeAnimatorController = combo[comboCounter].animOV;
                anim.Play("Attack State", 0, 0);
                weapon[0].damage = combo[comboCounter].damage;
                weapon[1].damage = combo[comboCounter].damage;
                comboCounter++;
                lastClickedTime = Time.time;

                if (comboCounter >= combo.Count)
                {
                    comboCounter = 0;
                }

            }
        }
    }

    public void ExitAttack()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > .9 && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            weapon[0].DisableTriggerBox();
            weapon[1].DisableTriggerBox();
            Invoke("EndCombo",1);
        }
    }

    public void EndCombo()
    {
        comboCounter = 0;
        lastComboEnd = Time.time;
    }
    #endregion

    #region Powered Combat System
    public void PoweredAttack()
    {
        if (Time.time - lastComboEnd > timeBetweenCombos && comboCounter <= combo.Count)
        {
            CancelInvoke("pEndCombo");

            if (Time.time - lastClickedTime >= cooldown)
            {
                anim.runtimeAnimatorController = poweredCombo[pComboCounter].animOV;
                anim.Play("Powered Attack State", 1, 0);
                weapon[0].damage = poweredCombo[pComboCounter].damage;
                weapon[1].damage = poweredCombo[pComboCounter].damage;
                pComboCounter++;
                pLastClickedTime = Time.time;

                if (pComboCounter >= poweredCombo.Count)
                {
                    pComboCounter = 0;
                }

            }
        }
    }

    public void pExitAttack()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > .9 && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            Invoke("pEndCombo", 1);
        }
    }

    public void pEndCombo()
    {
        pComboCounter = 0;
        pLastComboEnd = Time.time;
    }
    #endregion

}
