using UnityEngine;
using System;

/// <summary>
/// FPS Mouselook that handle the camera seperately by it's X axis and this transform by it's Y axis
/// 
/// @Author Michael Frye
/// </summary>
[Serializable]
public class PlayerMouseLook
{
    public GameObject cameraObject;

    public float minimumY = -60F;
    public float maximumY = 60F;

    private char playerSelection = '0';

    private float yRot, xRot;
    private float xInput, yInput;

    public void SetPlayerValue (char val)
    {
        playerSelection = val;
    }

    /// <summary>
    /// Moves the base transform around by it's Y Axis while also moving the camera transform by it's X axis
    /// Gets called by the PlayerController
    /// </summary>
    /// <param name="baseTransform"></param>
    /// <param name="XSensitivity"></param>
    /// <param name="YSensitivity"></param>
    /// <returns></returns>
    public Vector3 GetMouseInput(Transform baseTransform, float XSensitivity, float YSensitivity)
    {
        xInput = Input.GetAxis("Mouse X_Player" + playerSelection);
        yInput = Input.GetAxis("Mouse Y_Player" + playerSelection);

        xRot = baseTransform.localEulerAngles.y + xInput * XSensitivity;

        yRot += yInput * YSensitivity;
        yRot = Mathf.Clamp(yRot, minimumY, maximumY);

        cameraObject.transform.localEulerAngles = new Vector3(-yRot, 0, 0);

        return new Vector3(0, xRot, 0);
    }
}
