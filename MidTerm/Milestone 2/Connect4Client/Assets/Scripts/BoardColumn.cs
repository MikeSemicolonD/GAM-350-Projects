using System.Collections.Generic;
using UnityEngine;

public class BoardColumn : MonoBehaviour {

    //Assumed to be ordered from bottom to top
    public List<GameObject> pieces;

    public int columnIndexId = 0;

    /// <summary>
    /// Client side:
    /// When a client clicks a button to place a piece it'll call this function
    /// Client will end their turn if they place a piece sucessfully
    /// </summary>
    /// <param name="pieceID"></param>
    public void AddPiece(int pieceID)
    {
        //Loop til we hit the end or a piece that's filled
        for (int i = 0; i < pieces.Count; i++)
        {
            //If we hit a piece that's been placed, place one ontop of that piece
            if(pieces[i].activeSelf)
            {
                //We're at the top of the column, break
                if (i == 0)
                {
                    ExampleClient.GetInstance().UpdateServerStatus("Can't place a piece there.");
                    Debug.Log("No piece added");
                    break;
                }

                Debug.Log("We found a piece that's been placed, so lets place a piece before it.");
                pieces[i - 1].SetActive(true);
                pieces[i - 1].GetComponent<MeshRenderer>().material = (pieceID == 2) ? Connect4Board.GetInstance().team1 : (pieceID == 3) ? Connect4Board.GetInstance().team2 : Connect4Board.GetInstance().team0;
                ExampleClient.GetInstance().SendBoardStateData(i - 1, columnIndexId, pieceID);
                ExampleClient.GetInstance().clientNet.CallRPC("CallStartTurnOnClients", UCNetwork.MessageReceiver.ServerOnly, -1, true);
                Connect4Board.GetInstance().EndThisClientsTurn();
                Connect4Board.GetInstance().ActivateAndPlaceParticles(pieces[i-1].transform.position,pieceID);
                break;
            }
            //If we hit the bottom and there's no piece there, place one
            else if (!pieces[i].activeSelf && i == pieces.Count-1)
            {
                Debug.Log("We reached the end, and added a piece. i = "+i);
                pieces[i].SetActive(true);
                pieces[i].GetComponent<MeshRenderer>().material = (pieceID == 2) ? Connect4Board.GetInstance().team1 : (pieceID == 3) ? Connect4Board.GetInstance().team2 : Connect4Board.GetInstance().team0;
                ExampleClient.GetInstance().SendBoardStateData(i, columnIndexId, pieceID);
                ExampleClient.GetInstance().clientNet.CallRPC("CallStartTurnOnClients", UCNetwork.MessageReceiver.ServerOnly, -1, true);
                Connect4Board.GetInstance().EndThisClientsTurn();
                Connect4Board.GetInstance().ActivateAndPlaceParticles(pieces[i].transform.position, pieceID);
                break;
            }
        }
    }
    
    /// <summary>
    /// Server calls this
    /// </summary>
    /// <param name="row"></param>
    /// <param name="pieceID"></param>
    public void AddPiece(int row, int pieceID)
    {
        if (row > 5)
        {
            Debug.Log("Can't add a piece");
            return;
        }
        else
        {

            Debug.Log("Add Piece called on column " + row + ". With type " + pieceID);

            if (!pieces[row].activeSelf)
            {
                pieces[row].SetActive(true);
                pieces[row].GetComponent<MeshRenderer>().material = (pieceID == 2) ? Connect4Board.GetInstance().team1 : (pieceID == 3) ? Connect4Board.GetInstance().team2 : Connect4Board.GetInstance().team0;
                Connect4Board.GetInstance().ActivateAndPlaceParticles(pieces[row].transform.position, pieceID);
            }
        }
    }

}
