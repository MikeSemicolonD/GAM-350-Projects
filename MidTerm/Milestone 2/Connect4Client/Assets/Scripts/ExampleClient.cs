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

    // 1 = empty
    // 2 = Yellow
    // 3 = Red
    public int assignedPieceType = 1;

    public GameObject mainGameUI;

    public Text serverStatusText;

    public Text chatLog;

    public Text chatEntryBox;

    //This gets set by the server
    public string SetName = "NoNameSet";

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

    public int GetAssignedPiece()
    {
        return assignedPieceType;
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
    public void DeclareWinner(int pieceID)
    {
        Connect4Board.GetInstance().SetWinner(pieceID);
    }

    //Updates the name on the client
    public void UpdateName(string name)
    {
        Debug.Log("Name set to " + name);
        SetName = name;
    }

    //Sends the message that was typed on the clients side
    //This is sent to the server
    public void SendChatMessage(string message)
    {
        if (message != "" || message != " ")
        {
            Debug.Log("Sending chat message...");
            //Send message to server
            clientNet.CallRPC("TransmitMessage", UCNetwork.MessageReceiver.ServerOnly, -1, message, SetName);
        }
    }

    //Updates the client's chat log
    //Is called by the server
    public void UpdateChatLog(string name, string message)
    {
        Debug.Log("Updating chat log...");
        chatLog.text += name+':'+ message + '\n';
        chatLog.rectTransform.position += Vector3.up * 25;
        chatLog.rectTransform.sizeDelta += Vector2.up * 25;
    }

    //Updates text on the client
    //Is called by the server
    public void UpdateServerStatus(string message)
    {
        Debug.Log("Updating server status text...");
        serverStatusText.text = message;
    }

    //Gets called from the server, which sends the pieceType to the board
    //The board will enable buttons if the client has the same type as what's being passed in
    public void StartTurn(int pieceType)
    {
        if (pieceType == assignedPieceType)
        {
            Debug.Log("Starting turn for player with type " + pieceType);
            Connect4Board.GetInstance().StartTurn(pieceType);
        }
    }

    //Gets called from the server, assigns the piece that this client will be for the game
    public void SetPiece(int type)
    {
        Debug.Log("Piece set to "+type);
        assignedPieceType = type;
        Connect4Board.GetInstance().SetBoardColor(type);
    }

    //Sets a specific piece on the board to the given type
    public void SetBoardState(int y, int x, int type)
    {
        if (type == 2 || type == 3)
        {
            Debug.Log("Board is being set at x = " + x + ", y = " + y + " with type = " + type);
            Connect4Board.GetInstance().SetBoardPiece(x, y, type);
        }
    }

    //Gets called from the connect4board itself, meant to update the server on what peice is being placed and where
    //This would also update the other clients on what was placed on the board
    public void SendBoardStateData(int row, int column, int type)
    {
        Debug.Log("Sending Board Data...");
        clientNet.CallRPC("SetBoardState", UCNetwork.MessageReceiver.ServerOnly, -1, row, column, type);
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

        //Enable connect4board
        gameBoard.SetActive(true);

        //Tell server that this given player is ready
        clientNet.CallRPC("PlayerIsReady", UCNetwork.MessageReceiver.ServerOnly, -1);
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

    public void OnChangeArea()
    {
        Debug.Log("OnChangeArea called");

        // Tell the server we are ready
        myPlayer = clientNet.Instantiate("Player", Vector3.zero, Quaternion.identity);
        myPlayer.GetComponent<NetworkSync>().AddToArea(1);
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


