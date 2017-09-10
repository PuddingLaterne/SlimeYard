using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public enum Player
{
    Player1,
    Player2
}

public enum GameState
{
    Menu,
    Battle
}

public enum GameOverType
{
    Slime,
    WallCrash,
    SnailCrash
}

public class GameManager : MonoBehaviour
{
    [Header("Menu/Battle Stuff")]
    public CanvasHandlerBattle BattleCanvas;
    public CanvasHandlerMenu MenuCanvas;
    public Transform BattleCamPos;
    public Transform MenuCamPos;
    public Fill Fill;

    [Header("Player Specific Settings")]
    public Color NeutralColor;
    public Color[] PlayerColors = new Color[numPlayers];
    public Vector2[] PlayerStartPositions = new Vector2[numPlayers];
    public Vector2[] PlayerMenuPositions = new Vector2[numPlayers];
    public float[] PlayerMenuOrientation = new float[numPlayers];

    [Header("Prefabs and ObjectPools")]
    public ObjectPool BlobObjectPool;
    public GameObject SnailPrefab;
    public GameObject TrailPrefab;

    [Header("GUI References")]
    public UIObject Info;
    public GameObject[] PlayerPanels = new GameObject[numPlayers];

    [Header("Timing")]
    public float RoundWaitingTime = 2f;
    public float BattleStartDuration = 4f;
    public float BackToMenuDuration = 2f;

    private Snail[] snails = new Snail[numPlayers];
    private BoostGauge[] boostGauges = new BoostGauge[numPlayers];
    private ScoreDisplay[] scoreDisplays = new ScoreDisplay[numPlayers];
    private int[] score = new int[numPlayers];

    private bool gameOver = false;
    private GameState gameState;

    private const int maxScore = 3;
    private const int numPlayers = 2;

    public void Start()
    {
        for (int i = 0; i < numPlayers; i++)
        {
            int snailIndex = i;
            Snail snail = Instantiate(SnailPrefab).GetComponent<Snail>();
            snail.AssignedPlayer = (Player)i;
            CreateTrail(snail);

            snail.Color = PlayerColors[i];
            snail.Trail.Color = PlayerColors[i];

            snail.gameObject.tag = snail.AssignedPlayer.ToString();
            snail.Trail.gameObject.tag = snail.gameObject.tag;

            snail.OnCrash += (type) => { if(!gameOver) StartCoroutine(GameOver(snailIndex, type)); };
            snail.OnCreateSlimeBlob += (positions) => { CreateSlimeBlob(positions, snailIndex); };
            snail.OnBoostChargeChanged += (charge) => { UpdateBoostGUI(charge, snailIndex); };

            snails[i] = snail;

        }
        Info.Hide(0f, 0f);
        BlobObjectPool.Init();
        SlimeSplatPool.Instance.Init();
        InitGUI();
        UpdateScoreGUI();
        SetGameState(GameState.Menu);
    }

