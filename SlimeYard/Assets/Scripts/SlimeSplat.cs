using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeSplat : MonoBehaviour
{
    public float MinSize = 0.9f;
    public float MaxSize = 1.1f;
    public float MaxOffset = 0.1f;

    public float LifeTime { get; set; }

    private float creationTime;
    
    public Color Color
    {
        set
        {
            Color color = new Color(value.r, value.g, value.b, 0.6f);
            GetComponentInChildren<SpriteRenderer>().color = color;
        }
    }

    public void OnEnable()
    {
        creationTime = Time.time;
        transform.position += (Vector3)(Vector2.one * Random.Range(-MaxOffset, MaxOffset));
        transform.localScale = Vector3.one * Random.Range(MinSize, MaxSize);
        transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
    }

	public void Update ()
    {
		if(Time.time - creationTime > LifeTime)
        {
            gameObject.SetActive(false);
        }
	}
}
