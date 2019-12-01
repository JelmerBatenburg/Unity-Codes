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
    List<int> pathNodes = new List<int>();
    List<int> badPathNodes = new List<int>();

    public void Start()
    {
        GenerateGrid();
        GetWalkableTiles(floorCheckHeight);
    }

    public void Update()
    {
        if (test)
        {
            StartCoroutine(GetPath(testStartPos.position, testEndpos.position));
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
                        if (Physics.CheckBox(nodes[i + (heightOffset * yOffset)].position, Vector3.one * tileSize / 2, Quaternion.identity, checkMask))
                            isFloor = false;
                nodes[i].walkable = isFloor;
            }
    }

    public IEnumerator GetPath(Vector3 startPos, Vector3 endPos)
    {
        int yOffset = Mathf.RoundToInt(nodeAmount.x);
        int zOffset = Mathf.RoundToInt(nodeAmount.x * nodeAmount.y);
        pathNodes = new List<int>();
        badPathNodes = new List<int>();
        int currentNode = GetClosestTileIndex(startPos);
        pathNodes.Add(currentNode);

        while(currentNode != GetClosestTileIndex(endPos))
        {
            
        }

        yield return null;
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
            Gizmos.color = node.obstructed ? Color.black : Color.white;
            if (node.walkable)
                Gizmos.DrawSphere(node.position, tileSize / 2);
            Gizmos.DrawWireCube(node.position, Vector3.one * tileSize);
        }
        foreach (int index in pathNodes)
            Gizmos.DrawSphere(nodes[index].position, tileSize / 2f);
    }

    public class Node
    {
        public Vector3 position;
        public bool obstructed, walkable;
        public float arrivalCost;
        public float distanceCost;
        public float totalCost
        {
            get { return arrivalCost - distanceCost;}
        }

        public Node(Vector3 _position, bool _obstructed)
        {
            position = _position;
            obstructed = _obstructed;
        }
    }
}
