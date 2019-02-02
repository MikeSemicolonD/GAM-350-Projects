using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using System.Text;

/// <summary>
/// Handle everything needed to make the game run properly
/// 
/// @Author Michael Frye
/// </summary>
public class GameManager : MonoBehaviour {

    public static GameManager instance;

    //Camera rect presets for each camera for 3 scenarios (2-player, 3-player, 4-player)
    public List<Rect> FourPlayerPresets;
    public List<Rect> ThreePlayerPresets;
    public List<Rect> TwoPlayerPresets;

    public GameObject basePlayerPrefab;
    private GameObject winningCameraParent;
    private DestructableObject mainDefendingBase; 

    private float gameTimeLimit=100, gameTimeLimitRunTime;

    private List<GameObject> Team1PlayerInstances = new List<GameObject>();
    private List<GameObject> Team2PlayerInstances = new List<GameObject>();

    private List<PlayerData> ActivePlayers;

    private List<Rocket> Rockets = new List<Rocket>();
    private List<Turret> Turrets = new List<Turret>();

    private GameObject[] Team1SpawnPoints;
    private GameObject[] Team2SpawnPoints;

    private GameObject GameUI;
    private GameObject PostWinButtons;

    private TextMeshProUGUI WinText;
    private TextMeshProUGUI TimerUI;

    private StringBuilder TimerStringBuilder = new StringBuilder();

    private List<Rect> SetCameraPreset;

    private bool GameOn;
    private bool GameFinished;
    private bool loadingNewScene;

