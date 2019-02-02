using UnityEngine;

/// <summary>
/// Holds the player's data and the Ui objects needed to display if a player has joined.
/// 
/// @Author Michael Frye
/// </summary>
public class PlayerSelectSlot : MonoBehaviour {
    
    public PlayerData playerData;
    public LobbyManager lobbyManager;
    public GameObject DirectionalUI, TeamSelectUI, Team1UI,Team2UI;

    private void Update()
    {
        if (!playerData.joined)
        {
            //If this player hasn't joined yet and has hit start, join
            if (Input.GetButtonDown("Submit_Player" + playerData.playerValue))
            {
                Join();
            }
            else
            {
                return;
            }
        }
        else
        {
            //If the player hits start and the lobbyManager thinks you're ready, begin game
            if (Input.GetButtonDown("Submit_Player" + playerData.playerValue) && lobbyManager.LobbyIsReadyToStart())
            {
                lobbyManager.BeginGame();
            }
            else if (Input.GetButtonDown("Horizontal_Player" + playerData.playerValue)
                || Input.GetButtonDown("Use_Player" + playerData.playerValue))
            {
                SwitchTeams();
            }
            else if (Input.GetButtonDown("Cancel_Player" + playerData.playerValue))
            {
                Exit();
            }
            else
            {
                return;
            }
        }
    }

    /// <summary>
    /// Sets the playerData's joined value to true while showing it visually
    /// </summary>
    public void Join()
    {
        playerData.joined = true;
        DirectionalUI.SetActive(false);
        TeamSelectUI.SetActive(true);
        SwitchTeams();
    }

    /// <summary>
    /// Sets the playerData's joined value to false while showing it visually
    /// </summary>
    public void Exit()
    {
        playerData.joined = false;
        DirectionalUI.SetActive(true);
        TeamSelectUI.SetActive(false);
    }

    /// <summary>
    /// Switches teams in the player data while also expressing it visually 
    /// </summary>
    public void SwitchTeams()
    {
        if(Team1UI.activeSelf)
        {
            playerData.InTeam1 = false;
            Team1UI.SetActive(false);
            Team2UI.SetActive(true);
        }
        else
        {
            playerData.InTeam1 = true;
            Team1UI.SetActive(true);
            Team2UI.SetActive(false);
        }
    }
}
