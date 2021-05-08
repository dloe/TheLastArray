using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gasoline : Activatable
{
    public override void Activate()
    {
        Objectives.Instance.SendCompletedMessage(Condition.GetGasCan);
        Destroy(gameObject);
        Player.Instance.thingToActivate = null;
    }
}
