using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public enum levelTier
{
    level1,
    level2,
    level3,
    level4
}

public class LocalLevel : MonoBehaviour
{
    /// <summary>
    /// Local Level Script
    /// Dylan Loe
    /// 
    /// Updated: 5/17/22
    /// 
    /// REQURIES:
    ///     - a TileGen Prefab
    /// Notes:
    /// - keeps track of local level info
    /// - handles scene transitions into level
    /// - sets one of the objectvie types
    /// - saves player info
    /// - sets what type of tile assets we use
    /// 
    /// </summary>
    public bool tempEndLevel = false;
    //holds objective info and game loop info
    //consider putting ui here maybe
    TileGeneration _myTileGen;
    [Header("Player Data Obj")]
    public PlayerData myPlayerData;
    [Header("Level Asset Data Obj")]
    public LevelAssetsData myLvlAssetData;
    [HideInInspector]
    public List<int> _posObjectives;

    //interpolation
    bool fadeIn = false;
    bool fadeOut = false;
    float timeStart;
    float u;
    bool ctc = false;
    float a0, a1, a01;
    bool fading = false;
    public float timeDuration = 1.5f;
    [SerializeField]
    public Image transBar;

    [Header("Objective Number - choosen in this script")]
    public int objective;
    [Header("Objective Asset Obj")]
    public Objectives _myObjs;
    [Header("Local Player Script")]
    public Player myPlayer;

    //determines difficulty of level
    [Header("Tier of Level")]
    public levelTier thisLevelTier;
    public static LocalLevel Instance;

    [Header("Starting Tile Prefabs")]
    public List<GameObject> presetStartingTileAssets;
    [Header("The Small 1 Tile Preset Variations")]
    public List<GameObject> presetTileAssets;
    [Header("The Big 4 Tile Preset Variations")]
    public List<GameObject> presetBigTileAssets;
    [Header("Tile Prefabs with objectives")]
    public List<GameObject> presetObjectiveTiles;
    [Header("Big Tile Prefabs with objectives")]
    public List<GameObject> presetBigObjectiveTiles;
    public GameObject presetStartingTile;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        //assign proper preset tile data for level
        AssetDataSetup();

