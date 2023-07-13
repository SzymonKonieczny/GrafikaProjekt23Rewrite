using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public Camera cam;
    public float speed = 10;
    public float SprintMultiplier = 2;
    float SprintMultiplier_with_check = 1;
    public float Gravity = -1.81f;
    public Transform GroundCheck;
    public float GroundDistancs = 0.5f;
    public LayerMask groundMask;
    public int maxHP;



    int HP;
    bool isGrounded;
    float x;
    Vector3 velocity;
    float z;
    Vector3 move;


    public Transform Player;
    public Transform cams;
    public Transform weapon;
    public bool Dead = false;
    public float xsens;
    public float ysens;
    public float maxAngle;
    public static bool CursorLocked = true;
    bool cursorlockcooldown;
    public GameObject JumpscareObject;
    [SerializeField] LayerMask RayMask;
   // [SerializeField] Camera camera;
    public Transform Hand;
    Quaternion camcenter;

    [SerializeField] RestartUI RestartScreen;
    [SerializeField] Item HeldIted;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        DoMove();
        if (Cursor.lockState == CursorLockMode.Locked) DoLook();
        if (Input.GetKeyDown(KeyCode.Escape))
            RestartScreen.gameObject.SetActive(!RestartScreen.gameObject.activeInHierarchy);
        if (!cursorlockcooldown && Input.GetKeyDown(KeyCode.Escape)) StartCoroutine(UpdateCursorLock());

    }
    public void SetHandItem(Item i)
    {
        HeldIted = i;
    }
    public Item GetHandItem()
    {
        
        return HeldIted;
    }
    void DoMove()
    {
        isGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistancs, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }


        x = Input.GetAxisRaw("Horizontal");
        z = Input.GetAxisRaw("Vertical");
        move = transform.right * x + transform.forward * z;


        controller.Move(move * speed * SprintMultiplier_with_check * Time.deltaTime);

        if (isGrounded && Input.GetKey(KeyCode.Space)) velocity.y = 5f;

        velocity.y += Gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
    void DoLook()
    {
        if (Dead) return;
        setY();
        setX();
        InteractionRay();
    }
    void setY()
    {
        float t_input = Input.GetAxis("Mouse Y") * ysens * Time.deltaTime;
        Quaternion t_adjust = Quaternion.AngleAxis(t_input, -Vector3.right);
        Quaternion t_delta = cams.localRotation * t_adjust;

        if (t_delta.x >= 0.6f) t_delta.x = 0.6f;
        else if (t_delta.x <= -0.6f) t_delta.x = -0.6f;
        cams.localRotation = t_delta;
    }

    void setX()
    {
        float t_input = Input.GetAxis("Mouse X") * xsens * Time.deltaTime;
        Quaternion t_adjust = Quaternion.AngleAxis(t_input, Vector3.up);
        Quaternion t_delta = Player.localRotation * t_adjust;
        Player.localRotation = t_delta;
    }
    IEnumerator UpdateCursorLock()
    {
        cursorlockcooldown = true;
        if (CursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            CursorLocked = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            CursorLocked = true;
        }

        yield return new WaitForSeconds(1f);
        cursorlockcooldown = false;
    }
    public void AttatchToHand(Transform ToHold)
    {
        if (Hand.transform.childCount > 0) DropHeldItem(ToHold.position);
        if (ToHold.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
        ToHold.SetParent(Hand);
        ToHold.localPosition = new Vector3(0, 0, 0);
        ToHold.localEulerAngles = new Vector3(0, 0, 0);
    }
    void DropHeldItem(Vector3 PlaceTransform)
    {
        if (Hand.transform.childCount > 0)
        {
            Transform[] Held = Hand.GetComponentsInChildren<Transform>();
            Hand.transform.DetachChildren();
            foreach (Transform t in Held)
            {
                if (t.name == "Hand") continue; //VERY TEMPORARY, first element is the hand (so not a child, buuuut i guess according to Unity yeah)
                Debug.Log(t.name);
                if (t.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = false;
                }
                t.position = PlaceTransform + new Vector3(0, 0.5f, 0);
            }

        }
    }
    public Transform TakeAwayHeldItem()
    {
        if (Hand.transform.childCount > 0)
        {
            Transform[] Held = Hand.GetComponentsInChildren<Transform>();

            HeldIted = null;
            Hand.transform.DetachChildren();
            return Held[1];
        }
        return null;

 
    }
    public void PlayJumpscare()
    {
        StartCoroutine(PlayJumpscareEnumerator());
    }
    private IEnumerator PlayJumpscareEnumerator()
    {
        yield return new WaitForSecondsRealtime(0.02f);
        Dead = true;
        JumpscareObject.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        JumpscareObject.SetActive(false);
        Dead = false;
        RoomManager.instance.onCaught();

    }
    void InteractionRay()
    {
        RaycastHit hit;
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (Physics.Raycast(cams.transform.position, cams.transform.forward, out hit, 100))
            {
                bool isInteractive = hit.transform.gameObject.TryGetComponent(out IInteractible LookingAt);

                if (isInteractive)
                {

                    LookingAt.Interact(this);
                }
                else
                {
                    DropHeldItem(hit.point);
                }
            }

        }

    }
}
