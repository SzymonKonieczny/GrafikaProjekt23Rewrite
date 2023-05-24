using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Look : MonoBehaviour
{
    #region Variables
    public Transform Player;
    public Transform cams;
    public Transform weapon;
    public float xsens;
    public float ysens;
    public float maxAngle;
    public static bool CursorLocked = true;
    bool cursorlockcooldown;
    [SerializeField] LayerMask RayMask;
    [SerializeField] Camera camera;
    public Transform Hand;
    Quaternion camcenter;
    #endregion
    void Update()
    {

        setY();
        setX();
        if (!cursorlockcooldown && Input.GetKeyDown(KeyCode.Escape)) StartCoroutine(UpdateCursorLock());
        InteractionRay();

    }
    public void UpdateCursorLockFromAnOutsideScript()
    {
        StartCoroutine(UpdateCursorLock());
    }
    void setY()
    {
        float t_input = Input.GetAxis("Mouse Y") * ysens * Time.deltaTime;
        Quaternion t_adjust = Quaternion.AngleAxis(t_input, -Vector3.right);
        Quaternion t_delta = cams.localRotation * t_adjust;

        if (Quaternion.Angle(camcenter, t_delta) < maxAngle)
        {
            cams.localRotation = t_delta;
            //weapon.localRotation = t_delta;
        }


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
         
        if(ToHold.TryGetComponent(out Rigidbody rb))
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
    void InteractionRay()
    {
        RaycastHit hit;
        bool AlreadyInteracted = false;
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (Physics.Raycast(cams.transform.position, cams.transform.forward, out hit, 100))
            {
               bool isInteractive = hit.transform.gameObject.TryGetComponent ( out IInteractible LookingAt);

                if (isInteractive)
                {

                    LookingAt.Interact(this, gameObject.GetComponent<MovementScript>());
                    Debug.Log("Interacting");
                    AlreadyInteracted = true;
                }
                else
                {
                    DropHeldItem(hit.point);
                }
            }
           
        }
            //if (!AlreadyInteracted)DropHeldItem(transform);

    }
}
