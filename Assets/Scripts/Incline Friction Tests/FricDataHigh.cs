using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class fric_data_high : MonoBehaviour
{
    public string test_name = "unity_high_friction";
    public float startDelay = 1.0f;   // wait a bit before start

    private List<data_point> dataList = new List<data_point>();
    private float t_start;
    private Rigidbody rb_comp;
    private bool recording_now = false;

    [System.Serializable]
    public class data_point
    {
        public float t;
        public float posX, pos_y, pos_z;
        public float velx, vel_y, velz;
        public float spd;

        // convert unity y up to gazebo z up
        public data_point(float time_val, Vector3 pVec, Vector3 vVec)
        {
            t = time_val;
            posX = pVec.x;
            pos_y = pVec.z;
            pos_z = pVec.y;
            velx = vVec.x;
            vel_y = vVec.z;
            velz = vVec.y;
            spd = vVec.magnitude;
        }
    }

    void Start()
    {
        rb_comp = GetComponent<Rigidbody>();
        if (rb_comp == null) return;   // no rigidbody so nothing to log

        // auto start recording after delay
        Invoke("begin_recording", startDelay);
    }

    void FixedUpdate()
    {
        // log data every timestep when recording
        if (recording_now)
        {
            log_data();
        }
    }

    void begin_recording()
    {
        if (recording_now) return;
        recording_now = true;
        dataList.Clear();
        t_start = Time.time;
    }

    // unity will calls this when you hit the stop button (exit play mode)
    void OnApplicationQuit()
    {
        if (recording_now && dataList.Count > 0)
        {
            save_csv();
        }
    }

    void log_data()
    {
        float tt = Time.time - t_start;
        Vector3 p_now = transform.position;
        Vector3 v_now = rb_comp.velocity;
        dataList.Add(new data_point(tt, p_now, v_now));
    }

    void save_csv()
    {
        string file_path = "C:/Users/lsile/Documents/Honours Thesis/Friction_Data/" + test_name + ".csv";

        using (StreamWriter w = new StreamWriter(file_path))
        {
            // header row
            w.WriteLine("time,pos_x,pos_y,pos_z,vel_x,vel_y,vel_z,speed");

            // data rows
            for (int i = 0; i < dataList.Count; i++)
            {
                data_point d = dataList[i];
                w.WriteLine(
                    d.t.ToString("F4") + "," +
                    d.posX.ToString("F4") + "," +
                    d.pos_y.ToString("F4") + "," +
                    d.pos_z.ToString("F4") + "," +
                    d.velx.ToString("F4") + "," +
                    d.vel_y.ToString("F4") + "," +
                    d.velz.ToString("F4") + "," +
                    d.spd.ToString("F4")
                );
            }
        }
    }
}
