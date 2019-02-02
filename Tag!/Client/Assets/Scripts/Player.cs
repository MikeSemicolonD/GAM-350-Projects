using UnityEngine;

public class Player : MonoBehaviour {

    public float speed = 1;
    public float itSpeed = 1;
    bool isIt = false;
    NetworkSync networkSyncObj;
    ColorChange colorChanger = new ColorChange();
    Rigidbody baseRigid;

    private void Start()
    {
        baseRigid = GetComponent<Rigidbody>();
        networkSyncObj = GetComponent<NetworkSync>();
        colorChanger.PassMeshRenderer(GetComponent<MeshRenderer>());

        //Tell the server what netID I am. Server will keep track of which netId is the actual player
        networkSyncObj.CallRPC("PassId", UCNetwork.MessageReceiver.ServerOnly);

        //Ask the server if THIS object with THIS network id is 'it'
        //Server will call an RPC to THIS network id, making it 'it'
        networkSyncObj.CallRPC("AmIIt", UCNetwork.MessageReceiver.ServerOnly);

    }

    void Update()
    {
        if (networkSyncObj.owned)
        {
            if (isIt)
            {
                baseRigid.MovePosition(transform.position += new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * speed * Time.deltaTime);
            }
            else
            {
                baseRigid.MovePosition(transform.position += new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * itSpeed * Time.deltaTime);
            }
        }
    }

    //Marks the player as it by making it red
    //Will also randomly move the 'it' player somewhere on the left or right side of the map
    public void PlayerIsIt()
    {
        if (!isIt)
        {
            colorChanger.AssignColor(1, 0, 0);

            if (Random.Range(0, 2) == 0)
            {
                transform.position = new Vector3(Random.Range(-10, -6), 0, Random.Range(-10, 10));
            }
            else
            {
                transform.position = new Vector3(Random.Range(6, 10), 0, Random.Range(-10, 10));
            }

            isIt = true;
        }
        else
        {
            colorChanger.AssignColor(1, 1, 1);
            isIt = false;
        }
    }
}
