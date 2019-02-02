using UnityEngine;

public class Player : MonoBehaviour {

    public float speed = 1;
    public Material normalMat;
    public Material TransparentMat;
    BoxCollider MouseZone;
    Vector3 mousePos;
    NetworkSync networkSyncObj;
    MeshRenderer meshRend;

    private void Awake()
    {
        meshRend = GetComponent<MeshRenderer>();
        networkSyncObj = GetComponent<NetworkSync>();
    }

    private void Start()
    {
        if(Application.isEditor)
        {
            speed /= 4f;
        }

        MouseZone = GameObject.FindGameObjectWithTag("MouseZone").GetComponent<BoxCollider>();
        networkSyncObj.CallRPC("PassID", UCNetwork.MessageReceiver.ServerOnly);
    }

    /// <summary>
    /// Gets called by the server
    /// Visually represents who's turn it is.
    /// Server will pay attention to who's turn it is
    /// </summary>
    /// <param name="id"></param>
    public void PassTurn(int id)
    {
        if(id == networkSyncObj.GetId())
        {
            meshRend.material = normalMat;
            //Debug.Log("Normal Mat selected");
        }
        else
        {
            meshRend.material = TransparentMat;
            //Debug.Log("Transparent Mat selected");
        }
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && networkSyncObj.owned)
        {
            mousePos = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0) * speed * Time.deltaTime;

            //We don't want to move unless we're inside the MouseZone's bounds
            if (MouseZone.bounds.Contains(transform.position + mousePos))
            {
                transform.position += mousePos;
            }
        }
    }
}
