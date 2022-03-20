using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activatable : MonoBehaviour
{
    /// <summary>
    /// Activatable
    /// Dylan Loe
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - activatable parent object that is eventually overridden by each of the items able to be picked up by player
    ///     - includes player items
    ///     - weapons
    ///     - objectives
    ///     - entrances
    ///     - crafting table
    /// </summary>
    public bool isActivated = false;

    //virtual activation function that is overwritten for every time that spawns
    public virtual void Activate()
    {
        Debug.Log("Unset Activatable has be activated...");
    }
}
