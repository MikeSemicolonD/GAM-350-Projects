using System;
using UnityEngine;

[Serializable]
public class PlayerMovementInput {

    private float horizontal, vertical;
    private char playerValue = '0';

    public void SetPlayerValue(char val)
    {
        playerValue = val;
    }

    public Vector3 GetMovementInput(Transform playerRotation,float xSensitivity, float ySensitivity)
    {
        horizontal = Input.GetAxisRaw("Horizontal_Player" + playerValue);
        vertical = Input.GetAxisRaw("Vertical_Player" + playerValue);

        return (playerRotation.forward*vertical+playerRotation.right*horizontal).normalized;
    }
}
