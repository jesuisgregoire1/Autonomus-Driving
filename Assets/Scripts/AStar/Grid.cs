using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Grid : MonoBehaviour {

    public LayerMask unwalkableMask;
    // public Vector2 gridWorldSize;
    public float nodeRadius;
    [SerializeField]private CityGridGenerator _cityGridGenerator;
    Node[,] grid;
    
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Start() {
        nodeDiameter = nodeRadius*2;
        _cityGridGenerator.LetsCreateGrid += CreateGrid;
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }
    void CreateGrid() {
        gridSizeX = Mathf.RoundToInt(_cityGridGenerator.Getl/nodeDiameter);
        gridSizeY = Mathf.RoundToInt(_cityGridGenerator.GetL/nodeDiameter);
        grid = new Node[gridSizeX,gridSizeY];
        Vector3 worldBottomLeft = new Vector3(_cityGridGenerator.minDimensions.x-10,0,_cityGridGenerator.minDimensions.z-10);

        for (int x = 0; x < gridSizeX; x ++) {
            for (int y = 0; y < gridSizeY; y ++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
//                print("WP = " + worldPoint);
                bool walkable = (Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));
                grid[x,y] = new Node(walkable,worldPoint, x,y);
            }
        }

        for (int x = 0; x < gridSizeX; x++)
        {
        }
    }
    
    public List<Node> GetNeighbours(Node node) {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
                    neighbours.Add(grid[checkX,checkY]);
                }
            }
        }
        return neighbours;
    }
    
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + _cityGridGenerator.Getl/2) / _cityGridGenerator.Getl;
        float percentY = (worldPosition.z + _cityGridGenerator.GetL/2) / _cityGridGenerator.GetL;
        float xOffSet = _cityGridGenerator.GetCenterX;
        float yOffSet = _cityGridGenerator.GetCenterZ;
        
        
        
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX-1) * percentX - xOffSet);
        int y = Mathf.RoundToInt((gridSizeY-1) * percentY - yOffSet);
        //GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        //cylinder.transform.position = grid[x,y].worldPosition;
        return grid[x,y];
    }
    public List<Node> path;
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3(_cityGridGenerator.GetCenterX / 2, 0, _cityGridGenerator.GetCenterZ / 2),
            new Vector3(_cityGridGenerator.Getl, 1, _cityGridGenerator.GetL));

        if (grid != null) {
            foreach (Node n in grid) {
                if (n.walkable)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.red;
                }
                if (path != null)
                    if (path.Contains(n))
                        Gizmos.color = Color.black;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
            }
        }
    
    }
}