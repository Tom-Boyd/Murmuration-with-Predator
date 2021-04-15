using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Flock : MonoBehaviour
{
    public Vector3 focalPoint;
    public List<FlockAgent> agents = new List<FlockAgent>();
    public List<Transform> agentsTransform = new List<Transform>();
    public float neighborRadius = 2f;
    public int neighbourToConsider = 7;
    public float gravity = 9f;
    public int startingCount = 250;
    public float separationWeight = 1;
    public float cohesionWeight = 1;
    public float alignmentWeight = 1;
    public float predatorWeight = 1;
    public float focalPointWeight = 1;
    public float speed = 1f;
    public float predatorDetectionDist = 20f;
    public float turnSpeed = 0.5f;
    public float agentDensity = 0.004f;

    const float hardSphere = 0.2f;
    const float blindAngle = 180 - (9f / 2);

    public DataStorage dataStorage;
    public Initialise initialise;

    void Update()
    {
        if (Menu.runSimulation)
        {

            foreach (FlockAgent agent in agents)
            {
                agent.Boid();
            }
        }
    }

    public Transform GetRandomStarling()
    {
        return agents[Random.Range(0, agents.Count - 1)].transform;
    }

    public void DeleteFlock()
    {
        foreach (FlockAgent agent in agents)
        {
            Destroy(agent.gameObject);
        }
        agents.Clear();
    }

    public GameObject FindPredatorTarget()
    {
        return agents[Random.Range(0, agents.Count - 1)].gameObject;
    }

    public List<FlockAgent> GetNearbyStarlings(FlockAgent agent)
    {
        List<FlockAgent> context = new List<FlockAgent>();
        var closeAgents = new Dictionary<FlockAgent, float>();

        foreach (FlockAgent a in agents)
        {
            if (agent.name == a.name) continue; //Ignore itself
            float distance = Vector3.Distance(agent.transform.position, a.transform.position);
            if (distance >= neighborRadius) continue;
            Vector3 targetDir = a.transform.position - agent.transform.position;
            float angle = Vector3.Angle(targetDir, agent.transform.forward);
            if (angle >= blindAngle) continue; //avoid blind angle
            if (distance <= hardSphere) continue; //avoid if in hard sphere

            bool added = false;
            foreach (KeyValuePair<FlockAgent, float> pair in closeAgents)
            {
                if (pair.Value > distance)
                {
                    closeAgents.Remove(pair.Key);
                    closeAgents.Add(a, distance);
                    added = true;
                    break;
                }
            }
            if (!added && closeAgents.Count < neighbourToConsider)
            {
                closeAgents.Add(a, distance);
            }
            if (closeAgents.Count == neighbourToConsider) break;
        }

        foreach (KeyValuePair<FlockAgent, float> pair in closeAgents)
        {
            context.Add(pair.Key);
        }
        return context;
    }
}
