using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPAAgent : MonoBehaviour
{
    [SerializeField] HPAGrid grid;
    Rigidbody2D rb;
    [SerializeField] float speed;

    private void Start()
    {
        if (grid == null) grid = FindObjectOfType<HPAGrid>();
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var path = HPAPathfinder.TestFindPath(transform.position, mousePos, grid);
            FollowPath(path);
            grid.TestPath = path;
        }



    }

    void FollowPath(LinkedList<HPAEdge> edges)
    {
        LinkedList<HPATile> path = new LinkedList<HPATile>();

        LinkedListNode<HPAEdge> tempEdge = edges.First;
        while(tempEdge != null)
        {
            LinkedListNode<HPATile> tempTile = tempEdge.Value.path.First;
            while (tempTile != null)
            {
                path.AddLast(tempTile.Value);
                tempTile = tempTile.Next;
            }

            tempEdge = tempEdge.Next;
        }

        StopAllCoroutines();
        StartCoroutine(Move2Path(path));

    }

    IEnumerator Move2Path(LinkedList<HPATile> path)
    {
        while(true)
        {
            Vector2 dir = (path.First.Value.pos - transform.position).normalized;
            float dist = (path.First.Value.pos - transform.position).sqrMagnitude;

            if(dist <= 0.5f)
            {
                path.RemoveFirst();
            }

            if (path.Count <= 0)
            {
                rb.velocity = Vector2.zero;
                break;
            }
            rb.velocity = dir * speed;

            yield return null;
        }
    }


    
}
