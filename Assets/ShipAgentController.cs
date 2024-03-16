using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


// create a enum of possible actions
public enum ShipActions
{
    DeadAhead,
    Starboard20,
    Starboard30,
    Starboard40,
    Starboard50,
    Starboard60,
    Starboard70,
    Starboard80,
    Starboard90,
    Port20,
    Port30,
    Port40,
    Port50,
    Port60,
    Port70,
    Port80,
    Port90
}

public class ShipAgentController : Agent
{
    [SerializeField]
    private GameObject ownship;
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private Transform ownshipDestination;
    [SerializeField]
    private GameObject checkpointParent;
    
    private List<Transform> checkpoints = new List<Transform>();
    
    private int currentCheckpointIndex = 0;

    private Vector3 shipOldPosition;
    private Vector3 targetOldPosition;
    
    private float initialDistanceToDestination;
    
    Entity381 ownshipEntity;

    private void Start()
    {
        shipOldPosition = ownship.transform.position;
        targetOldPosition = target.transform.position;
        
        initialDistanceToDestination = Vector3.Distance(ownship.transform.position, ownshipDestination.position);
        ownshipEntity = ownship.GetComponent<Entity381>();
        
        foreach (Transform child in checkpointParent.transform)
        {
            if(child != checkpointParent.transform) // Ensure you don't add the parent itself
                checkpoints.Add(child);
        }

    }


