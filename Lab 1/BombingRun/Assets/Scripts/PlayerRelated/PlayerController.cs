using UnityEngine;

/// <summary>
/// Handles all scripts and values that are required to make the player move, die and respawn
/// 
/// @Author Michael Frye
/// </summary>
public class PlayerController : MonoBehaviour {

    public float respawnTime = 3f;
    public float MovementSpeed = 1f;
    public float LookXSensitivity = 2f, LookYSensitivity = 2f;
    public Camera PlayerCamera;
    public PlayerMouseLook mouseLook;
    public PlayerData playerData;
    private float runtimeRespawnTime;
    private PlayerMovementInput playerInput = new PlayerMovementInput();
    private Rigidbody baseRigibody;
    private bool isAlive = true;

    private void Awake()
    {
        baseRigibody = GetComponent<Rigidbody>();

        playerInput.SetPlayerValue(playerData.playerValue);
        mouseLook.SetPlayerValue(playerData.playerValue);
        runtimeRespawnTime = respawnTime;
    }

    private void Update()
    {
        //Continuously get input for as long as you're alive
        if (isAlive)
        {
            baseRigibody.velocity = (playerInput.GetMovementInput(transform, 1, 1) * MovementSpeed);
            transform.localEulerAngles = mouseLook.GetMouseInput(transform, LookXSensitivity, LookYSensitivity);
        }
        else
        {
            //Wait a few seconds then respawn
            if(runtimeRespawnTime <= 0)
            {
                RespawnObject();
                runtimeRespawnTime = respawnTime;
            }
            else
            {
                runtimeRespawnTime -= Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Reparents the camera back onto the player object
    /// Resets the player's rotation+position
    /// Rigidbody rotation is locked
    /// GameManager is called to "respawn" this object
    /// </summary>
    public void RespawnObject()
    {
        baseRigibody.constraints = RigidbodyConstraints.FreezeRotation;
        transform.localEulerAngles = Vector3.zero;
        PlayerCamera.transform.parent = transform;
        PlayerCamera.transform.localPosition = new Vector3(0, 0.725f, 0);
        PlayerCamera.transform.localEulerAngles = Vector3.zero;
        GameManager.instance.RespawnPlayerObject(transform, playerData.InTeam1);
        isAlive = true;
    }

    /// <summary>
    /// Displaces the player away from the camera
    /// Rigidbody is allowed to roll free
    /// </summary>
    public void Die()
    {
        isAlive = false;
        transform.parent = PlayerCamera.transform;
        transform.localPosition = new Vector3(0, transform.localPosition.y, 5f);
        baseRigibody.constraints = RigidbodyConstraints.None;
    }

    /// <summary>
    /// Accepts the PlayerData and assigns values on specific scripts using the data
    /// </summary>
    /// <param name="Data"></param>
    /// <param name="newCameraRect"></param>
    public void PassPlayerData(PlayerData Data, Rect newCameraRect)
    {
        //Assign player data, camera rect
        playerData = Data;
        playerInput.SetPlayerValue(playerData.playerValue);
        mouseLook.SetPlayerValue(playerData.playerValue);
        PlayerCamera.rect = newCameraRect;
    }
}
