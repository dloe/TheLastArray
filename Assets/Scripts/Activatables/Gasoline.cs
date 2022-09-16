using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gasoline : Activatable
{
    /// <summary>
    /// Activatable
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - activatable object used as an objective
    /// </summary>
    public override void Activate()
    {
        Objectives.Instance.SendCompletedMessage(Condition.GetGasCan);
        Destroy(gameObject);
        Player.Instance.thingToActivate = null;
    }
}
