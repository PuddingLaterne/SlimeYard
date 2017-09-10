using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct Point
{
    public float CreationTime;
    public Vector3 Position;
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class Trail : MonoBehaviour
{
    public Vector3 LatestPosition
    {
        get
        {
            return lineRenderer.positionCount != 0 ? lineRenderer.GetPosition(lineRenderer.positionCount - 1) : Vector3.zero;
        }
    }
    public LineRenderer LineRenderer
    {
        get
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();
            return lineRenderer;
        }
    }
    public EdgeCollider2D EdgeCollider
    {
        get
        {
            if (edgeCollider == null)
                edgeCollider = GetComponent<EdgeCollider2D>();
            return edgeCollider;
        }
    }
    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            color = new Color(value.r, value.g, value.b, 0.8f);
            LineRenderer.startColor = color;
            LineRenderer.endColor = color;
        }
    }

    public float PointLifetime = 1f;
    public float SplatPlacementProbability = 0.5f;

    private List<Point> points = new List<Point>();
    private LineRenderer lineRenderer;
    private EdgeCollider2D edgeCollider;
    private Color color;

    private const int excludeFromShapeCreationPointCount = 2;

    public void Clear()
    {
        points = new List<Point>();
        LineRenderer.positionCount = 0;
        EdgeCollider.enabled = false;
    }

    public void Update()
    {
        List<Point> pointsToRemove = new List<Point>();
        foreach(Point point in points)
        {
            if(Time.time - point.CreationTime >= PointLifetime)
            {
                pointsToRemove.Add(point);
            }
        }
        foreach(Point pointToRemove in pointsToRemove)
        {
            points.Remove(pointToRemove);
        }
        if(pointsToRemove.Count > 0)
        {
            UpdatePoints();
        }
    }

	public void AddPoint(Vector3 position)
    {
        Point newPoint = new Point { Position = position, CreationTime = Time.time };
        points.Add(newPoint);
        UpdatePoints();
    }

    public Vector2[] GetShapePositions(Vector2 collisionPos)
    {
        try
        {
            int closestPoint = -1;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < points.Count - excludeFromShapeCreationPointCount; i++)
            {
                float distance = Vector2.Distance(collisionPos, points[i].Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = i;
                }
            }
            int shapePointCount = points.Count - closestPoint + 1;

            List<Point> pointsToRemoveFromTrail = new List<Point>();
            Vector2[] positions = new Vector2[shapePointCount];
            Vector2 center = Vector2.zero;
            for (int i = 0; i < positions.Length - 1; i++)
            {
                pointsToRemoveFromTrail.Add(points[i + closestPoint]);
                positions[i] = points[i + closestPoint].Position;
                center += positions[i];
            }
            foreach (Point pointToRemove in pointsToRemoveFromTrail)
            {
                points.Remove(pointToRemove);
            }
            UpdatePoints();
            positions[positions.Length - 1] = center / positions.Length;
            return positions;
        }
        catch
        {
            Debug.Log("failed to create shape");
            return new Vector2[0];
        }
    }

    private void UpdatePoints()
    {
        LineRenderer.positionCount = points.Count;
        LineRenderer.SetPositions(points.GetPositions3D());

        if (points.Count >= 2)
        {
            EdgeCollider.enabled = true;
            EdgeCollider.points = points.GetPositions2D();
        }
        else
        {
            EdgeCollider.enabled = false;
        }
    }
}