    private void Awake ()
    {
        gameTimeLimitRunTime = gameTimeLimit;

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update ()
    {
        if (loadingNewScene && SceneManager.GetSceneByBuildIndex(1).isLoaded)
        {
            SetCursorLock(true);

            //Find objects needed to make the game run
            FindTaggedObjects();

            //Instantiate prefabs, moving them to spawn points according to their team
            SpawnPlayers();

            GameOn = true;

            loadingNewScene = false;
        }
        else
        {
            if (GameOn)
            {
                //If game timer <= 0 end the game, defenders win
                if (gameTimeLimitRunTime <= 0)
                {
                    ForcePlayersOutOfVehicle();
                    DestroyAllPlayers();

                    //Enable a camera that'll rotate around the base
                    winningCameraParent.transform.GetChild(0).gameObject.SetActive(true);

                    //Change timer to say who won
                    UpdateWinTextUI("Defenders Win!");

                    GameOn = false;
                    GameFinished = true;

                }
                else
                {
                    gameTimeLimitRunTime -= Time.deltaTime;

                    UpdateTimerUI(gameTimeLimitRunTime);

                    //If base has been destroy, attackers win
                    if (mainDefendingBase.HasBeenDestroyed())
                    {
                        ForcePlayersOutOfVehicle();
                        DestroyAllPlayers();

                        //Enable a camera that'll rotate around the base
                        winningCameraParent.transform.GetChild(0).gameObject.SetActive(true);

                        //Change timer to say who won
                        UpdateWinTextUI("Attackers Win!");

                        GameOn = false;
                        GameFinished = true;

                    }
                }
            }
            //Show WinScreen
            else if (GameFinished && !WinText.gameObject.activeSelf)
            {
                WinText.gameObject.SetActive(true);
                PostWinButtons.SetActive(true);
                SetCursorLock(false);
                GameFinished = false;
            }
            else
            {
                return;
            }
        }
	}

    private void FindPlayerSpawnPoints()
    {
        Team1SpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPointTeam1");
        Team2SpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPointTeam2");
    }

    /// <summary>
    /// Returns a random spawn point index, taking into account which team the player's on
    /// </summary>
    /// <param name="team1"></param>
    /// <returns></returns>
    private int GetRandomSpawnPoint(bool team1)
    {
        return (team1) ? Random.Range(0, Team1SpawnPoints.Length) : Random.Range(0, Team2SpawnPoints.Length);
    }

    /// <summary>
    /// Sets the cameraPreset for the entire game
    /// The SetCameraPreset will then be used to set all the player cameras when they spawn
    /// </summary>
    private void SetPlayerCameraPreset()
    {
        switch(ActivePlayers.Count)
        {
            case 2:
                SetCameraPreset = TwoPlayerPresets;
                break;

            case 3:
                SetCameraPreset = ThreePlayerPresets;
                break;

            case 4:
                SetCameraPreset = FourPlayerPresets;
                break;

            default:
                Debug.Log("ERROR : activePlayerCount = " + ActivePlayers.Count);
                break;
        }
    }

    /// <summary>
    /// Instantiates player objects while passing in what cameraPreset it should have and who the specific player object belongs to
    /// </summary>
    private void SpawnPlayers()
    {
        for (int i = 0; i < ActivePlayers.Count; i++)
        {
            //Defending team
            if(ActivePlayers[i].InTeam1)
            {
                int randSpawnPoint = GetRandomSpawnPoint(true);
                GameObject newPlayer = Instantiate(basePlayerPrefab, Team1SpawnPoints[randSpawnPoint].transform.position, Team1SpawnPoints[randSpawnPoint].transform.localRotation);
                newPlayer.GetComponent<PlayerController>().PassPlayerData(ActivePlayers[i], SetCameraPreset[i]);
                Team1PlayerInstances.Add(newPlayer);
            }
            else
            {
                int randSpawnPoint = GetRandomSpawnPoint(false);
                GameObject newPlayer = Instantiate(basePlayerPrefab, Team2SpawnPoints[randSpawnPoint].transform.position, Team2SpawnPoints[randSpawnPoint].transform.localRotation);
                newPlayer.GetComponent<PlayerController>().PassPlayerData(ActivePlayers[i], SetCameraPreset[i]);
                Team2PlayerInstances.Add(newPlayer);
            }
        }
    }

    /// <summary>
    /// Take the player transform and places it on the spawn point, taking into acount which team the player is on
    /// </summary>
    /// <param name="playerObject"></param>
    /// <param name="InTeam1"></param>
    public void RespawnPlayerObject(Transform playerObject, bool InTeam1)
    {
        if(InTeam1)
        {
            int randSpawnPoint = GetRandomSpawnPoint(true);
            playerObject.position = Team1SpawnPoints[randSpawnPoint].transform.position;
            playerObject.rotation = Team1SpawnPoints[randSpawnPoint].transform.localRotation;
        }
        else
        {
            int randSpawnPoint = GetRandomSpawnPoint(false);
            playerObject.position = Team2SpawnPoints[randSpawnPoint].transform.position;
            playerObject.rotation = Team2SpawnPoints[randSpawnPoint].transform.localRotation;
        }
    }
	
    /// <summary>
    /// Receives PlayerData from LobbyManager and sets the camera preset 
    /// </summary>
    /// <param name="newActivePlayers"></param>
    public void PassActivePlayerSlotData(List<PlayerData> newActivePlayers)
    {
        ActivePlayers = newActivePlayers;
        SetPlayerCameraPreset();
    }

    /// <summary>
    /// Loads the level requested (Always assume to be 1 for now)
    /// loadingNewScene is set to true which set off a chain reaction in Update() to start the game
    /// </summary>
    /// <param name="index"></param>
    public void LoadLevelAndBegin(int index)
    {
        SceneManager.LoadScene(index);

        loadingNewScene = true;
    }

    /// <summary>
    /// Cursor lock 
    /// </summary>
    /// <param name="value"></param>
    public void SetCursorLock(bool value)
    {
        if (!value)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// Takes the value of time and rounds it to one decimal place
    /// </summary>
    /// <param name="value"></param>
    private void UpdateTimerUI(float value)
    {
        TimerStringBuilder.Length = 0;
        TimerStringBuilder.Append(System.Math.Round(value, 1));
        TimerUI.text = TimerStringBuilder.ToString();
    }

    /// <summary>
    /// Updates the win text onscreen to call out which team won
    /// </summary>
    /// <param name="message"></param>
    private void UpdateWinTextUI(string message)
    {
        TimerStringBuilder.Length = 0;
        TimerStringBuilder.Append(message);
        WinText.text = TimerStringBuilder.ToString();
    }

    /// <summary>
    /// Destroys all player instances
    /// Gets called at the end of a game
    /// </summary>
    private void DestroyAllPlayers()
    {
        for(int i = 0; i < Team1PlayerInstances.Count; i++)
        {
            Destroy(Team1PlayerInstances[i]);
        }

        for (int i = 0; i < Team2PlayerInstances.Count; i++)
        {
            Destroy(Team2PlayerInstances[i]);
        }
    }

    /// <summary>
    /// Find all tagged objects in the scene that is required to make the game run
    /// </summary>
    private void FindTaggedObjects()
    {
        //Get the UI that's in the scene
        GetGameUI();

        //Get spawn points after loading scene
        FindPlayerSpawnPoints();

        winningCameraParent = GameObject.FindWithTag("WinningCamera");

        mainDefendingBase = GameObject.FindWithTag("PrimaryDefenderBase").GetComponent<DestructableObject>();

        FindRocketsAndTurrets();
    }

    /// <summary>
    /// Find all tagged objects labeled Rocket & Turret
    /// </summary>
    private void FindRocketsAndTurrets()
    {
        GameObject[] foundRockets = GameObject.FindGameObjectsWithTag("Rocket");

        GameObject[] foundTurrets = GameObject.FindGameObjectsWithTag("Turret");

        for (int i = 0; i < foundRockets.Length; i++)
        {
            if (foundRockets[i].GetComponent<Rocket>() != null)
            {
                Rockets.Add(foundRockets[i].GetComponent<Rocket>());
            }
        }

        for (int i = 0; i < foundTurrets.Length; i++)
        {
            Turrets.Add(foundTurrets[i].GetComponent<Turret>());
        }
    }

    public void RestartGame()
    {
        gameTimeLimitRunTime = gameTimeLimit;

        Team1PlayerInstances = new List<GameObject>();
        Team2PlayerInstances = new List<GameObject>();
        Rockets = new List<Rocket>();
        Turrets = new List<Turret>();

        SceneManager.LoadScene(0);
        LoadLevelAndBegin(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ReturnToSelectScreen()
    {
        gameTimeLimitRunTime = gameTimeLimit;

        Team1PlayerInstances = new List<GameObject>();
        Team2PlayerInstances = new List<GameObject>();
        Rockets = new List<Rocket>();
        Turrets = new List<Turret>();

        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Forces the player out of any turret/rocket being used currently
    /// </summary>
    private void ForcePlayersOutOfVehicle()
    {
        for(int i = 0; i < Turrets.Count; i++)
        {
            if (Turrets[i].BeingUsed())
            {
                Turrets[i].ExitTurret();
            }
        }

        for (int i = 0; i < Rockets.Count; i++)
        {
            if (Rockets[i].IsBeingUsed())
            {
                Rockets[i].ExitRocket();
            }
        }
    }

    /// <summary>
    /// Gets the main game UI via a tag
    /// Then goes into it's child objects and gets components we need
    /// </summary>
    private void GetGameUI()
    {
        GameUI = GameObject.FindWithTag("GameUI");

        TimerUI = GameUI.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        WinText = GameUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        PostWinButtons = GameUI.transform.GetChild(2).gameObject;
    }
}
