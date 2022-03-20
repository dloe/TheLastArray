using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    /// <summary>
    /// Door Script
    /// Dylan Loe
    /// 
    /// Lasat Updated: 4/20/21
    /// 
    ///     - UNUSED
    ///     Doors act as connectors, each door must have a reference to each room its connected to otherwise, its a no longer a door
    ///     The room that initially spawns in doors is A, the second one is always B. Works out from center
    /// </summary>
    public GameObject roomA;
    public GameObject roomB;
}
