using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPAGraph
{
    public int level;
    public List<HPACluster> clusters;
    public HPAGraph(int level_)
    {
        level = level_;
        clusters = new List<HPACluster>();
    }

    public HPACluster GetClusterByNode(HPANode node)
    {
        foreach(var c in clusters)
        {
            if(c.IsInBoundary(node.tile))
            {
                return c;
            }
        }
        return null;
    }

}
