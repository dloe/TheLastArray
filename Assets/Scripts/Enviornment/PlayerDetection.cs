using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    /// <summary>
    /// Plyer Detection
    /// Dylan Loe
    /// 
    /// Updated 5/25/22
    /// 
    /// Notes:
    /// - Very simply fog of war element to hide unexplored tiles
    /// - when player steps on tile, it removes the barrior from the map view
    /// </summary>
    [Header("Fog of war for minimap")]
    public bool hasBeenVisited = false;
    public GameObject fogofwar;

}
