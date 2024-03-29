﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainEntry : Activatable
{
    /// <summary>
    /// Activatable
    /// Dylan Loe
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - activatable used for the player to enter the train
    /// </summary>
    public static TrainEntry Instance;
    public Text trainText;
    LocalLevel _localLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    public void Start()
    {
        _localLevel = FindObjectOfType<LocalLevel>();
    }

    public override void Activate()
    {
        if (!isActivated && Objectives.Instance.mainObjective.complete && _localLevel.thisLevelTier != levelTier.level4)
        {
            isActivated = true;

            _localLevel.LevelBeat();
        }
        else if (!isActivated && Objectives.Instance.mainObjective.complete && _localLevel.thisLevelTier == levelTier.level4)
        {
            Objectives.Instance.objectiveText.text = "You Win!";
            Player.Instance.endScreenText.text = "You Win!";
            Time.timeScale = 0;
            Player.Instance.endScreen.SetActive(true);
            Objectives.Instance.SendCompletedMessage(Condition.KillEnemy);
        }
    }
}
