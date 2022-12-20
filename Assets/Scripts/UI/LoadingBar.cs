﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingBar : MonoBehaviour
{
    /// <summary>
    /// Player Script
    /// Dylan Loe
    /// 
    /// Last Updated: 4/27/21
    /// 
    /// Notes:
    ///  - Level loading screen UI
    ///  
    /// - This needs a second look though, main goal is for this to run when levels are generating not just for Unity level load
    /// </summary>
    public Image progress;

    void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.1f);
        string NextScene = PlayerPrefs.GetString("SelectedLevel");
        float fill = 0;
        if (Application.CanStreamedLevelBeLoaded(NextScene))
        {
            AsyncOperation gameLevel = SceneManager.LoadSceneAsync(NextScene);
            while (gameLevel.progress < 1)
            {
                fill += (gameLevel.progress - fill) / 2;
                progress.fillAmount = fill;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            Debug.LogError("Invalid Scene Name: " + NextScene);
            SceneManager.LoadScene(0);
        }
    }
}
