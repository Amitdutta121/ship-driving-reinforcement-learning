using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointVisualizer : MonoBehaviour
{

    void OnDrawGizmos()
    {
        DrawAllChildrenGizmos(this.transform);
    }

    void DrawAllChildrenGizmos(Transform parent)
    {
        // Iterate over all child transforms and draw Gizmos
        foreach (Transform child in parent)
        {
            Gizmos.DrawSphere(child.position, 0.25f); // Draw a sphere at the child's position. Adjust the size as needed.
            DrawAllChildrenGizmos(child); // Recursively draw Gizmos for children of children
        }
    }
}
