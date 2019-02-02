using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ExampleClient : MonoBehaviour
{
    public ClientNetwork clientNet;

    // Get the instance of the client
    static ExampleClient instance = null;

    // Are we in the process of logging into a server
    private bool loginInProcess = false;

    public GameObject loginScreen;

    public GameObject gameBoard;

    public GameObject myPlayer;
    
    // 2 = Blue
    // 3 = Red
    public int assignedPieceType = 1;

    public GameObject mainGameUI;

    public Text ScoreText;

    public Text serverStatusText;

    public Text chatLog;

    public Text chatEntryBox;

    //This gets set by the server
    public string PlayerName = "NoNameSet";

    private GameObject ballInstance;

    private Camera mainCamera;

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

        mainCamera = Camera.main;
    }

    /// <summary>
    /// Gets called by the Ball object itself, shows the active score in the game
    /// </summary>
    /// <param name="value"></param>
    public void UpdateScore(int value)
    {
        ScoreText.text = value.ToString();
    }

    // Start the process to login to a server
    public void ConnectToServer(string aServerAddress, int aPort)
    {
        if (loginInProcess)
        {
            return;
        }
        loginInProcess = true;

        ClientNetwork.port = aPort;
        clientNet.Connect(aServerAddress, ClientNetwork.port, "", "", "", 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            clientNet.Disconnect("Peace out");

            loginScreen.SetActive(true);

            mainGameUI.SetActive(false);

            gameBoard.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && !gameBoard.activeInHierarchy)
        {
            Application.Quit();
        }
    }
   
    /// <summary>
    /// Gets called from the server, which enables particles to show who's the winner
    /// </summary>
    /// <param name="pieceID"></param>
    public void DeclareWinner(int pieceID, int bounces)
    {
        if(pieceID == assignedPieceType)
        {
            UpdateServerStatus("You lost!");
        }
        else
        {
            UpdateServerStatus("You won!");

            //Randomize which colored particles show up because color doesn't matter
            GameBoard.GetInstance().SetWinner(Random.Range(1,3));
            PlayerPrefs.SetInt("NumberOfPasses", bounces);
        }
    }

    /// <summary>
    /// Gets called by the server to create a ball object. Server randomly decides which client to spawn it
    /// </summary>
    public void SpawnBall()
    {
        if (GameObject.FindGameObjectWithTag("Ball") == null)
        {
            Debug.Log("I spawned the ball!");
            ballInstance = clientNet.Instantiate("Ball", new Vector3(0, 2.5f, 9.65f), Quaternion.identity);
        }
    }

    /// <summary>
    /// Updates the name on the client, gets called by the server
    /// </summary>
    /// <param name="name"></param>
    public void UpdateName(string name)
    {
        Debug.Log("Name set to " + name);
        PlayerName = name;
    }

    /// <summary>
    /// Sends the message that was typed on the clients side to the server
    /// </summary>
    /// <param name="message"></param>
    public void SendChatMessage(string message)
    {
        if (message != "" || message != " ")
        {
            Debug.Log("Sending chat message...");
            //Send message to server
            clientNet.CallRPC("TransmitMessage", UCNetwork.MessageReceiver.ServerOnly, -1, message, PlayerName);
        }
    }

    /// <summary>
    /// Updates the client's chat log gets called by the server
    /// </summary>
    /// <param name="name"></param>
    /// <param name="message"></param>
    public void UpdateChatLog(string name, string message)
    {
        Debug.Log("Updating chat log...");
        chatLog.text += name+':'+ message + '\n';
        chatLog.rectTransform.position += Vector3.up * 25;
        chatLog.rectTransform.sizeDelta += Vector2.up * 25;
    }

    /// <summary>
    /// Updates text on the client gets called by the server
    /// Useful for displaying announcements from the server
    /// </summary>
    /// <param name="message"></param>
    public void UpdateServerStatus(string message)
    {
        Debug.Log("Updating server status text...");
        serverStatusText.text = message;
    }

    /// <summary>
    /// Gets called from the server, assigns the piece that this client will be for the game
    /// </summary>
    /// <param name="type"></param>
    public void SetPiece(int type)
    {
        Debug.Log("Piece set to "+type);
        assignedPieceType = type;
        //Connect4Board.GetInstance().SetBoardColor(type);
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

        mainGameUI.SetActive(true);

        Debug.Log("OnNetStatusConnected called");

        clientNet.AddToArea(1);
        
        gameBoard.SetActive(true);

    }

    void OnNetStatusDisconnecting()
    {
        Debug.Log("OnNetStatusDisconnecting called");

        if (myPlayer)
        {
            clientNet.Destroy(myPlayer.GetComponent<NetworkSync>().GetId());
        }
    }

    void OnNetStatusDisconnected()
    {
        Debug.Log("OnNetStatusDisconnected called");
        SceneManager.LoadScene("Client");
        
        loginInProcess = false;

        if (myPlayer)
        {
            clientNet.Destroy(myPlayer.GetComponent<NetworkSync>().GetId());
        }
    }

    /// <summary>
    /// Gets called from the server to reverse the ball when a player hits it
    /// </summary>
    public void ReverseBall()
    {
        ballInstance.GetComponent<Ball>().Reverse();
    }

    public void OnChangeArea()
    {
        Debug.Log("OnChangeArea called");

        // Tell the server we are ready
        myPlayer = clientNet.Instantiate("Player", new Vector3(Random.Range(-1, 1), Random.Range(-5,-4),9.65f), Quaternion.Euler(0f,0f,-90f));
        myPlayer.GetComponent<NetworkSync>().AddToArea(1);
        //Tell server that this given player is ready
        clientNet.CallRPC("PlayerIsReady", UCNetwork.MessageReceiver.ServerOnly, -1);
    }

    // RPC Called by the server once it has finished sending all area initization data for a new area
    public void AreaInitialized()
    {
        Debug.Log("AreaInitialized called");
    }
    
    void OnDestroy()
    {
        if (myPlayer)
        {
            clientNet.Destroy(myPlayer.GetComponent<NetworkSync>().GetId());
        }
        if (clientNet.IsConnected())
        {
            clientNet.Disconnect("Peace out");
        }
    }
}


