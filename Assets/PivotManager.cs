using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotManager : MonoBehaviour
{
    [Header("Detection Settings")]
    public LayerMask pivotLayer;           // Layer mask for pivot points
    public float pivotDetectionRadius = 5f;// How far you can detect pivots

    [Header("Runtime Info")]
    public Transform currentPivot; // Closest pivot this frame
    public float currentPivotDistance;      // Distance to that pivot

    public void DetectClosestPivot(Vector3 origin)
    {
        // Find all colliders in detection radius
        Collider[] hits = Physics.OverlapSphere(origin, pivotDetectionRadius, pivotLayer);

        currentPivot = null;
        currentPivotDistance = Mathf.Infinity;

        if (hits.Length == 0)
            return;

        // Find closest pivot
        foreach (var h in hits)
        {
            float dist = Vector3.Distance(origin, h.transform.position);
            if (dist < currentPivotDistance)
            {
                currentPivotDistance = dist;
                currentPivot = h.transform;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pivotDetectionRadius);

        // Draw line to current pivot for debug
        if (currentPivot != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentPivot.position);
        }
    }
}
