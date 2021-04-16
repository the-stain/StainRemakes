using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WeatherNodeGen : MonoBehaviour
{
    Vector2Int currentCell = new Vector2Int(1,6); // the coordinates of the cell being evaluated. Used by both GenPath and PlaceWalls
    Vector2Int[] pathDirections = new Vector2Int[] { new Vector2Int(), new Vector2Int(), new Vector2Int(), new Vector2Int() } ;// the directions in which the currentCell can move
    int indexRand; // used by GenPath to determine the next direction as well as WallPlace to determine whether or not to place down a wall
    Vector2Int nextCell01 = new Vector2Int(); // this and nextCell02 are compared against the pathTiles List to make sure that the next move doesn't cross over a previously traveled cell
    Vector2Int nextCell02 = new Vector2Int(); 
    bool cantGoLeft;
    List<Vector2Int> pathCells = new List<Vector2Int>();
    bool isValidCell;
    public Tile wallV;
    public Tile wallH;
    public Tile path;

    List<Vector2Int> wallCells = new List<Vector2Int>();

    // Start is called before the first frame update
    void Start()
    {
        SetDirections();
    }

    public void SetDirections()
    {
        pathDirections[0] = Vector2Int.up;
        pathDirections[1] = Vector2Int.down;
        pathDirections[2] = Vector2Int.left;
        pathDirections[3] = Vector2Int.right;
    }

    public void ResetMaze()
    {
        currentCell.x = 1;
        currentCell.y = 6;
        SetDirections();
        pathCells.Clear();
        wallCells.Clear();
        GetComponent<Tilemap>().SwapTile(path, null);
        GetComponent<Tilemap>().SwapTile(wallH, null);
        GetComponent<Tilemap>().SwapTile(wallV, null);
    }

    public void GenMaze()
    {
        ResetMaze();
        pathCells.Add(currentCell);
        currentCell += Vector2Int.down;
        pathCells.Add(currentCell);
        while (currentCell.x < 17)
            GenPath();
        if (currentCell.x == 17)
            while (currentCell.y > 0)
            {
                currentCell += Vector2Int.down;
                pathCells.Add(currentCell);
            }
        PlaceWalls();
        //foreach (Vector2Int cell in pathCells)
        //{
        //    GetComponent<Tilemap>().SetTile((Vector3Int)cell, path);
        //    Debug.Log(cell.ToString());
        //}
    }

    void GenPath()
    {
        if (currentCell.y == 1)
            pathDirections[1] = Vector2Int.up;

        if (currentCell.y == 5)
            pathDirections[0] = Vector2Int.down;

        if (currentCell.x == 1)
            pathDirections[2] = Vector2Int.right;

        if (currentCell.y == 1 || currentCell.y == 5 || currentCell.x == 1)
        {
            cantGoLeft = true;
        }
        indexRand = Random.Range(0, pathDirections.Length);
        nextCell01 = currentCell + pathDirections[indexRand];
        nextCell02 = nextCell01 + pathDirections[indexRand];
        if (cantGoLeft)
        {
            if (pathDirections[indexRand] == Vector2Int.left)
            {
                isValidCell = false;
            }
        }
        foreach (Vector2Int cell in pathCells)
        {
            if (nextCell01 == cell || nextCell02 == cell)
            {
                isValidCell = false;
            }
        }
        if (isValidCell)
        {
            pathCells.Add(nextCell01);
            pathCells.Add(nextCell02);
            currentCell = nextCell02;
        }
        isValidCell = true;
        cantGoLeft = false;
        SetDirections();
    }

    public void PlaceWalls()
    {
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 18; col++)
            {
                currentCell.x = col;
                currentCell.y = row;
                foreach (Vector2Int cell in pathCells)
                {
                    if (currentCell == cell)
                    {
                        isValidCell = false;
                    }
                }
                if (currentCell.x % 2 == 0 && currentCell.y % 2 == 0)
                {
                    isValidCell = false;
                }
                if (isValidCell)
                {
                    if (currentCell.y % 2 == 0)
                    {
                        WallPlace(wallH);
                    }
                    else if (currentCell.x % 2 == 0)
                    {
                        WallPlace(wallV);
                    }

                    isValidCell = true;
                }
                else
                {
                    isValidCell = true;
                }
            }


        }
    }

    void WallPlace(Tile wall)
    {
        indexRand = Random.Range(0, 10);
        if (indexRand >= 2)
        {
            GetComponent<Tilemap>().SetTile((Vector3Int)currentCell, wall);
        }

    }
}
