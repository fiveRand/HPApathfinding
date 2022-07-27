using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPACluster
{
    public int lineSize;
    public Vector2Int index, minBoundary, maxBoundary;
    public Dictionary<HPATile, HPANode> entrances;
    public List<HPACluster> lowLevelCluster;

    public HPACluster(int lineSize_, int x, int y)
    {
        lineSize = lineSize_;
        index = new Vector2Int(x, y);
        minBoundary = new Vector2Int(x * lineSize, y * lineSize);
        maxBoundary = new Vector2Int(minBoundary.x + lineSize - 1, minBoundary.y + lineSize - 1);
        entrances = new Dictionary<HPATile, HPANode>();
    }
    public bool IsInBoundary(Vector2Int tile)
    {

        return
            tile.x >= minBoundary.x && tile.x <= maxBoundary.x &&
            tile.y >= minBoundary.y && tile.y <= maxBoundary.y;
    }
    public bool IsInBoundary(int x,int y)
    {
        return IsInBoundary(new Vector2Int(x, y));
    }

    public bool IsInBoundary(HPATile tile)
    {

        return
            tile.x >= minBoundary.x && tile.x <= maxBoundary.x &&
            tile.y >= minBoundary.y && tile.y <= maxBoundary.y;
    }
    public bool IsInBoundary(HPACluster other)
    {
        return
            other.minBoundary.x >= minBoundary.x &&
            other.minBoundary.y >= minBoundary.y &&
            other.maxBoundary.x <= maxBoundary.x &&
            other.maxBoundary.y <= maxBoundary.y;
    }

}
