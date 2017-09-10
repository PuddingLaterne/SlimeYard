using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CanvasHandlerMenu : MonoBehaviour, CanvasHandler
{
    public CanvasGroup Group;

    public void Hide(float duration, float delay)
    {
        Group.DOKill();
        Group.DOFade(0f, duration).SetDelay(delay);
    }

    public void Show(float duration, float delay)
    {
        Group.DOKill();
        Group.DOFade(1f, duration).SetDelay(delay);
    }

}
