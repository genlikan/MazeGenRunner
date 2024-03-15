using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentCode: Agent
{

    // public MazeGenerator mazeGenerator;
    // public Transform Target;
    private HashSet<Vector2Int> visitedAreas = new HashSet<Vector2Int>();
    private float gridSize = 1f; // Size of the grid cell, adjust based on your maze scale
    public float forceMultiplier = 1;
    // public Vector3 StartPosition;
    // public Vector3 EndPosition;
    public MazeGenerator mazeGenerator;
    public MazeManager mazeManager;
    public GameObject MazeReward;
    private float stationaryTime = 0f; // Time the agent has been stationary
    public float stationaryThreshold = 0.1f; // Velocity below which is considered stationary
    public float maxStationaryTime = 2f; // Time after which a penalty is applied
    private float timeSinceLastProgress = 0f; // Time since last progress
    public float progressTimeout = 100f; // 100 seconds timeout for progress


    Rigidbody rBody;
    StatsRecorder m_statsRecorder;
    
    void Start() {
        mazeManager = FindObjectOfType<MazeManager>();
        if (mazeManager == null) {
            Debug.LogError("MazeManager not found in the scene.");
        }
    }
    // Start is called before the first frame update
    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        m_statsRecorder = Academy.Instance.StatsRecorder;
    }

    public override void OnEpisodeBegin()
    {
        // Reset the agent's velocity
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity *= 0f;
        m_statsRecorder.Add("Goal/Correct", 0, StatAggregationMethod.Sum);
        // Clear visited areas
        visitedAreas.Clear();
        // Reset the progress timeout timer
        timeSinceLastProgress = 0f;

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        sensor.AddObservation(StepCount / (float)MaxStep);
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(transform.rotation);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        Vector3 forwardMovement = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        int action = act[0];
        switch (action)
        {
            case 1:
                // Roll forwards
                forwardMovement = transform.forward * 1f;
                break;
            case 2:
                // Roll backwards
                forwardMovement = transform.forward * -1f;
                break;
            case 3:
                // Turn right
                rotation = transform.up * 1f;
                break;
            case 4:
                // Turn left
                rotation = transform.up * -1f;
                break;
        }
        // Apply rotation
        transform.Rotate(rotation, Time.deltaTime * 150f);
        // Apply forward/backward movement
        rBody.AddForce(forwardMovement * forceMultiplier, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        base.OnActionReceived(actionBuffers);

        AddReward(-1f / MaxStep);
        MoveAgent(actionBuffers.DiscreteActions);
        Vector2Int currentGridPos = CalculateGridPosition(transform.position);
        if (!visitedAreas.Contains(currentGridPos))
        {
            visitedAreas.Add(currentGridPos);
            AddReward(10f); // Adjust reward value as needed
            timeSinceLastProgress = 0f;
            // Debug.Log(visitedAreas);
        }
        // Check if the agent is considered stationary
        if (rBody.velocity.magnitude < stationaryThreshold)
        {
            stationaryTime += Time.fixedDeltaTime; // Increment the stationary time
        }
        else
        {
            stationaryTime = 0; // Reset if moving
        }

        // Apply a penalty if the agent has been stationary for too long
        if (stationaryTime >= maxStationaryTime)
        {
            AddReward(-1f); // Apply penalty
            // stationaryTime = 0; // Reset to avoid continuous penalty
        }
        // if(receivedReward) {
        //     timeSinceLastProgress = 0f;
        // }
    }

    Vector2Int CalculateGridPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / gridSize);
        int z = Mathf.FloorToInt(position.z / gridSize);
        return new Vector2Int(x, z);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        // Check if the agent hits a wall
        if (collision.gameObject.CompareTag("MazeWall"))
        {
            AddReward(-1.0f); // Penalize for hitting a wall
            // EndEpisode();     // End the episode
            // Debug.Log("Hit a wall");
        }
        // // Check if the agent leaves the maze
        // else if (collision.gameObject.CompareTag("Boundary"))
        // {
        //     AddReward(-1.0f); // Penalize for leaving the maze
        //     EndEpisode();     // End the episode
        // }
        if (collision.gameObject.CompareTag("MazeReward"))
        {
            AddReward(10000.0f);
            Debug.Log("MAZE REWARD GET!! Time Elapsed: " + timeSinceLastProgress);
            // Debug.Log(timeSinceLastProgress);
            m_statsRecorder.Add("Goal/Correct", 1, StatAggregationMethod.Sum);
            EndEpisode();

            // Trigger maze regeneration
            mazeManager.RegenerateMazes();
        }

    }
    void FixedUpdate()
    {
        // Debug.Log(timeSinceLastProgress);
        timeSinceLastProgress += Time.fixedDeltaTime;
        if (timeSinceLastProgress >= progressTimeout) {
            Debug.Log("Progress timeout reached. Ending episode.");
            EndEpisode();
            // Trigger maze regeneration
            mazeManager.RegenerateMazes();
            timeSinceLastProgress = 0f;
        }
        // rBody.AddForce(new Vector3(1, 0, 1) * forceMultiplier);
        /*float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        rBody.AddForce(movement * speed);*/
    }
}
