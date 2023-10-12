using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    /// <summary>
    /// Player Detection
    /// Dylan Loe
    /// 
    /// Updated 5/25/22
    /// 
    /// Notes:
    /// - Very simple fog of war element to hide unexplored tiles (consider updating in future)
    /// - when player steps on tile, it removes the barrier from the map view
    /// </summary>
    [Header("Fog of war for minimap")]
    public bool hasBeenVisited = false;
    public GameObject fogofwar;
}
