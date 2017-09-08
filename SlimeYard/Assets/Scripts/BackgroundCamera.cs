using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundCamera : MonoBehaviour
{
    public RenderTexture Texture { get; private set; }

    private void OnEnable()
    {
        CreateRenderTexture();
        GetComponent<Camera>().targetTexture = Texture;
    }

    private void CreateRenderTexture()
    {
        Texture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
        Texture.Create();
    }

    private void OnDisable()
    {
        Texture.Release();
    }
}
