using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Snail : MonoBehaviour
{
    public Transform TrailOrigin;

    public UnityAction<bool> OnCrash;
    public UnityAction<Vector2[]> OnCreateSlimeBlob;
    public UnityAction<int> OnBoostChargeChanged;

    public Player AssignedPlayer { get; set; }

    public float MoveSpeed = 1f;
    public float TurnSpeed = 90f;
    public AnimationCurve BoostSpeedMultiplier;
    public float MaxBoostDuration = 1;
    public int BoostSteps = 4;
    public float BoostRecoveryTime = 2f;

    public Trail Trail { get; set; }

    private float boostMultiplier;
    private bool isBoosting;
    private float boostActivationTime;
    private float boostCharge;
    private float boostDuration;

    public Color Color
    {
        set
        {
            GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", value);
        }
    }

    private const float pointThreshold = 0.1f;

    public void Reset()
    {
        boostCharge = 1f;
    }

    public void Update ()
    {
        UpdateTransform();
        UpdateTrail();
        UpdateBoost();
    }

    private void UpdateBoost()
    {
        if(!isBoosting)
        {
            int charge = (int)(boostCharge * BoostSteps);
            OnBoostChargeChanged(charge);
            boostCharge += Time.deltaTime / BoostRecoveryTime;
            boostCharge = Mathf.Min(boostCharge, 1f);
            if (Input.GetButton("Boost" + AssignedPlayer) && charge > 0)
            {
                OnBoostChargeChanged(-1);
                isBoosting = true;
                boostDuration = MaxBoostDuration * ((float)charge / BoostSteps);
                boostActivationTime = Time.time;
                boostCharge = 0f;
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
        if(Vector3.Distance(TrailOrigin.position, Trail.LatestPosition) > pointThreshold)
            Trail.AddPoint(TrailOrigin.position);
    }

    private void UpdateTransform()
    {
        transform.position += transform.up * MoveSpeed * (isBoosting ? boostMultiplier : 1f) * Time.deltaTime;

        float rotation = transform.eulerAngles.z;
        rotation += Input.GetAxis(AssignedPlayer.ToString()) * TurnSpeed  * Time.deltaTime;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, rotation);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Environment"))
        {
            OnCrash(false);
        }
        else
        {
            OnCrash(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.gameObject.CompareTag(AssignedPlayer.ToString()))
        {
            OnCrash(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag(AssignedPlayer.ToString()) && collider.GetComponent<Trail>() != null)
        {
            OnCreateSlimeBlob(Trail.GetShapePositions(transform.position));
        }
    }
}
