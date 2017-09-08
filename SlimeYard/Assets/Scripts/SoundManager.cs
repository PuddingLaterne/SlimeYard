using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SoundManager : MonoBehaviour
{
    public enum SoundEffectType
    {
        SnailCrash, WallCrash, ScoreChange, GameOver, GameStart, Slime
    }

    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<SoundManager>();
            return instance;
        }
    }
    private static SoundManager instance; 

    public AudioSource MenuMusic;
    public AudioSource BattleMusic;

    public SoundEffect SnailCrash;
    public SoundEffect WallCrash;
    public SoundEffect ScoreChange;
    public SoundEffect GameOver;
    public SoundEffect GameStart;
    public SoundEffect Slime;

    private AudioSource activeMusic;

    public void SwitchMusic(GameState gameState, float transitionDuration)
    {
        AudioSource oldMusic = activeMusic;
        AudioSource newMusic = gameState == GameState.Menu ? MenuMusic : BattleMusic;
        newMusic.volume = 0f;
        newMusic.Stop();
        newMusic.Play();
        newMusic.DOFade(1f, transitionDuration);
        if(oldMusic != null)
        {
            oldMusic.DOFade(0f, transitionDuration);
        }
        activeMusic = newMusic;
    }	

    public void PlaySound(SoundEffectType type, float delay = 0f)
    {
        SoundEffect effect = null;
        switch(type)
        {
            case SoundEffectType.SnailCrash:
                effect = SnailCrash;
                break;
            case SoundEffectType.WallCrash:
                effect = WallCrash;
                break;
            case SoundEffectType.ScoreChange:
                effect = ScoreChange;
                break;
            case SoundEffectType.GameOver:
                effect = GameOver;
                break;
            case SoundEffectType.GameStart:
                effect = GameStart;
                break;
            case SoundEffectType.Slime:
                effect = Slime;
                break;
        }
        StartCoroutine(WaitForPlaySound(effect, delay));
    }

    private IEnumerator WaitForPlaySound(SoundEffect effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.Play();
    }
}
