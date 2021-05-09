using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/* 
 * This handles all the procedural generation for the maze. It uses a Tilemap and a variation of a passage-carving maze gen algorithm to create a path of tiles, 
 * then add walls at random on the appropriate tiles. The result is a maze that can have multiple solutions, isolated areas, dead ends, loops, and junctions -- 
 * just like the original WN puzzle from Among Us.
 */

public class WeatherNodeGen : MonoBehaviour
{

    // The coordinates of the cell being evaluated, used by both GenPath and PlaceWalls. It's initialized to the starting position in the maze
    Vector2Int currentCell = new Vector2Int(1,6);

    // The directions in which the currentCell can move. Normally it is just up[0] down[1] left[2] and right[3], but can be changed depending on the currentCell's position.
    Vector2Int[] pathDirections = new Vector2Int[] { new Vector2Int(), new Vector2Int(), new Vector2Int(), new Vector2Int() } ;

    // Used by GenPath to determine the next direction as well as WallPlace() to determine whether or not to place down a wall. In GenPath(), the range is 0-3; in WallPlace(),
    // the range is 0-10. Having these methods share a variable for randomization is probably bad practice because the ranges are different, but in this case I don't give a fuck
    int indexRand;

    // The list of all coordinates in the grid that are designated as path cells for the current maze.
    List<Vector2Int> pathCells = new List<Vector2Int>();

    // This and nextCell02 are used in GenPath. Because of the maze's configuration, moves have to be made in increments of two blocks; both of these cells need to be compared
    // against the pathCells list to make sure that they don't cross over any pathTiles
    Vector2Int nextCell01 = new Vector2Int(); 
    Vector2Int nextCell02 = new Vector2Int();

    // If this is true, then the coordinates of the randomly chosen index needs to be checked to make sure it isn't moving left
    bool cantGoLeft;

    // Used by GenPath() and WallPlace() to determine if the evaluated cell is valid
    bool isValidCell;

    public Tile wallV;
    public Tile wallH;
    public Tile path;

    // Start is called before the first frame update
    void Start()
    {
        SetDirections();
    }


    // This array needs to be set not only on Start(), but also at the end of every GenPath() call (in case it was altered). The words up/down/left/right are just
    // shorthand for (0,1)/(0,-1)/(-1,0)/(1,0).
    public void SetDirections()
    {
        pathDirections[0] = Vector2Int.up;
        pathDirections[1] = Vector2Int.down;
        pathDirections[2] = Vector2Int.left;
        pathDirections[3] = Vector2Int.right;
    }

    // Rather self-explanatory. While other properties (like indexRand) will most likely be changed from their initial values, resetting those values is only important for 
    // these ones here -- others can just be written over as needed.
    public void ResetMaze()
    {
        currentCell.x = 1;
        currentCell.y = 6;
        SetDirections();
        pathCells.Clear();
        GetComponent<Tilemap>().SwapTile(path, null);
        GetComponent<Tilemap>().SwapTile(wallH, null);
        GetComponent<Tilemap>().SwapTile(wallV, null);
    }

    // The star of the show. ResetMaze() is always called first to ensure that the algorithm is working with a clean slate. It immediately adds the starting position 
    // cell (1,6) to pathCells, moves down to (1,5), and adds that cell. Then it begins using GenPath() to carve a path to the end position (17,0). The way GenPath() is 
    // written ensures that it will eventually reach the furthest column which is directly above the end position. At that point, the algorithm stops using GenPath()
    // and simply carves downward until it reaches (17,0). After the path is complete, PlaceWalls() is called to... place walls.

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

    // This is where the maze gen magic actually happens. (Buckle in)
    // First, the the currentCell's coordinates are checked to see if the pathDirections array should be altered. While the array doesn't absolutely need to change,
    // doing so helps the next step (randomly choosing a direction) avoid rolling a direction that is always invalid. When a direction is rolled, the next two
    // coordinates are calculated and then compared against the list of pathCells to see if either of them match any of the ones in the list. If neither nextCells01
    // or nextCell02 match the pathCells list, then the cells are valid and they're both added to the list (and currentCell being updated to match). Whether or not 
    // the cells are valid, GenPath() then ends by resetting the booleans used and calling SetDirections() to reset the pathDirections array.

