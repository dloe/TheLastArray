using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainEntry : Activatable
{
    public static TrainEntry Instance;
    public Text trainText;
    LocalLevel localLevel;

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
        localLevel = FindObjectOfType<LocalLevel>();
    }

    public override void Activate()
    {
        if (!isActivated && Objectives.Instance.mainObjective.complete && localLevel.thisLevelTier != levelTier.level4)
        {
            isActivated = true;

            localLevel.LevelBeat();

        }
        else if (!isActivated && Objectives.Instance.mainObjective.complete && localLevel.thisLevelTier == levelTier.level4)
        {
            Objectives.Instance.objectiveText.text = "You Win!";
            Player.Instance.endScreenText.text = "You Win!";
            Time.timeScale = 0;
            Player.Instance.endScreen.SetActive(true);
            Objectives.Instance.SendCompletedMessage(Condition.KillEnemy);
        }

    }
}
