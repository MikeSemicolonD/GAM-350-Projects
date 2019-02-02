using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleClient : MonoBehaviour
{
    public ClientNetwork clientNet;

    // Get the instance of the client
    static ExampleClient instance = null;

    // Are we in the process of logging into a server
    private bool loginInProcess = true;

    public GameObject loginScreen;

    public GameObject myPlayer;

    bool waitOnPlayerSpawn;

    // Singleton support
    public static ExampleClient GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("ExampleClient is uninitialized");
            return null;
        }
        return instance;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        // Make sure we have a ClientNetwork to use
        if (clientNet == null)
        {
            clientNet = GetComponent<ClientNetwork>();
        }
        if (clientNet == null)
        {
            clientNet = (ClientNetwork)gameObject.AddComponent(typeof(ClientNetwork));
        }
    }
    
    // Start the process to login to a server
    public void ConnectToServer(string aServerAddress, int aPort)
    {
        if (loginInProcess)
        {

            loginInProcess = false;
        }
        else
        {
            return;
        }

        ClientNetwork.port = aPort;
        clientNet.Connect(aServerAddress, ClientNetwork.port, "", "", "", 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && clientNet.IsConnected())
        {
            clientNet.Disconnect("Peace out");
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        //If the player has spawned and the server is told this client that it's player is it, do it.
        else if (waitOnPlayerSpawn && myPlayer != null)
        {
            myPlayer.GetComponent<Player>().PlayerIsIt();
            waitOnPlayerSpawn = false;
        }
        else
            return;
    }

    public void NewClientConnected(long aClientId, string aValue)
    {
        Debug.Log("RPC NewClientConnected has been called with " + aClientId + " " + aValue);
    }

    // Networking callbacks
    // These are all the callbacks from the ClientNetwork
    void OnNetStatusNone()
    {
        Debug.Log("OnNetStatusNone called");
    }

    void OnNetStatusInitiatedConnect()
    {
        Debug.Log("OnNetStatusInitiatedConnect called");
    }

    void OnNetStatusReceivedInitiation()
    {
        Debug.Log("OnNetStatusReceivedInitiation called");
    }

    void OnNetStatusRespondedAwaitingApproval()
    {
        Debug.Log("OnNetStatusRespondedAwaitingApproval called");
    }

    void OnNetStatusRespondedConnect()
    {
        Debug.Log("OnNetStatusRespondedConnect called");
    }

    void OnNetStatusConnected()
    {
        loginScreen.SetActive(false);

        Debug.Log("OnNetStatusConnected called");

        clientNet.AddToArea(1);
    }

    void OnNetStatusDisconnecting()
    {
        Debug.Log("OnNetStatusDisconnecting called");

        clientNet.Destroy(myPlayer.GetComponent<NetworkSync>().GetId());
    }

    void OnNetStatusDisconnected()
    {
        Debug.Log("OnNetStatusDisconnected called");
        SceneManager.LoadScene("Client");
        
        loginInProcess = false;

        clientNet.Destroy(myPlayer.GetComponent<NetworkSync>().GetId());
    }

    public void OnChangeArea()
    {
        Debug.Log("OnChangeArea called");
        myPlayer = clientNet.Instantiate("Player", new Vector3(Random.Range(-4f, 4.1f), 0, Random.Range(-4f, 4.1f)), Quaternion.identity);
        myPlayer.GetComponent<NetworkSync>().AddToArea(1);
    }

    // RPC Called by the server once it has finished sending all area initization data for a new area
    public void AreaInitialized()
    {
        Debug.Log("AreaInitialized called");
    }

    //If the Server told the server that a specific player is it but the net obj hasn't spawned yet, set a flag and do the operation later.
    public void PlayerIsIt()
    {
        if(myPlayer == null)
        {
            waitOnPlayerSpawn = true;
            return;
        }
        myPlayer.GetComponent<Player>().PlayerIsIt();
    }
    
    void OnDestroy()
    {
        clientNet.Destroy(myPlayer.GetComponent<NetworkSync>().GetId());

        if (clientNet.IsConnected())
        {
            clientNet.Disconnect("Peace out");
        }
    }
}


