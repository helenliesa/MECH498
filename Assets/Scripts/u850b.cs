using System;
using System.IO;
using UnityEngine;

public class u850b : MonoBehaviour
{
    [Header("UF850 ArticulationBody Joints (J1 to J6 in order)")]
    public ArticulationBody joint1;
    public ArticulationBody joint2;
    public ArticulationBody joint3;
    public ArticulationBody joint4;
    public ArticulationBody joint5;
    public ArticulationBody joint6;

    [Header("Test Control")]
    public bool startTest = false;

    // BASIC TRAJECTORY - matching Gazebo 850b.py

    // Times: 0, 6, 12, 18, 24, 30, 36 seconds
    // Positions from 850b.py converted to degrees for Unity
    private float[][] waypoints = new float[][]
    {
        new float[] {0f,   0f, 40.11f, -40.11f, 0f, 0f, 0f},                   
        new float[] {6f,   17.19f, 45.84f, -45.84f, 11.46f, 11.46f, 11.46f},   
        new float[] {12f,  28.65f, 51.57f, -51.57f, 17.19f, 17.19f, 17.19f},   
        new float[] {18f,  34.38f, 57.30f, -57.30f, 22.92f, 22.92f, 20.05f},   
        new float[] {24f,  22.92f, 48.70f, -48.70f, 14.32f, 14.32f, 14.32f},   
        new float[] {30f,  11.46f, 42.97f, -42.97f, 5.73f, 5.73f, 5.73f},      
        new float[] {36f,  0f, 40.11f, -40.11f, 0f, 0f, 0f}                    
    };

    private float testTime;
    private StreamWriter writer;
    private bool isRunning;
    private ArticulationBody[] joints;
    private float[] commandedAngles;
    private float[] actualAngles;
    private float totalDuration = 56f; // 36s trajectory + 20s recording

    void Start()
    {
        joints = new ArticulationBody[] { joint1, joint2, joint3, joint4, joint5, joint6 };

        for (int i = 0; i < 6; i++)
        {
            if (joints[i] == null)
            {
               
                return;
            }
        }

        commandedAngles = new float[6];
        actualAngles = new float[6];

        string path = Path.Combine(@"C:\Users\lsile\Documents\Honours Thesis\Trajectory_Tracking_Data",
                                   "u850b.csv");
        writer = new StreamWriter(path, false);

        writer.WriteLine("time,joint1_actual,joint2_actual,joint3_actual,joint4_actual,joint5_actual,joint6_actual,joint1_cmd,joint2_cmd,joint3_cmd,joint4_cmd,joint5_cmd,joint6_cmd");

       
    }

    void FixedUpdate()
    {
        if (startTest && !isRunning)
        {
            isRunning = true;
            testTime = 0f;
           
        }

        if (!isRunning) return;

        testTime += Time.fixedDeltaTime;

        if (testTime >= totalDuration)
        {
            isRunning = false;
            startTest = false;
            CloseWriter();
            return;
        }

        for (int seg = 0; seg < waypoints.Length - 1; seg++)
        {
            float t0 = waypoints[seg][0];
            float t1 = waypoints[seg + 1][0];

            if (testTime >= t0 && testTime <= t1)
            {
                float alpha = (testTime - t0) / (t1 - t0);
                float smoothAlpha = alpha * alpha * (3f - 2f * alpha);

                for (int i = 0; i < 6; i++)
                {
                    float angle0 = waypoints[seg][i + 1];
                    float angle1 = waypoints[seg + 1][i + 1];
                    commandedAngles[i] = Mathf.Lerp(angle0, angle1, smoothAlpha);

                    ArticulationDrive drive = joints[i].xDrive;
                    drive.target = commandedAngles[i];
                    joints[i].xDrive = drive;

                    actualAngles[i] = joints[i].jointPosition[0] * Mathf.Rad2Deg;
                }

                break;
            }
            else if (testTime > waypoints[waypoints.Length - 1][0])
            {
                for (int i = 0; i < 6; i++)
                {
                    commandedAngles[i] = waypoints[waypoints.Length - 1][i + 1];
                    actualAngles[i] = joints[i].jointPosition[0] * Mathf.Rad2Deg;
                }
            }
        }

        string line = $"{testTime:F6}";

        for (int i = 0; i < 6; i++)
            line += $",{(actualAngles[i] * Mathf.Deg2Rad):F6}";

        for (int i = 0; i < 6; i++)
            line += $",{(commandedAngles[i] * Mathf.Deg2Rad):F6}";

        writer.WriteLine(line);
    }

    void CloseWriter()
    {
        if (writer == null) return;
        try
        {
            writer.Flush();
            writer.Close();
            writer.Dispose();
        }
        catch (ObjectDisposedException) { }
        writer = null;
    }

    void OnDestroy()
    {
        CloseWriter();
    }
}
