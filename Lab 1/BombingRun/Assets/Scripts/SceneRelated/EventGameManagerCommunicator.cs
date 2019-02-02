using UnityEngine;

/// <summary>
/// UI buttons are meant to be connected to this through the inspector so as to communitcate with the GameManager.
/// 
/// @Author Michael Frye
/// </summary>
public class EventGameManagerCommunicator : MonoBehaviour
{

    public void QuitGame()
    {
        GameManager.instance.QuitGame();
    }

    public void RestartGame()
    {
        GameManager.instance.RestartGame();
    }

    public void ReturnToSelectScreen()
    {
        GameManager.instance.ReturnToSelectScreen();
    }

}
