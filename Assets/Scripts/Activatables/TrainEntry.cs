using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainEntry : Activatable
{
    LocalLevel localLevel;
    public void Start()
    {
        localLevel = FindObjectOfType<LocalLevel>();
    }

    public override void Activate()
    {
        if (!isActivated && Objectives.Instance.mainObjective.complete)
        {
            isActivated = true;

            localLevel.LevelBeat();
                
        }
        else if(!isActivated && localLevel.thisLevelTier == levelTier.level4)
            Objectives.Instance.SendCompletedMessage(Condition.KillEnemy);

    }
}
