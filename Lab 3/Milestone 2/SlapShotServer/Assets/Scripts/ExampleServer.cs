using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class ExampleServer : MonoBehaviour
{
    public ServerNetwork serverNet;

    public int portNumber = 603;

    enum PieceType { Blue = 2, Red = 3 };

    // Stores a player
    [Serializable]
    class Player
    {
        public string name;
        public long clientId;
        public int netID=-2;
        public bool isReady;
        public bool isConnected;
        public PieceType Type;
    }

    [Serializable]
    class Ball
    {
        public long ownedClientId;
        public int netID = 0;
    }

    List<Player> players = new List<Player>();

    Ball ball = new Ball();

    int playerTurnIndex = -1;
    readonly float BallHitWaitTime = 1f;
    float BallHitTimeRunTime;
    readonly float PassTurnWaitTime = 1;
    float PassTurnWaitTimeRunTime;

    bool gameStarted;
    bool blueAssigned;
    bool redAssigned;

    // Initialization of the server network
    void Awake()
    {

        ServerNetwork.port = portNumber;
        if (serverNet == null)
        {
            serverNet = GetComponent<ServerNetwork>();
        }
        if (serverNet == null)
        {
            serverNet = (ServerNetwork)gameObject.AddComponent(typeof(ServerNetwork));
            Debug.Log("ServerNetwork component added.");
        }

        //serverNet.EnableLogging("rpcLog.txt");
    }

    void Update()
    {
        if(!gameStarted || ball.netID == 0 || players.Count < 2)
        {
            return;
        }
        else
        {
            //Gets called once at the start of the game
            if(playerTurnIndex == -1 && players[0].netID != -2 && players[1].netID != -2)
            {
                playerTurnIndex = UnityEngine.Random.Range(0, 2);
                serverNet.CallRPC("PassTurn", UCNetwork.MessageReceiver.AllClients, players[playerTurnIndex].netID, players[playerTurnIndex].netID);
                int indexInverse = (playerTurnIndex == 0) ? 1 : 0;
                serverNet.CallRPC("PassTurn", UCNetwork.MessageReceiver.AllClients, players[indexInverse].netID, players[playerTurnIndex].netID);
            }

            if (BallHitTimeRunTime <= 0 && (serverNet.GetNetObjById(players[playerTurnIndex].netID).position - serverNet.GetNetObjById(ball.netID).position).magnitude < 1.25f)
            {
                Debug.Log("Ball's been hit called from Player hit");
                //Tell ball to reverse velocity
                serverNet.CallRPC("ReverseBall", UCNetwork.MessageReceiver.AllClients, -1);

                //Limit how fast a particular player can hit the ball (once every 1.5 seconds)
                BallHitTimeRunTime = BallHitWaitTime;
            }
            else
            {
                if (BallHitTimeRunTime > 0)
                {
                    BallHitTimeRunTime -= Time.deltaTime;
                }

                if (PassTurnWaitTimeRunTime > 0)
                {
                    PassTurnWaitTimeRunTime -= Time.deltaTime;
                }
            }

           // serverNet.LogText("Dist = " + (serverNet.GetNetObjById(players[playerTurnIndex].netID).position - serverNet.GetNetObjById(ball.netID).position).magnitude);
        }
    }

    /// <summary>
    /// Gets called by a player object when it gets created, tells the server what net id that player is
    /// </summary>
    /// <param name="netid"></param>
    public void PassID(int netid)
    {
        serverNet.LogText("PassID id = "+netid);

        foreach(Player p in players)
        {
            if(p.clientId == serverNet.SendingClientId && p.netID == -2)
            {
                p.netID = netid;
            }
        }
    }
    
    /// <summary>
    /// Gets called by the ball when it hits a collider at the bottom of the board/screen
    /// </summary>
    /// <param name="netid"></param>
    public void EndTheGame(int netid,int bounces)
    {
        gameStarted = false;
        serverNet.CallRPC("DeclareWinner", UCNetwork.MessageReceiver.AllClients, -1, (int) players[playerTurnIndex].Type, bounces);
        Debug.Log("EndGame called from Id " + netid + " from client " + ball.ownedClientId);
    }

    /// <summary>
    /// Gets called by the ball to tell the server what client spawned it and what net id it is
    /// </summary>
    /// <param name="id"></param>
    public void IAmTheBall(int id)
    {
        if (ball.netID == 0)
        {
            ball.netID = id;
            ball.ownedClientId = serverNet.SendingClientId;
            Debug.Log("Ball set with Id "+id+" from client "+ball.ownedClientId);
        }
    }

    // A client has just requested to connect to the server
    void ConnectionRequest(ServerNetwork.ConnectionRequestInfo data)
    {
        Debug.Log("Connection request from " + data.username);

        if (!gameStarted && players.Count < 2)
        {
            // We either need to approve a connection or deny it
            Player newPlayer = new Player
            {
                clientId = data.id,
                isConnected = false,
                name = "Player " + players.Count
            };

            if (!blueAssigned)
            {
                newPlayer.Type = PieceType.Blue;
                blueAssigned = true;
            }
            else if (!redAssigned)
            {
                newPlayer.Type = PieceType.Red;
                redAssigned = true;
            }
            else
            {
                Debug.Log("No piece was assigned to the player");
            }
            
            players.Add(newPlayer);

            serverNet.ConnectionApproved(data.id);

            Debug.Log("Approved : " + data.username);

            SendServerMessageToClients("Waiting for a second player...");
        }
        else
        {

            serverNet.ConnectionDenied(data.id);

            Debug.Log("DENIED! : " + data.username);
            Debug.Log("Max number of players reached");

        }
    }

    void OnClientConnected(long aClientId)
    {
        // Set the isConnected to true on the player
        foreach (Player p in players)
        {
            if (p.clientId == aClientId)
            {
                serverNet.CallRPC("UpdateName", aClientId, -1, p.Type.ToString()+" Player");
                serverNet.CallRPC("SetPiece", aClientId, -1, (int)p.Type);
                p.isConnected = true;
            }
        }
    }

    /// <summary>
    /// Updates the server status text that is included on all clients
    /// </summary>
    /// <param name="message"></param>
    public void SendServerMessageToClients(string message="")
    {
        serverNet.CallRPC("UpdateServerStatus", UCNetwork.MessageReceiver.AllClients, -1, message);
    }

    /// <summary>
    /// Gets called by the client when it connects
    /// </summary>
    public void PlayerIsReady()
    {
        // Who called this RPC: serverNet.SendingClientId
        Debug.Log("Player is ready");

        int isReadyCount = 0;

        // Set the isConnected to true on the player
        foreach (Player p in players)
        {
            if (p.clientId == serverNet.SendingClientId)
            {
                p.isConnected = true;
                p.isReady = true;
            }
        }

        foreach(Player p in players)
        {
            if(p.isReady)
            {
                isReadyCount++;
            }
        }

        //If we have the max number of players needed, start a clients turn
        if(isReadyCount == players.Count && players.Count >= 2)
        {
            SendServerMessageToClients();

            serverNet.CallRPC("SpawnBall", UCNetwork.MessageReceiver.AllClients, -1);


            //int BallSpawnIndex = UnityEngine.Random.Range(0,2);
            //Pick a host to instantiate the ball
            //serverNet.CallRPC("SpawnBall", players[BallSpawnIndex].clientId, -1);
            //serverNet.CallRPC("SpawnBall", players[(BallSpawnIndex == 0) ? 1 : 0].clientId, -1);

            //Debug.Log(players[BallSpawnIndex].name+" spawned the ball.");

            //Call RPC on player to start turn
            //serverNet.CallRPC("PassTurn", UCNetwork.MessageReceiver.AllClients, players[playerTurnIndex].netID, players[playerTurnIndex].netID);
            gameStarted = true;
        }

    }

    /// <summary>
    /// Gets called from the ball when the ball hits a collider at the top of the board
    /// Includes a timer so it can take one call at a time for every 1 second
    /// </summary>
    /// <param name="id"></param>
    public void PassTurnFromBall(int id)
    {
        if (PassTurnWaitTimeRunTime <= 0)
        {

            if (players[0].netID != -2 && players[1].netID != -2)
            {
                PassTurnWaitTimeRunTime = PassTurnWaitTime;

                if (playerTurnIndex == 1)
                {
                    playerTurnIndex = 0;
                }
                else
                {
                    playerTurnIndex = 1;
                }

                serverNet.CallRPC("PassTurn", UCNetwork.MessageReceiver.AllClients, players[playerTurnIndex].netID, players[playerTurnIndex].netID);
                int indexInverse = (playerTurnIndex == 0) ? 1 : 0;
                serverNet.CallRPC("PassTurn", UCNetwork.MessageReceiver.AllClients, players[indexInverse].netID, players[playerTurnIndex].netID);
            }
        }
    }

    /// <summary>
    /// After recieving a chat message from a client, it will broadcast the message to all clients
    /// </summary>
    /// <param name="messageToEcho"></param>
    /// <param name="name"></param>
    public void TransmitMessage(string messageToEcho,string name)
    {
        serverNet.CallRPC("UpdateChatLog", UCNetwork.MessageReceiver.AllClients, -1, name, messageToEcho);
    }

    void OnClientDisconnected(long aClientId)
    {
        // Set the isConnected to false on the player
        foreach (Player p in players)
        {
            if (p.clientId == aClientId)
            {
                p.isReady = false;
                p.isConnected = false;

                if(p.Type == PieceType.Blue)
                {
                    blueAssigned = false;
                }
                else
                {
                    redAssigned = false;
                }

                players.Remove(p);
                break;
            }
        }

        if(players.Count == 0 || players.Count < 2 && gameStarted)
        {
            Debug.Log("Restarting Server, everyone disconnected...");
            //serverNet.CallRPC("UpdateServerStatus", UCNetwork.MessageReceiver.AllClients, -1, "No players left, restarting server...");
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}
