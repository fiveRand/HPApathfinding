using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPANode
{
    public HPATile tile;
    public List<HPAEdge> edges;

    public HPANode(HPATile tile_)
    {
        tile = tile_;
        edges = new List<HPAEdge>();
    }

    public void PrintThis()
    {
        Debug.Log("x : " + tile.pos.x + " , " + "y :" + tile.pos.y);
    }
}
