using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Unity.VisualScripting;
using Timer = System.Timers.Timer;

public class PathFinding : MonoBehaviour
{
    private Vector3 seeker, target;
    Grid grid;
    //public Brain brain;
    private LineRenderer _lineRenderer;
    public static event Action<Vector3> CreateCar; 

    void Awake()
    {
        grid = GetComponent<Grid>();
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    private int counter = 0;
    private float timer = 0;

    private bool _isseekerNotNull = false;
    private bool _istargetNotNull = false;
    public Vector3 GetSeeker
    {
        get => seeker;
    }
    void Update()
    {

        if (_isseekerNotNull && _istargetNotNull)
        {
            FindPath(seeker, target);
            CreateCar?.Invoke(seeker);
            _isseekerNotNull = false;
            _istargetNotNull = false;
            seeker = Vector3.zero;
            target = Vector3.zero;
            counter = 0;
        }
    

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (counter == 0)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100,1 << 6)) {
                    seeker = hit.point;
                    _isseekerNotNull = true;
                    print("seeker = " + seeker);
                    counter++;
                }
            }
            else if(counter == 1)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100,1 << 6))
                {
                    target = hit.point;
                    _istargetNotNull = true;
                    print("target = " + target);
                    counter++;
                }
            }
        }
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {

        Stopwatch sw = new Stopwatch();
        sw.Start();
        Node startNode = grid.NodeFromWorldPoint(startPos);
        print("startNode : " + startNode.worldPosition);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);
        print("targetNode : " + targetNode.worldPosition);
        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node node = openSet.RemoveFirst();
            closedSet.Add(node);

            if (node == targetNode) {
                sw.Stop();
                print("path found " + sw.ElapsedMilliseconds + " ms");
                RetracePath(startNode,targetNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(node)) {
                if (!neighbour.walkable || closedSet.Contains(neighbour)) {
                    continue;
                }

                int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = node;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
    }
    public List<Node> path = new List<Node>();
    void RetracePath(Node startNode, Node endNode) {
       
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        _lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; ++i)
        {
            _lineRenderer.SetPosition(i,path[i].worldPosition);
        }
        grid.path = path;
        //brain.path = path;

    }

    int GetDistance(Node nodeA, Node nodeB) {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14*dstY + 10* (dstX-dstY);
        return 14*dstX + 10 * (dstY-dstX);
    }
    private void OnGUI()
    {
        //GUI.Label(new Rect(10, 20, 200, 20), "start : " + seeker.transform.position);
        //GUI.Label(new Rect(10, 40, 200, 20), "end : " + target.transform.position);
    }
}
