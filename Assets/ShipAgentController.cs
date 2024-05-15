using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField]
    private TextMeshProUGUI rewardText;
    
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
        
        
        ownship.GetComponent<Entity381>().heading = 0;
        ownship.GetComponent<Entity381>().desiredHeading = 0;
        
        
        
        float crossingVsOvertaking = UnityEngine.Random.value > 0.5f ? 0 : 1;

        if (crossingVsOvertaking == 0)
        {
            target.GetComponent<Entity381>().position = targetOldPosition;
            float randomHeading = UnityEngine.Random.Range(0, 140);
            target.GetComponent<Entity381>().heading = -randomHeading;
            target.GetComponent<Entity381>().desiredHeading = -randomHeading;
            // change target z position
            float randomPosition = UnityEngine.Random.Range(0, 200);
            bool randomBool = UnityEngine.Random.value > 0.5f;
            target.transform.position = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z + (randomBool ? randomPosition : -randomPosition));
        }
        else
        {
            target.transform.rotation = ownship.transform.rotation;
        
            // move the target ship to the front of the ownship
            // target.transform.position = new Vector3(0,0,200);
            target.GetComponent<Entity381>().position = new Vector3(0, 0, 300);
        
            target.GetComponent<Entity381>().heading = 0;
            target.GetComponent<Entity381>().desiredHeading = 0;
        }
        
        
        
        initialDistanceToDestination = Vector3.Distance(ownship.transform.position, ownshipDestination.position);
        currentCheckpointIndex = 0; // Reset checkpoint progress

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get the input from the action buffer
        int chosenHeading = ActionToHeading(actions.DiscreteActions[0]);
    
        ownshipEntity.desiredHeading = chosenHeading;
    
        // Calculate the direction towards the next checkpoint or destination
        Vector3 directionToNextCheckpoint = (checkpoints[currentCheckpointIndex].position - ownship.transform.position).normalized;
    
        // Calculate the angle between current direction and direction to checkpoint
        float angleToCheckpoint = Vector3.SignedAngle(ownship.transform.forward, directionToNextCheckpoint, Vector3.up);
    
        // Convert the angle to a heading value (assuming 360 degrees, adjust as needed)
        int requiredHeading = (int)Mathf.Repeat(ownshipEntity.heading + angleToCheckpoint, 360);
        
        // Debug.Log("requiredHeading: " + requiredHeading);
    
        // Calculate reward
        // float reward = CalculateReward(chosenHeading, requiredHeading);
    
        float reward = RewardBasedOnDestinationDistance();
        // Debug.Log("Reward: " + reward);
        // Apply reward to agent
        
        //Reward for moving to starboard
        if (chosenHeading > 0 && chosenHeading < 180)
        {
            reward += 10f;
        }
        
        SetReward(reward);
        
        

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
    
    
    private float CalculateReward(int chosenHeading, int requiredHeading)
    {
        // Define constants
        float headingRewardWeight = 0.5f;
        float checkpointRewardWeight = 1.0f;
    
        // Calculate heading reward based on how close the chosen heading is to the required heading
        float headingReward = headingRewardWeight * Mathf.Cos(Mathf.Deg2Rad * Mathf.Abs(chosenHeading - requiredHeading));
    
        // Calculate distance to checkpoint
        float distanceToCheckpoint = Vector3.Distance(ownship.transform.position, checkpoints[currentCheckpointIndex].position);
    
        // Give a higher reward for being closer to the checkpoint
        float checkpointReward = checkpointRewardWeight / distanceToCheckpoint;
        
        // If checkpoint reached, give a big reward
        if (distanceToCheckpoint < 10.0f) // Threshold for reaching a checkpoint
        {
            checkpointReward += 100f; // Additional reward for reaching the checkpoint
            currentCheckpointIndex++; // Move to the next checkpoint
        }
    
        // Combine rewards
        float totalReward = headingReward + checkpointReward;
    
        return totalReward;
    }

    public float calculateRewardBasedOnDistance()
    {
        float totalDistanceRequired = 0f;
        
        for (int i = 0; i < checkpoints.Count-1; i++)
        {
            totalDistanceRequired += Vector3.Distance(checkpoints[i].position, checkpoints[i+1].position);
        }
        
        Vector3 ownShipPosition = ownship.transform.position;
        
        // Calculate total distance covered by the ship
        float totalDistanceCovered = Vector3.Distance(ownShipPosition, checkpoints[0].position); // Assuming the starting point is the first checkpoint
    
        for (int i = 0; i < checkpoints.Count - 1; i++)
        {
            totalDistanceCovered += Vector3.Distance(checkpoints[i].position, checkpoints[i + 1].position);
        }

        // Calculate the ratio of distance covered to total distance required
        float distanceRatio = totalDistanceCovered / totalDistanceRequired;

        // Calculate reward based on the ratio
        float reward = Mathf.Clamp(100f * distanceRatio, 0f, 100f);
    
        return reward;
    }

    public float RewardBasedOnDestinationDistance()
    {
        float currentDistanceToDestination = Vector3.Distance(ownship.transform.position, ownshipDestination.position);
        float distanceDelta = initialDistanceToDestination - currentDistanceToDestination;
        if (distanceDelta > 0)
        {
            // Ownship is getting closer to the destination
            return 0.1f * distanceDelta; // You can adjust the reward factor as needed
        }else
        {
            // Ownship is getting away from the destination
            return -0.1f * distanceDelta; // You can adjust the reward factor as needed
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
    
        // Default action is 0
        int action = 0;

        // Check for keyboard inputs
        if (Input.GetKey(KeyCode.RightArrow))
        {
            // Increase action value
            action += 1;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // Decrease action value
            action -= 1;
        }

        // Assign the action to the discreteActions
        discreteActions[0] = action;
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
        // float currentReward = GetCumulativeReward();
        //
        // Debug.Log("Current Reward: " + currentReward);
        //
        //
        // if (currentCheckpointIndex < checkpoints.Count)
        // {
        //     // CheckForCheckpointArrival();
        //     AddRewardBasedOnDistanceToCheckpoint();
        //     // RewardForCorrectHeading();
        //     // AddRewardBasedOnFinalDistance();
        // }
        // else
        // {
        //     CheckForDestinationArrival();
        // }
        OutOfScreenEndEpisode();

        // float reward = calculateRewardBasedOnDistance();
        // rewardText.text = "Reward: " + reward;
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
            shipOldPosition = ownship.transform.position; // Update old position for next checkpoint calculation // recheck

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
            AddReward(-5f);
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

    public int ActionToHeading(int action)
    {
        if (action == 0)
        {
            return 0;
        }
        else if (action == 1)
        {
            return 20;
        }
        else if (action == 2)
        {
            return 30;
        }
        else if (action == 3)
        {
            return 40;
        }
        else if (action == 4)
        {
            return 50;
        }
        else if (action == 5)
        {
            return 60;
        }
        else if (action == 6)
        {
            return 70;
        }
        else if (action == 7)
        {
            return 80;
        }
        else if (action == 8)
        {
            return 90;
        }
        else if (action == 9)
        {
            return 340;
        }
        else if (action == 10)
        {
            return 330;
        }
        else if (action == 11)
        {
            return 320;
        }
        else if (action == 12)
        {
            return 310;
        }
        else if (action == 13)
        {
            return -60;
        }
        else if (action == 14)
        {
            return 300;
        }
        else if (action == 15)
        {
            return 280;
        }
        else if (action == 16)
        {
            return 270;
        }
        else
        {
            return 0;
        }
    }

}