    private void Update()
    {
        if(gameState == GameState.Menu && Input.anyKeyDown)
        {
            SetGameState(GameState.Battle, BattleStartDuration);
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void SetGameState(GameState newState, float transitionDuration = 0f)
    {
        gameState = newState;
        CanvasHandler newCanvas = null;
        CanvasHandler oldCanvas = null;
        Transform targetTransform = null;

        switch(gameState)
        {
            case GameState.Battle:
                SoundManager.Instance.PlaySound(SoundManager.SoundEffectType.GameStart);
                targetTransform = BattleCamPos;
                newCanvas = BattleCanvas;
                oldCanvas = MenuCanvas;
                StartRound(transitionDuration);
                StartCoroutine(WaitForEnableMovement(transitionDuration, true));
                break;
            case GameState.Menu:
                targetTransform = MenuCamPos;
                newCanvas = MenuCanvas;
                oldCanvas = BattleCanvas;
                BlobObjectPool.Reset();
                SlimeSplatPool.Instance.Reset();
                for (int i = 0; i < numPlayers; i++)
                {
                    score[i] = 0;
                    Snail snail = snails[i];
                    snail.transform.position = PlayerMenuPositions[i];
                    snail.transform.eulerAngles = new Vector3(0f, 0f, PlayerMenuOrientation[i]);
                    snail.enabled = false;
                    snail.Trail.Clear();
                    snail.Anim.SetTrigger("start");
                }
                break;
        }
        Camera.main.DOKill();
        Camera.main.transform.DOMove(targetTransform.position, transitionDuration);
        Camera.main.transform.DORotateQuaternion(targetTransform.rotation, transitionDuration);

        newCanvas.Show(transitionDuration * 0.4f, transitionDuration * 0.6f);
        oldCanvas.Hide(transitionDuration * 0.4f, 0f);
        Info.Hide(transitionDuration * 0.4f, 0f);

        SoundManager.Instance.SwitchMusic(gameState, transitionDuration);
    }

    private void StartRound(float transitionDuration = 0f)
    {
        UpdateScoreGUI();
        for (int i = 0; i < numPlayers; i++)
        {
            int index = i;
            Snail snail = snails[i];
            snail.Reset();
            snail.transform.DOKill();
            snail.transform.eulerAngles = new Vector3(0f, 0f, PlayerMenuOrientation[index]);
            snail.transform.DOMove(PlayerStartPositions[index], transitionDuration);
            snail.Trail.Clear();
            snail.Anim.SetTrigger("move");
        }
        BlobObjectPool.Reset();
        SlimeSplatPool.Instance.Reset();
        gameOver = false;
    }
   
    private void InitGUI()
    {
        for (int i = 0; i < numPlayers; i++)
        {
            PlayerPanels[i].GetComponent<Image>().color = PlayerColors[i];

            foreach (Text text in PlayerPanels[i].GetComponentsInChildren<Text>())
            {
                text.color = PlayerColors[i];
            }

            boostGauges[i] = PlayerPanels[i].GetComponentInChildren<BoostGauge>();
            boostGauges[i].Color = PlayerColors[i];
            boostGauges[i].BoostCharge = 0;

            scoreDisplays[i] = PlayerPanels[i].GetComponentInChildren<ScoreDisplay>();
            scoreDisplays[i].Color = PlayerColors[i];
        }
    }

    private void UpdateScoreGUI()
    {
        for(int i = 0; i < numPlayers; i++)
        {
            scoreDisplays[i].Score = score[i];
        }
    }

    private void UpdateBoostGUI(int boostCharge, int snailIndex)
    {
        boostGauges[snailIndex].BoostCharge = boostCharge;
    }

    private void CreateSlimeBlob(Vector2[] positions, int snailIndex)
    {
        if (positions.Length < 3) { Debug.Log("failed to create slimeblob"); return; }

        GameObject slimeBlobObject = BlobObjectPool.GetObjectFromPool();
        slimeBlobObject.SetActive(true);
        slimeBlobObject.name = "Slimeblob " + (Player)snailIndex;
        slimeBlobObject.tag = ((Player)snailIndex).ToString();

        SlimeBlob slimeBlob = slimeBlobObject.GetComponent<SlimeBlob>();
        slimeBlob.Color = PlayerColors[snailIndex];
        slimeBlob.Create(positions);
    }
    
    private void CreateTrail(Snail snail)
    {
        GameObject trail = Instantiate(TrailPrefab);
        trail.name = "Trail " + snail.AssignedPlayer;
        snail.Trail = trail.GetComponent<Trail>();
    }

    private IEnumerator WaitForEnableMovement(float waitingTime, bool enabled)
    {
        yield return new WaitForSeconds(waitingTime);
        EnableSnailMovement(enabled);
    }

    private void EnableSnailMovement(bool enabled)
    {
        foreach (Snail snail in snails)
        {
            snail.enabled = enabled;
        }
    }

    private IEnumerator GameOver(int snailIndex, GameOverType gameOverType)
    {
        gameOver = true;
        EnableSnailMovement(false);

        int winnerIndex = gameOverType != GameOverType.SnailCrash ? (snailIndex + 1) % 2 : -1;
        Color color = NeutralColor;

        switch(gameOverType)
        {
            case GameOverType.SnailCrash:
                SoundManager.Instance.PlaySound(SoundManager.SoundEffectType.SnailCrash);
                Info.Text = "Snail Crash!";
                color = NeutralColor;
                break;
            case GameOverType.WallCrash:
                SoundManager.Instance.PlaySound(SoundManager.SoundEffectType.WallCrash);
                Info.Text = "Shattered!";
                score[winnerIndex]++;
                color = PlayerColors[winnerIndex];
                break;
            case GameOverType.Slime:
                SoundManager.Instance.PlaySound(SoundManager.SoundEffectType.Slime);
                score[winnerIndex]++;
                color = PlayerColors[winnerIndex];
                break;
        }
        Info.Color = color;

        UpdateScoreGUI();
        SoundManager.Instance.PlaySound(SoundManager.SoundEffectType.ScoreChange, RoundWaitingTime * 0.5f);

        if (winnerIndex == -1 || score[winnerIndex] != maxScore)
        {

            Info.Show(RoundWaitingTime * 0.3f, RoundWaitingTime * 0.3f);
            Fill.FillScreen(color, RoundWaitingTime * 0.4f, RoundWaitingTime * 0.4f);
            yield return new WaitForSeconds(RoundWaitingTime);        
            Info.Hide(RoundWaitingTime * 0.3f, 0f);
            EnableSnailMovement(true);
            StartRound();
        }
        else
        {
            Info.Text = "Game Over!";
            Info.Show(BackToMenuDuration * 0.3f, RoundWaitingTime * 0.2f);
            Fill.FillScreen(color, BackToMenuDuration * 0.4f, BackToMenuDuration * 0.3f);
            yield return new WaitForSeconds(BackToMenuDuration);
            SetGameState(GameState.Menu, BackToMenuDuration);
        }
    }
}
