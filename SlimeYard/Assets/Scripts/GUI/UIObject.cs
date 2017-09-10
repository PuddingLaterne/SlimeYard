using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIObject : MonoBehaviour
{
    public Vector3 PosHidden;
    public Vector3 PosVisible;

    public RectTransform Rect { get { if (rect == null) rect = GetComponent<RectTransform>(); return rect; } }
    public string Text {set { if (text == null) text = GetComponentInChildren<Text>(); text.text = value; } }
    public Color Color { set { if (text == null) text = GetComponentInChildren<Text>(); text.color = value; } }

    private RectTransform rect;
    private Text text;

    public void Hide(float duration, float delay)
    {
        Rect.DOKill();
        Rect.DOAnchorPos(PosHidden, duration).SetDelay(delay).SetEase(Ease.InBack);
    }

    public void Show(float duration, float delay)
    {
        Rect.DOKill();
        Rect.DOAnchorPos(PosVisible, duration).SetDelay(delay).SetEase(Ease.OutBack);
    }
}
