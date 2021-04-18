using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour
{

    private int settingsType = -1;
    private string pausePlay = "Pause";
    public static bool runSimulation = true;
    public static Menu instance;

    public static float predMoveSpeed = 0.3f;
    public static float predMoveTurnSpeed = 1.5f;
    public static float predHoverHeight = 200f;
    public static float predStoopSpeed = 1.0f;

    public Flock flock;
    public DataStorage dataStorage;
    public Initialise initialise;

    void Awake()
    {
        instance = this;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 130, 30), "Flock Settings"))
        {
            if (settingsType == 0)
            {
                settingsType = -1;
                Cam.isEnabled = true;
            }
            else
            {
                settingsType = 0;
                Cam.isEnabled = false;
            }
        }
        if (GUI.Button(new Rect(130, 0, 130, 30), "Predator Settings"))
        {
            if (settingsType == 1)
            {
                settingsType = -1;
                Cam.isEnabled = true;
            }
            else
            {
                settingsType = 1;
                Cam.isEnabled = false;
            }
        }
        if (GUI.Button(new Rect(260, 0, 130, 30), "Simulation Settings"))
        {
            if (settingsType == 2)
            {
                settingsType = -1;
                Cam.isEnabled = true;
            }
            else
            {
                settingsType = 2;
                Cam.isEnabled = false;
            }
        }
        if (GUI.Button(new Rect(390, 0, 130, 30), "Camera Controls"))
        {
            if (settingsType == 3)
            {
                settingsType = -1;
                Cam.isEnabled = true;
            }
            else
            {
                settingsType = 3;
                Cam.isEnabled = false;
            }
        }

        if (settingsType == 0)
        {
            GUI.Box(new Rect(0, 30, 265, 480), "Flock Settings");
            //Flock Size
            int y = 50;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "2000");
            flock.startingCount = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.startingCount, 1, 2000));
            GUI.Label(new Rect(235, y, 120, 30), flock.startingCount.ToString());
            if (GUI.Button(new Rect(5, y, 120, 30), "Flock Size Reset"))
            {
                PlayerPrefs.SetInt("startingCount", flock.startingCount);
                initialise.Reset();
            }

            //Flock Speed
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "1");
            flock.speed = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.speed, 0.0f, 1.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Speed");
            GUI.Label(new Rect(235, y, 120, 30), flock.speed.ToString("F2"));

            //Flock Turn Speed
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "5");
            flock.turnSpeed = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.turnSpeed, 0.0f, 5.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Turn Speed");
            GUI.Label(new Rect(235, y, 120, 30), flock.turnSpeed.ToString("F2"));

            //Cohesion Weight
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "5");
            flock.cohesionWeight = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.cohesionWeight, 0.0f, 5.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Cohesion Weight");
            GUI.Label(new Rect(235, y, 120, 30), flock.cohesionWeight.ToString("F2"));

            //Separation Weight
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "5");
            flock.separationWeight = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.separationWeight, 0.0f, 5.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Separation Weight");
            GUI.Label(new Rect(235, y, 120, 30), flock.separationWeight.ToString("F2"));

            //Alignment Weight
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "5");
            flock.alignmentWeight = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.alignmentWeight, 0.0f, 5.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Alignment Weight");
            GUI.Label(new Rect(235, y, 120, 30), flock.alignmentWeight.ToString("F2"));

            //Focal Point Attriction Weight
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "5");
            flock.focalPointWeight = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.focalPointWeight, 0.0f, 5.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Focal Point Weight");
            GUI.Label(new Rect(235, y, 120, 30), flock.focalPointWeight.ToString("F2"));

            //Gravity
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "2");
            flock.gravity = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.gravity, 0.0f, 2.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Gavity");
            GUI.Label(new Rect(235, y, 120, 30), flock.gravity.ToString("F2"));

            //Neighbours to Consider
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "1");
            GUI.Label(new Rect(210, y + 10, 100, 30), "20");
            flock.neighbourToConsider = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.neighbourToConsider, 1, 20));
            GUI.Label(new Rect(5, y, 120, 30), "Neighours");
            GUI.Label(new Rect(235, y, 120, 30), flock.neighbourToConsider.ToString());

            //Neighbour Radius
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0.1");
            GUI.Label(new Rect(210, y + 10, 100, 30), "2");
            flock.neighborRadius = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.neighborRadius, 0.1f, 2.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Neighbour Radius");
            GUI.Label(new Rect(235, y, 120, 30), flock.neighborRadius.ToString("F0"));

            //Predator Detection Distance
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0.1");
            GUI.Label(new Rect(210, y + 10, 100, 30), "5");
            flock.predatorDetectionDist = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.predatorDetectionDist, 0.1f, 5.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Pred Detect Dist");
            GUI.Label(new Rect(235, y, 120, 30), flock.predatorDetectionDist.ToString("F0"));

            //Predator Weight
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "5");
            flock.predatorWeight = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), flock.predatorWeight, 0.0f, 5.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Pred Weight");
            GUI.Label(new Rect(235, y, 120, 30), flock.predatorWeight.ToString("F2"));

            //Reset values and save values
            y += 35;
            if (GUI.Button(new Rect(5, y, 120, 30), "Reset To Defaults"))
            {
                dataStorage.SetupFlockPrefs();
            }
            if (GUI.Button(new Rect(135, y, 120, 30), "Save Settings"))
            {
                dataStorage.SaveFlockPrefs();
            }
        }
        else if (settingsType == 1)
        {
            GUI.Box(new Rect(0, 30, 265, 440), "Predator Settings");

            //Speed
            int y = 50;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "1");
            predMoveSpeed = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), predMoveSpeed, 0.0f, 1.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Speed");
            GUI.Label(new Rect(235, y, 120, 30), predMoveSpeed.ToString("F1"));

            //Dive Speed
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "1");
            predStoopSpeed = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), predStoopSpeed, 0.0f, 1.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Dive Speed");
            GUI.Label(new Rect(235, y, 120, 30), predStoopSpeed.ToString("F1"));

            //Turn Speed
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "2");
            predMoveTurnSpeed = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), predMoveTurnSpeed, 0.0f, 2.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Turn Speed");
            GUI.Label(new Rect(235, y, 120, 30), predMoveTurnSpeed.ToString("F1"));

            //Circle Height
            y += 35;
            GUI.Label(new Rect(130, y + 10, 100, 30), "0");
            GUI.Label(new Rect(210, y + 10, 100, 30), "15");
            predHoverHeight = GUI.HorizontalSlider(new Rect(130, y + 5, 100, 30), predHoverHeight, 0.0f, 15.0f);
            GUI.Label(new Rect(5, y, 120, 30), "Circle Height");
            GUI.Label(new Rect(235, y, 120, 30), predHoverHeight.ToString("F0"));

            //Reset values and save values
            y += 35;
            if (GUI.Button(new Rect(5, y, 120, 30), "Reset To Defaults"))
            {
                dataStorage.SetupPredPrefs();
            }
            if (GUI.Button(new Rect(135, y, 120, 30), "Save Settings"))
            {
                dataStorage.SavePredPrefs();
            }
        }
        else if (settingsType == 2)
        {
            GUI.Box(new Rect(0, 30, 265, 440), "Simulation Settings");

            int y = 50;
            if (GUI.Button(new Rect(5, y, 120, 30), "Reset"))
            {
                initialise.Reset();
            }
            if (GUI.Button(new Rect(135, y, 120, 30), pausePlay))
            {
                if (runSimulation)
                {
                    runSimulation = false;
                    Time.timeScale = 1;
                    pausePlay = "Resume";
                }
                else
                {
                    runSimulation = true;
                    Time.timeScale = 0;
                    pausePlay = "Pause";
                }
            }
        }
        else if (settingsType == 3)
        {
            GUI.Box(new Rect(0, 30, 265, 440), "Camera Controls");

            int y = 50;
            GUI.Label(new Rect(5, y, 120, 30), "Action");
            GUI.Label(new Rect(150, y, 120, 30), "Control");

            y += 35;
            GUI.Label(new Rect(5, y, 120, 30), "Attatch to Agent");
            GUI.Label(new Rect(150, y, 120, 30), "<Space>");

            y += 35;
            GUI.Label(new Rect(5, y, 120, 30), "Rotate with Agent");
            GUI.Label(new Rect(150, y, 120, 30), "<T>");

            y += 35;
            GUI.Label(new Rect(5, y, 140, 30), "Toggle Predator/Starling");
            GUI.Label(new Rect(150, y, 120, 30), "<F>");

            y += 35;
            GUI.Label(new Rect(5, y, 120, 30), "Switch Starling");
            GUI.Label(new Rect(150, y, 120, 30), "<G>");

            y += 35;
            GUI.Label(new Rect(5, y, 120, 30), "Basic Movement");
            GUI.Label(new Rect(150, y, 120, 30), "<W,A,S,D>");

            y += 35;
            GUI.Label(new Rect(5, y, 120, 30), "Move Up");
            GUI.Label(new Rect(150, y, 120, 30), "<E>");

            y += 35;
            GUI.Label(new Rect(5, y, 120, 30), "Move Down");
            GUI.Label(new Rect(150, y, 120, 30), "<Q>");
        }
    }
}
