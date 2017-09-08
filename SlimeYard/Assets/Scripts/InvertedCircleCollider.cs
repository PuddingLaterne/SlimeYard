using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(EdgeCollider2D))]
public class InvertedCircleCollider : MonoBehaviour
{
    public float Radius;
    public int NumPoints;

    public void Awake()
    {
        EdgeCollider2D collider = GetComponent<EdgeCollider2D>();
        List<Vector2> points = new List<Vector2>();
        for(int i = 0; i <= NumPoints; i++)
        {
            points.Add(Vector2.up.Rotate(i * (360 / NumPoints)) * Radius);
        }
        collider.points = points.ToArray();
    }
}
