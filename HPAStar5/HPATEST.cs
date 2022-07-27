using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPATEST : MonoBehaviour
{
    public HPACluster cluster;
    public LinkedList<HPATile> pathTile = new LinkedList<HPATile>();
    [SerializeField] bool showGizmos;
    [SerializeField] bool showWeights;
    [SerializeField] bool showEdge;
    [SerializeField] bool showPath;
    [SerializeField] bool showEntrance;
    private void OnDrawGizmos()
    {


        if (showGizmos)
        {
            HashSet<HPANode> visited = new HashSet<HPANode>();
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;

            if (showPath)
            {
                foreach (HPATile t in pathTile)
                {
                    Gizmos.DrawWireCube(t.pos, Vector3.one);
                }
            }

            foreach (HPANode n in cluster.entrances.Values)
            {
                visited.Add(n);
                if(showEntrance)
                {
                    Gizmos.DrawWireCube(n.tile.pos, Vector3.one);
                }

                if(showEdge)
                {

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
