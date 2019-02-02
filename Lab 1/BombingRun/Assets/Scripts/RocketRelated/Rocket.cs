using UnityEngine;

/// <summary>
/// Take the player and launches by parenting to a rotating object
/// When it collides with an 'Untagged' object it'll leave behind an explosion, kill the player and reset
/// 
/// @Author Michael Frye
/// </summary>
public class Rocket : MonoBehaviour {

    public Transform TrajectoryObject;
    public Transform RocketCameraPosition;
    public GameObject Enterance;
    public GameObject LaunchParticles;
    public RocketLegHandler legHandler;
    public RocketExplosion ExplosionObject;

    private float horizontal;
    private float vertical;
    private float PlayerMovementSpeed;
    private char playerSelection;
    private bool beingUsed;
    private PlayerController player;
    private Vector3 OriginalLaunchPosition;

    private void Start()
    {
        OriginalLaunchPosition = transform.position;
    }

    private void Update()
    {
        if (beingUsed)
        {
            horizontal = Input.GetAxis("Horizontal_Player" + playerSelection);
            vertical = Input.GetAxis("Vertical_Player" + playerSelection);

            transform.localPosition += (transform.forward * vertical + transform.right * horizontal) / PlayerMovementSpeed;

        }
        else
        {
            return;
        }
    }

    public bool IsBeingUsed()
    {
        return beingUsed;
    }

    /// <summary>
    /// Resets the rocket object so that it's at it's proper position and rotation, ready to be used
    /// </summary>
    private void ResetRocket()
    {
        beingUsed = false;
        LaunchParticles.SetActive(false);
        transform.parent = null;
        legHandler.ResetLegs();
        transform.position = OriginalLaunchPosition;
        transform.localEulerAngles = Vector3.zero;
        Enterance.SetActive(true);
    }

    /// <summary>
    /// Enables the player object, reseting the camera position and moving the player to the position of the rocket
    /// </summary>
    public void ExitRocket()
    {
        player.gameObject.SetActive(true);
        player.transform.parent = null;
        //player.transform.localPosition = transform.localPosition;
        player.mouseLook.cameraObject.transform.parent = player.transform;
        player.mouseLook.cameraObject.transform.localPosition = Vector3.zero;
        player.mouseLook.cameraObject.transform.localEulerAngles = Vector3.zero;
        beingUsed = false;
    }

    /// <summary>
    /// Enable the explosion and unparent it and trigger it
    /// Force the player out of the rocket and kill the player
    /// Then reset the rocket
    /// </summary>
    public void DestroyRocket()
    {
        ExplosionObject.gameObject.SetActive(true);
        ExplosionObject.DecupleAndTriggerExplosion(transform);
        ExitRocket();
        player.Die();
        ResetRocket();
    }

    /// <summary>
    /// Gets playerSelection, sensitivity, change the position+rotation of the camera
    /// Also plays an animation that'll pull up the legs of the Rocket
    /// Parent itself to the always rotating TrajectoryObject to make it appear like it's flying
    /// Enable particles, disable the enterance
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player", System.StringComparison.Ordinal))
        {
            player = other.GetComponent<PlayerController>();
            playerSelection = player.playerData.playerValue;
            PlayerMovementSpeed = player.MovementSpeed;

            player.mouseLook.cameraObject.transform.parent = RocketCameraPosition;
            player.mouseLook.cameraObject.transform.position = RocketCameraPosition.position;
            player.mouseLook.cameraObject.transform.localEulerAngles = Vector3.zero;

            player.transform.parent = transform;

            player.gameObject.SetActive(false);

            legHandler.PullUp();
            transform.parent = TrajectoryObject;

            Enterance.SetActive(false);
            LaunchParticles.SetActive(true);
            beingUsed = true;
        }
    }
}
