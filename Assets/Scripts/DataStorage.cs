using UnityEngine;
using System.Collections;

public class DataStorage : MonoBehaviour
{

    public Flock flock;

    public void SetupFlockPrefs()
    {
        flock.startingCount = PlayerPrefs.GetInt("startingCount", flock.startingCount);
        flock.speed = PlayerPrefs.GetFloat("speed", flock.speed);
        flock.turnSpeed = PlayerPrefs.GetFloat("turnSpeed", flock.turnSpeed);
        flock.cohesionWeight = PlayerPrefs.GetFloat("cohesionWeight", flock.cohesionWeight);
        flock.separationWeight = PlayerPrefs.GetFloat("separationWeight", flock.separationWeight);
        flock.alignmentWeight = PlayerPrefs.GetFloat("alignmentWeight", flock.alignmentWeight);
        flock.focalPointWeight = PlayerPrefs.GetFloat("focalPointWeight", flock.focalPointWeight);
        flock.gravity = PlayerPrefs.GetFloat("gavity", flock.gravity);
        flock.neighbourToConsider = PlayerPrefs.GetInt("neighbourToConsider", flock.neighbourToConsider);
        flock.neighborRadius = PlayerPrefs.GetFloat("neighborRadius", flock.neighborRadius);
        flock.predatorDetectionDist = PlayerPrefs.GetFloat("detectionDist", flock.predatorDetectionDist);
        flock.predatorWeight = PlayerPrefs.GetFloat("predatorWeight", flock.predatorWeight);
    }

    public void SetupPredPrefs()
    {
        Menu.predMoveSpeed = PlayerPrefs.GetFloat("predSpeed", Menu.predMoveSpeed);
        Menu.predStoopSpeed = PlayerPrefs.GetFloat("predStoopSpeed", Menu.predStoopSpeed);
        Menu.predMoveTurnSpeed = PlayerPrefs.GetFloat("predTurnSpeed", Menu.predMoveTurnSpeed);
        Menu.predHoverHeight = PlayerPrefs.GetFloat("hoverHeight", Menu.predHoverHeight);
    }

    public void SaveFlockPrefs()
    {
        PlayerPrefs.SetInt("startingCount", flock.startingCount);
        PlayerPrefs.SetFloat("speed", flock.speed);
        PlayerPrefs.SetFloat("turnSpeed", flock.turnSpeed);
        PlayerPrefs.SetFloat("cohesionWeight", flock.cohesionWeight);
        PlayerPrefs.SetFloat("separationWeight", flock.separationWeight);
        PlayerPrefs.SetFloat("alignmentWeight", flock.alignmentWeight);
        PlayerPrefs.SetFloat("focalPointWeight", flock.focalPointWeight);
        PlayerPrefs.SetFloat("gavity", flock.gravity);
        PlayerPrefs.SetInt("neighbourToConsider", flock.neighbourToConsider);
        PlayerPrefs.SetFloat("neighborRadius", flock.neighborRadius);
        PlayerPrefs.SetFloat("detectionDist", flock.predatorDetectionDist);
        PlayerPrefs.SetFloat("predatorWeight", flock.predatorWeight);
    }

    public void SavePredPrefs()
    {
        PlayerPrefs.SetFloat("predSpeed", Menu.predMoveSpeed);
        PlayerPrefs.SetFloat("predStoopSpeed", Menu.predStoopSpeed);
        PlayerPrefs.SetFloat("predTurnSpeed", Menu.predMoveTurnSpeed);
        PlayerPrefs.SetFloat("hoverHeight", Menu.predHoverHeight);
    }
}
