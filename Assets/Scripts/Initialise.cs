using UnityEngine;
using System.Collections;

public class Initialise : MonoBehaviour
{
    public DataStorage dataStorage;
    public Flock flock;
    public FlockAgent flockAgent;
    public PredatorAgent predatorPrefab;
    public Cam camPrefab;

    private PredatorAgent predatorAgent;
    private Cam cam;


    public void Start()
    {
        //Setup Prefs
        dataStorage.SetupFlockPrefs();
        dataStorage.SetupPredPrefs();

        //Check if first time setup
        if (PlayerPrefs.GetInt("startingCount", 0) == 0)
        {
            dataStorage.SaveFlockPrefs();
            dataStorage.SavePredPrefs();
        }

        float radius = flock.startingCount * flock.agentDensity * 5;

        //Spawn in the predator away from murmuration
        Vector3 predatorPos = Random.insideUnitSphere * flock.startingCount * flock.agentDensity * 5 * 4;
        float predRadius = radius * 2;
        while ((predatorPos.x < predRadius && predatorPos.x > predRadius * -1) ||
        (predatorPos.z < predRadius && predatorPos.z > predRadius * -1) ||
        (predatorPos.y < predRadius && predatorPos.y > predRadius * -1))
        {
            predatorPos = Random.insideUnitSphere * flock.startingCount * flock.agentDensity * 5 * 4;
        }
        predatorPos += flock.focalPoint;
        predatorAgent = Instantiate(
            predatorPrefab,
            predatorPos,
            Random.rotation,
            transform
        );
        predatorAgent.name = "Predator";
        predatorAgent.Initialize(flock);

        //Spawn in the flock of birds
        for (int i = 1; i <= flock.startingCount; i++)
        {
            FlockAgent starling = Instantiate(
                flockAgent,
                (Random.insideUnitSphere * flock.startingCount * flock.agentDensity * 5) + flock.focalPoint,
                Random.rotation,
                transform
                );
            starling.name = "Agent " + i;
            starling.Initialize(flock, predatorAgent);
            flock.agents.Add(starling);
            flock.agentsTransform.Add(starling.transform);
            StartCoroutine(starling.starlingAnimate(starling.GetComponentInChildren<Animator>()));
        }

        //Setup Camera
        cam = Instantiate(camPrefab, flock.focalPoint, camPrefab.transform.rotation);
        cam.Initialise(flock, flock.agents[0].transform, flock.agents[0].transform, predatorAgent.transform);
    }

    public void Reset()
    {
        flock.DeleteFlock();
        Destroy(predatorAgent.gameObject);
        Destroy(cam.gameObject);
        Start();
    }
}
