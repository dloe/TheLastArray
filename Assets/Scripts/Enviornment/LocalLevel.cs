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
    /// - sets one of the objective types
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
    bool _fadeIn = false;
    bool _fadeOut = false;
    float _timeStart;
    float _u;
    bool _ctc = false;
    float _a0, _a1, _a01;
    bool _fading = false;
    public float timeDuration = 1.5f;
    [SerializeField]
    public Image transBar;

    [Header("Objective Number - chosen in this script")]
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
        if(_fadeIn)
        {
            LevelFadeIn();
        }
        else if(_fadeOut)
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
    ///     - saves data to Scriptable obj
    ///     - fade out
    ///     - player leaves this scene, goes to crafting scene (for now next level)
    ///     - update levels beaten var
    /// </summary>
    IEnumerator LevelWonEvent()
    {
        StartFadeOut();
        //save player data to Scriptable obj
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
        _fadeIn = true;
        _ctc = true;
        _u = 1.0f;
    }

    public void StartFadeOut()
    {
        _fadeOut = true;
        _ctc = true;
        _u = 0.0f;
    }
    /// <summary>
    /// Scene Transitions, will incorporate a fade in and out
    /// - will not use animator on canvas to avoid stuff being in update
    /// - will use interpolation on panels alpha
    /// </summary>
    public void LevelFadeOut()
    {
        if(_ctc)
        {
            _a0 = 0f;
            _a1 = 1.0f;
            _ctc = false;
            _fading = true;
            _timeStart = Time.time;
        }
        if(_fading)
        {
            _u = (Time.time - _timeStart) / timeDuration;

            //u = 1 - u;
            if (_u >= 1.0)
            {
                _u = 1;
                _fading = false;
                _fadeOut = false;
                //Debug.Log("off");
            }

            _a01 = (1 - _u) * _a0 + _u * _a1;

            Color temp = transBar.color;
            temp.a = _a01;
            transBar.color = temp;
        }
    }


    public void LevelFadeIn()
    {
        if (_ctc)
        {
            _a0 = 1.0f;
            _a1 = 0f;
            _ctc = false;
            _fading = true;
            _timeStart = Time.time;
        }
        if (_fading)
        {
            _u = (Time.time - _timeStart) / timeDuration;
            _u = 1 - _u;
            if (_u <= 0.0f)
            {
                _u = 0;
                _fading = false;
                _fadeIn = false;
               //Debug.Log("off");
            }

            _a01 = (1 - _u) * _a0 + _u * _a1;

            Color temp = transBar.color;
            temp.a = 1 - _a01;
            transBar.color = temp;
        }
    }
    #endregion
}
