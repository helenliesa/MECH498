using System.IO;
using UnityEngine;

public class Finux6c : MonoBehaviour
{
    // joints in order j1..j6
    public ArticulationBody j1;
    public ArticulationBody j2;
    public ArticulationBody j3;
    public ArticulationBody j4;
    public ArticulationBody j5;
    public ArticulationBody j6;

    // points: time (s) then joint angles in deg
    float[][] wpts = new float[][]
    {
        new float[] {0f,   0f,    0f,     0f,      0f,     0f,     0f},
        new float[] {5f,   68.75f, 40.11f, -80.21f, 45.84f, 34.38f, 57.30f},
        new float[] {10f,  85.94f, 22.92f, -57.30f, 68.75f, 45.84f, 85.94f},
        new float[] {15f,  28.65f, 68.75f,-103.13f, 17.19f,-40.11f, 28.65f},
        new float[] {20f, -85.94f, 22.92f, -57.30f,-68.75f, 45.84f,-85.94f},
        new float[] {25f, -68.75f, 40.11f, -80.21f,-45.84f,-34.38f,-57.30f},
        new float[] {30f,  0f,    51.57f, -85.94f, 0f,    -28.65f, 0f},
        new float[] {35f,  0f,    0f,     0f,      0f,     0f,     0f}
    };

    float total_time = 55f;   // 35s traj + 20s extra
    float t_now;
    bool running;

    ArticulationBody[] joint_arr;
    float[] cmd_deg = new float[6];
    float[] act_deg = new float[6];

    StreamWriter wr;

    // turn this on in inspector to start
    public bool start_test = false;

    void Start()
    {
        joint_arr = new ArticulationBody[] { j1, j2, j3, j4, j5, j6 };

        string path = @"C:\Users\lsile\Documents\Honours Thesis\Trajectory_Tracking_Data\u x6c.csv";
        wr = new StreamWriter(path, false);
        wr.WriteLine("time,j1_a,j2_a,j3_a,j4_a,j5_a,j6_a,j1_c,j2_c,j3_c,j4_c,j5_c,j6_c");
    }

    void FixedUpdate()
    {
        if (start_test && !running)
        {
            running = true;
            t_now = 0f;
        }

        if (!running) return;

        t_now += Time.fixedDeltaTime;
        if (t_now >= total_time)
        {
            running = false;
            close_writer();
            return;
        }

        // figure out which segment we are in
        for (int s = 0; s < wpts.Length - 1; s++)
        {
            float t0 = wpts[s][0];
            float t1 = wpts[s + 1][0];

            if (t_now >= t0 && t_now <= t1)
            {
                float a = (t_now - t0) / (t1 - t0);
                // smoothing
                float aa = a * a * (3f - 2f * a);

                for (int i = 0; i < 6; i++)
                {
                    float ang0 = wpts[s][i + 1];
                    float ang1 = wpts[s + 1][i + 1];
                    cmd_deg[i] = Mathf.Lerp(ang0, ang1, aa);

                    ArticulationDrive d = joint_arr[i].xDrive;
                    d.target = cmd_deg[i];
                    joint_arr[i].xDrive = d;

                    act_deg[i] = joint_arr[i].jointPosition[0] * Mathf.Rad2Deg;
                }
                break;
            }
            else if (t_now > wpts[wpts.Length - 1][0])
            {
                for (int i = 0; i < 6; i++)
                {
                    cmd_deg[i] = wpts[wpts.Length - 1][i + 1];
                    act_deg[i] = joint_arr[i].jointPosition[0] * Mathf.Rad2Deg;
                }
            }
        }

        // write one line: time, actual(rad), cmd(rad)
        string line = t_now.ToString("F6");
        for (int i = 0; i < 6; i++)
            line += "," + (act_deg[i] * Mathf.Deg2Rad).ToString("F6");
        for (int i = 0; i < 6; i++)
            line += "," + (cmd_deg[i] * Mathf.Deg2Rad).ToString("F6");
        wr.WriteLine(line);
    }

    void close_writer()
    {
        if (wr == null) return;
        wr.Flush();
        wr.Close();
        wr = null;
    }

    void OnDestroy()
    {
        close_writer();
    }
}