    public override void OnEpisodeBegin()
    {
        
        ownship.GetComponent<Entity381>().position = shipOldPosition;
        target.GetComponent<Entity381>().position = targetOldPosition;
        
        ownship.GetComponent<Entity381>().heading = 0;
        ownship.GetComponent<Entity381>().desiredHeading = 0;
        
        
        target.GetComponent<Entity381>().heading = -90;
        target.GetComponent<Entity381>().desiredHeading = -90;
        
        
        initialDistanceToDestination = Vector3.Distance(ownship.transform.position, ownshipDestination.position);
        currentCheckpointIndex = 0; // Reset checkpoint progress

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Debug.Log("Action Received: " + actions.DiscreteActions[0]);
        // Get the input from the action buffer
        if (actions.DiscreteActions[0] == 0)
        {
            ownshipEntity.desiredHeading = 0;
        }
        else if (actions.DiscreteActions[0] == 1)
        {
            ownshipEntity.desiredHeading = 20;
        }
        else if (actions.DiscreteActions[0] == 2)
        {
            ownshipEntity.desiredHeading = 30;
        }
        else if (actions.DiscreteActions[0] == 3)
        {
            ownshipEntity.desiredHeading = 40;
        }
        else if (actions.DiscreteActions[0] == 4)
        {
            ownshipEntity.desiredHeading = 50;
        }
        else if (actions.DiscreteActions[0] == 5)
        {
            ownshipEntity.desiredHeading = 60;
        }
        else if (actions.DiscreteActions[0] == 6)
        {
            ownshipEntity.desiredHeading = 70;
        }
        else if (actions.DiscreteActions[0] == 7)
        {
            ownshipEntity.desiredHeading = 80;
        }
        else if (actions.DiscreteActions[0] == 8)
        {
            ownshipEntity.desiredHeading = 90;
        }
        else if (actions.DiscreteActions[0] == 9)
        {
            ownshipEntity.desiredHeading = -20;
        }
        else if (actions.DiscreteActions[0] == 10)
        {
            ownshipEntity.desiredHeading = -30;
        }
        else if (actions.DiscreteActions[0] == 11)
        {
            ownshipEntity.desiredHeading = -40;
        }
        else if (actions.DiscreteActions[0] == 12)
        {
            ownshipEntity.desiredHeading = -50;
        }
        else if (actions.DiscreteActions[0] == 13)
        {
            ownshipEntity.desiredHeading = -60;
        }
        else if (actions.DiscreteActions[0] == 14)
        {
            ownshipEntity.desiredHeading = -70;
        }
        else if (actions.DiscreteActions[0] == 15)
        {
            ownshipEntity.desiredHeading = -80;
        }
        else if (actions.DiscreteActions[0] == 16)
        {
            ownshipEntity.desiredHeading = -90;
        }

        

        // float inputDesiredHeading  = actions.ContinuousActions[0];
        //
        // // Debug.Log("inputDesiredHeading: " + inputDesiredHeading);
        // // Normalize the input to a range of 0 to 1
        // float normalizedDesiredHeading = (inputDesiredHeading + 1) / 2;
        //
        // // Scale the normalized value to a range of 0 to 360
        // float scaledDesiredHeading = normalizedDesiredHeading * 180;
        //
        //
        // ownshipEntity.desiredHeading = scaledDesiredHeading;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        // continuousActions[0] = 0f;
        
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ownship.transform.position);
        sensor.AddObservation(target.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("targetship"))
        {
            SetReward(-50000f);
            EndEpisode();
        }
    }
    
    private void FixedUpdate()
    {
        // float currentDistanceToDestination = Vector3.Distance(ownship.transform.position, ownshipDestination.position);
        // float distanceDelta = initialDistanceToDestination - currentDistanceToDestination;
        // if (distanceDelta > 0)
        // {
        //     // Ownship is getting closer to the destination
        //     SetReward(0.1f * distanceDelta); // You can adjust the reward factor as needed
        // }else
        // {
        //     // Ownship is getting away from the destination
        //     SetReward(-0.1f * distanceDelta); // You can adjust the reward factor as needed
        // }
        //
        float currentReward = GetCumulativeReward();
        
        Debug.Log("Current Reward: " + currentReward);
        
        
        if (currentCheckpointIndex < checkpoints.Count)
        {
            // CheckForCheckpointArrival();
            AddRewardBasedOnDistanceToCheckpoint();
            // RewardForCorrectHeading();
            // AddRewardBasedOnFinalDistance();
        }
        else
        {
            CheckForDestinationArrival();
        }
        OutOfScreenEndEpisode();
    }

    void AddRewardBasedOnFinalDistance()
    {
        float currentDistanceToDestination = Vector3.Distance(ownship.transform.position, ownshipDestination.position);
        float distanceDelta = initialDistanceToDestination - currentDistanceToDestination;
        if (distanceDelta > 0)
        {
            // Ownship is getting closer to the destination
            SetReward(100f * distanceDelta); // You can adjust the reward factor as needed
        }else
        {
            // Ownship is getting away from the destination
            SetReward(-100f * distanceDelta); // You can adjust the reward factor as needed
        }
    }

    void AddRewardBasedOnDistanceToCheckpoint()
    {
        
        
        
        float distanceToCheckpoint = Vector3.Distance(ownship.transform.position, checkpoints[currentCheckpointIndex].position);
        float distanceDelta = Vector3.Distance(ownship.transform.position, shipOldPosition) - distanceToCheckpoint;

        // Reward the agent for getting closer to the current checkpoint
        float reward = 0f;
        if (distanceDelta > 0)
        {
            reward = 10f * distanceDelta; // Reward for getting closer to the checkpoint
        }
        else
        {
            reward = -10f * distanceDelta; // Penalize for moving away from the checkpoint
        }

        // Check if the agent reached the current checkpoint
        if (distanceToCheckpoint < 10.0f) // Threshold for reaching a checkpoint
        {
            reward += 100f; // Additional reward for reaching the checkpoint
            currentCheckpointIndex++; // Move to the next checkpoint
            shipOldPosition = ownship.transform.position; // Update old position for next checkpoint calculation

            // If all checkpoints are cleared, give a big reward and end the episode
            if (currentCheckpointIndex >= checkpoints.Count)
            {
                reward += 500f; // Big reward for completing all checkpoints
                EndEpisode();
            }
        }

        SetReward(reward);
        
        
    }
    
    void RewardForCorrectHeading()
    {
        // Calculate the direction towards the next checkpoint or destination
        Vector3 directionToNextCheckpoint = (checkpoints[currentCheckpointIndex].position - ownship.transform.position).normalized;
    
        // Calculate the angle between the current heading and the direction towards the next checkpoint or destination
        float angle = Vector3.Angle(ownship.transform.forward, directionToNextCheckpoint);

        // Reward the agent for maintaining a correct heading (adjust rewards as needed)
        float headingReward = Mathf.Lerp(1f, -1f, angle / 180f); // Reward inversely proportional to the angle
        SetReward(headingReward);
    }

    void OutOfScreenEndEpisode()
    {
        if (ownship.transform.position.x < -900 || ownship.transform.position.x > 900 || ownship.transform.position.z < -900 || ownship.transform.position.z > 900)
        {
            SetReward(-500f);
            EndEpisode();
        }
    }


    void CheckForCheckpointArrival()
    {
        if (Vector3.Distance(ownship.transform.position, checkpoints[currentCheckpointIndex].position) < 10.0f) // Threshold for reaching a checkpoint
        {
            Debug.Log("Reached checkpoint! no: " + currentCheckpointIndex + " of " + checkpoints.Count);
            SetReward(1.0f); // Reward for reaching a checkpoint
            currentCheckpointIndex++; // Move to the next checkpoint
            if(currentCheckpointIndex >= checkpoints.Count)
            {
                Debug.Log("All checkpoints cleared. Heading to final destination.");
            }
        }
    }

    void CheckForDestinationArrival()
    {
        float distanceToDestination = Vector3.Distance(ownship.transform.position, ownshipDestination.position);
        if (distanceToDestination < 10.0f) // Threshold for reaching final destination
        {
            Debug.Log("Destination Reached!");
            SetReward(200.0f); // Bigger reward for reaching the final destination
            EndEpisode();
        }
    }
    
}