        //assign tileGen obj
        _myTileGen = FindObjectOfType<TileGeneration>();
        //number of objectives - REMOVED NUMBER 2 WILL READD LATER
        _posObjectives = new List<int> { 1, 2, 3 };
        StartCoroutine(FadeIn());
    }

    private void Update()
    {
        if(fadeIn)
        {
            LevelFadeIn();
        }
        else if(fadeOut)
        {
            LevelFadeOut();
        }

        //temp test of level transition
        if(tempEndLevel)
        {
            LevelBeat();
            tempEndLevel = false;
        }
    }

    /// <summary>
    /// - list of objectives
    /// - remove previous obj from list
    /// - pick randomly from updated list
    /// </summary>
    public void ChooseObjective()
    {
        if (thisLevelTier != levelTier.level4)
        {
            //picks objective - cant be the previous objective
            if (myPlayerData.previouslyCompletedObj == -1)
            {
                //randomly pick any
                int rand = Random.Range(0, _posObjectives.Count);
                objective = _posObjectives[rand];
            }
            else
            {
                //exclude previous obj index from choice
                _posObjectives.Remove(myPlayerData.previouslyCompletedObj);
                _posObjectives = reshuffle(_posObjectives);
                objective = _posObjectives[Random.Range(0, _posObjectives.Count)];
            }
        }
        else
        {
            //final objective
            objective = 4;
        }
        myPlayerData.previouslyCompletedObj = objective;
    }

    public void LevelBeat()
    {
        StartCoroutine(LevelWonEvent());
    }

    /// <summary>
    ///  - when player accomplishes object and gets to extraction
    ///     - saves data to scriptable obj
    ///     - fade out
    ///     - player leaves this scene, goes to crafting scene (for now next level)
    ///     - update levels beaten var
    /// </summary>
    IEnumerator LevelWonEvent()
    {
        StartFadeOut();
        //save player data to scriptable obj
        Debug.Log("Level " + ((int)thisLevelTier + 1) + " is beaten!");
        myPlayer.levelsBeaten[(int)thisLevelTier] = true;
        myPlayer.SavePlayer();

        yield return new WaitForSeconds(1.0f);

        //myPlayerData.currentLevelNumber++;
        myPlayerData.previousLevelName = SceneManager.GetActiveScene().name;
        //transition scene

        if((int)thisLevelTier < 3)
            LevelLoader.Instance.LoadLevel("Train");
        else
            LevelLoader.Instance.LoadLevel("MainMenu");

        Debug.Log("Loading next scene");
    }

    //reshuffle list
    List<int> reshuffle(List<int> ar)
    {
        for (int t = 0; t < ar.Count; t++)
        {
            int tmp = ar[t];
            int r = Random.Range(t, ar.Count);
            ar[t] = ar[r];
            ar[r] = tmp;
        }
        return ar;
    }

    /// <summary>
    /// - establish which type of tile the generation system will be using
    /// </summary>
    void AssetDataSetup()
    {
        switch (thisLevelTier)
        {
            case levelTier.level1:
                //forest
                presetTileAssets = new List<GameObject>(myLvlAssetData.forest_presetTileAssets);
                presetBigTileAssets = new List<GameObject>(myLvlAssetData.forest_presetBigTileAssets);
                presetObjectiveTiles = new List<GameObject>(myLvlAssetData.forest_presetObjectiveTiles);
                presetBigObjectiveTiles = new List<GameObject>(myLvlAssetData.forest_presetBigTileObjectives);
                presetStartingTile = myLvlAssetData.forest_presetStartingTile;
                break;
            case levelTier.level2:
                //outskirts
                presetTileAssets = new List<GameObject>(myLvlAssetData.outskirts_presetTileAssets);
                presetBigTileAssets = new List<GameObject>(myLvlAssetData.outskirts_presetBigTileAssets);
                presetObjectiveTiles = new List<GameObject>(myLvlAssetData.outskirts_presetObjectiveTiles);
                presetBigObjectiveTiles = new List<GameObject>(myLvlAssetData.outskirts_presetBigTileObjectives);
                presetStartingTile = myLvlAssetData.outskirts_presetStartingTile;
                break;
            case levelTier.level3:
                presetTileAssets = new List<GameObject>(myLvlAssetData.urban_presetTileAssets);
                presetBigTileAssets = new List<GameObject>(myLvlAssetData.urban_presetBigTileAssets);
                presetObjectiveTiles = new List<GameObject>(myLvlAssetData.urban_presetObjectiveTiles);
                presetBigObjectiveTiles = new List<GameObject>(myLvlAssetData.urban_presetBigTileObjectives);
                presetStartingTile = myLvlAssetData.urban_presetStartingTile;
                break;
            case levelTier.level4:
                presetTileAssets = new List<GameObject>(myLvlAssetData.plant_presetTileAssets);
                presetBigTileAssets = new List<GameObject>(myLvlAssetData.plant_presetBigTileAssets);
                presetObjectiveTiles = new List<GameObject>(myLvlAssetData.plant_presetObjectiveTiles);
                presetBigObjectiveTiles = new List<GameObject>(myLvlAssetData.plant_presetBigTileObjectives);
                presetStartingTile = myLvlAssetData.plant_presetStartingTile;
                break;
            default:
                break;
        }
    }

    #region Scene Transitions

    IEnumerator FadeIn()
    {
        Color temp = transBar.color;
        temp.a = 1;
        transBar.color = temp;
        yield return new WaitForSeconds(0.25f);
        StartFadeIn();
    }

    public void StartFadeIn()
    {
        // Debug.Log("on");
        fadeIn = true;
        ctc = true;
        u = 1.0f;
    }

    public void StartFadeOut()
    {
        fadeOut = true;
        ctc = true;
        u = 0.0f;
    }
    /// <summary>
    /// Scene Transitions, will incorperate a fade in and out
    /// - will not use animator on canvas to avoid stuff being in update
    /// - will use interpolation on panels alpha
    /// </summary>
    public void LevelFadeOut()
    {
        if(ctc)
        {
            a0 = 0f;
            a1 = 1.0f;
            ctc = false;
            fading = true;
            timeStart = Time.time;
        }
        if(fading)
        {
            u = (Time.time - timeStart) / timeDuration;

            //u = 1 - u;
            if (u >= 1.0)
            {
                u = 1;
                fading = false;
                fadeOut = false;
                //Debug.Log("off");
            }

            a01 = (1 - u) * a0 + u * a1;

            Color temp = transBar.color;
            temp.a = a01;
            transBar.color = temp;
        }
    }


    public void LevelFadeIn()
    {
        if (ctc)
        {
            a0 = 1.0f;
            a1 = 0f;
            ctc = false;
            fading = true;
            timeStart = Time.time;
        }
        if (fading)
        {
            u = (Time.time - timeStart) / timeDuration;
            u = 1 - u;
            if (u <= 0.0f)
            {
                u = 0;
                fading = false;
                fadeIn = false;
               //Debug.Log("off");
            }

            a01 = (1 - u) * a0 + u * a1;

            Color temp = transBar.color;
            temp.a = 1 - a01;
            transBar.color = temp;
        }
    }
    #endregion
}
