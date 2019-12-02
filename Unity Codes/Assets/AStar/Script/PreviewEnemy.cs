using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewEnemy : MonoBehaviour
{
    public Transform target;
    public AStar astar;
    public float movementSpeed;
    public float nextDistance;

    public void Awake()
    {
        StartCoroutine(DelayedStart());
    }

    public IEnumerator DelayedStart()
    {
        yield return null;
        StartCoroutine(FollowPath(astar.GetPath(transform.position, target.position)));
    }

    public IEnumerator FollowPath(Vector3[] path)
    {
        int currentNode = 0;
        while (true)
        {
            Vector3 dir = -(transform.position - path[currentNode]).normalized;
            transform.position += dir * Time.deltaTime * movementSpeed;
            if (Vector3.Distance(transform.position, path[currentNode]) <= nextDistance)
                currentNode++;

            if (currentNode == path.Length)
                break;

            yield return null;
        }
    }
}
