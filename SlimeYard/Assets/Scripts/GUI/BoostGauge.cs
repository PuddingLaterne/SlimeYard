using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostGauge : MonoBehaviour
{
    public GameObject BoostIndicatorPrefab;
    
    public Color Color
    {
        set
        {
            color = value;
            foreach(Image boostIndicator in BoostIndicators)
            {
                boostIndicator.color = value;
            }
        }
    }

    public int BoostSteps
    {
        set
        {
            if (BoostIndicatorPrefab == null) return;

            for(int i = 0; i < value; i++)
            {
                GameObject indicator = Instantiate(BoostIndicatorPrefab);
                indicator.transform.SetParent(transform);
                indicator.transform.localScale = Vector3.one;
            }
        }
    }

    public Image[] BoostIndicators
    {
        get
        {
            if (boostIndicators == null)
                boostIndicators = GetComponentsInChildren<Image>();
            return boostIndicators;
        }
    }

    public CanvasGroup Group
    {
        get
        {
            if (group == null)
                group = GetComponent<CanvasGroup>();
            return group;
        }
    }

    public int BoostCharge
    {
        set
        {
            Color mixColor = new Color(0.4f, 0.4f, 0.4f, 0.2f);
            for (int i = 0; i < BoostIndicators.Length; i++)
            {
                BoostIndicators[i].color = BoostIndicators.Length - i <= value ? color : color * mixColor;
            }
            Group.alpha = value == -1 ? 0.5f : 1f;
        }
    }

    private Color color;
    private Image[] boostIndicators;
    private CanvasGroup group;
}
