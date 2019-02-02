using UnityEngine;
using UnityEngine.UI;

public class TitleScreenLogic : MonoBehaviour {

    public Text highscoreText;
    public Text server;
    public Text port;
    public ExampleClient client;

    void Start()
    {
        highscoreText.text = "Highscore: " + PlayerPrefs.GetInt("NumberOfPasses", 0);
    }

    public void Connect()
    {
        client.ConnectToServer(server.text, int.Parse(port.text));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
