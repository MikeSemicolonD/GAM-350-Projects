using UnityEngine;

public class BoardButtons : MonoBehaviour {

    public int column = 0;

    private void OnMouseDown()
    {
        Debug.Log("Button Mouse Down Called");
        if (Connect4Board.GetInstance().isClientsTurn())
        {
            Connect4Board.GetInstance().AddPieceToBoard(column,ExampleClient.GetInstance().GetAssignedPiece());
        }
    }
}
