using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class DragAndDrop : MonoBehaviour
{
    bool mousePosA = false;
    Vector2Int nodePosInt = new Vector2Int();
    Vector2Int mousePosInt = new Vector2Int();
    public Tilemap targetGrid;
    //Vector3 worldPos;
    //Vector3 screenCursorPos;
    Vector2 mousePos = new Vector2();
    readonly Vector2Int[] nodeDirections = new[] { new Vector2Int(), new Vector2Int(), new Vector2Int(), new Vector2Int() };
    Vector2Int[] adjacentCells = new[] { new Vector2Int(), new Vector2Int(), new Vector2Int(), new Vector2Int() };
    Vector3Int snapPos = new Vector3Int();
    Vector3Int targetPos = new Vector3Int(17, 0, 0);
    Vector3Int startPos = new Vector3Int(1, 6, 0);
    public Tile highlight;
    BoundsInt mazeBounds;
    public Image blockerPanel;
    public Text finishText;
    public Tile wallBlock;

    private void Start()
    {
        nodeDirections[0] = Vector2Int.up;
        nodeDirections[1] = Vector2Int.down;
        nodeDirections[2] = Vector2Int.left;
        nodeDirections[3] = Vector2Int.right;
        mazeBounds.SetMinMax(new Vector3Int(0, 0, 0), new Vector3Int(19, 7, 1));
        SetNode();
    }

    public void SetNode()
    {
        transform.position = targetGrid.GetCellCenterWorld(startPos);
    }
    void OnMouseDrag()
    {
        targetGrid.SetTile(startPos, highlight);
        nodePosInt = (Vector2Int)targetGrid.WorldToCell(transform.position);
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosInt = (Vector2Int)targetGrid.WorldToCell(mousePos);
        for (int i = 0; i < adjacentCells.Length; i++)
        {
            adjacentCells[i] = nodePosInt + nodeDirections[i];
        }
        if (mousePosInt != nodePosInt)
        {
            Debug.Log(mousePosInt);
            if (mazeBounds.Contains((Vector3Int)mousePosInt))
            {
                Debug.Log("passed Contains");
                foreach (Vector2Int a in adjacentCells)
                {
                    if (mousePosInt == a)
                        mousePosA = true;
                }
                if (mousePosA)
                {
                    snapPos = (Vector3Int)mousePosInt;
                    if (!targetGrid.HasTile(snapPos))
                    {
                        transform.position = targetGrid.GetCellCenterWorld(snapPos);
                        targetGrid.SetTile(snapPos, highlight);
                        //Debug.Log("snapPos is " + snapPos.ToString());
                    }
                    mousePosA = false;
                }

            }

        }
        if (snapPos == targetPos )
        {
            CompleteMaze();
        }

    }

    private void CompleteMaze()
    {
        Debug.Log("target reached!");
        StartCoroutine(CompleteNodeDropDown());
        targetGrid.GetComponent<WeatherNodeGen>().ResetMaze();
        targetGrid.GetComponent<WeatherNodeGen>().GenMaze();
        SetNode();
        snapPos.Set(0, 0, 0);
    }

    private void OnMouseUp()
    {
        if(snapPos != targetPos)
        {
            transform.position = targetGrid.GetCellCenterWorld(startPos);
            targetGrid.SwapTile(highlight, null);
        }
    }

    IEnumerator CompleteNodeDropDown()
    {
        Debug.Log("COROUTINE");
        blockerPanel.raycastTarget = true;
        blockerPanel.GetComponent<Animator>().SetTrigger("CompleteNode");
        yield return new WaitUntil(() => blockerPanel.raycastTarget == false);
        blockerPanel.GetComponent<Animator>().SetTrigger("CompleteNode");
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
