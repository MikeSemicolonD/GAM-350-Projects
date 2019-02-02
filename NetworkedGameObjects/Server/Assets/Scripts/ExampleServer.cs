using UnityEngine;
using System.Collections.Generic;

public class ExampleServer : MonoBehaviour
{
    public static ExampleServer instance;

    public ServerNetwork serverNet;

    public int portNumber = 603;
    
    // Stores a player
    class Player
    {
        public long clientId;
        public bool isConnected;
    }

    List<Player> players = new List<Player>();
    
    // Use this for initialization
    void Awake()
    {
        instance = this;

        // Initialization of the server network
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

        // We either need to approve a connection or deny it
        if (players.Count < 8)
        {
            Player newPlayer = new Player();
            newPlayer.clientId = data.id;
            newPlayer.isConnected = false;
            players.Add(newPlayer);
            //serverNet.CallRPC("SetLong", UCNetwork.MessageReceiver.SingleClient, -1, data.id);
            serverNet.ConnectionApproved(data.id);
        }
        else
        {
            serverNet.ConnectionDenied(data.id);
        }
    }

    public void SendColorChangeInfo(int id, float r, float g, float b)
    {
        serverNet.CallRPC("SetColor", UCNetwork.MessageReceiver.AllClients, id, r, g, b, id);
    }

    void OnClientConnected(long aClientId)
    {
        // Set the isConnected to true on the player
        foreach (Player p in players)
        {
            if (p.clientId == aClientId)
            {
                p.isConnected = true;
            }
        }
    }

    void OnClientDisconnected(long aClientId)
    {
        // Set the isConnected to true on the player
        foreach (Player p in players)
        {
            if (p.clientId == aClientId)
            {
                p.isConnected = false;
            }
        }
    }
}
