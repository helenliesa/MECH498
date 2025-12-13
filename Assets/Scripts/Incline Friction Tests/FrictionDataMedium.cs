using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FrictionDataMedium : MonoBehaviour
{
    [Header("Data Collection Settings")]
    public string testName = "unity_medium_friction";
    public bool autoStartRecording = true;
    public float autoStopDelay = 1.0f; // Wait 1 second after box settles before starting

    private List<DataPoint> dataPoints = new List<DataPoint>();
    private float startTime;
    private Rigidbody rb;
    private bool isRecording = false;
    private bool hasStarted = false;

    [System.Serializable]
    public class DataPoint
    {
        public float time;
        public float pos_x, pos_y, pos_z;
        public float vel_x, vel_y, vel_z;
        public float speed;

       
       
        public DataPoint(float t, Vector3 unityPos, Vector3 unityVel)
        {
            time = t;

            // Convert Unity Y-up to Gazebo Z-up
            pos_x = unityPos.x;      
            pos_y = unityPos.z;      
            pos_z = unityPos.y;       

            vel_x = unityVel.x;      
            vel_y = unityVel.z;      
            vel_z = unityVel.y;      

            speed = unityVel.magnitude;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
       

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }

        if (isRecording && rb.velocity.magnitude < 0.001f && Time.time - startTime > 5f)
        {
           
            StopRecording();
        }
    }

    void FixedUpdate()
    {
        if (isRecording)
        {
            RecordData();
        }
    }

    void StartRecording()
    {
        if (isRecording) return;

        isRecording = true;
        dataPoints.Clear();
        startTime = Time.time;
        hasStarted = true;
       
    }

    void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        SaveToCSV();
       
    }

    void RecordData()
    {
        float currentTime = Time.time - startTime;
        Vector3 currentPosition = transform.position;
        Vector3 velocity = rb.velocity;

        DataPoint dp = new DataPoint(currentTime, currentPosition, velocity);
        dataPoints.Add(dp);
    }

    void SaveToCSV()
    {
        string filePath = $"C:/Users/lsile/Documents/Honours Thesis/Friction_Data/{testName}.csv";

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("time,pos_x,pos_y,pos_z,vel_x,vel_y,vel_z,speed");

                foreach (DataPoint dp in dataPoints)
                {
                    writer.WriteLine($"{dp.time:F4},{dp.pos_x:F4},{dp.pos_y:F4},{dp.pos_z:F4}," +
                                   $"{dp.vel_x:F4},{dp.vel_y:F4},{dp.vel_z:F4},{dp.speed:F4}");
                }
            }

           

            if (dataPoints.Count > 0)
            {
                float maxSpeed = 0;
                foreach (var dp in dataPoints)
                {
                    if (dp.speed > maxSpeed) maxSpeed = dp.speed;
                }
               
            }
        }
        
    }
}
