using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/*
 * This abomination of a script (attached to the WeatherNodeMarker object) handles the interaction/navigation of the node through the maze as well 
 * as the procedure for solving it, with a pinch of maze setup. Like a little code salad.
 */

public class DragAndDrop : MonoBehaviour
{
    // Used for judging if the mouse position is in a cell adjacent to the node's position
    bool mousePosA = false;

    // The highlight tiles mark the trail created by dragging the node through the maze
    public Tile highlight;
    public Tile wallBlock;
    public Tilemap targetGrid;

    // Used for when the maze is completed
    public Image blockerPanel;
    // I don't know why I put this here, but... here it is :|
    public Text finishText;
    
    // Used to define a "valid area" on the tilemap that the node is supposed to stay in
    BoundsInt mazeBounds;

    // Because the method needed to get the mouse's position returns a Vector2, this variable holds it so it can be converted into a more usable Vector2Int
    Vector2 mousePos = new Vector2();

    // The coordinates in which the node currently sits
    Vector2Int nodePosInt = new Vector2Int();

    // The coordinates in which the mouse cursor sits (when this doesn't match nodePosInt, the algorithm then determines if this is a valid target for the node)
    Vector2Int mousePosInt = new Vector2Int();

    // This keeps track of the cells directly adjacent to the node
    Vector2Int[] adjacentCells = new[] { new Vector2Int(), new Vector2Int(), new Vector2Int(), new Vector2Int() };

    // This is used to calculate what the adjacentCells are
    readonly Vector2Int[] nodeDirections = new[] { new Vector2Int(), new Vector2Int(), new Vector2Int(), new Vector2Int() };

    // Full disclosure: I don't remember exactly why I used Vector3Ints for these instead of Vector2Ints. I think it had something to do with the methods returning
    // Vector3Ints and I couldn't just cast them into Vector2Ints or something
    Vector3Int snapPos = new Vector3Int();
    Vector3Int targetPos = new Vector3Int(17, 0, 0);
    Vector3Int startPos = new Vector3Int(1, 6, 0);

    // The nodeDirections array has to be initialized and the area of the mazeBounds is set (think of it as a rectangle with the bottom-left corner being the 
    // first variable and the top-right being the second). SetNode is also called here for some reason that I'm sure was useful.
    private void Start()
    {
        nodeDirections[0] = Vector2Int.up;
        nodeDirections[1] = Vector2Int.down;
        nodeDirections[2] = Vector2Int.left;
        nodeDirections[3] = Vector2Int.right;
        mazeBounds.SetMinMax(new Vector3Int(0, 0, 0), new Vector3Int(19, 7, 1));
        SetNode();
    }

    // Rather self-explanatory. Sets the node to the starting position of the maze (which is needed when the maze is solved or the player stops dragging the node)

    public void SetNode()
    {
        transform.position = targetGrid.GetCellCenterWorld(startPos);
    }

    // Where all the navigation stuff is done. As soon as this is triggered, the cell at the starting position (where the node is already sitting) is highlighted
    // and nodePosInt and mousePosInt are given their respective coordinates. Then the adjacentCells array is filled by using nodeDirections to calculate the cells 
    // adjacent from nodePosInt. Now the algorithm checks for whether the mouse and node positions match (since this is in OnMouseDrag() this whole sequence is 
    // called every frame that the mouse is held down). When they don't, the mazeBounds (the boundary rectangle made for the maze) is checked to make sure that the
    // mouse is still in it. If it is, then the adjacentCells array is checked to make sure that the mouse position matches one of them; if it does match, then one
    // more check needs to be done to make sure that the mouse position's cell doesn't have a tile on it (the node can only move into empty cells). If it passes 
    // this, then finally the node can be moved to the mouse's position and that position can be highlighted.
    void OnMouseDrag()
    {
        targetGrid.SetTile(startPos, highlight);
        nodePosInt = (Vector2Int)targetGrid.WorldToCell(transform.position);
        // because ScreenToWorldPoint() gives a position from a different "space" than the grid, we have to use WorldToCell() to convert it to actual coordinates
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosInt = (Vector2Int)targetGrid.WorldToCell(mousePos);
        // this is where the cells adjacent to the current one (nodePosInt) are calculated
        for (int i = 0; i < adjacentCells.Length; i++)
        {
            // since nodeDirections consists of up/down/left/right, adding each one to nodePosInt gives the adjacent cell in that direction.
            adjacentCells[i] = nodePosInt + nodeDirections[i];
        }
        // if this is true, then the mouse cursor has moved off of the node's cell and we start evaluating the new cell to see if it's valid
        if (mousePosInt != nodePosInt)
        {
            Debug.Log(mousePosInt);
            // this checks the mazeBounds thing to see if the new cell would be inside of it
            if (mazeBounds.Contains((Vector3Int)mousePosInt))
            {
                Debug.Log("passed Contains");
                // this checks to make sure that the new cell is one of the adjacentCells
                foreach (Vector2Int a in adjacentCells)
                {
                    if (mousePosInt == a)
                        mousePosA = true;
                }
                if (mousePosA)
                {
                    // this is where I take mousePosInt and put it into a Vector3Int. I don't remember why I did this, but I can only assume I needed to
                    snapPos = (Vector3Int)mousePosInt;
                    // this checks to make sure that the new cell doesn't have a tile occupying it already; if it does, then it's a block, a wall, or a path which
                    // would never be valid targets for obvious reasons
                    if (!targetGrid.HasTile(snapPos))
                    {
                        // this is where, if the new cell passes all of the checks, the node is moved to that position and the cell is highlighted
                        // I THINK this is why I needed a Vector3Int (because transform.position needs it), but don't quote me on that I don't remember
                        transform.position = targetGrid.GetCellCenterWorld(snapPos);
                        targetGrid.SetTile(snapPos, highlight);
                        //Debug.Log("snapPos is " + snapPos.ToString());
                    }
                    // then this is set back to false in case it was changed
                    mousePosA = false;
                }

            }

        }
        // If this is true, then the node is at the end of the maze and the maze completion stuff can begin.
        // Technically, checking snapPos isn't ideal because snapPos is set to the new cell's position before it's cleared as a valid target.
        // If there was a tile on the new cell, then that cell could be invalidated; but if that new cell was targetPos, then the if statement below would 
        // still be true. However, because the WeatherNodeGen prevents a tile from being put on targetPos, this isn't a problem for us. 
        if (snapPos == targetPos )
        {
            CompleteMaze();
        }

    }

