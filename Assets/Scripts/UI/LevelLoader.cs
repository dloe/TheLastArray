using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    /// <summary>
    /// Level Loader
    /// Dylan Loe
    /// 
    /// Last Updated: 4/25/21
    /// 
    /// - handles changing of levels system
    /// - always keeps an instance present in each level
    /// </summary>
    public static LevelLoader Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    public void LoadLevel(string level)
    {
        Debug.Log("Loading level: " + level);
        PlayerPrefs.SetString("SelectedLevel", level);
        SceneManager.LoadScene("LoadingScreen");
    }
}
