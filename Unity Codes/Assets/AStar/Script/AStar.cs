using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    [Header("GridArea")]
    public float tileSize;
    public Vector3 area;
    [Header("Detection")]
    public LayerMask checkMask;
    public float floorCheckHeight;

    public List<Node> nodes = new List<Node>();
    public List<Vector3> debugNodes = new List<Vector3>();
    public float debugFloat;
    Vector3 nodeAmount;

    public void Start()
    {
        GenerateGrid();
        GetWalkableTiles(floorCheckHeight);
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
            SurroundingReturnInfo[] neighboringNodes = GetSurroundingNodes(currentNode);
            foreach(SurroundingReturnInfo node in neighboringNodes)
            {
                if (closedList.Contains(node.index))
                    continue;

                float NodeCost = 1 + (0.4f * (Mathf.Abs(node.offset.x) + Mathf.Abs(node.offset.y) + Mathf.Abs(node.offset.z) - 1));

                if (pathNodes.Contains(node.index) && nodes[node.index].arrivalCost <= nodes[currentNode].arrivalCost + NodeCost)
                {
                    nodes[node.index].arrivalCost = nodes[currentNode].arrivalCost + NodeCost;
                    continue;
                }

                nodes[node.index].arrivalCost = nodes[currentNode].arrivalCost + NodeCost;
                nodes[node.index].distanceCost = Vector3.Distance(nodes[endIndex].position, nodes[node.index].position);
                pathNodes.Add(node.index);
            }
            if (pathNodes.Count == closedList.Count)
            {
                Debug.Log("Failed");
                break;
            }

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

            SurroundingReturnInfo[] neighbouringNodes = GetSurroundingNodes(currentNode);
            int lowest = 0;
            foreach (SurroundingReturnInfo node in neighbouringNodes)
                if (usedTiles.Contains(node.index))
                    if (lowest == 0 || nodes[node.index].arrivalCost <= nodes[lowest].arrivalCost)
                        lowest = node.index;

            currentNode = lowest;
        }
        pathNodes.Reverse();
        debugNodes = pathNodes;
        return pathNodes.ToArray();
    }

    public int GetLowestCost(int[] checkNodes, List<int> closedNodes)
    {
        int lowest = 0;

        foreach (int node in checkNodes)
            if (nodes[lowest].totalCost == 0 && !closedNodes.Contains(node) || nodes[node].totalCost <= nodes[lowest].totalCost && !closedNodes.Contains(node))
                lowest = node;

        return lowest;
    }

    public SurroundingReturnInfo[] GetSurroundingNodes(int node)
    {
        int yOffset = Mathf.RoundToInt(nodeAmount.z);
        int xOffset = Mathf.RoundToInt(nodeAmount.z * nodeAmount.y);
        List<SurroundingReturnInfo> checkNodes = new List<SurroundingReturnInfo>();

        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
                for (int z = -1; z <= 1; z++)
                {
                    if ((Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z)) == 0 || (Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z)) == 3)
                        continue;
                    int checkIndex = node + (x * xOffset) + (y * yOffset) + z;
                    if (checkIndex < nodes.Count && checkIndex >= 0 && nodes[checkIndex].walkable && Vector3.Distance(nodes[node].position, nodes[checkIndex].position) <= (tileSize * 3))
                        checkNodes.Add(new SurroundingReturnInfo(checkIndex, new Vector3(x, y, z)));
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
            Gizmos.color = node.obstructed ? Color.black : new Color(1, 1, 1, debugFloat);
            if (node.walkable)
                Gizmos.DrawSphere(node.position, tileSize / 2);
            Gizmos.DrawWireCube(node.position, Vector3.one * tileSize);
        }
        Gizmos.color = Color.red;

        /*Gizmos.color = Color.red;
        for (int i = 0; i < debugNodes.Count - 1; i++)
            Gizmos.DrawLine(debugNodes[i], debugNodes[i + 1]);*/
    }

    public class SurroundingReturnInfo
    {
        public int index;
        public Vector3 offset;
        public SurroundingReturnInfo(int _index, Vector3 _offset)
        {
            index = _index;
            offset = _offset;
        }
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
