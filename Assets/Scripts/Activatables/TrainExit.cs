﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainExit : Activatable
{
    public PlayerData playerData;
    
    public override void Activate()
    {
        if (!isActivated)
        {
            Debug.Log("Train Exit Called");
            isActivated = true;

            SceneManager.LoadScene(GetLevelToLoad(playerData.previousLevelName));
        }

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
