using UnityEngine;
using System.Collections;

/// <summary>
/// Basic player object for visualization
/// by: Michael Frye
/// </summary>
public class Player : MonoBehaviour {

    float speed = 5;

    public int team;

    public int playerClass;

    public string name;

    public int attackDistance = 1;

    public int spacesForEveryTurn;

    public int spacesMovedSoFarForThisTurn = 0;

    public int health;

    public int networkID;

    NetworkSync networkSyncComp;

    public Mesh WarriorMesh, WizardMesh, RogueMesh;

    public int XPosition, YPosition;

    private void Awake()
    {
        networkSyncComp = GetComponent<NetworkSync>();

        if (networkSyncComp.owned)
        {
            name = PlayerPrefs.GetString("PlayerName","ForgotToPutInAName");
            playerClass = PlayerPrefs.GetInt("PlayerTeam", 3);

            SetHealthAndSpacesPerTurn(playerClass);

            networkSyncComp.CallRPC("SetName", UCNetwork.MessageReceiver.AllClients, name);
            networkSyncComp.CallRPC("SetCharacterType", UCNetwork.MessageReceiver.AllClients, playerClass);
        }
    }

    public void SetPlayerHealth(int newHealthValue)
    {
        health = newHealthValue;

        if(health <= 0)
        {
            health = 0;
            JustDied();
        }
    }

    public void JustDied()
    {
        GetComponent<MeshFilter>().mesh = null;
    }

    public void SetReadyStatus(bool status)
    {
        if(status)
        {
            Debug.Log("I am ready!");
        }
        else
        {
            Debug.Log("I'm NOT ready!");
        }
    }

    public void IsThisPlayersTurn()
    {
        spacesMovedSoFarForThisTurn = spacesForEveryTurn;
    }

    public void SetPosition(int x, int y)
    {
        XPosition = x;
        YPosition = y;

        transform.position = MapCreator.GetInstance().GetBlockPosition(x, y);
    }

    public void SetNetworkIDAndTeam(int newNetworkID, int teamValue, bool assignIDToNetSyncComponent=false)
    {
        SetNetworkID(newNetworkID, assignIDToNetSyncComponent);
        SetTeam(teamValue);
    }

    public int GetNetworkID()
    {
        return networkSyncComp.GetId();
    }

    public void SetNetworkID(int value, bool setNetSyncID = false)
    {
        if (setNetSyncID)
        {
            GetComponent<NetworkSync>().SetId(value);
        }

        networkID = value;
    }

    private void SetHealthAndSpacesPerTurn(int selectPlayerClass)
    {
        switch(selectPlayerClass)
        {
            //Warrior
            case 1:
                GetComponent<MeshFilter>().mesh = WarriorMesh;
                health = 100;
                spacesForEveryTurn = 2;
                break;

            //Rogue
            case 2:
                GetComponent<MeshFilter>().mesh = RogueMesh;
                health = 70;
                spacesForEveryTurn = 5;
                break;

            //Wizard
            case 3:
                GetComponent<MeshFilter>().mesh = WizardMesh;
                health = 30;
                spacesForEveryTurn = 4;
                attackDistance = 6;
                break;

            default:
                Debug.Log("ERROR : Invalid class selelection " + selectPlayerClass);
                break;
        }
    }

    public void SetPlayerClass(int val)
    {
        playerClass = val;
        SetHealthAndSpacesPerTurn(playerClass);
    }

    public void SetTeam(int val)
    {
        team = val;
    }

    public void SetName(string val)
    {
        name = val;
    }
}
