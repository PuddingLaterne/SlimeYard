using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SlimeBlob : MonoBehaviour
{
    public float Distortion = 0.1f;
    public float Lifetime = 2f;
    public float SplatPlacementProbability = 0.5f;
    public AnimationCurve SizeOverLifetime;

    public Color Color
    {
        set
        {
            color = new Color(value.r, value.g, value.b, 0.8f);
            GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", color);
        }
    }

    private Color color;
    private float creationTime;

    public void Create(Vector2[] positions)
    {
        Vector2 center = positions[positions.Length - 1];
        Vector2[] localPositions = new Vector2[positions.Length - 1];
        for (int i = 0; i < localPositions.Length; i++)
        {   
            localPositions[i] = positions[i] - center + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * Distortion;
            if (Random.Range(0f, 1f) <= SplatPlacementProbability)
            {
                GameObject splat = SlimeSplatPool.Instance.CreateSlimeSplat(color, Vector3.zero, Lifetime);
                splat.transform.parent = transform;
                splat.transform.localPosition = localPositions[i];
            }
        }
        transform.position = center;

        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        collider.points = localPositions;

        int pointCount = localPositions.Length;
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[pointCount];
        Vector2[] uv = new Vector2[pointCount];
        for (int j = 0; j < pointCount; j++)
        {
            Vector2 actual = localPositions[j];
            vertices[j] = new Vector3(actual.x, actual.y, 0);
            uv[j] = actual;
        }
        Triangulator tr = new Triangulator(localPositions);
        int[] triangles = tr.Triangulate();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        meshFilter.mesh = mesh;

        creationTime = Time.time;
    }


    public void Update()
    {
        float timeAlive = Time.time - creationTime;
        if (timeAlive > Lifetime)
        {
            gameObject.SetActive(false);
        }
        transform.localScale = Vector3.one * SizeOverLifetime.Evaluate(timeAlive / Lifetime);
    }
	
}
