using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ExampleClient : MonoBehaviour
{
    public ClientNetwork clientNet;

    // Get the instance of the client
    static ExampleClient instance = null;
    
    private bool loginInProcess = false, HasATurn, IsAlive = true, ClientIsReady, GameTimerStarted, GameStarted;

    public GameObject loginScreen;

    public GameObject gameScreen;

    public Text readyText;

    public Text timerText;

    public Text chatText;

    public MapCreator mapCreator;

    int XPosition, YPosition;

    int assignedPlayerID;
    int assignedTeam;

    float timeTillgameStart, timeTillTurnPass;

    Dictionary<int,Player> activeClientPlayers = new Dictionary<int, Player>();

    public Player basePlayerOBJ;


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
            return;
        }
        loginInProcess = true;
        
        ClientNetwork.port = aPort;
        clientNet.Connect(aServerAddress, ClientNetwork.port, "", "", "", 0);
    }
    
    void Update()
    {
        if (clientNet.IsConnected())
        {
            if (!GameStarted && Input.GetKeyDown(KeyCode.Space))
            {
                if (!ClientIsReady)
                {
                    clientNet.CallRPC("Ready", UCNetwork.MessageReceiver.ServerOnly, -1, true);
                }
                else
                {
                    clientNet.CallRPC("Ready", UCNetwork.MessageReceiver.ServerOnly, -1, false);
                }
            }
            else if (ClientIsReady)
            {
                if (GameStarted)
                {
                    if (HasATurn)
                    {
                        if (IsAlive)
                        {
                            if (Input.GetKeyDown(KeyCode.Escape))
                            {
                                ForceLeave();
                            }
                            //Forward (+Z axis)
                            else if (Input.GetKeyDown(KeyCode.S))
                            {
                                clientNet.CallRPC("RequestMove", UCNetwork.MessageReceiver.ServerOnly, -1, basePlayerOBJ.XPosition, (basePlayerOBJ.YPosition + 1));
                            }
                            //Backward (-Z axis)
                            else if (Input.GetKeyDown(KeyCode.W))
                            {
                                clientNet.CallRPC("RequestMove", UCNetwork.MessageReceiver.ServerOnly, -1, basePlayerOBJ.XPosition, (basePlayerOBJ.YPosition - 1));
                            }
                            //Left (-X axis)
                            else if (Input.GetKeyDown(KeyCode.D))
                            {
                                clientNet.CallRPC("RequestMove", UCNetwork.MessageReceiver.ServerOnly, -1, (basePlayerOBJ.XPosition - 1), basePlayerOBJ.YPosition);
                            }
                            //Right (+X axis)
                            else if (Input.GetKeyDown(KeyCode.A))
                            {
                                clientNet.CallRPC("RequestMove", UCNetwork.MessageReceiver.ServerOnly, -1, (basePlayerOBJ.XPosition + 1), basePlayerOBJ.YPosition);
                            }
                            //Forward Attack (+Z axis)
                            else if (Input.GetKeyDown(KeyCode.DownArrow))
                            {
                                clientNet.CallRPC("RequestAttack", UCNetwork.MessageReceiver.ServerOnly, -1, basePlayerOBJ.XPosition, (basePlayerOBJ.YPosition + 1));
                            }
                            //Backward Attack (-Z axis)
                            else if (Input.GetKeyDown(KeyCode.UpArrow))
                            {
                                clientNet.CallRPC("RequestAttack", UCNetwork.MessageReceiver.ServerOnly, -1, basePlayerOBJ.XPosition, (basePlayerOBJ.YPosition - 1));
                            }
                            //Left Attack (-X axis)
                            else if (Input.GetKeyDown(KeyCode.RightArrow))
                            {
                                clientNet.CallRPC("RequestAttack", UCNetwork.MessageReceiver.ServerOnly, -1, (basePlayerOBJ.XPosition - 1), basePlayerOBJ.YPosition);
                            }
                            //Right Attack (+X axis)
                            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                            {
                                clientNet.CallRPC("RequestAttack", UCNetwork.MessageReceiver.ServerOnly, -1, (basePlayerOBJ.XPosition + 1), basePlayerOBJ.YPosition);
                            }
                            else if (Input.GetKeyDown(KeyCode.Space))
                            {
                                CallPassTurnRequest();
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            CallPassTurnRequest();
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (GameTimerStarted)
                    {
                        if (timeTillgameStart <= 0)
                        {
                            UpdateTimerUI(0,true);
                            GameTimerStarted = false;
                            GameStarted = true;
                            gameScreen.SetActive(true);
                            readyText.text = "";
                            GameCamera.GetInstance().ChangeCameraFlagToSkybox();
                        }
                        else
                        {
                            timeTillgameStart -= Time.unscaledDeltaTime;
                            UpdateTimerUI(Mathf.Round(timeTillgameStart));
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            
        } 
        else
        {
            return;
        }
        /*
        timeToSend -= Time.deltaTime;
        if (timeToSend <= 0)
        {
            clientNet.CallRPC("RequestMove", UCNetwork.MessageReceiver.ServerOnly, -1, 1, 1, "x");
            clientNet.CallRPC("Blah", UCNetwork.MessageReceiver.ServerOnly, -1, 1, 1, "x");
            timeToSend = 10;
        }
        */
    }

    public void OnDestroyNetworkObject()
    {

    }

    public void CallPassTurnRequest()
    { 
        HasATurn = false;
        clientNet.CallRPC("PassTurn", UCNetwork.MessageReceiver.ServerOnly, -1);
    }

    public void UpdateState(int x, int y, string player)
    {
        // Update the visuals for the game
    }

    //Messages from the server to the client:
    //Login Phase:

    // Called to tell your client what your player id will be for the session
    public void SetPlayerId(int playerId)
    {
        assignedPlayerID = playerId;
        basePlayerOBJ.GetComponent<Player>().SetNetworkID(playerId);
    }

    //Team will be 1 or 2
    public void SetTeam(int team)
    {
        assignedTeam = team;
        basePlayerOBJ.GetComponent<Player>().SetTeam(team);
    }

    public void UpdateTimerUI(float time, bool emptyTheText = false)
    {
        if (!emptyTheText)
        {
            timerText.text = time.ToString();
        }
        else
        {
            timerText.text = "";
        }
    }

    //Another player has connected to the game
    public void NewPlayerConnected(int playerId, int team)
    {
        if (playerId == assignedPlayerID)
        {
            basePlayerOBJ.SetTeam(team);
        }
        else
        {
            GameObject newPlayer = clientNet.Instantiate("Player", Vector3.zero, Quaternion.identity);
            //newPlayer.GetComponent<Player>().SetTeam(team);
            newPlayer.GetComponent<Player>().SetNetworkIDAndTeam(playerId, team);
            //activeClientPlayers[playerId].SetName(playerName);
            activeClientPlayers.Add(playerId, newPlayer.GetComponent<Player>());
        }
    }

    //Another player has changed their name
    public void PlayerNameChanged(int PlayerID, string playerName)
    {
        if (PlayerID == assignedPlayerID)
        {
            basePlayerOBJ.SetName(playerName);
        }
        else
        {
            activeClientPlayers[PlayerID].SetName(playerName);
        }
    }

    //Another player is ready to play
    public void PlayerIsReady(int playerId, bool isReady)
    {
        if (playerId == assignedPlayerID)
        {
            ClientIsReady = isReady;
            readyText.text = (isReady) ? "Ready!" : "Not Ready!";
        }
        else
        {
            activeClientPlayers[playerId].SetReadyStatus(isReady);
        }
    }

    //Another player has changed their character type
    public void PlayerClassChanged(int playerId, int type)
    {
        if (playerId == assignedPlayerID)
        {
            basePlayerOBJ.SetPlayerClass(type);
        }
        else
        {
            activeClientPlayers[playerId].SetPlayerClass(type);
        }
    }

    //The game will start after the given amount of time
    public void GameStart(int time)
    {
        Debug.Log("Game start called with a value of " + time);
        timeTillgameStart = (float) time;
        UpdateTimerUI(timeTillgameStart);
        GameTimerStarted = true;
    }


    //Game Phase:

    //Tell the client the size of the map
    public void SetMapSize(int x, int y)
    {
        mapCreator.CreateMap(x, y);
    }

    //Tell the client that a specific space on the map is “blocked”
    public void SetBlockedSpace(int x, int y)
    {
        mapCreator.SetBlockedParts(x,y);
    }

    //A player has moved and is now at the position given
    public void SetPlayerPosition(int playerId, int x, int y)
    {
        Debug.Log("Set player " + playerId + " to position "+x+" "+y);

        if (playerId == basePlayerOBJ.networkID)
        {
            basePlayerOBJ.SetPosition(x, y);
            GameCamera.GetInstance().SetCameraPosition(basePlayerOBJ.transform.position);
        }
        else if (activeClientPlayers.ContainsKey(playerId))
        {
            activeClientPlayers[playerId].SetPosition(x, y);
            GameCamera.GetInstance().SetCameraPosition(activeClientPlayers[playerId].transform.position);
        }
        else
        {
            for (int i = 1; i < 3; i++)
            {
                if(activeClientPlayers[i].GetNetworkID() == playerId)
                {
                    activeClientPlayers[i].SetPosition(x, y);
                    GameCamera.GetInstance().SetCameraPosition(activeClientPlayers[i].transform.position);
                }
            }
        }
    }

    //Called when it is the start of the given player’s turn
    public void StartTurn(int playerId)
    {
        if (playerId == assignedPlayerID)
        {
            HasATurn = true;
            GameCamera.GetInstance().SetCameraPosition(basePlayerOBJ.transform.position);
            //basePlayerOBJ.IsThisPlayersTurn();
        }
        else
        {
            GameCamera.GetInstance().SetCameraPosition(activeClientPlayers[playerId].transform.position);
            //activeClientPlayers[playerId].IsThisPlayersTurn();
        }
    }

    //The given player just made an attack at the given location
    public void AttackMade(int playerId, int x, int y)
    {
        Debug.Log("Player : "+playerId + " made an attack on position ("+x+','+y+')');
    }

    public void SendChatMessage(string message)
    {
        clientNet.CallRPC("SendChat",UCNetwork.MessageReceiver.ServerOnly,-1,message);
    }

    //Display a chat message from another client or the server
    public void DisplayChatMessage(string message)
    {
        chatText.text += message+'\n';
    }

    //Update the health of a player, will be called whenever a player’s health changes
    public void UpdateHealth(int playerId, int newHealth)
    {
        if (playerId == assignedPlayerID)
        {
            basePlayerOBJ.SetPlayerHealth(newHealth);
        }
        else
        {
            activeClientPlayers[playerId].SetPlayerHealth(newHealth);
        }
    }

    public void OnInstantiateNetworkObject(int id)
    {

        Debug.Log("Created object with ID = "+id);
        //activeClientPlayers.Add(id,);
    }

    public void RPCTest(int aInt)
    {
        Debug.Log("RPC Test has been called with " + aInt);
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

        basePlayerOBJ = Instantiate(Resources.Load<GameObject>("Player"), Vector3.zero, Quaternion.identity).GetComponent<Player>();//clientNet.Instantiate("Player", Vector3.zero, Quaternion.identity).GetComponent<Player>();

        clientNet.CallRPC("SetName", UCNetwork.MessageReceiver.ServerOnly, -1, PlayerPrefs.GetString("PlayerName", "DEFAULTNAME"));
        clientNet.CallRPC("SetCharacterType", UCNetwork.MessageReceiver.ServerOnly, -1, PlayerPrefs.GetInt("PlayerClass"));

        //basePlayerOBJ.SetNetworkID(-1);
        //basePlayerOBJ.GetComponent<Player>().SetNetworkID(-1);

        //clientNet.CallRPC("SetName", UCNetwork.MessageReceiver.ServerOnly, -1, PlayerPrefs.GetString("PlayerName", "DefaultPlayerName"));
        //clientNet.AddToArea(1);
    }

    void OnNetStatusDisconnecting()
    {
        Debug.Log("OnNetStatusDisconnecting called");



        //if (myPlayer)
        //{
        //    clientNet.Destroy(myPlayer.GetComponent<NetworkSync>().GetId());
        //}
    }
    void OnNetStatusDisconnected()
    {
        Debug.Log("OnNetStatusDisconnected called");
        SceneManager.LoadScene("Client");
        
        loginInProcess = false;

        //if (myPlayer)
        //{
        //    clientNet.Destroy(myPlayer.GetComponent<NetworkSync>().GetId());
        //}
    }
    public void OnChangeArea()
    {
        Debug.Log("OnChangeArea called");
        

        // Tell the server we are ready////
        //Instantiate Player
        //myPlayer.GetComponent<NetworkSync>().AddToArea(1);
    }

    // RPC Called by the server once it has finished sending all area initization data for a new area
    public void AreaInitialized()
    {
        Debug.Log("AreaInitialized called");
    }

    public void ForceLeave()
    {
        if (clientNet.IsConnected())
        {
            clientNet.Disconnect("I'm out! I'm gonna make my own multiplayer game! With blackjack, and hookers!");
        }
    }
    
    void OnDestroy()
    {
        //if (myPlayer)
        //{
        //    clientNet.Destroy(myPlayer.GetComponent<NetworkSync>().GetId());
        //}
        if (clientNet.IsConnected())
        {
            clientNet.Disconnect("I'm out! I'm gonna make my own multiplayer game! With blackjack, and hookers!");
        }
    }
}


