using UnityEngine;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour {

    static GameBoard instance;

    public List<GameObject> boardParts;

    public GameObject YellowWinsParticles;
    public GameObject RedWinsParticles;

    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Singleton support
    public static GameBoard GetInstance()
    {
        return instance;
    }

    /// <summary>
    /// Enables a set of particles to show who won the game
    /// </summary>
    /// <param name="pieceID"></param>
    public void SetWinner(int pieceID)
    {
        if (pieceID == 2)
        {
            YellowWinsParticles.SetActive(true);
        }
        else
        {
            RedWinsParticles.SetActive(true);
        }
    }
}
