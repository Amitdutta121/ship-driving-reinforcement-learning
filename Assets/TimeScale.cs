using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScale : MonoBehaviour
{
    public float timeScale = 1.0f;

    void Update()
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02f * timeScale; // Adjust the fixed time step accordingly
    }
}
