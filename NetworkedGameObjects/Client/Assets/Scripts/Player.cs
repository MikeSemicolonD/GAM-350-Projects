using UnityEngine;

public class Player : MonoBehaviour {

    public float speed = 1;
    //public float itSpeed = 6;
    //bool isIt;
    NetworkSync networkSyncObj;
    ColorChange colorChanger = new ColorChange();
    int playerIndex;

    private void Start()
    {
        networkSyncObj = GetComponent<NetworkSync>();
        colorChanger.PassMeshRenderer(GetComponent<MeshRenderer>());
    }

    void Update()
    {
        if (networkSyncObj.owned)
        {
            //if (isIt)
            //{
                transform.position += new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * speed;
            //}
            //else
            //{
            //    transform.position += new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * itSpeed;
            //}
        }
    }

    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
    }

    public void SetColor(float r, float g, float b, float id)
    {
        if((int)id == networkSyncObj.GetId())
        {
            colorChanger.AssignColor(r, g, b);
        }
    }

    public void ChangeColor()
    {
        float r = Random.Range(0, 1.1f), g = Random.Range(0, 1.1f), b = Random.Range(0, 1.1f);
        colorChanger.AssignColor(r, g, b);
        networkSyncObj.CallRPC("SendColorChangeInfo", UCNetwork.MessageReceiver.ServerOnly, r, g, b);
    }

    //public void IsNowIt(float r, float g, float b)
    //{
    //    colorChanger.AssignColor(r, g, b);
    //    isIt = true;
    //}
}
