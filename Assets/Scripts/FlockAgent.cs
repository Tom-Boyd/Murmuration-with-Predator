using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockAgent : MonoBehaviour
{

    private Flock flock;
    private PredatorAgent predatorAgent;

    private Vector3 velocity = Vector3.zero;
    public Vector3 AgentVelocity { get { return velocity; } }

    private List<FlockAgent> context;

    public Initialise initialise;

    public void Initialize(Flock flock, PredatorAgent predatorAgent)
    {
        this.flock = flock;
        this.predatorAgent = predatorAgent;
    }

    public void Boid()
    {
        this.context = flock.GetNearbyStarlings(this);
        Move(CalculateBoidForce());
    }

    private Vector3 CalculateBoidForce()
    {
        Vector3 move = FocalPoint() * flock.focalPointWeight;
        if (context.Count > 0)
        {
            Vector3 cohesion = Cohesion() * flock.cohesionWeight;
            Vector3 separation = Separation() * flock.separationWeight;
            Vector3 alignment = Alignment() * flock.alignmentWeight;
            move += separation + cohesion + alignment;
        }

        Vector3 predator = Predator() * flock.predatorWeight;
        Debug.DrawRay(transform.position, predator, Color.red);
        if (predator != Vector3.zero)
        {
            move += predator;
        }

        return move.normalized;
    }

    public IEnumerator starlingAnimate(Animator animator)
    {
        float wait_time = Random.Range(0.0f, 1.0f);
        yield return new WaitForSeconds(wait_time);
        animator.SetBool("flap", true);
    }

    public void Move(Vector3 pull)
    {
        float singleStep = flock.turnSpeed * Time.deltaTime;
        Vector3 direction = Vector3.RotateTowards(transform.forward, pull, singleStep, 0.0f);
        if (direction.y > 0.8f) direction.y = 0.8f;
        if (direction.y < -0.8f) direction.y = -0.8f;
        transform.rotation = Quaternion.LookRotation(direction);
        
        velocity = direction * flock.speed;
        velocity += Vector3.down * flock.gravity;
        transform.position += velocity;
    }

    public Vector3 Separation()
    {
        //Separation rule
        //steer away from neighbours
        Vector3 separationMove = Vector3.zero;
        foreach (FlockAgent otherStarling in context)
        {
            float dist = Vector3.Distance(otherStarling.transform.position, transform.position);
            float invserseDist = 1 / dist;
            Vector3 repulsion = (otherStarling.transform.position - transform.position) * invserseDist * -1;
            separationMove += repulsion;
        }

        separationMove = separationMove.normalized;

        return separationMove;
    }

    public Vector3 Cohesion()
    {
        //Cohesion rule
        //move towards the average position of neighbours
        Vector3 averagePos = Vector3.zero;
        foreach (FlockAgent otherStarling in context)
        {
            averagePos += otherStarling.transform.position;
        }

        Vector3 cohesionMove = averagePos - transform.position;

        cohesionMove = cohesionMove.normalized;

        return cohesionMove;
    }

    public Vector3 Alignment()
    {
        //Alignment rule
        //get in line with the average direction of neighbours
        Vector3 alignment = Vector3.zero;
        foreach (FlockAgent otherStarling in context)
        {
            alignment += otherStarling.AgentVelocity;
        }

        alignment = alignment.normalized;

        return alignment;
    }

    public Vector3 FocalPoint()
    {
        //move towards focal point
        float dist = Vector3.Distance(flock.focalPoint, transform.position);
        float invserseDist = 1 / dist;
        Vector3 attraction = (flock.focalPoint - transform.position) * invserseDist;
        attraction = attraction.normalized;
        return attraction;
    }

    public Vector3 Predator()
    {
        //move away from predator if within detection distance
        float dist = Vector3.Distance(predatorAgent.transform.position, transform.position);
        if (dist < flock.predatorDetectionDist)
        {
            float invserseDist = 1 / dist;
            Vector3 repulsion = (predatorAgent.transform.position - transform.position) * invserseDist * -1;
            repulsion = repulsion.normalized;
            return repulsion;
        }
        else
        {
            return Vector3.zero;
        }
    }
}