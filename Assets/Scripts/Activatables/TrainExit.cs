using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainExit : Activatable
{
    /// <summary>
    /// Activatable
    /// Dylan Loe
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - activatable exit door used by the player to exit the train hub space
    /// </summary>
    public PlayerData playerData;
    public Transitions myTransition;

    public override void Activate()
    {
        if (!isActivated)
        {
            Debug.Log("Train Exit Called");
            isActivated = true;
            Player.Instance.SavePlayer();
            StartCoroutine(ExitTransition());
        }
    }

    IEnumerator ExitTransition()
    {
        myTransition.StartFadeOut();
        yield return new WaitForSeconds(1.0f);
        LevelLoader.Instance.LoadLevel(GetLevelToLoad(playerData.previousLevelName));
    }

    public string GetLevelToLoad(string previousLevelName)
    {
        string result = "";

        if(previousLevelName == playerData.levelOneName)
        {
            result = playerData.levelTwoName;
        }
        else if(previousLevelName == playerData.levelTwoName)
        {
            result = playerData.levelThreeName;
        }
        else if (previousLevelName == playerData.levelThreeName)
        {
            result = playerData.levelFourName;
        }
        else 
        {
            Debug.LogError("Previous Level Name is not valid");
        }

        playerData.previousLevelName = result;

        return result;
    }
}
