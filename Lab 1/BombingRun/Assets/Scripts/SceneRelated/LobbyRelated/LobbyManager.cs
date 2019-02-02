using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds onto all player slots in the scene, and holds onto the UI that indicates when it's OK to start.
/// 
/// @Author Michael Frye
/// </summary>
public class LobbyManager : MonoBehaviour
{
    public int sceneToLoad = 1;

    public List<PlayerSelectSlot> PlayerSlots;
    public GameObject StartGameDirectionUI;

    private List<PlayerData> ActivePlayerSlots;
    private bool isReadyToStart;

    private void Update()
    {
        //Continuously check and set when the lobby is 'ready'
        //'ready' = when there's at least 1 player on each team
        for (int i = 0, t1 = 0, t2 = 0; i < PlayerSlots.Count; i++)
        {
            if (PlayerSlots[i].playerData.joined)
            {
                if (PlayerSlots[i].playerData.InTeam1)
                {
                    t1++;
                }
                else
                {
                    t2++;
                }
            }

            if (t1 > 0 && t2 > 0)
            {
                SetLobbyStatus(true);
            }
            else
            {
                SetLobbyStatus(false);
            }
        }
    }

    /// <summary>
    /// Sets the status of the lobby
    /// true = the lobby can start the game
    /// false = the lobby can't start the game
    /// </summary>
    /// <param name="isReady"></param>
    private void SetLobbyStatus(bool isReady)
    {
        isReadyToStart = isReady;
        StartGameDirectionUI.SetActive(isReady);
    }

    /// <summary>
    /// Passes the data of all the active players in the lobby to the GameManager
    /// GameManager then Loads a pre-selected level
    /// Gets called by PlayerSelectSlot
    /// </summary>
    public void BeginGame()
    {

        ActivePlayerSlots = new List<PlayerData>();

        for (int i = 0; i < PlayerSlots.Count; i++)
        {
            if(PlayerSlots[i].playerData.joined)
            {
                ActivePlayerSlots.Add(PlayerSlots[i].playerData);
            }
        }

        GameManager.instance.PassActivePlayerSlotData(ActivePlayerSlots);

        GameManager.instance.LoadLevelAndBegin(sceneToLoad);
    }

    /// <summary>
    /// Gets isReadyToStart value
    /// Gets called from PlayerSelectSlot in order to verify 
    /// </summary>
    /// <returns></returns>
    public bool LobbyIsReadyToStart()
    {
        return isReadyToStart;
    }
}
