using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;


    // Update is called once per frame
    //void Update()
    //{
    //    if (CombatSystem.Instance == null) return;

    //    if (CombatSystem.Instance.state == BattleState.Won && loading == false)
    //    {
    //        Debug.Log("Level Won");
    //        LoadNextLevel();
    //    }
    //}

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
