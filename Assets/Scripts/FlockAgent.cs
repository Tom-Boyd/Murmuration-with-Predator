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

    public IEnumerator starlingAnimate(Animator animator)
    {
        float wait_time = Random.Range(0.0f, 1.0f);
        yield return new WaitForSeconds(wait_time);
        animator.SetBool("flap", true);
    }

    public void Move(Vector3 pull, bool escaping)
    {
        Vector3 acceleration;
        float maxSpeed = flock.speed;
        if (!escaping)
        {
            acceleration = pull * flock.acceleration;
        }
        else
        {
            acceleration = pull * flock.acceleration * 2;
            maxSpeed = flock.speed * 2;
        }
        Vector3 newVelocity = velocity + acceleration * Time.deltaTime;
        if (newVelocity.magnitude < maxSpeed)
        {
            velocity = newVelocity;
        }
        else
        {
            velocity = newVelocity.normalized * maxSpeed;
        }
        float limit = 0.8f * velocity.magnitude;
        if (velocity.y > limit) velocity.y = limit;
        if (velocity.y < -limit) velocity.y = -limit;
        transform.position += velocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(velocity);

    }
}