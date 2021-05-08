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

    public float hoverSpeed = 0.3f;
    public float hoverTurnSpeed = 1.5f;
    public float hoverCircleSpeed = 0.5f;
    public float hoverCircleRadius = 1;

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

        Vector3 accelerationV = targetDirection * Menu.predMoveAcceleration;
        Vector3 newVelocity = velocity + accelerationV * Time.deltaTime;
        if (newVelocity.magnitude < Menu.predMoveSpeed)
        {
            velocity = newVelocity;
        }
        else
        {
            velocity = newVelocity.normalized * Menu.predMoveSpeed;
        }
        float limit = 0.8f * velocity.magnitude;
        if (velocity.y > limit) velocity.y = limit;
        if (velocity.y < -limit) velocity.y = -limit;
        transform.position += velocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(velocity);

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
        float limit = 0.2f;
        if (direction.y > limit) direction.y = limit;
        if (direction.y < -limit) direction.y = -limit;
        transform.rotation = Quaternion.LookRotation(direction);

        velocity = direction * hoverSpeed;
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
        keepGoing = false;
    }

    bool keepGoing = false;
    private bool Stoop()
    {
        Vector3 targetPos = target.transform.position;
        if (!keepGoing)
        {
            Vector3 targetDirection = (targetPos - transform.position).normalized;

            Vector3 accelerationV = targetDirection * Menu.predStoopAcceleration;
            Vector3 newVelocity = velocity + accelerationV * Time.deltaTime;
            if (newVelocity.magnitude < Menu.predStoopSpeed)
            {
                velocity = newVelocity;
            }
            else
            {
                velocity = newVelocity.normalized * Menu.predStoopSpeed;
            }
        }
        transform.position += velocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(velocity);

        if (transform.position.y < targetPos.y - 6)
        {
            stooping = false;
            return true;
        } else if (transform.position.y < targetPos.y)
        {
            keepGoing = true;
        }

        return false;
    }
}
