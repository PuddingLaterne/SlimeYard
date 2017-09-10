using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fill : MonoBehaviour
{
    public float alpha = 0.8f;
    public int splatsPerStep = 3;
    public float fillSpeed = 0.01f;
    private SpriteRenderer[] splats;

    public void Start()
    {
        splats = GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer splat in splats)
        {
            splat.gameObject.SetActive(false);
        }
    }
	
    public void FillScreen(Color color, float delay, float stayTime)
    {
        StopAllCoroutines();
        color = new Color(color.r, color.g, color.b, alpha);
        foreach (SpriteRenderer splat in splats)
        {
            splat.color = color;
        }
        StartCoroutine(WaitForFill(delay, stayTime));
    }

    private IEnumerator WaitForFill(float delay, float stayTime)
    {
        yield return new WaitForSeconds(delay);

        List<SpriteRenderer> splatsToShow = new List<SpriteRenderer>();
        for (int i = 0; i < splats.Length; i++) splatsToShow.Add(splats[i]);

        for(int i = 0; i < splats.Length; i += splatsPerStep)
        {
            for (int j = 0; j < splatsPerStep; j++)
            {
                ChangeSplatState(ref splatsToShow, true);
            }
            yield return new WaitForSeconds(fillSpeed);
        }

        yield return new WaitForSeconds(stayTime);
        for (int i = 0; i < splats.Length; i++) splatsToShow.Add(splats[i]);

        for (int i = 0; i < splats.Length; i += splatsPerStep)
        {
            for (int j = 0; j < splatsPerStep; j++)
            {
                ChangeSplatState(ref splatsToShow, false);
            }
            yield return new WaitForSeconds(fillSpeed);
        }
    }

    private void ChangeSplatState(ref List<SpriteRenderer> splats, bool active)
    {
        if (splats.Count == 0) return;
        SpriteRenderer splat = splats[Random.Range(0, splats.Count)];
        splats.Remove(splat);
        splat.gameObject.SetActive(active);
    }
}
