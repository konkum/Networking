using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Ball : MonoBehaviour
{
    private ObjectPool<Ball> _pool;
    public Vector3 Destination;

    public ObjectPool<Ball> Pool
    {
        get => _pool;
        set => _pool = value;
    }

    private void Update()
    {
        this.transform.position = Vector3.MoveTowards(this.transform.position, Destination, 5 * Time.deltaTime);
        if (Vector3.Magnitude(Destination - this.transform.position) < 0.5f)
        {
            _pool.Release(this);
        }
    }
}