using UnityEngine;
using UnityEngine.UI;

public class TitleScreenLogic : MonoBehaviour {

    public InputField PlayerNameField;
    public Text server;
    public Text port;
    public ExampleClient client;

    public GameObject WarriorSelectUIHighlight;
    public GameObject RogueSelectUIHighlight;
    public GameObject WizardSelectUIHighlight;

    public int selectedClass = 3;

    public void Awake()
    {
        if(PlayerPrefs.HasKey("ServerIP"))
        {
            server.text = PlayerPrefs.GetString("ServerIP");
        }
        
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            PlayerNameField.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void selectClass(int NewlySelectedClass)
    {
        switch(NewlySelectedClass)
        {
            //Warrior
            case 1:
                WarriorSelectUIHighlight.SetActive(true);
                RogueSelectUIHighlight.SetActive(false);
                WizardSelectUIHighlight.SetActive(false);
                selectedClass = NewlySelectedClass;
                break;

            //Rogue
            case 2:
                WarriorSelectUIHighlight.SetActive(false);
                RogueSelectUIHighlight.SetActive(true);
                WizardSelectUIHighlight.SetActive(false);
                selectedClass = NewlySelectedClass;
                break;

            //Wizard
            case 3:
                WarriorSelectUIHighlight.SetActive(false);
                RogueSelectUIHighlight.SetActive(false);
                WizardSelectUIHighlight.SetActive(true);
                selectedClass = NewlySelectedClass;
                break;

            default:
                Debug.Log("ERROR : Invalid class selelection " + NewlySelectedClass);
                break;
        }
    }

    public void Connect()
    {
        PlayerPrefs.SetString("PlayerName", PlayerNameField.text);
        PlayerPrefs.SetInt("PlayerClass",selectedClass);
        PlayerPrefs.SetString("ServerIP", server.text);
        client.ConnectToServer(server.text, int.Parse(port.text));
    }
}