    // This is triggered when the player gets the node to the targetPos. A coroutine (basically a method that can be run more "slowly" than normal)
    // plays that displays a "completed" message, the maze is reset and a new one is generated, and snapPos is set to (0,0,0) to make sure that 
    // the OnMouseDrag() method has a "clean slate" to work with (I think)
    private void CompleteMaze()
    {
        Debug.Log("target reached!");
        StartCoroutine(CompleteNodeDropDown());
        targetGrid.GetComponent<WeatherNodeGen>().ResetMaze();
        targetGrid.GetComponent<WeatherNodeGen>().GenMaze();
        SetNode();
        snapPos.Set(0, 0, 0);
    }

    // This is triggered when the player lets go of the node they are dragging. It checks to see if the node is at the maze endpoint and if it's
    // not, it sets the node back to the starting position and clears any highlighted tiles
    private void OnMouseUp()
    {
        // if OnMouseUp is being triggered, then this is basically always true because CompleteMaze() would be triggered if snapPos == targetPos
        if(snapPos != targetPos)
        {
            // I realize as I'm writing of these comments that I could've called SetNode() instead of writing this out, but I didn't 
            // probably because I wrote SetNode() after writing this thing and forgot about it
            transform.position = targetGrid.GetCellCenterWorld(startPos);
            targetGrid.SwapTile(highlight, null);
        }
    }

    // This is the coroutine being called in CompleteMaze(). While a normal method runs every instruction as quickly as possible, this
    // can be "slowed down" to run in a certain timeframe. It can be used to make animations without having to use Unity's animation
    // system, or it can be used with animations like I did here to create sequences (kind of like cutscenes), or other stuff I'm sure.
    // Here, a currently-invisible UI panel that normally sits over the maze becomes a raycast target, which effectively blocks clicks 
    // from reaching anything underneath it. Then, an animation on that panel is triggered that notifies the player that they've properly
    // solved the maze. Only when that animation is complete does the trigger reset the animator and the panel becomes unclickable again.
    IEnumerator CompleteNodeDropDown()
    {
        Debug.Log("COROUTINE");
        // there's a couple of ways that a panel like this can be handled, but in this case this way worked best IMO
        blockerPanel.raycastTarget = true;
        // the panel's animator begins in the "neutral" state, and this causes it to move to the PanelFadeIn state (which is where the animation is)
        blockerPanel.GetComponent<Animator>().SetTrigger("CompleteNode");
        // this tells the coroutine to wait until the panel's raycastTarget is disabled, which happens at the very end of the animation clip
        yield return new WaitUntil(() => blockerPanel.raycastTarget == false);
        // this moves the animator back to the neutral state so it's in the right place the next time this coroutine happens
        blockerPanel.GetComponent<Animator>().SetTrigger("CompleteNode");
        // this makes extra super duper sure that this is set to false again
        blockerPanel.raycastTarget = false;
    }

    //void Start()
    //{
    //    objectPos = targetGrid.WorldToLocal(transform.position);
    //    Debug.Log("objectPos Local is at " + objectPos.ToString());
    //}

    //void OnMouseDown()
    //{
    //    nodePosInt = (Vector2Int)targetGrid.WorldToCell(transform.position);
    //    mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    Debug.Log("mousePos is " + mousePos.ToString());
    //    mousePosInt = (Vector2Int)targetGrid.WorldToCell(mousePos);
    //    //Debug.Log("nodePosInt is " + nodePosInt.ToString());
    //    Debug.Log("mousePosInt is " + mousePosInt.ToString());
    //}



}
