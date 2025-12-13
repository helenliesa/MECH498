using System;
using System.IO;
using UnityEngine;

public class ux6b : MonoBehaviour
{
    [Header("xArm6 ArticulationBody Joints (J1 to J6 in order)")]
    public ArticulationBody joint1;
    public ArticulationBody joint2;
    public ArticulationBody joint3;
    public ArticulationBody joint4;
    public ArticulationBody joint5;
    public ArticulationBody joint6;

    [Header("Test Control")]
    public bool startTest = false;

    // OPEN PATH TRAJECTORY - arm extends outward, J5 reversed
    // Converted from radians to degrees for Unity
    private float[][] waypoints = new float[][]
    {
        new float[] {0f,   0f, 0f, 0f, 0f, 0f, 0f},                          // t=0s:   Home
        new float[] {6f,   34.38f, 28.65f, -45.84f, 17.19f, -22.92f, 28.65f}, // t=6s:   [0.6, 0.5, -0.8, 0.3, -0.4, 0.5]
        new float[] {12f,  51.57f, 45.84f, -68.75f, 28.65f, -34.38f, 45.84f}, // t=12s:  [0.9, 0.8, -1.2, 0.5, -0.6, 0.8]
        new float[] {18f,  0f, 57.30f, -85.94f, 0f, -45.84f, 0f},            // t=18s:  [0.0, 1.0, -1.5, 0.0, -0.8, 0.0]
        new float[] {24f, -51.57f, 45.84f, -68.75f, -28.65f, -34.38f, -45.84f}, // t=24s: [-0.9, 0.8, -1.2, -0.5, -0.6, -0.8]
        new float[] {30f, -34.38f, 28.65f, -45.84f, -17.19f, -22.92f, -28.65f}, // t=30s: [-0.6, 0.5, -0.8, -0.3, -0.4, -0.5]
        new float[] {36f,  0f, 0f, 0f, 0f, 0f, 0f}                          // t=36s:  Home
    };

    private float testTime;
    private StreamWriter writer;
    private bool isRunning;
    private ArticulationBody[] joints;
    private float[] commandedAngles;
    private float[] actualAngles;
    private float totalDuration = 56f;

    void Start()
    {
        joints = new ArticulationBody[] { joint1, joint2, joint3, joint4, joint5, joint6 };

        for (int i = 0; i < 6; i++)
        {
            if (joints[i] == null)
            {
                
            }
        }

        commandedAngles = new float[6];
        actualAngles = new float[6];

        string path = Path.Combine(@"C:\Users\lsile\Documents\Honours Thesis\Trajectory_Tracking_Data",
                                   "ux6b.csv");
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
