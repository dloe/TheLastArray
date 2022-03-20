using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MainMenuUI : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject options;
    public GameObject credits;
    public GameObject howToPlay; 

    private void Start()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        LoadAudioLevels();
       // SetGraphics(PlayerPrefs.GetInt("Quality Level", 5));
    }

    /// <summary>
    /// Shows the GameObject screenToShow
    /// </summary>
    /// <param name="screenToShow">GameObject / Screen to Show</param>
    public void ShowScreen(GameObject screenToShow)
    {
        screenToShow.SetActive(true);
    }

    /// <summary>
    /// Hides the GameObject screenToHide
    /// </summary>
    /// <param name="screenToHide">GameObject / Screen to Hide</param>
    public void HideScreen(GameObject screenToHide)
    {
        screenToHide.SetActive(false);
    }

    /// <summary>
    /// Loads Level by name using levelName
    /// </summary>
    /// <param name="levelName">name of level to be loaded</param>
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }


    /// <summary>
    /// Exits the Game
    /// </summary>
    public void Quit()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    /// <summary>
    /// Opens Link Specified by input string "url"
    /// </summary>
    /// <param name="url">URL to be opened</param>
    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }

    #region Options
    public AudioMixer mixer;
    public Slider master, music, sfx; // qualitySlider;


    public Text masterText, musicText, sfxText; // qualityText;

    //public void SetGraphics(float qualityIndex)
    //{
    //
    //    QualitySettings.SetQualityLevel((int)qualityIndex);
    //    PlayerPrefs.SetInt("Quality Level", (int)qualityIndex);
    //    qualityText.text = "Quality: " + QualitySettings.names[(int)qualityIndex];
    //
    //    if (qualitySlider.value != PlayerPrefs.GetInt("Quality Level"))
    //    {
    //        qualitySlider.value = PlayerPrefs.GetInt("Quality Level");
    //    }
    //}

    public void ToggleFullScreen()
    {
        //Debug.LogAssertion("(Previous) Width: " + Screen.width + "  " + "Height: " + Screen.height);
        //Debug.LogAssertion("Ratio: " + (float)Screen.width / Screen.height);
        if (Screen.fullScreen)
        {
            Screen.SetResolution((int)(Screen.width * 0.90), (int)(Screen.height * 0.90), false);
        }
        else
        {
            Screen.SetResolution((int)(Screen.currentResolution.width), (int)(Screen.currentResolution.height), true);
        }
        //StartCoroutine(DebugResolution());
    }

    IEnumerator DebugResolution()
    {
        yield return null;
        yield return null;
        Debug.LogAssertion("(Current) Width: " + Screen.width + "  " + "Height: " + Screen.height);
        Debug.LogAssertion("Ratio: " + (float)Screen.width / Screen.height);
    }

    public void SetMasterLevel(float sliderValue)
    {
        mixer.SetFloat("Master", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("MasterVol", Mathf.Log10(sliderValue) * 20);
        masterText.text = "Master: " + Mathf.Round(sliderValue * 100);

    }

    public void SetMusicLevel(float sliderValue)
    {
        mixer.SetFloat("Music", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20);
        musicText.text = "Music: " + Mathf.Round(sliderValue * 100);

    }

    public void SetSFXLevel(float sliderValue)
    {
        mixer.SetFloat("SFX", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("SFXVol", Mathf.Log10(sliderValue) * 20);
        sfxText.text = "SFX: " + Mathf.Round(sliderValue * 100);
    }

    private void LoadAudioLevels()
    {
        mixer.GetFloat("Master", out float mastValue);
        mixer.GetFloat("Music", out float musicValue);
        mixer.GetFloat("SFX", out float sfxValue);

        if (PlayerPrefs.HasKey("MasterVol"))
        {
            master.value = Mathf.Pow(10f, (PlayerPrefs.GetFloat("MasterVol") / 20));
            mixer.SetFloat("Master", PlayerPrefs.GetFloat("MasterVol"));
        }
        else
        {
            PlayerPrefs.SetFloat("MasterVol", mastValue);
            master.value = Mathf.Pow(10f, (mastValue / 20));
        }

        if (PlayerPrefs.HasKey("MusicVol"))
        {
            music.value = Mathf.Pow(10f, (PlayerPrefs.GetFloat("MusicVol") / 20));
            mixer.SetFloat("Music", PlayerPrefs.GetFloat("MusicVol"));
        }
        else
        {
            PlayerPrefs.SetFloat("MusicVol", musicValue);
            music.value = Mathf.Pow(10f, (musicValue / 20));
        }

        if (PlayerPrefs.HasKey("SFXVol"))
        {
            sfx.value = Mathf.Pow(10f, (PlayerPrefs.GetFloat("SFXVol") / 20));
            mixer.SetFloat("SFX", PlayerPrefs.GetFloat("SFXVol"));
        }
        else
        {
            PlayerPrefs.SetFloat("SFXVol", sfxValue);
            sfx.value = Mathf.Pow(10f, (sfxValue / 20));
        }



        masterText.text = "Master: " + Mathf.Round(master.value * 100);
        musicText.text = "Music: " + Mathf.Round(music.value * 100);
        sfxText.text = "SFX: " + Mathf.Round(sfx.value * 100);
    }
    #endregion
}
