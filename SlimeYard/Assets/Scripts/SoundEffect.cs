using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    public float MinPitch = 0.5f;
    public float MaxPitch = 1.2f;

    public float MinVolume = 0.9f;
    public float MaxVolume = 1f;

    private AudioSource source;

    public void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void Play()
    {
        source.pitch = Random.Range(MinPitch, MaxPitch);
        source.volume = Random.Range(MinVolume, MaxVolume);
        source.Play();
    }

}
