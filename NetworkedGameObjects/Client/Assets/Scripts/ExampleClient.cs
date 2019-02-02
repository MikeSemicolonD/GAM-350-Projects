using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ExampleClient : MonoBehaviour
{
    public ClientNetwork clientNet;

    // Get the instance of the client
    static ExampleClient instance = null;

    // Are we in the process of logging into a server
    private bool loginInProcess = true;

    public GameObject loginScreen;

    public List<GameObject> myPlayers;

    public Text serverStatusText;

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

    public void UpdateColor(float r, float g, float b, int id)
    {
        foreach(GameObject player in myPlayers)
        {
            player.GetComponent<Player>().SetColor(r, g, b, id);
        }
    }

    void Update()
    {
        if (!loginInProcess && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                if (hit.transform.tag == "Player")
                {
                    hit.transform.GetComponent<Player>().ChangeColor();
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && clientNet.IsConnected())
        {
            clientNet.Disconnect("Peace out");
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void SetColor(float r, float g, float b)
    {

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

        //Tell server that this given player is ready
        //clientNet.CallRPC("PlayerIsReady", UCNetwork.MessageReceiver.ServerOnly, -1);
    }

    void OnNetStatusDisconnecting()
    {
        Debug.Log("OnNetStatusDisconnecting called");

        if (myPlayers.Count > 0)
        {
            foreach(GameObject player in myPlayers)
            {
                clientNet.Destroy(player.GetComponent<NetworkSync>().GetId());
            }
        }
    }

    void OnNetStatusDisconnected()
    {
        Debug.Log("OnNetStatusDisconnected called");
        SceneManager.LoadScene("Client");
        
        loginInProcess = false;

        if (myPlayers.Count > 0)
        {
            foreach (GameObject player in myPlayers)
            {
                clientNet.Destroy(player.GetComponent<NetworkSync>().GetId());
            }
        }
    }

    public void OnChangeArea()
    {
        Debug.Log("OnChangeArea called");

        float RandomXValue;
        float RandomZValue;

        for (int i = 0; i < 4; i++)
        {
            RandomXValue = Random.Range(-5f, 5.1f);
            RandomZValue = Random.Range(-5f, 5.1f);
            myPlayers.Add(clientNet.Instantiate("Player", new Vector3(RandomXValue,0,RandomZValue), Quaternion.identity));
            myPlayers[i].GetComponent<Player>().SetPlayerIndex(i);
        }

        foreach (GameObject player in myPlayers)
        {
            player.GetComponent<NetworkSync>().AddToArea(1);
        }
    }

    // RPC Called by the server once it has finished sending all area initization data for a new area
    public void AreaInitialized()
    {
        Debug.Log("AreaInitialized called");
    }
    
    void OnDestroy()
    {
        if (myPlayers.Count > 0)
        {
            foreach (GameObject player in myPlayers)
            {
                clientNet.Destroy(player.GetComponent<NetworkSync>().GetId());
            }
        }
        if (clientNet.IsConnected())
        {
            clientNet.Disconnect("Peace out");
        }
    }
}


