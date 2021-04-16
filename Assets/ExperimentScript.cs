using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class ExperimentScript : MonoBehaviour
{
    public Tilemap targetMap;
    public Tile piece01;
    public Tile piece02;
    public Vector3Int position = new Vector3Int(0, 0, 0);
    public Vector3Int positionOrigin = new Vector3Int(0, 0, 0);


    // Start is called before the first frame update
    void Start()
    {
        targetMap.SetTile(position, piece01);
        Debug.Log("targetMap is" + targetMap.size.ToString());
        Debug.Log("position coordinates are " + position.ToString());
        for (int i = 0; i < 5; i++)
        {
            targetMap.SetTile(position, piece01);
            //position.x += 1;
            //position.y += 1;
        }
        Debug.Log("targetMap origin: " + targetMap.origin.ToString());
        Debug.Log("targetMap cellBounds: " + targetMap.cellBounds.ToString());

        targetMap.SetTile(positionOrigin, piece02);

        Debug.Log("positionOrigin coordinates are " + positionOrigin.ToString());
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
