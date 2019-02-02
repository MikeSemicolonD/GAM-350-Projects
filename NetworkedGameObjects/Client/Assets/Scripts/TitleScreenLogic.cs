using UnityEngine;
using UnityEngine.UI;

public class TitleScreenLogic : MonoBehaviour {

    public Text server;
    public Text port;
    public ExampleClient client;

    public void Connect()
    {
        client.ConnectToServer(server.text, int.Parse(port.text));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
