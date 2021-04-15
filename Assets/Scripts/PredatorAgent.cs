using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorAgent : MonoBehaviour
{
    public Vector3 velocity = Vector3.zero;

    private readonly int hoverTime = 5;
    private float angle = 0;
    private float HoverTimeCounter = 0;
    private bool stooping = false;
    private GameObject target;
    private Animator falconAnimator;

    const float hoverSpeed = 0.3f;
    const float stoopTurnSpeed = 3f;
    const float hoverTurnSpeed = 1.5f;
    const float hoverCircleSpeed = 0.5f;
    const float hoverCircleRadius = 1;

    private Flock flock;

    private enum PredState
    {
        MoveAboveMurmuration,
        Hover,
        Stoop,
    }
    private PredState predState = PredState.MoveAboveMurmuration;

    private void Start()
    {
        falconAnimator = this.GetComponentInChildren<Animator>();
    }

    public void Initialize(Flock flock)
    {
        this.flock = flock;
    }

    void Update()
    {
        if (Menu.runSimulation)
        {
            switch (predState)
            {
                case PredState.MoveAboveMurmuration:
                    if (Move())
                    {
                        predState = PredState.Hover;
                        falconAnimator.SetInteger("flightType", 1);
                    }
                    break;
                case PredState.Hover:
                    if (Hover())
                    {
                        predState = PredState.Stoop;
                        GameObject predTarget = flock.FindPredatorTarget();
                        InitStoop(predTarget);
                        falconAnimator.SetInteger("flightType", 2);
                    }
                    break;
                case PredState.Stoop:
                    if (Stoop())
                    {
                        predState = PredState.MoveAboveMurmuration;
                        falconAnimator.SetInteger("flightType", 0);
                    }
                    break;
            }
        }
    }

    private bool Move()
    {
        Vector3 hoverPoint = new Vector3(0, Menu.predHoverHeight, 0) + flock.focalPoint;
        Vector3 targetDirection = hoverPoint - transform.position;
        targetDirection = targetDirection.normalized;
        float singleStep = Menu.predMoveTurnSpeed * Time.deltaTime;
        Vector3 direction = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
        transform.rotation = Quaternion.LookRotation(direction);

        velocity = direction * Menu.predMoveSpeed;
        velocity += Vector3.down * flock.gravity;
        transform.position += velocity;

        //return true if its within 1 unit of target position
        float dist = Vector3.Distance(hoverPoint, transform.position);
        if (dist < 3) return true;
        else return false;
    }

    private bool Hover()
    {
        Vector3 hoverPoint = new Vector3(0, Menu.predHoverHeight, 0) + flock.focalPoint;
        angle += Time.deltaTime * hoverCircleSpeed;
        angle = angle % 360;
        float toRadians = angle * Mathf.PI / 180;
        float x = hoverCircleRadius * Mathf.Sin(toRadians);
        float y = hoverCircleRadius * Mathf.Cos(toRadians);
        Vector3 circlePoint = new Vector3(x, 0, y) + hoverPoint;

        Vector3 targetDirection = circlePoint - transform.position;
        targetDirection = targetDirection.normalized;
        float singleStep = hoverTurnSpeed * Time.deltaTime;
        Vector3 direction = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
        transform.rotation = Quaternion.LookRotation(direction);

        velocity = direction * hoverSpeed;
        velocity += Vector3.down * flock.gravity;
        transform.position += velocity;

        HoverTimeCounter += Time.deltaTime;
        if (HoverTimeCounter > hoverTime)
        {
            HoverTimeCounter = 0;
            return true;
        }
        return false;
    }

    private void InitStoop(GameObject target)
    {
        this.target = target;
    }

    private bool Stoop()
    {
        Vector3 targetPos = target.transform.position;
        Vector3 targetDirection = (targetPos - transform.position).normalized;
        float singleStep = stoopTurnSpeed * Time.deltaTime;
        Vector3 direction = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
        if (direction.y < -0.8f)
        {
            velocity = direction * Menu.predStoopSpeed;
            stooping = true;
        }
        else
        {
            if (!stooping)
            {
                velocity = direction * Menu.predMoveSpeed;
            }
            else
            {
                direction.y = -0.8f;
            }
        }
        if (!(transform.position.y < targetPos.y))
            transform.rotation = Quaternion.LookRotation(direction);

        transform.position += velocity;

        if (transform.position.y < targetPos.y - 4)
        {
            stooping = false;
            return true;
        }
        else return false;
    }
}
