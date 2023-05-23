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
     [SerializeField] Camera camera;

    Quaternion camcenter;
    #endregion
    void Update()
    {

        setY();
        setX();
        if (!cursorlockcooldown && Input.GetKeyDown(KeyCode.Escape)) StartCoroutine(UpdateCursorLock());


    }
    public void UpdateCursorLockFromAnOutsideScript()
    {
        StartCoroutine(UpdateCursorLock());
    }
    void setY()
    {
        float t_input = Input.GetAxis("Mouse Y") * ysens * Time.deltaTime;
        Quaternion t_adjust = Quaternion.AngleAxis(t_input,-Vector3.right);
        Quaternion t_delta = cams.localRotation* t_adjust;

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
       if(CursorLocked)
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
}