    void GenPath()
    {

        // If this condition is true, then the currentCell is at the bottom-most row and would never be able to move down; thus, the index for "down" is replaced with "up".
        if (currentCell.y == 1)
            pathDirections[1] = Vector2Int.up;

        // If this is true, then currentCell is at the top row; thus, the index for "up" is replaced with "down".
        if (currentCell.y == 5)
            pathDirections[0] = Vector2Int.down;

        // If this is true, then currentCell is at the left-most column; thus, the index for "left" is replaced with "right".
        if (currentCell.x == 1)
            pathDirections[2] = Vector2Int.right;

        // If the currentCell has any of these coordinates, then turning left would result in an unsolvable maze.
        // As I write these comments weeks after this code, I realize that I could've just replaced the "left" index of pathDirections with something else. But I didn't.
        // I don't remember why I did it like this, but I'll be damned if I so much as breathe on any of this code right now so here it will remain.
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

    // Now that the path has been generated, walls can be placed. The algorithm iterates through each cell (starting from the bottom left of the grid, by going across 
    // a whole row before moving up the next column each time) and first checks if it's a path cell. If it doesn't, then it can check if it's a valid target for a wall.
    //
    // Note that walls can only be placed on cells that are directly adjacent to block tiles. Because the placement of the blocks is uniform, I can use a simple 
    // equation to determine this. On my grid, blocks are at every cell that has an even-numbered x AND y coordinate; so for walls to be directly adjacent, they have to
    // have at one coordinate that is also even. This means that walls can never be placed at cells that have an odd-numbered x AND y coordinate. So as long as the
    // currentCell has one even coordinate, it's valid.
    // Now the currentCell has to be checked to see which coordinate is even-numbered (since they will always have one odd and one even coordinate). There are two
    // types of wall tiles: horizontal and vertical. When it's determined which wall should be used, then the WallPlace() method is called with that tile. 
    // Note that this doesn't guarantee that a wall will be placed; WallPlace() is what actually makes that decision.

    public void PlaceWalls()
    {
        // the maze is 19 x 7 cells, starting at (0,0)
        // technically, the 6 and 18 in these loops should be 7 and 19 respectively, but I didn't notice until I wrote this comment weeks after finishing the code
        // and I remain adamant in refusing to touch any of this shit -- just be aware that they should be 7 and 19 in order to replicate the original WN maze.
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
                // this checks if both the x and y coordinates are odd-numbered (the percentage meaning that the result is the remainder after division)
                if (currentCell.x % 2 == 0 && currentCell.y % 2 == 0)
                {
                    isValidCell = false;
                }
                // this is where the type of wall tile is determined
                if (isValidCell)
                {
                    // if the currentCell has an even y coordinate, it uses the horizontal tile
                    if (currentCell.y % 2 == 0)
                    {
                        WallPlace(wallH);
                    }
                    // if the currentCell has an even x coordinate, it uses the vertical tile.
                    // Technically, the if statement doesn't need to be here, since if the previous one is false then this will always be true, but I just
                    // liked the uniformity. And also didn't realize it until I wrote this comment.
                    else if (currentCell.x % 2 == 0)
                    {
                        WallPlace(wallV);
                    }

                    // this didn't need to be here but... here it is <_<
                    isValidCell = true;
                }
                else
                {
                    // after everything is done, this is reset in case it was set to false
                    isValidCell = true;
                }
            }


        }
    }

    // When PlaceWalls() has determined that its currentCell is a valid wall target, it calls this method (passing the appropriate wall tile for it to add.
    // indexRand is assigned a random integer from 0-10 and if it is 2 or greater, it puts the corresponding wall tile on the currentCell. While I could've 
    // just used 2 numbers (since it's a yes/no question), I wanted it to be more likely that it would place a wall than not -- hence the larger range.
    void WallPlace(Tile wall)
    {
        indexRand = Random.Range(0, 10);
        if (indexRand >= 2)
        {
            GetComponent<Tilemap>().SetTile((Vector3Int)currentCell, wall);
        }

    }
}
