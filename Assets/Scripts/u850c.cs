using System;
using System.IO;
using UnityEngine;

public class u850c : MonoBehaviour
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

    // COMPLEX TRAJECTORY - matching Gazebo 850c.py
 
  
    private float[][] waypoints = new float[][]
    {
        new float[] {0f,   0f, 57.30f, -57.30f, 0f, 0f, 0f},                    // t=0s:  [0.0, 1.0, -1.0, 0.0, 0.0, 0.0]
        new float[] {5f,   51.57f, 74.48f, -74.48f, 28.65f, 28.65f, 28.65f},   // t=5s:  [0.9, 1.3, -1.3, 0.5, 0.5, 0.5]
        new float[] {10f,  51.57f, 45.84f, -45.84f, 40.11f, 40.11f, 40.11f},   // t=10s: [0.9, 0.8, -0.8, 0.7, 0.7, 0.7]
        new float[] {15f, -51.57f, 45.84f, -45.84f, -34.38f, -34.38f, -28.65f},// t=15s: [-0.9, 0.8, -0.8, -0.6, -0.6, -0.5]
        new float[] {20f, -51.57f, 80.21f, -80.21f, -45.84f, -45.84f, -40.11f},// t=20s: [-0.9, 1.4, -1.4, -0.8, -0.8, -0.7]
        new float[] {25f,  0f, 63.03f, -63.03f, 17.19f, 17.19f, 17.19f},       // t=25s: [0.0, 1.1, -1.1, 0.3, 0.3, 0.3]
        new float[] {30f,  22.92f, 54.43f, -54.43f, 11.46f, 11.46f, 11.46f},   // t=30s: [0.4, 0.95, -0.95, 0.2, 0.2, 0.2]
        new float[] {35f,  0f, 57.30f, -57.30f, 0f, 0f, 0f}                    
    };

    private float testTime;
    private StreamWriter writer;
    private bool isRunning;
    private ArticulationBody[] joints;
    private float[] commandedAngles;
    private float[] actualAngles;
    private float totalDuration = 55f; // 35s trajectory + 20s recording

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
                                   "u850c.csv");
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
