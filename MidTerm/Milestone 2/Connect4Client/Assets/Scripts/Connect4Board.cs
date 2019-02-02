using UnityEngine;
using System.Collections.Generic;

public class Connect4Board : MonoBehaviour {

    //team0 = piece 1
    //team1 = piece 2
    //team2 = piece 3
    public Material team0, team1, team2;

    static Connect4Board instance;

    public List<BoardButtons> Buttons;
    public List<BoardColumn> Columns;

    public List<GameObject> boardParts;

    public bool isThisPlayersTurn;

    public ParticleSystem placePieceParticleFX;

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
    public static Connect4Board GetInstance()
    {
        return instance;
    }

    public bool isClientsTurn()
    {
        return isThisPlayersTurn;
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

    /// <summary>
    /// Plays the particle system at a specific spot with a specific piece type color
    /// </summary>
    /// <param name="position"></param>
    /// <param name="pieceType"></param>
    public void ActivateAndPlaceParticles(Vector3 position,int pieceType)
    {
        placePieceParticleFX.gameObject.SetActive(true);

        placePieceParticleFX.transform.position = position;

        if(pieceType == 2)
        {
            placePieceParticleFX.GetComponent<ParticleSystemRenderer>().material = team1;
        }
        else
        {
            placePieceParticleFX.GetComponent<ParticleSystemRenderer>().material = team2;
        }

        placePieceParticleFX.Play();
    }

    /// <summary>
    /// Sets the color of the board itself in order to communicate to the client what color they are.
    /// </summary>
    /// <param name="type"></param>
    public void SetBoardColor(int type)
    {
        if(type == 2)
        {
            for(int i = 0; i < boardParts.Count; i++)
            {
                boardParts[i].GetComponent<MeshRenderer>().material = team1;
            }
        }
        else if (type == 3)
        {
            for (int i = 0; i < boardParts.Count; i++)
            {
                boardParts[i].GetComponent<MeshRenderer>().material = team2;
            }
        }
        else
        {
            Debug.Log("Change change board to given type = "+type);
        }
    }

    /// <summary>
    /// Gets called from the server with the peiceType that's gonna take the next turn.
    /// If this client has the same type as what's being passed in, 
    /// the client's buttons will enable, allowing the client to take his/her turn.
    /// </summary>
    /// <param name="peiceType"></param>
    public void StartTurn(int peiceType)
    {
        if (ExampleClient.GetInstance().assignedPieceType == peiceType)
        {
            EnableThisClientsTurn();
        }
        else
        {
            EndThisClientsTurn();
        }
    }

    /// <summary>
    /// Gets called from the Client's board buttons
    /// </summary>
    /// <param name="columnNumber"></param>
    /// <param name="pieceID"></param>
    public void AddPieceToBoard(int columnNumber, int pieceID)
    {
        Columns[columnNumber].AddPiece(pieceID);
    }

    /// <summary>
    /// Sets a peice on the board to a given type if it isn't already taken
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="pieceType"></param>
    public void SetBoardPiece(int column, int row, int pieceType)
    {
        if (pieceType != 0)
        {
            Columns[column].AddPiece(row, pieceType);
        }
    }

    /// <summary>
    /// Enables all buttons on this client's board if it's this client's turn
    /// </summary>
    public void EnableThisClientsTurn()
    {
        for(int i=0; i < Buttons.Count; i++)
        {
            Buttons[i].gameObject.SetActive(true);
        }

        isThisPlayersTurn = true;
    }

    /// <summary>
    /// Disables all buttons on this clients board if it's not this client's turn
    /// </summary>
    public void EndThisClientsTurn()
    {
        isThisPlayersTurn = false;

        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].gameObject.SetActive(false);
        }
    }

}
