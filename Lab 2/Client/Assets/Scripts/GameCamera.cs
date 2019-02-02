using UnityEngine;

/// <summary>
/// Meant to move the camera around the map to show which player is moving
/// by: Michael Frye
/// </summary>
public class GameCamera : MonoBehaviour {

    static GameCamera instance;
    Camera mainGameCamera;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        mainGameCamera = gameObject.GetComponent<Camera>();
    }

    public static GameCamera GetInstance()
    {
        if(instance == null)
        {
            return null;
        }

        return instance;
    }

    public void ChangeCameraFlagToSkybox()
    {
        mainGameCamera.clearFlags = CameraClearFlags.Skybox;
    }
    
    public void SetCameraPosition(Vector3 position)
    {
        transform.rotation = new Quaternion(0f,90,0f,0f);
        transform.position = position + (Vector3.up * 3)+(Vector3.forward*6);
    }
}
