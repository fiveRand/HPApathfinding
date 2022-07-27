using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System;

public class HPAGrid : MonoBehaviour
{
    public HPATEST test;


    public Vector2Int gridSize = Vector2Int.one * 100;
    public int lowestClusterLineSize = 10;
    public int maxLevel = 3;
    int freeTile = 0;
    [SerializeField] TerrainType[] terrain;
    [SerializeField] bool drawGizmos;
    [SerializeField] bool showWeights;
    [SerializeField] int gizmosClusterLevel;

    public HPANode[,] nodes;
    public HPAGraph graph;
    public LinkedList<HPAEdge> TestPath = new LinkedList<HPAEdge>();
    
    bool isInitialize = false;
    List<HPANode> tempNodes = new List<HPANode>(2);
    int nodeDiameter = 1;
    float nodeRadius
    {
        get { return nodeDiameter * 0.5f; }
    }

    Vector2Int gridIndex
    {
        get
        {
            int gridIndexX = Mathf.RoundToInt(gridSize.x / nodeDiameter);
            int gridIndexY = Mathf.RoundToInt(gridSize.y / nodeDiameter);
            return new Vector2Int(gridIndexX,gridIndexY);
        }
    }


    HPACluster testClst;
    List<HPACluster> testClsts = new List<HPACluster>();
    
    private void Start()
    {
        ScanMap();
        var lineLevels = GetLine_And_Indexs();
        CreateCluster(lineLevels);

        testClst = graph.clusters[3];
        //testClsts = graphs[1].clusters[12].lowLevelCluster;
        isInitialize = true;
    }

    void ScanMap()
    {
        nodes = new HPANode[gridIndex.x, gridIndex.y];
        Vector3 wbl = transform.position - Vector3.right * gridSize.x / 2 - Vector3.up * gridSize.y / 2;

        for (int x = 0; x < gridIndex.x; x++)
        {
            for (int y = 0; y < gridIndex.y; y++)
            {
                Vector3 offsetX = Vector3.right * (x * nodeDiameter + nodeDiameter * 0.5f);
                Vector3 offsetY = Vector3.up * (y * nodeDiameter + nodeDiameter * 0.5f);
                Vector3 pos = wbl + offsetX + offsetY;

                int penalty = 0;
                bool imPassable = false;
                foreach (TerrainType region in terrain)
                {
                    bool hit = Physics2D.OverlapBox(pos, Vector2.one * nodeDiameter * 0.9f, 0, region.terrainLayer);

                    if (hit)
                    {
                        penalty = region.terrainPenalty;
                        imPassable = region.impassable;
                    }
                }
                nodes[x, y] = new HPANode(new HPATile(pos, x, y, penalty, imPassable));
            }
        }

    }

    List<KeyValuePair<int,Vector2Int>> GetLine_And_Indexs()
    {
        List<KeyValuePair<int, Vector2Int>> clstPair = new List<KeyValuePair<int, Vector2Int>>(maxLevel);
        int lineSIze = lowestClusterLineSize;
        for (int i = 0; i <= maxLevel; i++)
        {
            if (i != 0)
            {
                int num = 2;
                while (gridSize.x % (lineSIze * num) != 0)
                {

                    num++;
                }
                lineSIze *= num;
            }

            int clusterWidth = Mathf.CeilToInt(gridSize.x / lineSIze);
            int clusterHeight = Mathf.CeilToInt(gridSize.y / lineSIze);
            var index = new Vector2Int(clusterWidth, clusterHeight);

            if (clusterWidth <= 1 && clusterHeight <= 1)
            {
                maxLevel = i - 1;
                break;
            }
            clstPair.Add(new KeyValuePair<int, Vector2Int>(lineSIze,index));
        }
        return clstPair;
    }

    void CreateCluster(List<KeyValuePair<int, Vector2Int>> clstMaterials)
    {
        int lineSize = clstMaterials[0].Key;
        Vector2Int index = clstMaterials[0].Value;
        graph = new HPAGraph(0);
        graph.clusters = BuildCluster(0, lineSize, index.x, index.y);
        foreach (var clst in graph.clusters)
        {
            CreateINTRAEdge(0, clst);
        }
    }

    void CreateINTRAEdge(int level,HPACluster clst)
    {
        List<HPANode> nodes = new List<HPANode>(clst.entrances.Values);
        for (int i = 0; i < nodes.Count; i++)
        {
            for(int j =i+1; j < nodes.Count; j++)
            {
                ConnectNodes(nodes[i], nodes[j], clst);
            }
        }
    }

