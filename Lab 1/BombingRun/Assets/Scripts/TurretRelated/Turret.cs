using UnityEngine;

/// <summary>
/// Is meant to take the players control after the player has entered via OnTrigger.
/// Turret contain 2 TurretGun's that the object will switch and fire between
/// Will kill the player using this object if it's hit by a rocket explosion.
/// 
/// @Author Michael Frye
/// </summary>
public class Turret : MonoBehaviour {

    public Transform TurretHead;
    public Transform TurretCameraPosition;
    public TurretGun[] turretGuns = new TurretGun[2];
    private float xInput;
    private float yInput;
    private float xRot;
    private float yRot;
    private float minimumY = 0f;
    private float maximumY = 90f;
    private float YSensitivity;
    private float XSensitivity;
    private char playerSelection;
    private PlayerController player;
    private bool beingUsed;
    private bool useLeftGun;

    private void Update()
    {
        //If we're being used currently then we should accept player input
        if (beingUsed)
        {
            if(Input.GetAxisRaw("Fire_Player" + playerSelection) > 0 || Input.GetButtonDown("Fire_Player" + playerSelection))
            {
                //If true, fire the gun on the left, else fire the gun on the right.
                if (useLeftGun)
                {
                    if (!turretGuns[1].AnimationComponentIsPlaying())
                    {
                        turretGuns[0].PlayFireAnimation();
                        turretGuns[0].Fire();
                        useLeftGun = false;
                    }
                }
                else
                {
                    if (!turretGuns[0].AnimationComponentIsPlaying())
                    {
                        turretGuns[1].PlayFireAnimation();
                        turretGuns[1].Fire();
                        useLeftGun = true;
                    }
                }
            }
            else if (Input.GetButtonDown("Cancel_Player" + playerSelection))
            {
                ExitTurret();
            }

            xInput = Input.GetAxis("Mouse X_Player" + playerSelection);
            yInput = Input.GetAxis("Mouse Y_Player" + playerSelection);

            xRot = TurretHead.localEulerAngles.y + xInput * XSensitivity;

            yRot += yInput * YSensitivity;
            yRot = Mathf.Clamp(yRot, minimumY, maximumY);

            TurretHead.localEulerAngles = new Vector3(-yRot, xRot, 0);
        }
        else
        {
            return;
        }
    }

    /// <summary>
    /// Gets called by RocketExplosion.cs
    /// </summary>
    public void KillPlayer()
    {
        ExitTurret();
        player.Die();
    }

    /// <summary>
    /// Enable the player, move the player away from the turret and reset the player's camera position
    /// </summary>
    public void ExitTurret()
    {
        player.gameObject.SetActive(true);
        player.transform.localPosition += transform.localPosition/100;
        player.mouseLook.cameraObject.transform.parent = player.transform;
        player.mouseLook.cameraObject.transform.localPosition = Vector3.zero;
        player.mouseLook.cameraObject.transform.localEulerAngles = Vector3.zero;
        beingUsed = false;
    }

    /// <summary>
    /// Get playerSelection, sensitivity, and change the position+rotation
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if(!beingUsed && other.tag.Equals("Player",System.StringComparison.Ordinal))
        {
            player = other.GetComponent<PlayerController>();
            playerSelection = player.playerData.playerValue;
            YSensitivity = player.LookYSensitivity;
            XSensitivity = player.LookXSensitivity;
            player.mouseLook.cameraObject.transform.parent = TurretCameraPosition;
            player.mouseLook.cameraObject.transform.position = TurretCameraPosition.position;
            player.mouseLook.cameraObject.transform.localEulerAngles = Vector3.zero;
            player.gameObject.SetActive(false);
            beingUsed = true;
        }
    }

    public bool BeingUsed()
    {
        return beingUsed;
    }

}
