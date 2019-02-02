using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Meant to exhibit the behaviour of a cube exploding into peices.
/// The cube itself is already broken up beforehand 
/// (The fracture peices are expected to be assigned to those peices through the inspector)
/// </summary>
public class DestructableObject : MonoBehaviour {

    public List<Rigidbody> FracturedPeices;
    private bool hasBeenDestroyed;
    
    /// <summary>
    /// Disables the box collider that's connected to the object
    /// Then disables kinematics on all rigidboy peices while also unparenting them
    /// </summary>
    public void DestroyObject()
    {
        GetComponent<BoxCollider>().enabled = false;

        for (int i = 0; i < FracturedPeices.Count; i++)
        {
            FracturedPeices[i].isKinematic = false;
            FracturedPeices[i].transform.parent = null;
        }

        hasBeenDestroyed = true;
    }

    public bool HasBeenDestroyed()
    {
        return hasBeenDestroyed;
    }
}
