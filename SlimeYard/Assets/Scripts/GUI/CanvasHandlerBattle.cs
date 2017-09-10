using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CanvasHandlerBattle : MonoBehaviour, CanvasHandler
{
    public UIObject[] UIObjects;

    public void Hide(float duration, float delay)
    {
        foreach(UIObject obj in UIObjects)
        {
            obj.Hide(duration, delay);
        }
    }

    public void Show(float duration, float delay)
    {
        foreach (UIObject obj in UIObjects)
        {
            obj.Show(duration, delay);
        }
    }
}