    List<HPACluster> BuildCluster(int level, int lineSize, int width, int height)
    {
        List<HPACluster> result = new List<HPACluster>();
        HPACluster clst;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                clst = new HPACluster(lineSize, x, y);


                result.Add(clst);
            }
        }

        for (int i = 0; i < result.Count; i++)
        {
            for (int j = i + 1; j < result.Count; j++)
            {
                BuildEntranceAndINTEREdge(result[i], result[j], CalculateConcreteEntranceAmount);
            }
        }
        return result;
    }
    bool ConnectNodes(HPANode a,HPANode b,HPACluster c)
    {
        LinkedList<HPATile> path = GetINTRAPathAStarSearch(a, b,c);
        float weight = 0f;

        if(path.Count > 0)
        {

            HPAEdge aEdge = new HPAEdge(a, b, HPAEdgeType.INTRA); 
            aEdge.path = path;
            HPAEdge bEdge = new HPAEdge(b,a, HPAEdgeType.INTRA);

            LinkedListNode<HPATile> iter = path.Last;

            while(iter != null)
            {
                bEdge.path.AddLast(iter.Value);
                weight += iter.Value.penalty + 1;
                iter = iter.Previous;
            }
            aEdge.weight = weight;
            bEdge.weight = weight;

            a.edges.Add(aEdge);
            b.edges.Add(bEdge);
            return true;

        }
        return false;

    }

    LinkedList<HPATile> GetINTRAPathAStarSearch(HPANode start, HPANode end, HPACluster clst = null)
    {
        HashSet<HPATile> visited = new HashSet<HPATile>();
        Dictionary<HPATile, HPATile> cameFrom = new Dictionary<HPATile, HPATile>();
        Dictionary<HPATile, float> score = new Dictionary<HPATile, float>();
        SimplePriorityQueue<HPATile> pq = new SimplePriorityQueue<HPATile>();

        score.Add(start.tile, 0f);
        pq.Enqueue(start.tile, 0);

        while (pq.Count > 0)
        {
            HPATile cur = pq.Dequeue();

            if (cur == end.tile)
            {
                return RebuildPath(cameFrom, start.tile,end.tile);
            }

            visited.Add(cur);
            foreach (HPATile neighbor in GetNeighbor(clst,cur,true))
            {


                if (visited.Contains(neighbor))
                    continue;

                float newCost = score[cur] + neighbor.penalty;

                if (score.TryGetValue(neighbor, out float prevCost) && newCost >= prevCost)
                    continue;

                cameFrom[neighbor] = cur;
                score[neighbor] = newCost;
                pq.Enqueue(neighbor, newCost + Distance(neighbor, end.tile));
            }
        }
        return new LinkedList<HPATile>();
    }
    LinkedList<HPATile> RebuildPath(Dictionary<HPATile, HPATile> parent, HPATile start,HPATile end)
    {
        LinkedList<HPATile> tempList = new LinkedList<HPATile>();
        HPATile cur = end;
        while (cur != start)
        {
            if(!parent.ContainsKey(cur))
            {
                Debug.Log("EMPTY");
                return tempList;
            }

            tempList.AddFirst(cur);
            cur = parent[cur];
        }
        return tempList;
    }
    delegate void CreateBorderNodes(HPACluster c1, HPACluster c2, bool x);
    /// <summary>
    /// 0,0 -> 0,1, 0,2 0,3... 1,0, 1,1, 1,2...
    /// 0,1 -> 0,2 0,3 0,4.... 1,1
    /// Only Up&Right Loop
    /// </summary>
    /// <param name="cur"></param>
    /// <param name="b"></param>
    /// <param name="level"></param>
    void BuildEntranceAndINTEREdge(HPACluster cur, HPACluster b,CreateBorderNodes CreateBorderNode)
    {
        if (cur.minBoundary.x == b.minBoundary.x) // X is same, check if is Up or down
        {
            if (cur.maxBoundary.y == b.minBoundary.y - 1) // if up, cur set b to neighbor
            {
                CreateBorderNode(cur, b, false);
            }
            else if (cur.minBoundary.y - 1 == b.maxBoundary.y) // if down, b set cur to neighbor
            {
                CreateBorderNode(b, cur, false);
            }
        }
        else if (cur.minBoundary.y == b.minBoundary.y)
        {
            if (cur.maxBoundary.x == b.minBoundary.x - 1)
            {
                CreateBorderNode(cur, b, true);
            }
            else if (cur.minBoundary.x - 1 == b.maxBoundary.x)
            {
                CreateBorderNode(b, cur, true);
            }
        }
    }

    void CalculateConcreteEntranceAmount(HPACluster cur,HPACluster neighbor,bool isX)
    {
        int i, iMin, iMax;

        if(isX)
        {
            iMin = cur.minBoundary.y;
            iMax = cur.maxBoundary.y + 1;
        }
        else
        {
            iMin = cur.minBoundary.x;
            iMax = cur.maxBoundary.x + 1;
        }

        int openSize = 0;

        for(i = iMin; i < iMax; i++)
        {
            if(isX)
            {
                if(nodes[cur.maxBoundary.x, i].tile.imPassable || nodes[neighbor.minBoundary.x,i].tile.imPassable)
                {
                    CalculateEntranceIndexByOpenSize(cur, neighbor, isX, ref openSize, i, ConcreteEntrance);
                }
                else
                {
                    openSize++;
                }
            }
            else
            {
                if (nodes[i,cur.maxBoundary.y].tile.imPassable || nodes[i,neighbor.minBoundary.y].tile.imPassable)
                {
                    CalculateEntranceIndexByOpenSize(cur, neighbor, isX, ref openSize, i,ConcreteEntrance);
                }
                else
                {
                    openSize++;
                }
            }
        }

        CalculateEntranceIndexByOpenSize(cur, neighbor, isX, ref openSize, i, ConcreteEntrance);
    }

    delegate void CreateConcreteType(HPACluster cur, HPACluster neighbor, bool isX, int i);
    void CalculateEntranceIndexByOpenSize(HPACluster cur, HPACluster neighbor, bool isX,ref int openSize, int i, CreateConcreteType CreateType)
    {
        if(openSize > 0)
        {
            if(openSize <= cur.lineSize * 0.5f)
            {
                CreateType(cur, neighbor, isX, i - (openSize / 2 + 1));
            }
            else
            {
                CreateType(cur, neighbor, isX, i - openSize);
                CreateType(cur, neighbor, isX, i -1);
            }

            openSize = 0;
        }
    }

    void ConcreteEntrance(HPACluster cur, HPACluster neighbor, bool isX, int i)
    {
        HPATile curTile, neighborTile;
        HPANode curNode, neighborNode;

        if (isX)
        {
            curTile = nodes[cur.maxBoundary.x, i].tile;
            neighborTile = nodes[neighbor.minBoundary.x, i].tile;
        }
        else
        {
            curTile = nodes[i, cur.maxBoundary.y].tile;
            neighborTile = nodes[i, neighbor.minBoundary.y].tile;
        }


        if (!cur.entrances.TryGetValue(curTile, out curNode)) // if clst.entrances does not have curTile
        {
            curNode = new HPANode(curTile);
            cur.entrances.Add(curTile, curNode); // then add it!

        }
        if(!neighbor.entrances.TryGetValue(neighborTile,out neighborNode))
        {
            neighborNode = new HPANode(neighborTile);
            neighbor.entrances.Add(neighborTile,neighborNode);
        }

        HPAEdge curEdge = new HPAEdge(curNode, neighborNode, HPAEdgeType.INTER);
        HPAEdge neighborEdge = new HPAEdge(neighborNode, curNode, HPAEdgeType.INTER);

        curNode.edges.Add(curEdge);
        neighborNode.edges.Add(neighborEdge);
    }

    List<HPATile> GetNeighbor(HPACluster clst,HPATile node,bool isManHattan)
    {
        List<HPATile> neighbours = new List<HPATile>(4);

        if (node == null) return null;


        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) 
                    continue;

                if (isManHattan)
                {
                    if (x == -1 && y == -1 || x == 1 && y == -1 || x == 1 && y == 1 || x == -1 && y == 1) 
                        continue;
                }



                int aroundX = node.x + x;
                int aroundY = node.y + y;

                if(clst.IsInBoundary(aroundX, aroundY) && nodes[aroundX, aroundY].tile.imPassable == false)
                {
                    neighbours.Add(nodes[aroundX, aroundY].tile);
                }
            }
        }
        return neighbours;
    }

    void  TestConnect2Border(HPANode node,HPACluster c,int level)
    {
        if (c.entrances.TryGetValue(node.tile, out HPANode newNode)) // 누른 곳이 entrance라면 새로 만들필요없이 빌려옴
            node = newNode;
        else
        {
            foreach(var n in c.entrances.Values)
            {
                ConnectNodes(node, n, c);
            }
        }
        tempNodes.Add(node);

    }

    float Distance(HPATile a, HPATile b)
    {
        int dstX = Mathf.Abs(a.x - b.x);
        int dstY = Mathf.Abs(a.y - b.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
    bool isInBoundary(int x, int y)
    {
        if ((x >= 0) && (x < gridIndex.x) && (y >= 0) && (y < gridIndex.y))
            return true;
        return false;
    }
    bool isPassable(int x,int y)
    {
        if(nodes[x,y].tile.imPassable == false)
        {
            return true;
        }
        return false;
    }
    bool isValidNode(int x,int y)
    {
        if(isInBoundary(x,y) && isPassable(x,y))
        {
            return true;
        }
        return false;
    }
    public Vector2Int GetIndexFromWorldPos(Vector3 position)
    {
        float percentX = (position.x + gridSize.x * 0.5f - transform.position.x) / gridSize.x;
        float percentY = (position.y + gridSize.y * 0.5f - transform.position.y) / gridSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridIndex.x) * percentX);
        int y = Mathf.RoundToInt((gridIndex.y) * percentY);

        return new Vector2Int(x, y);
    }
    public HPATile GetTile(Vector3 pos) => GetTile(GetIndexFromWorldPos(pos));
    public HPATile GetTile(Vector2Int index) => GetTile(index.x, index.y);
    public HPATile GetTile(int x,int y)
    {
        if(isValidNode(x,y))
        {
            return nodes[x,y].tile;
        }
        else
        {
            for(int neighborX = -1; neighborX <= 1; x++)
            {
                for(int neighborY = -1; neighborY <=1; y++)
                {
                    if (neighborX == 0 && neighborY == 0) continue;

                    int aroundX = x + neighborX;
                    int aroundY = y + neighborY;

                    if (isValidNode(aroundX, aroundY))
                    {
                        return nodes[aroundX, aroundY].tile;
                    }
                }
            }
        }
        return null;
    }

    public HPANode TestInsertNode(Vector3 pos)
    {
        HPATile tempTile = GetTile(pos);
        if(tempTile == null)
        {
            Debug.LogError("Cant find tile");
            return null;
        }
        HPANode newNode = nodes[tempTile.x, tempTile.y];
        HPACluster clst = null;
        foreach (var c in graph.clusters)
        {
            if (c.IsInBoundary(tempTile))
            {
                clst = c;
                break;
            }
        }
        TestConnect2Border(newNode, clst, 0); // newNode have been connected through all level of grpahs.

        return newNode;
    }

    public void RemoveTempNode()
    {
        foreach (HPANode added in tempNodes)
        {
            foreach (HPAEdge e in added.edges)
            {
                e.nEnd.edges.RemoveAll((ee) => ee.nEnd == added);
            }
        }
    }
    private void OnDrawGizmos()
    {
        if(drawGizmos)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;

            Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, gridSize.y));


            if(isInitialize)
            {
                // show basic tiles like, wall, mud, road things
                foreach (var node in nodes)
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(0, byte.MaxValue, node.tile.penalty));
                    if (node.tile.imPassable)
                    {
                        Gizmos.color = Color.red;
                    }
                    Gizmos.DrawWireCube(node.tile.pos, Vector3.one * nodeDiameter);
                }

                Gizmos.color = Color.yellow;

                // show entire Clusters
                foreach (HPACluster c in graph.clusters)
                {
                    foreach(HPANode n in c.entrances.Values)
                    {

                        Gizmos.DrawWireCube(n.tile.pos,Vector3.one);
                        
                        foreach (HPAEdge e in n.edges)
                        {
                            Gizmos.DrawLine(e.nStart.tile.pos, e.nEnd.tile.pos);
                        }
                        
                    }
                }
                // show startEnd Path
                Gizmos.color = Color.blue;
                foreach(var e in TestPath)
                {
                    Gizmos.DrawLine(e.nStart.tile.pos,e.nEnd.tile.pos);
                }


                // show Test
                Gizmos.color = Color.green;

                if(testClsts != null)
                {
                    foreach (HPACluster c in testClsts)
                    {
                        foreach (HPANode n in c.entrances.Values)
                        {
                            Gizmos.DrawWireCube(n.tile.pos, Vector3.one);

                        }
                    }
                }

                Gizmos.color = Color.cyan;
                if(testClst != null)
                {
                    foreach (HPANode n in testClst.entrances.Values)
                    {
                        Gizmos.DrawWireCube(n.tile.pos, Vector3.one);
                        foreach (HPAEdge e in n.edges)
                        {
                            if (e.type == HPAEdgeType.INTRA)
                            {


                                Vector3 midPos = Vector3.Lerp(e.nStart.tile.pos, e.nEnd.tile.pos, 0.3f);

                                if (showWeights)
                                {
                                    UnityEditor.Handles.Label(midPos, e.weight.ToString(), style);
                                }
                                Gizmos.DrawLine(e.nStart.tile.pos, e.nEnd.tile.pos);
                            }
                        }
                    }
                }
            }
        }
    }
}
