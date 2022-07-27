using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Priority_Queue;

public static class HPAPathfinder 
{

    public static LinkedList<HPAEdge> TestFindPath(Vector3 startPos, Vector3 endPos, HPAGrid grid)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        // 1. 노드를 각 레벨로 클러스터 연결
        HPANode startNode = grid.TestInsertNode(startPos);
        HPANode endNode = grid.TestInsertNode(endPos);

        if (startNode == null || endNode == null) // 없으면 포기
        {
            Debug.LogError("EMPTY");
            return null;
        }

        // 2. 가장 높은 레벨에서 탐색 
        var abstractpath = HierarchicalPath(startNode, endNode);
        grid.RemoveTempNode();

        Debug.Log("millisecond : " + sw.Elapsed.TotalMilliseconds);
        Debug.Log("second : " + sw.Elapsed.TotalSeconds);
        sw.Reset();
        return abstractpath;
    }

    public static LinkedList<HPAEdge> SearchForPath(HPANode nStart, HPANode nEnd, HPAGrid grid)
    {



        return null;
    }

    public static LinkedList<HPAEdge> HierarchicalPath(HPANode nStart,HPANode nEnd, HPACluster clst = null)
    {
        HashSet<HPANode> visited = new HashSet<HPANode>();
        Dictionary<HPATile, HPAEdge> cameFrom = new Dictionary<HPATile, HPAEdge>();
        Dictionary<HPATile, float> score = new Dictionary<HPATile, float>();
        SimplePriorityQueue<HPANode> pq = new SimplePriorityQueue<HPANode>();

        score[nStart.tile] = 0;
        pq.Enqueue(nStart, EuclidianDistance(nStart.tile,nEnd.tile));

        while(pq.Count > 0)
        {
            HPANode cur = pq.Dequeue();


            if(cur.tile == nEnd.tile)
            {
                Debug.Log("FOUND");
                return RebuildPath(cameFrom, nEnd);
            }
            visited.Add(cur);
            // score.TryGetValue(cur.tile, out float curScore);
            foreach(HPAEdge e in cur.edges)
            {
                if (clst != null && !clst.IsInBoundary(e.nEnd.tile))
                    continue;
                if (visited.Contains(e.nEnd))
                    continue;
                float tempScore = score[cur.tile] + e.weight;

                if (score.TryGetValue(e.nEnd.tile, out float prevScore) && tempScore >= prevScore)
                    continue;

                cameFrom[e.nEnd.tile] = e;
                score[e.nEnd.tile] = tempScore;
                pq.Enqueue(e.nEnd, tempScore + EuclidianDistance(e.nEnd.tile,nEnd.tile));
            }
        }
        return new LinkedList<HPAEdge>();
    }
    static LinkedList<HPAEdge> RebuildPath(Dictionary<HPATile, HPAEdge> cameFrom,HPANode endNode)
    {
        LinkedList<HPAEdge> tempList = new LinkedList<HPAEdge>();
        HPANode cur = endNode;

        while(cameFrom.TryGetValue(cur.tile,out HPAEdge e))
        {
            tempList.AddFirst(e);
            cur = e.nStart;
        }
        return tempList;
    }
    static float EuclidianDistance(HPATile a, HPATile b)
    {
        return
            Mathf.Sqrt(Mathf.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2));
    }

}
