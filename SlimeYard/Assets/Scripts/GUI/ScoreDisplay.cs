using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public int Score
    {
        set
        {
            Color mixColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            for (int i = 0; i < PointDisplays.Length; i++)
            {
                PointDisplays[i].color = PointDisplays.Length - i <= value ? color : color * mixColor;
            }
        }
    }

    public Color Color
    {
        set
        {
            foreach(Image point in PointDisplays)
            {
                point.color = value;
            }
            color = value;
        }
    }

    public Image[] PointDisplays
    {
        get
        {
            if (pointDisplays == null)
                pointDisplays = GetComponentsInChildren<Image>();
            return pointDisplays;
        }
    }

    private Color color;
    private Image[] pointDisplays;
}
