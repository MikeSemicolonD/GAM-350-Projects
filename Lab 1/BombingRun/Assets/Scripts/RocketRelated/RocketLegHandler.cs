using UnityEngine;

public class RocketLegHandler : MonoBehaviour
{
    public Animation[] Legs;

    public void PullUp()
    {
        for(int i = 0; i < Legs.Length; i++)
        {
            Legs[i].Play();
        }
    }

    public void ResetLegs()
    {
        for (int i = 0; i < Legs.Length; i++)
        {
            Legs[i].transform.localEulerAngles = Vector3.zero;
        }
    }
}
