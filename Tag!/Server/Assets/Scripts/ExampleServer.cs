using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ExampleServer : MonoBehaviour
{
    public static ExampleServer instance;

    public ServerNetwork serverNet;

    public int portNumber = 603;

    public int itNetId = 0;

    public float ItSendWaitTime=5f;
    public float ItSendWaitTimeTemp=0;

    // Stores a player
    class Player
    {
        public long clientId;
        public int netId=-1;
        public bool isConnected;
        public bool isIt;
    }
    //Dictionary<int, Vector3> networkedObjects;
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
        if (players.Count < 2)
        {
            Player newPlayer = new Player
            {
                clientId = data.id,
                isConnected = false
            };

            if (players.Count == 0)
            {
                newPlayer.isIt = true;
            }

            players.Add(newPlayer);
            serverNet.ConnectionApproved(data.id);
        }
        else
        {
            serverNet.ConnectionDenied(data.id);
        }
    }

    private void Update()
    {
        if (players.Count < 2)
        {
            return;
        }
        if(ItSendWaitTimeTemp > 0)
        {
            ItSendWaitTimeTemp -= Time.fixedDeltaTime;
        }
        else if (ItSendWaitTimeTemp <= 0 && (serverNet.GetNetObjById(players[0].netId).position - serverNet.GetNetObjById(players[1].netId).position).magnitude <= 1)
        {
            if(players[0].isIt)
            {
                players[1].isIt = true;
                players[0].isIt = false;
            }
            else
            {
                players[0].isIt = true;
                players[1].isIt = false;
            }

            if(itNetId == players[1].netId)
            {
                itNetId = players[0].netId;
            }
            else
            {
                itNetId = players[1].netId;
            }

            serverNet.CallRPC("PlayerIsIt", UCNetwork.MessageReceiver.AllClients, players[1].netId);
            serverNet.CallRPC("PlayerIsIt", UCNetwork.MessageReceiver.AllClients, players[0].netId);
            ItSendWaitTimeTemp = ItSendWaitTime;
        }
        
    }

    public void PassId(int id)
    {
        Player p = GetPlayerByClientId(serverNet.SendingClientId);
        if (p.netId == -1)
        {
            p.netId = id;
        }

        if(p.isIt)
        {
            itNetId = id;
        }
    }

    public void AmIIt(int id)
    {
        Player p = GetPlayerByNetId(id);

        if(p.isIt && id == itNetId)
        {
            Debug.Log("player " + id+" is it");
            serverNet.CallRPC("PlayerIsIt", serverNet.SendingClientId, id);
        }

    }

    void OnClientConnected(long aClientId)
    {
        // Set the isConnected to true on the player
        foreach (Player p in players)
        {
            if (p.clientId == aClientId)
            {
                p.isConnected = true;

                if (players.Count == 1)
                {
                    p.isIt = true;
                }
            }
        }
    }

    // Get the player for the given Net id
    Player GetPlayerByNetId(int netId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].netId == netId)
            {
                return players[i];
            }
        }
        Debug.Log("Unable to get player for unknown client " + netId);
        return null;
    }

    // Get the player for the given client id
    Player GetPlayerByClientId(long aClientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId == aClientId)
            {
                return players[i];
            }
        }
        Debug.Log("Unable to get player for unknown client " + aClientId);
        return null;
    }

    void OnClientDisconnected(long aClientId)
    {
        // Set the isConnected to false on the player
        foreach (Player p in players)
        {
            if (p.clientId == aClientId)
            {
                p.isConnected = false;
            }

            players.Remove(p);
        }

        if(players.Count == 0)
        {
            Debug.Log("Restarting Server....");
            SceneManager.LoadScene(gameObject.scene.buildIndex);
        }
    }
}
