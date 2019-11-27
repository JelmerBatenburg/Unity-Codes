using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissionDiscSampling : MonoBehaviour
{
    [Header("Location and Size")]
    public Vector2Int range;
    public float tileSize;

    [Header("Settings")]
    public int tries;
    public float placementDelay;
    TileInformation[] nodes = new TileInformation[0];

    [Header("PreviewSettings")]
    public int placeAmount;
    public GameObject[] previewObjects;
    public float offsetRange;

    public void Start()
    {
        GenerateAllPoints();
        StartCoroutine(PlacePoints(placeAmount, previewObjects));
    }

    public void GenerateAllPoints()
    {
        List<TileInformation> tiles = new List<TileInformation>();
        for (int x = 0; x < range.x; x++)
            for (int z = 0; z < range.y; z++)
            {
                TileInformation newTile = new TileInformation();
                RaycastHit hit = new RaycastHit();
                float minX = (-(range.x / 2) + x) * tileSize + transform.position.x;
                float minZ = (-(range.y / 2) + z) * tileSize + transform.position.z;

                newTile.minPos = new Vector3(minX, transform.position.y, minZ);
                if (Physics.Raycast(newTile.minPos, Vector3.down, out hit, Mathf.Infinity))
                    newTile.minPos.y = hit.point.y;

                newTile.maxpos = new Vector3(minX + tileSize, transform.position.y, minZ + tileSize);
                if (Physics.Raycast(newTile.maxpos, Vector3.down, out hit, Mathf.Infinity))
                    newTile.maxpos.y = hit.point.y;

                tiles.Add(newTile);
            }
        nodes = tiles.ToArray();
    }

    public IEnumerator PlacePoints(int amount, GameObject[] placeableObjects)
    {
        List<TileInformation> availableTiles = new List<TileInformation>(nodes);
        List<TileInformation> usedTiles = new List<TileInformation>();
        Vector3 currentPos = Vector3.zero;
        while(amount >= 0)
        {
            Vector3 offset = Vector3.zero;
            int currentTries = tries;
            if (currentPos != Vector3.zero)
            {
                offset = new Vector3(Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange));
                while (GetAreaPositionindex(availableTiles, currentPos + offset) == -1 && currentTries >= 0)
                {
                    offset = new Vector3(Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange));
                    currentTries--;
                    if (currentTries < 0)
                        currentPos = Vector3.zero;
                }
            }
            currentTries = tries;
            int index = (currentPos != Vector3.zero) ? GetAreaPositionindex(availableTiles, currentPos + offset) : Random.Range(0, availableTiles.Count);
            while (currentTries >= 0)
            {
                Vector3 placeSpot = RandomSpotBetweenTwoPositions(availableTiles[index].minPos, availableTiles[index].maxpos);
                for (int x = -1; x <= 1; x++)
                    for (int z = -1; z <= 1; z++)
                        if (GetAreaPositionindex(usedTiles, placeSpot + (new Vector3(x, 0, z) * tileSize)) != -1 && !AllowedPlaceDistance(usedTiles, placeSpot + (new Vector3(x, 0, z) * tileSize), placeSpot, tileSize))
                            currentTries--;

                if (currentTries >= 0)
                {
                    GameObject g = Instantiate(placeableObjects[Random.Range(0, placeableObjects.Length)], placeSpot, Quaternion.identity, transform);
                    availableTiles[index].position = placeSpot;
                    currentPos = placeSpot;
                    usedTiles.Add(availableTiles[index]);
                    availableTiles.RemoveAt(index);
                    amount--;
                    break;
                }
            }
            yield return new WaitForSeconds(placementDelay);
        }
    }

    public bool AllowedPlaceDistance(List<TileInformation> points, Vector3 checkPos, Vector3 pos, float radius)
    {
        TileInformation tile = points[GetAreaPositionindex(points, checkPos)];
        return Vector3.Distance(new Vector3(pos.x, tile.position.y, pos.z), tile.position) >= radius * 2;
    }

    public int GetAreaPositionindex(List<TileInformation> points, Vector3 position)
    {
        for (int i = 0; i < points.Count; i++)
            if (points[i].minPos.x < position.x && points[i].maxpos.x > position.x)
                if (points[i].minPos.z < position.z && points[i].maxpos.z > position.z)
                    return i;

        return -1;
    }

    public static Vector3 RandomSpotBetweenTwoPositions(Vector3 pos1, Vector3 pos2)
    {
        Vector3 pos = new Vector3();

        pos.x = Random.Range(pos1.x, pos2.x);
        pos.y = Random.Range(pos1.y, pos2.y);
        pos.z = Random.Range(pos1.z, pos2.z);

        return pos;
    }

    public void OnDrawGizmos()
    {
        foreach (TileInformation tile in nodes)
        {
            Gizmos.DrawLine(tile.minPos, tile.maxpos);
            Gizmos.DrawWireCube(Vector3.Lerp(tile.minPos, tile.maxpos, 0.5f), new Vector3(tileSize, 0, tileSize));
        }
        Gizmos.DrawWireCube(transform.position + new Vector3(tileSize / 2, 0, tileSize / 2), new Vector3(range.x * tileSize, 1, range.y * tileSize));
    }

    public class TileInformation
    {
        public Vector3 minPos, maxpos;
        public bool taken = false;
        public Vector3 position;
    }
}
