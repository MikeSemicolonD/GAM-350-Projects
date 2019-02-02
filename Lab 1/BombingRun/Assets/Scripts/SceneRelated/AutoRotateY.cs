using UnityEngine;

public class AutoRotateY : MonoBehaviour {

    public float ySpeed = 1f;
    
	void Update ()
    {
        transform.Rotate(0,ySpeed,0,Space.Self);
	}
}
