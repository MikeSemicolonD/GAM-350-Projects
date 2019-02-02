using UnityEngine;

public class AutoRotateX : MonoBehaviour {

    public float xSpeed = 1f;
    
	void Update ()
    {
        transform.Rotate(xSpeed,0,0,Space.Self);
	}
}
