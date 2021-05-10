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
    public bool runMetrics = false;

    public double k = 15; //extension collision penalty (> = worse)
    public double ro = 180; //polarization collision penalty (> = worse)
    //Weights for quality metric
    //  0 < sigma, gamma < 1
    //  sigma + gamma = 1
    public double sigma = 0.5; //extension weight for quality
    public double gamma = 0.5; //polarization weight for quality
    public double runningExtensionSum;
    public double runningPolarizationSum;
    public double runningQualitySum;
    public double cnsExtT;
    public double cnsPolT;
    public double qltyT;
    public double avgExtension;
    public double avgPolarization;
    public double quality;
    public int itter = 0;


    const float hardSphere = 0.2f;
    const float blindAngle = 180 - (9f / 2);

    public DataStorage dataStorage;
    public Initialise initialise;
    public Grid grid;
    public PredatorAgent predatorAgent;

    void Update()
    {
        if (Menu.runSimulation)
        {
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

            Vector3 sumDirection = new Vector3();
            Vector3 flockCentre = new Vector3();
            List<int> colliding = new List<int>();
            if (runMetrics)
            {
                itter++;
                float x = 0;
                float y = 0;
                float z = 0;
                for (int i = 0; i < agents.Count; i++)
                {
                    Vector3 direction = agents[i].AgentVelocity.normalized;
                    sumDirection = new Vector3(direction.x + sumDirection.x, direction.y + sumDirection.y, direction.z + sumDirection.z);

                    Vector3 position = agents[i].transform.position;
                    x += position.x;
                    y += position.y;
                    z += position.z;
                }

                //Find colliding Starlings
                for (int i = 0; i < agents.Count; i++)
                {
                    if (colliding.Contains(i)) continue;
                    List<int> neighbours = grid.GetNeighbours(i);
                    bool collided = false;
                    for (int neighbourIndex = 0; neighbourIndex < neighbours.Count; neighbourIndex++)
                    {
                        if (Vector3.Distance(agents[i].transform.position, agents[neighbourIndex].transform.position) < 0.1f)
                        {
                            collided = true;
                            colliding.Add(neighbourIndex);
                        }
                    }
                    if (collided) colliding.Add(i);
                }
                sumDirection = sumDirection.normalized;
                flockCentre = new Vector3(x / agents.Count, y / agents.Count, z / agents.Count);
            }

            double distanceSum = 0;
            double maxE = 0;
            double deltaAngleSum = 0;
            for (int i = 0; i < agents.Count; i++)
            {
                if (runMetrics)
                {
                    //EXTENSION
                    //Finds distance of all starlings to the flock centre
                    double d = Mathf.Abs(Vector3.Distance(agents[i].transform.position, focalPoint));
                    distanceSum += d;

                    //updates max extension
                    if (d > maxE) { maxE = d; }

                    //POLARIZATION
                    Vector3 direction = agents[i].AgentVelocity.normalized;
                    float dotAngle = sumDirection.x * direction.x + sumDirection.y * direction.y + sumDirection.z * direction.z;
                    deltaAngleSum += (180 / Mathf.PI) * Mathf.Cos(dotAngle);
                }
                agents[i].Move(newVelocities[i], escaping[i]);
                grid.UpdatePosition(int.Parse(agents[i].gameObject.name), agents[i].transform.position);
            }

            


            if (runMetrics)
            {
                //Consistency of Extension
                //  How Well boids stick together
                //  normalized between 0 & 1 (closer to 1 is better)
                Debug.Log("((" + distanceSum + " + " + maxE + " * (" + colliding.Count + ")) / (" + maxE + " * " + agents.Count + "))");
                cnsExtT = ((distanceSum + maxE * (colliding.Count)) / (maxE * agents.Count));
                //Consistency of polarization
                //  How much the boids are going the same direction
                //  normaized between 0 & 1 (closer to 1 is better)
                cnsPolT = 1 - ((deltaAngleSum + ro * colliding.Count) / (ro * agents.Count));
                ///////////////////////////////////
                //quality
                //  weighted sum of polarization and extension
                qltyT = sigma * cnsExtT + gamma * cnsPolT;

                //averaging extension and polarization
                runningExtensionSum += cnsExtT;
                avgExtension = runningExtensionSum / (itter);
                runningPolarizationSum += cnsPolT;
                avgPolarization = runningPolarizationSum / (itter);
                //calculates the running quality over the entire simulation
                runningQualitySum += qltyT;
                quality = runningQualitySum / (itter);
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
