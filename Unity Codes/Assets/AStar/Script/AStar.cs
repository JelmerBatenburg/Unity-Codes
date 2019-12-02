using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    [Header("GridArea")]
    public float tileSize;
    public Vector3 area;
    public List<Node> nodes = new List<Node>();
    public LayerMask checkMask;
    Vector3 nodeAmount;
    public float floorCheckHeight;
    public bool test;
    public Transform testStartPos, testEndpos;
    public float checkValue;
    public Vector3[] finalPath = new Vector3[0];

    public void Start()
    {
        GenerateGrid();
        GetWalkableTiles(floorCheckHeight);
    }

    public void Update()
    {
        if (test)
        {
            finalPath = GetPath(testStartPos.position, testEndpos.position);
            test = false;
        }
    }

    public void GenerateGrid()
    {
        nodes = new List<Node>();
        nodeAmount.x = Mathf.RoundToInt(area.x / tileSize);
        nodeAmount.y = Mathf.RoundToInt(area.y / tileSize);
        nodeAmount.z = Mathf.RoundToInt(area.z / tileSize);
        for (int x = 0; x < nodeAmount.x; x++)
            for (int y = 0; y < nodeAmount.y; y++)
                for (int z = 0; z < nodeAmount.z; z++)
                {
                    Vector3 loc = transform.position + new Vector3(((-nodeAmount.x / 2) + x) * tileSize, ((-nodeAmount.y / 2) + y) * tileSize, ((-nodeAmount.z / 2) + z) * tileSize);
                    nodes.Add(new Node(loc, Physics.CheckBox(loc, Vector3.one * tileSize / 2f, Quaternion.identity, checkMask)));
                }
    }

    public void GetWalkableTiles(float height)
    {
        int yOffset = Mathf.RoundToInt(nodeAmount.x);
        int heightAmount = Mathf.CeilToInt(height / tileSize);
        for (int i = 0; i < nodes.Count; i++)
            if(nodes[i].obstructed)
            {
                bool isFloor = true;

                for (int heightOffset = 1; heightOffset < heightAmount; heightOffset++)
                    if (i + (heightOffset * yOffset) < nodes.Count)
                        if (Physics.CheckBox(nodes[i + (heightOffset * yOffset)].position, Vector3.one * tileSize / 2f, Quaternion.identity, checkMask))
                            isFloor = false;
                nodes[i].walkable = isFloor;
            }
    }

    public Vector3[] GetPath(Vector3 startPos, Vector3 endPos)
    {
        List<int> pathNodes = new List<int>();
        List<int> closedList = new List<int>();
        int currentNode = GetClosestTileIndex(startPos);
        int endIndex = GetClosestTileIndex(endPos);
        pathNodes.Add(currentNode);
        closedList.Add(currentNode);

        while (currentNode != endIndex)
        {
            closedList.Add(currentNode);
            int[] neighboringNodes = GetSurroundingNodes(currentNode);
            foreach(int node in neighboringNodes)
            {
                if (closedList.Contains(node))
                    continue;

                if (pathNodes.Contains(node) && nodes[node].arrivalCost <= nodes[currentNode].arrivalCost + 1)
                {
                    nodes[node].arrivalCost = nodes[currentNode].arrivalCost + 1;
                    continue;
                }

                nodes[node].arrivalCost = nodes[currentNode].arrivalCost + 1;
                nodes[node].distanceCost = Vector3.Distance(nodes[endIndex].position, nodes[node].position);
                pathNodes.Add(node);
            }
            if (pathNodes.Count == closedList.Count)
                break;

            currentNode = GetLowestCost(pathNodes.ToArray(), closedList);
        }
        closedList.Add(currentNode);

        return GetFinalPath(closedList);
    }

    public Vector3[] GetFinalPath(List<int> usedTiles)
    {
        List<Vector3> pathNodes = new List<Vector3>();
        int currentNode = usedTiles[usedTiles.Count - 1];
        while(currentNode != usedTiles[0])
        {
            pathNodes.Add(nodes[currentNode].position);

            int[] neighbouringNodes = GetSurroundingNodes(currentNode);
            int lowest = 0;
            foreach (int node in neighbouringNodes)
                if (usedTiles.Contains(node))
                    if (lowest == 0 || nodes[node].arrivalCost <= nodes[lowest].arrivalCost)
                        lowest = node;

            currentNode = lowest;
        }
        pathNodes.Reverse();
        return pathNodes.ToArray();
    }

    public int GetLowestCost(int[] checkNodes, List<int> closedNodes)
    {
        int lowest = 0;

        foreach (int node in checkNodes)
            if (nodes[lowest].totalCost == 0 || nodes[node].totalCost <= nodes[lowest].totalCost && !closedNodes.Contains(node))
                lowest = node;

        return lowest;
    }

    public int[] GetSurroundingNodes(int node)
    {
        int yOffset = Mathf.RoundToInt(nodeAmount.x);
        int zOffset = Mathf.RoundToInt(nodeAmount.x * nodeAmount.y);
        List<int> checkNodes = new List<int>();

        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
                for (int z = -1; z <= 1; z++)
                {
                    if ((Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z)) == 3)
                        continue;

                    int checkIndex = node + x + (y * yOffset) + (z * zOffset);
                    if (checkIndex < nodes.Count && checkIndex >= 0 && nodes[checkIndex].walkable && Vector3.Distance(nodes[node].position, nodes[checkIndex].position) <= (tileSize * 3))
                        checkNodes.Add(checkIndex);
                }
        return checkNodes.ToArray();
    }

    public int GetClosestTileIndex(Vector3 pos)
    {
        int closestPoint = 0;

        for (int i = 0; i < nodes.Count; i++)
            if (Vector3.Distance(pos, nodes[i].position) <= Vector3.Distance(pos, nodes[closestPoint].position) && nodes[i].walkable)
                closestPoint = i;
        
        return closestPoint;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, area);
        foreach(Node node in nodes)
        {
            Gizmos.color = node.obstructed ? Color.black : new Color(1, 1, 1, 0.1f);
            if (node.walkable)
                Gizmos.DrawSphere(node.position, tileSize / 2);
            Gizmos.DrawWireCube(node.position, Vector3.one * tileSize);
        }
        Gizmos.color = Color.red;
        foreach (Vector3 pos in finalPath)
            Gizmos.DrawSphere(pos, tileSize / 2f);
    }

    public class Node
    {
        public Vector3 position;
        public bool obstructed, walkable;
        public float arrivalCost;
        public float distanceCost;
        public float totalCost
        {
            get { return arrivalCost + distanceCost;}
        }

        public Node(Vector3 _position, bool _obstructed)
        {
            position = _position;
            obstructed = _obstructed;
        }
    }
}
