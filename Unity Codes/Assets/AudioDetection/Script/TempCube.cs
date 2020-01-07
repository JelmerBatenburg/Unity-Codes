using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempCube : MonoBehaviour
{
    public float speed;

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
    }
}
