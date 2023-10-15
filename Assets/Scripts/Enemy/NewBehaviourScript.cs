using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    /// <summary>
    /// Dylan Loe
    /// Updated: 4-12
    /// 
    /// Test script for boss tracking behaviors
    /// 
    /// - NOT IN USE CURRENTLY, TESTING ONLY
    /// </summary>
    public GameObject target;
    public float angle;

    private void Update()
    {
        Vector3 temp = transform.position - target.transform.position;
        angle = Mathf.Atan2(temp.x, temp.z) * Mathf.Rad2Deg;

    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.transform.position);
    }
}
