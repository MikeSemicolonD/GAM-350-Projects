using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using LitJson;

public class ExampleServer : MonoBehaviour
{
    public ServerNetwork serverNet;

    public int portNumber = 603;
    
    enum PieceType {Empty = 1,Yellow = 2,Red = 3};

    // Stores a player
    class Player
    {
        public string name;
        public long clientId;
        public bool isReady;
        public bool isConnected;
        public PieceType Type;
    }

    List<Player> players = new List<Player>();

    PieceType currentTypeTurn;

    PieceType[,] board = new PieceType[7,6];

    bool gameStarted;
    bool gameFinished;
    bool xAssigned;
    bool oAssigned;

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

    // A client has just requested to connect to the server
    void ConnectionRequest(ServerNetwork.ConnectionRequestInfo data)
    {
        Debug.Log("Connection request from " + data.username);

        if (!gameFinished && players.Count < 2)
        {
            // We either need to approve a connection or deny it
            Player newPlayer = new Player();
            newPlayer.clientId = data.id;
            newPlayer.isConnected = false;
            newPlayer.name = "Player " + players.Count;

            if (!xAssigned)
            {
                Debug.Log("Assigned 2 to a player");
                newPlayer.Type = PieceType.Yellow;
                xAssigned = true;
            }
            else if (!oAssigned)
            {
                Debug.Log("Assigned 1 to a player");
                newPlayer.Type = PieceType.Red;
                oAssigned = true;
            }
            else
            {
                Debug.Log("Assigned empty piece to a player");
                newPlayer.Type = PieceType.Empty;
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

        // Send the client the current state of the board
        for (int i=0; i<7; i++)
        {
            for (int j=0; j<6; j++)
            {
                //x length = 7
                //y length = 6
                //Board[x,y]
                serverNet.CallRPC("SetBoardState", aClientId, -1, j, i, (int)board[i, j]);
            }
        }
    }

    //Updates the server status text that is included on all clients
    public void SendServerMessageToClients(string message="")
    {
        serverNet.CallRPC("UpdateServerStatus", UCNetwork.MessageReceiver.AllClients, -1, message);
    }

    /// <summary>
    /// Gets called from a clients Connect4Board, this also allows the server to keep track of what's on the board. 
    /// It'll then call the SetBoardState function on all clients, updating the clients to what just happened 
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="type"></param>
    public void SetBoardState(int row, int column, int type)
    {
        Debug.Log("Board being set on column "+column+" row "+ row +" with type "+type);

        SendServerMessageToClients();

        if (row <= 5 && column <= 6)
        {
            board[column,row] = (PieceType)type;
        }
        else
        {
            Debug.Log("Couldn't add a piece, index is out of range");
        }

        serverNet.CallRPC("SetBoardState", UCNetwork.MessageReceiver.AllClients, -1, row, column, type);
        
    }

    /// <summary>
    /// Recursive function that returns the number of pieces found when traveling in a specific direction on the board
    /// </summary>
    /// <param name="currentXIndex"></param>
    /// <param name="currentYIndex"></param>
    /// <param name="xDirection"></param>
    /// <param name="yDirection"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    int WinCountCheck(int currentXIndex, int currentYIndex, int xDirection, int yDirection, int targetType)
    {
        if(currentXIndex < 7 && currentYIndex < 6 && currentXIndex >= 0 && currentYIndex >= 0)   
        {
            if ((int)board[currentXIndex, currentYIndex] == targetType)
            {
                Debug.Log("Piece found at x index = "+currentXIndex+" y index = "+currentYIndex+"\n With direction of x = "+xDirection+" y = "+yDirection);
                return 1 + WinCountCheck(currentXIndex + xDirection, currentYIndex + yDirection, xDirection, yDirection, targetType);
            }
        }
        return 0;
    }

    /// <summary>
    /// Goes through the board array, and checks if there's a pattern: 
    /// flat/horizontal row of 4 or a diagonal row of 4
    /// returns 1 if no winner was found, 2 if yellow wins, 3 if red wins
    /// </summary>
    int CheckForWinner()
    {
        //Loop for the two types
        for (int type = 2; type < 4; type++)
        {
            //x
            for (int x = 0; x < 7; x++)
            {
                //y
                for (int y = 0; y < 6; y++)
                {
                    if ((int)board[x, y] == type)
                    {
                        //Check left
                        if (WinCountCheck(x, y, -1, 0, type) >= 4)
                        {
                            return type;
                        }

                        //Check right
                        if (WinCountCheck(x, y, 1, 0, type) >= 4)
                        {
                            return type;
                        }

                        //Check above
                        if (WinCountCheck(x, y, 0, 1, type) >= 4)
                        {
                            return type;
                        }

                        //Check below
                        if (WinCountCheck(x, y, 0, -1, type) >= 4)
                        {
                            return type;
                        }

                        //Check all 4 diagonal directions
                        //Check diagonally (-1/1)
                        if (WinCountCheck(x, y, 1, -1, type) >= 4)
                        {
                            return type;
                        }

                        //Check diagonally (1/1)
                        if (WinCountCheck(x, y, 1, 1, type) >= 4)
                        {
                            return type;
                        }

                        //Check diagonally (1/-1)
                        if (WinCountCheck(x, y, -1, 1, type) >= 4)
                        {
                            return type;
                        }

                        //Check diagonally (-1/-1)
                        if (WinCountCheck(x, y, -1, -1, type) >= 4)
                        {
                            return type;
                        }

                    }
                }
            }
        }

        return 1;
    }

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
            // Tell the clients whose turn it is
            if(!gameStarted)
            {
                CallStartTurnOnClients();
                gameStarted = true;
            }
            else
            {
                CallStartTurnOnClients(false);
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

    /// <summary>
    /// Sets the next turn on a given client
    /// Client verifies it on their side on if it's their turn or not
    /// </summary>
    public void CallStartTurnOnClients(bool passTurn = true)
    {
        int winState = CheckForWinner();

        if (winState == 1)
        {
            if (passTurn)
            {
                currentTypeTurn = (currentTypeTurn == PieceType.Yellow) ? PieceType.Red : PieceType.Yellow;

            }

            serverNet.CallRPC("StartTurn", UCNetwork.MessageReceiver.AllClients, -1, (int)currentTypeTurn);

            SendServerMessageToClients("It's now " + currentTypeTurn + "'s turn.");
        }
        else
        {
            if (winState == 2)
            {
                SendServerMessageToClients("Yellow wins!");
            }
            else
            {
                SendServerMessageToClients("Red wins!");
            }

            serverNet.CallRPC("DeclareWinner", UCNetwork.MessageReceiver.AllClients, -1, winState);

            gameFinished = true;
        }
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

                if(p.Type == PieceType.Yellow)
                {
                    xAssigned = false;
                }
                else
                {
                    oAssigned = false;
                }

                players.Remove(p);
                break;
            }
        }

        if(players.Count == 0)
        {
            Debug.Log("Restarting Server, everyone disconnected...");
            //serverNet.CallRPC("UpdateServerStatus", UCNetwork.MessageReceiver.AllClients, -1, "No players left, restarting server...");
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}
