using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a basic grid of cubes, while also giving an option to set cubes as blocked
/// by : Michael Frye
/// </summary>
public class MapCreator : MonoBehaviour
{
    const float positionXOffset = 1.25f;
    const float positionZOffset = 1.25f;

    const float positionY = -3.75f;

    static MapCreator instance;

    public float CurrentXPosition = 0f;
    public float CurrentZPosition = 0f;

    public GameObject[,] mapBlockInstances;

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
    public static MapCreator GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("MapCreator is uninitialized");
            return null;
        }
        return instance;
    }

    // Returns a vector representation of the position the player should be
    public Vector3 GetBlockPosition(int x, int y)
    {
        return mapBlockInstances[x, y].transform.position+Vector3.up;
    }

    //Creates a map using primitive cubes
    public void CreateMap(int x, int y)
    {
        mapBlockInstances = new GameObject[x,y];

        for(int i = 0; i < y; i++)
        {
            for (int i2 = 0; i2 < x; i2++)
            {
                mapBlockInstances[i2, i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mapBlockInstances[i2, i].transform.position = new Vector3(CurrentXPosition, positionY, CurrentZPosition);
                mapBlockInstances[i2, i].transform.parent = transform;

                CurrentXPosition += positionXOffset;
                
            }
            CurrentXPosition = 0f;
            CurrentZPosition += positionZOffset;
        }
    }

    //Disable the cube that's considered 'blocked'
    public void SetBlockedParts(int x, int y)
    {
        mapBlockInstances[x, y].SetActive(false);
    }
}
