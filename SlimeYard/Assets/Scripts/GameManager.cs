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
    [Header("Round System")]
    public int NumRounds = 3;

    [Header("Menu/Battle Stuff")]
    public CanvasGroup BattleCanvas;
    public CanvasGroup MenuCanvas;
    public Transform BattleCamPos;
    public Transform MenuCamPos;

    [Header("Player Specific Settings")]
    public Color NeutralColor;
    public Color[] PlayerColors = new Color[numPlayers];
    public Vector2[] PlayerStartPositions = new Vector2[numPlayers];
    public float[] PlayerStartOrientation = new float[numPlayers];
    public Vector2[] PlayerMenuPositions = new Vector2[numPlayers];
    public float[] PlayerMenuOrientation = new float[numPlayers];

    [Header("Prefabs and ObjectPools")]
    public ObjectPool BlobObjectPool;
    public GameObject SnailPrefab;
    public GameObject TrailPrefab;

    [Header("GUI References")]
    public GameObject InfoOverlay;
    public Text InfoText;
    public Text RoundText;
    public GameObject[] PlayerPanels = new GameObject[numPlayers];
    public Text[] Score = new Text[numPlayers];
    public Text[] Names = new Text[numPlayers];

    [Header("Timing")]
    public float RoundWaitingTime = 2f;
    public float ResetWaitingTime = 2f;

    private Snail[] snails = new Snail[numPlayers];
    private BoostGauge[] boostGauges = new BoostGauge[numPlayers];
    private int[] score = new int[numPlayers];

    private bool gameOver = false;
    private int currentRound;
    private GameState gameState;

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
        BlobObjectPool.Init();
        InitGUI();
        UpdateScoreGUI();
        SetGameState(GameState.Menu);
    }

    private void Update()
    {
        if(gameState == GameState.Menu && Input.anyKeyDown)
        {
            SetGameState(GameState.Battle, ResetWaitingTime);
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void SetGameState(GameState newState, float transitionDuration = 0f)
    {
        gameState = newState;
        CanvasGroup newCanvas = null;
        CanvasGroup oldCanvas = null;
        Transform targetTransform = null;

        switch(gameState)
        {
            case GameState.Battle:
                SoundManager.Instance.PlaySound(SoundManager.SoundEffectType.GameStart);
                targetTransform = BattleCamPos;
                newCanvas = MenuCanvas;
                oldCanvas = BattleCanvas;
                StartRound(transitionDuration);
                StartCoroutine(WaitForEnableMovement(transitionDuration, true));
                break;
            case GameState.Menu:
                targetTransform = MenuCamPos;
                newCanvas = BattleCanvas;
                oldCanvas = MenuCanvas;
                BlobObjectPool.Reset();
                currentRound = 1;
                for (int i = 0; i < numPlayers; i++)
                {
                    score[i] = 0;
                    Snail snail = snails[i];
                    snail.transform.position = PlayerMenuPositions[i];
                    snail.transform.eulerAngles = new Vector3(0f, 0f, PlayerMenuOrientation[i]);
                    snail.enabled = false;
                    snail.Trail.Clear();
                }
                break;
        }
        Camera.main.DOKill();
        Camera.main.transform.DOMove(targetTransform.position, transitionDuration);
        Camera.main.transform.DORotateQuaternion(targetTransform.rotation, transitionDuration);

        newCanvas.DOKill();
        newCanvas.DOFade(0f, transitionDuration * 0.5f);
        oldCanvas.DOKill();
        oldCanvas.DOFade(1f, transitionDuration * 0.5f).SetDelay(transitionDuration * 0.5f);

        SoundManager.Instance.SwitchMusic(gameState, transitionDuration);
    }

    private void StartRound(float transitionDuration = 0f)
    {
        UpdateScoreGUI();
        for (int i = 0; i < numPlayers; i++)
        {
            Snail snail = snails[i];
            snail.Reset();
            snail.transform.DOKill();
            snail.transform.DOMove(PlayerStartPositions[i], transitionDuration).SetDelay(transitionDuration * 0.4f);
            snail.transform.DORotate(new Vector3(0f, 0f, PlayerStartOrientation[i]), transitionDuration * 0.4f);
            snail.Trail.Clear();

            InfoText.text = "";
            RoundText.text = "Round " + currentRound + " : " + NumRounds;
        }
        BlobObjectPool.Reset();
        gameOver = false;
        InfoOverlay.SetActive(false);
    }
   
    private void InitGUI()
    {
        for (int i = 0; i < numPlayers; i++)
        {
            foreach (Text text in PlayerPanels[i].GetComponentsInChildren<Text>())
            {
                text.color = PlayerColors[i];
            }
            boostGauges[i] = PlayerPanels[i].GetComponentInChildren<BoostGauge>();
            boostGauges[i].BoostSteps = snails[i].BoostSteps;
            boostGauges[i].Color = PlayerColors[i];
        }
    }

    private void UpdateScoreGUI()
    {
        for(int i = 0; i < numPlayers; i++)
        {
            Names[i].text = ((Player)i).ToString();
            Score[i].text = score[i].ToString();
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
        slimeBlob.Create(positions);
        slimeBlob.Color = PlayerColors[snailIndex];
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

        bool lastRound = (currentRound == NumRounds);
        InfoOverlay.SetActive(true);

        int winnerIndex = gameOverType != GameOverType.SnailCrash ? (snailIndex + 1) % 2 : -1;

        switch(gameOverType)
        {
            case GameOverType.SnailCrash:
                SoundManager.Instance.PlaySound(SoundManager.SoundEffectType.SnailCrash);
                InfoText.text = "Snail Crash!";
                InfoText.color = NeutralColor;
                break;
            case GameOverType.WallCrash:
                SoundManager.Instance.PlaySound(SoundManager.SoundEffectType.WallCrash);
                IncreaseAndShowWinnerScore(winnerIndex);
                break;
            case GameOverType.Slime:
                SoundManager.Instance.PlaySound(SoundManager.SoundEffectType.Slime);
                IncreaseAndShowWinnerScore(winnerIndex);
                break;
        }

        UpdateScoreGUI();
        SoundManager.Instance.PlaySound(SoundManager.SoundEffectType.ScoreChange, RoundWaitingTime * 0.5f);
        yield return new WaitForSeconds(RoundWaitingTime);
        if (!lastRound)
        {
            currentRound++;
            EnableSnailMovement(true);
            StartRound();
        }
        else
        {
            DisplayResult();
            yield return new WaitForSeconds(ResetWaitingTime);
            SetGameState(GameState.Menu, ResetWaitingTime);
        }
    }

    private void IncreaseAndShowWinnerScore(int winnerIndex)
    {
        score[winnerIndex]++;
        InfoText.text = (Player)winnerIndex + " won!";
        InfoText.color = PlayerColors[winnerIndex];
    }

    private void DisplayResult()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEffectType.GameOver);
        int winnerIndex = -1;
        int highestScore = -1;
        int lastScore = score[0];
        bool tie = true;
        for(int i = 0; i < numPlayers; i++)
        {
            tie = tie && score[i] == lastScore;
            if(highestScore < score[i])
            {
                highestScore = score[i];
                winnerIndex = i;
            }
        }

        if (tie)
        {
            InfoText.text = "Tie!";
            InfoText.color = NeutralColor;
        }
        else
        {
            InfoText.text = "Game over! " + (Player)winnerIndex + " won!";
            InfoText.color = PlayerColors[winnerIndex];
        }
    }

}
