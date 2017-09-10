using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class Snail : MonoBehaviour
{
    public Transform Model;
    public Transform TrailOrigin;

    [Header("Sound")]
    public AudioSource SlimeSound;
    public SoundEffect BoostSound;
    public SoundEffect BlobCreateSound;

    [Header("Speed")]
    public float MoveSpeed = 1f;
    public float TurnSpeed = 90f;
    public float SlimeSpeedBonus = 0.2f;

    [Header("Boost")]
    public AnimationCurve BoostSpeedMultiplier;
    public float MaxBoostDuration = 1;
    public int BoostSteps = 4;
    public float BoostRecoveryTime = 2f;

    public UnityAction<GameOverType> OnCrash;
    public UnityAction<Vector2[]> OnCreateSlimeBlob;
    public UnityAction<int> OnBoostChargeChanged;

    public Player AssignedPlayer { get; set; }
    public Trail Trail { get; set; }
    public Color Color
    {
        set
        {
            Mat.SetColor("_Color", value);
        }
    }
    public Animator Anim
    {
        get
        {
            if (anim == null)
                anim = GetComponentInChildren<Animator>();
            return anim;
        }
    }
    public Material Mat { get { if (mat == null) mat = GetComponentInChildren<SkinnedMeshRenderer>().material; return mat; } }

    private float boostMultiplier;
    private bool isBoosting;
    private bool isMovingOnSlime;
    private float boostActivationTime;
    private float boostCharge;
    private float boostDuration;
    private Animator anim;
    private Material mat; 

    private const float pointThreshold = 0.15f;

    public void Reset()
    {
        boostCharge = 0f;
        isMovingOnSlime = false;
    }

    public void Update ()
    {
        UpdateTransform();
        UpdateTrail();
        UpdateBoost();
    }

    public void OnEnable()
    {
        SlimeSound.Play();
    }

    public void OnDisable()
    {
        SlimeSound.Stop();
        Mat.SetColor("_EmissionColor", Color.black);
    }

    private void UpdateBoost()
    {
        if(!isBoosting)
        {
            boostCharge += Time.deltaTime / BoostRecoveryTime;
            boostCharge = Mathf.Min(boostCharge, 1f);
            int charge = (int)(boostCharge * BoostSteps);
            float chargeNormalized = (float)charge / BoostSteps;
            OnBoostChargeChanged(charge);

            Mat.SetColor("_EmissionColor", Color.white * Mathf.Abs(Mathf.Sin(Time.time * 10 * chargeNormalized)) * 0.3f * chargeNormalized);

            if (Input.GetButton("Boost" + AssignedPlayer) && charge > 0)
            {
                BoostSound.Play();
                OnBoostChargeChanged(-1);
                isBoosting = true;
                boostDuration = MaxBoostDuration * chargeNormalized;
                Model.DOKill();
                Model.DOScale(1.3f, boostDuration * 0.5f).OnComplete(() => Model.DOScale(1f, boostDuration * 0.5f));
                boostActivationTime = Time.time;
                boostCharge = 0f;
                Mat.SetColor("_EmissionColor", Color.white * 0.3f * chargeNormalized);
            }
        }
        else
        {
            float boostTime = Time.time - boostActivationTime;
            boostMultiplier = BoostSpeedMultiplier.Evaluate(boostTime / boostDuration);
            if (boostTime > MaxBoostDuration)
            {
                isBoosting = false;
                boostActivationTime = -1f;
            }
        }
    }

    private void UpdateTrail()
    {
        if (Vector3.Distance(TrailOrigin.position, Trail.LatestPosition) > pointThreshold)
        {
            if (Random.Range(0f, 1f) <= Trail.SplatPlacementProbability)
            {
                SlimeSplatPool.Instance.CreateSlimeSplat(Trail.Color, TrailOrigin.position, Trail.PointLifetime);
            }
            Trail.AddPoint(TrailOrigin.position);
        }
    }

    private void UpdateTransform()
    {
        float boostMultiplier = isBoosting ? this.boostMultiplier : 0f;
        float slimeMultiplier = isMovingOnSlime ? SlimeSpeedBonus : 0f; 

        float moveSpeed = MoveSpeed * (1f + boostMultiplier) * (1f + slimeMultiplier);
        float turnSpeed = TurnSpeed * (1f + boostMultiplier * 0.5f) * (1f + slimeMultiplier * 0.5f);

        transform.position += transform.up * moveSpeed * Time.deltaTime;

        float rotation = transform.eulerAngles.z;
        float rotationInput = Input.GetAxis(AssignedPlayer.ToString());
        Anim.SetFloat("rotation", rotationInput);
        rotation += rotationInput * turnSpeed * Time.deltaTime;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, rotation);
    }
 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Environment"))
        {
            Anim.SetTrigger("dead");
            OnCrash(GameOverType.WallCrash);
        }
        else
        {
            Anim.SetTrigger("dead");
            OnCrash(GameOverType.SnailCrash);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag(AssignedPlayer.ToString()))
        {
            Anim.SetTrigger("dead");
            OnCrash(GameOverType.Slime);
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.GetComponent<SlimeBlob>() != null)
        {
            isMovingOnSlime = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag(AssignedPlayer.ToString()) && collider.GetComponent<Trail>() != null)
        {
            BlobCreateSound.Play();
            OnCreateSlimeBlob(Trail.GetShapePositions(transform.position));
        }
        if(collider.GetComponent<SlimeBlob>() != null)
        {
            isMovingOnSlime = false;
        }
    }
}
