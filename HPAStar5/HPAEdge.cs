using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPAEdge 
{
    public HPANode nStart, nEnd;
    public HPAEdgeType type;
    public float weight;

    public LinkedList<HPATile> path;

    public HPAEdge() { }
    public HPAEdge(HPANode pStart_, HPANode pEnd_, HPAEdgeType type_,float weight_ =1)
    {
        nStart = pStart_;
        nEnd = pEnd_;
        type = type_;
        weight = weight_;
        path = new LinkedList<HPATile>();
    }

}

public enum HPAEdgeType
{
    INTRA, // EntranceNode that inside Cluster that compute length eachother
    INTER // Connect Cluster, length is alway 1.
}
