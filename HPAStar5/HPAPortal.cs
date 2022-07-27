using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPAPortal
{
    public Vector2Int index;
    public List<HPAEdge> edges;
    public HPAPortal child;

    public HPAPortal(Vector2Int index_)
    {
        index = index_;
        edges = new List<HPAEdge>();
    }
    public HPAPortal(int x,int y)
    {
        index = new Vector2Int(x, y);
        edges = new List<HPAEdge>();
    }
}
