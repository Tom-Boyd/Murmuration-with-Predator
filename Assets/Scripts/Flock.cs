using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;

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
    public float acceleration = 5f;
    public float predatorDetectionDist = 20f;
    public float turnSpeed = 0.5f;
    public float agentDensity = 0.01f;
    public bool runSimulation = false;
    public bool runMetrics = false;

    const float hardSphere = 0.2f;
    const float blindAngle = 180 - (9f / 2);

    public DataStorage dataStorage;
    public Initialise initialise;
    public Grid grid;
    public PredatorAgent predatorAgent;

    void Update()
    {
        if (runSimulation)
        {
            // Create a native array of a single float to store the result. This example waits for the job to complete for illustration purposes
            NativeArray<Vector3> newVelocities = new NativeArray<Vector3>(agents.Count, Allocator.TempJob);
            NativeArray<bool> escaping = new NativeArray<bool>(agents.Count, Allocator.TempJob);
            NativeArray<Vector3> agentPositions = new NativeArray<Vector3>(agents.Count, Allocator.TempJob);
            NativeArray<Vector3> agentVelocities = new NativeArray<Vector3>(agents.Count, Allocator.TempJob);
            NativeMultiHashMap<int,int> boidCellNeighbours = new NativeMultiHashMap<int, int>(agents.Count, Allocator.TempJob);
            for (int i = 0; i < agents.Count; i++)
            {
                List<int> cellNeighboursList = grid.GetNeighbours(i);
                foreach (int cellNeighbour in cellNeighboursList)
                {
                    boidCellNeighbours.Add(i, cellNeighbour);
                }
                agentPositions[i] = agents[i].transform.position;
                agentVelocities[i] = agents[i].AgentVelocity;
            }
            // Set up the job data
            MyJob jobData = new MyJob()
            {
                newVelocities = newVelocities,
                escaping = escaping,
                agentPositions = agentPositions,
                agentVelocities = agentVelocities,
                boidCellNeighbours = boidCellNeighbours,
                neighborRadius = neighborRadius,
                neighbourToConsider = neighbourToConsider,
                focalPointWeight = focalPointWeight,
                cohesionWeight = cohesionWeight,
                separationWeight = separationWeight,
                alignmentWeight = alignmentWeight,
                predatorWeight = predatorWeight,
                focalPoint = focalPoint,
                predatorDetectionDist = predatorDetectionDist,
                predatorPosition = predatorAgent.transform.position,
            };
            // Schedule the job
            JobHandle handle = jobData.Schedule(newVelocities.Length, 1);
            // Wait for the job to complete
            handle.Complete();
            // All copies of the NativeArray point to the same memory, you can access the result in "your" copy of the NativeArray
            for (int i = 0; i < agents.Count; i++)
            {
                agents[i].Move(newVelocities[i], escaping[i]);
                grid.UpdatePosition(int.Parse(agents[i].gameObject.name), agents[i].transform.position);
            }

            // Free the memory allocated by the result array
            newVelocities.Dispose();
            escaping.Dispose();
            agentPositions.Dispose();
            agentVelocities.Dispose();
            boidCellNeighbours.Dispose();
        }
    }


    public struct MyJob : IJobParallelFor
    {
        public NativeArray<Vector3> newVelocities;
        public NativeArray<bool> escaping;
        [ReadOnly]
        public NativeArray<Vector3> agentPositions;
        [ReadOnly]
        public NativeArray<Vector3> agentVelocities;
        [ReadOnly]
        public NativeMultiHashMap<int, int> boidCellNeighbours;

        public void Execute(int index)
        {
            List<int> context = GetNearbyStarlings(index);
            newVelocities[index] = CalculateBoidForce(index, context);
        }

        public float focalPointWeight;
        public float cohesionWeight;
        public float separationWeight;
        public float alignmentWeight;
        public float predatorWeight;
        private Vector3 CalculateBoidForce(int index, List<int> context)
        {
            Vector3 move = FocalPoint(index) * focalPointWeight;
            if (context.Count > 0)
            {
                Vector3 cohesion = Cohesion(index, context) * cohesionWeight;
                Vector3 separation = Separation(index, context) * separationWeight;
                Vector3 alignment = Alignment(index, context) * alignmentWeight;
                move += separation + cohesion + alignment;
            }

            Vector3 predator = Predator(index) * predatorWeight;
            if (predator != Vector3.zero)
            {
                move += predator;
                escaping[index] = true;
            }
            else
            {
                escaping[index] = false;
            }

            return move.normalized;
        }

        public Vector3 Separation(int index, List<int> context)
        {
            //Separation rule
            //steer away from neighbours
            Vector3 separationMove = Vector3.zero;
            foreach (int otherStarlingIndex in context)
            {
                float dist = Vector3.Distance(agentPositions[otherStarlingIndex], agentPositions[index]);
                float invserseDist = 1 / dist;
                Vector3 repulsion = (agentPositions[otherStarlingIndex] - agentPositions[index]) * invserseDist * -1;
                separationMove += repulsion;
            }

            separationMove = separationMove.normalized;

            return separationMove;
        }

        public Vector3 Cohesion(int index, List<int> context)
        {
            //Cohesion rule
            //move towards the average position of neighbours
            Vector3 averagePos = Vector3.zero;
            foreach (int otherStarlingIndex in context)
            {
                averagePos += agentPositions[otherStarlingIndex];
            }

            Vector3 cohesionMove = averagePos - agentPositions[index];

            cohesionMove = cohesionMove.normalized;

            return cohesionMove;
        }

        public Vector3 Alignment(int index, List<int> context)
        {
            //Alignment rule
            //get in line with the average direction of neighbours
            Vector3 alignment = Vector3.zero;
            foreach (int otherStarlingIndex in context)
            {
                alignment += agentVelocities[otherStarlingIndex];
            }

            alignment = alignment.normalized;

            return alignment;
        }

        public Vector3 focalPoint;
        public Vector3 FocalPoint(int index)
        {
            //move towards focal point
            float dist = Vector3.Distance(focalPoint, agentPositions[index]);
            float invserseDist = 1 / dist;
            Vector3 attraction = (focalPoint - agentPositions[index]) * invserseDist;
            attraction = attraction.normalized;
            return attraction;
        }

        public float predatorDetectionDist;
        public Vector3 predatorPosition;
        public Vector3 Predator(int index)
        {
            //move away from predator if within detection distance
            float dist = Vector3.Distance(predatorPosition, agentPositions[index]);
            if (dist < predatorDetectionDist)
            {
                float invserseDist = 1 / dist;
                Vector3 repulsion = (predatorPosition - agentPositions[index]) * invserseDist * -1;
                repulsion = repulsion.normalized;
                return repulsion;
            }
            else
            {
                return Vector3.zero;
            }
        }
        
        public float neighborRadius;
        public int neighbourToConsider;
        public List<int> GetNearbyStarlings(int index)
        {
            var closeAgents = new Dictionary<int, float>();

            List<int> context = new List<int>();
            foreach (int agentIndex in boidCellNeighbours.GetValuesForKey(index))
            {
                if (agentIndex == index) continue; //Ignore itself
                float distance = Vector3.Distance(agentPositions[index], agentPositions[agentIndex]);
                if (distance >= neighborRadius) continue;
                if (distance <= hardSphere) continue; //avoid if in hard sphere

                bool added = false;
                foreach (KeyValuePair<int, float> pair in closeAgents)
                {
                    if (pair.Value > distance)
                    {
                        closeAgents.Remove(pair.Key);
                        closeAgents.Add(agentIndex, distance);
                        added = true;
                        break;
                    }
                }
                if (!added && closeAgents.Count < neighbourToConsider)
                {
                    closeAgents.Add(agentIndex, distance);
                }
                if (closeAgents.Count == neighbourToConsider) break;
            }

            foreach (KeyValuePair<int, float> pair in closeAgents)
            {
                context.Add(pair.Key);
            }

            return context;
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
        List<FlockAgent> cellBoids = new List<FlockAgent>();
        List<int> boidIndexes = grid.GetNeighbours(int.Parse(agent.gameObject.name));

        foreach (int i in boidIndexes) {
            cellBoids.Add(agents[i]);
        }

        var closeAgents = new Dictionary<FlockAgent, float>();

        List<FlockAgent> context = new List<FlockAgent>();
        foreach (FlockAgent a in cellBoids)
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
