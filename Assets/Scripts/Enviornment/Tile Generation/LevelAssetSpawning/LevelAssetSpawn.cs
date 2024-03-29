﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAssetSpawn : MonoBehaviour
{
    /// <summary>
    /// Level Asset Spawn System
    /// Dylan Loe
    /// 
    /// Last Updated: 5/20/21
    /// 
    /// REQURIES:
    ///     - a LevelInfo Prefab
    ///     - system used for spawning assets on each tile
    ///     - has caps for items, enemies and objectives
    ///     - links some rooms into 2 x 2 tiles, also has caps
    ///     - called mainly from tile gen script
    ///     
    /// </summary>
    [Header("Tile Gen Script")]
    //ref to tile gen script
    public TileGeneration myTileGeneration;
    [Header("Level Asset Data Obj")]
    public LevelAssetsData myLevelAsset;
    [Header("Player Data Obj")]
    public PlayerData myPlayerData;
    [Header("Local Level Data")]
    public LocalLevel myLocalLevel;

    //to prevent to many of one asset spawning
    [Space(10)]
    [Header("How many of which assets are spawned in")]
    public int[] assetCountArray;
    public int[] bigAssetCountArray;

    Tile[] _tArray;
    [Header("How many big tiles have been spawned")]
    public int fourSomeCount = 0;
    Vector3 _av;

    //each tile preset has possible locations for resources, those go here
    public List<GameObject> _possibleItems = new List<GameObject>();
    [Header("Resources In Level")]
    public List<GameObject> resourcesInLevelList = new List<GameObject>();
    [Header("Items In Level")]
    public List<GameObject> itemsInLevelList = new List<GameObject>();
    [Header("Weapons In Level")]
    public List<GameObject> weaponsInLevelList = new List<GameObject>();
    //possible objectives
    public List<GameObject> _possibleObjectives = new List<GameObject>();

    [Header("Enemies In Level")]
    public List<GameObject> enemiesInLevel = new List<GameObject>();
    public List<GameObject> miniBossesInLevel = new List<GameObject>();
    public List<GameObject> _possibleEnemiesInLevel = new List<GameObject>();

    [Header("number of possible drop prefabs")]
    public int dontSpawnCount = 3;

    public GameObject endObjTile;
    [Header("Objectives In Level")]
    public List<GameObject> objectivesInLevel = new List<GameObject>();
    public List<GameObject> _possibleTileObjectivesInLevel = new List<GameObject>();

    [Header("Total Collectibles Spawned")]
    public int collectables;
    [Header("Amount of enemies spawned in level")]
    public int enemyCount;
    public int possibleminiBossCount = 0;
    static int tier1MiniBossCap = 1;
    static int tier2MiniBossCap = 1;
    static int tier3MiniBossCap = 2;
    static int tier4MiniBossCap = 2;
    int _miniBossCap;
    static int tier1EnemyCap = 15;
    static int tier2EnemyCap = 22;
    static int tier3EnemyCap = 30;
    static int tier1CollectableCap = 25;
    static int tier2CollectableCap = 20;
    static int tier3CollectableCap = 17;
    public int currentMiniBossCount = 0;

    //first number represents the number of times tiles in that list were spawned
    //second number represents the tile numbers that were spawned that amount of times
    List<List<int>> _magAssetCount;
    
    [Header("Ref to player data obj")]
    public GameObject playerPref;
    [Header("Light obj")]
    public GameObject lightObj;
    [HideInInspector]
    public GameObject playerSpawn;

    public List<GameObject> parents;
    public List<GameObject> _bigTilesList = new List<GameObject>();

    static private float twoBYtwo_SpawnChance = 0.25f;
    GameObject _play;

    //debug prints from TileGeneration
    bool _debugPrints;

    //set up prints to carry over here as well
    private void Awake()
    {
        _debugPrints = myTileGeneration.debugPrints;
    }

    /// <summary>
    /// Populate grid with assets, called from TileGeneration once it is done setting up
    /// </summary>
    public void PopulateGrid()
    {
        if (_debugPrints)
            myTileGeneration.PrintToLog("Populating Grid....", false);

        StartUpLevelAssetSpawn();

        GridAnalysis();

        ActivateLevelKey();

        if(myLocalLevel.thisLevelTier > levelTier.level2)
            ActivateSecretRoom();

        myLocalLevel.ChooseObjective();
        
        //ACTIVATE OBJECTIVES
        ActivateObjectives();
        
        //SPAWN IN RESOURCES
        ActivateItems();

        //ACTIVATE ENEMIES
        ActivateEnemies();

        
    }

    //rename TileGrid for easy debugging when tile system is finished
    private void Start()
    {
        StartCoroutine(LateStart(2.0f));
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        RenameTileGrid();
    }

    void RenameTileGrid()
    {
        if (this.transform.name == "TileGen")
        {
            //based on what level we are on, we are renaming the map from TileGen
            switch (myLocalLevel.thisLevelTier)
            {
                case levelTier.level1:
                    this.transform.name = "Level1_GridMap_Forest";
                    break;
                case levelTier.level2:
                    this.transform.name = "Level2_GridMap_Outskirts";
                    break;
                case levelTier.level3:
                    this.transform.name = "Level3_GridMap_Urban";
                    break;
                case levelTier.level4:
                    this.transform.name = "Level4_GridMap_LastArray";
                    break;
                default:
                    break;
            }
        }
    }

    //sets some values when this script is first called
    void StartUpLevelAssetSpawn()
    {
        if (_debugPrints)
            myTileGeneration.PrintToLog("Establishing Level Asset Values....",false);

        assetCountArray = new int[myLocalLevel.presetTileAssets.Count];
        bigAssetCountArray = new int[myLocalLevel.presetBigTileAssets.Count];

        switch (myLocalLevel.thisLevelTier)
        {
            case levelTier.level1:
                _miniBossCap = tier1MiniBossCap;
                break;
            case levelTier.level2:
                _miniBossCap = tier2MiniBossCap;
                break;
            case levelTier.level3:
                _miniBossCap = tier3MiniBossCap;
                break;
            case levelTier.level4:
                _miniBossCap = tier4MiniBossCap;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// checks each tile for purpose of adding walls, linking 2 x 2, bringing in 2 x 2 tiles
    /// </summary>
    void GridAnalysis()
    {
        if (_debugPrints)
            myTileGeneration.PrintToLog("Entering Grid Analysis....",false);

        //activate walls
        foreach (Tile t in myTileGeneration._allActiveTiles)
        {
            if(t.tileStatus != Tile.TileStatus.startingRoom)
                t.ActivateWalls();
            if (myLocalLevel.thisLevelTier != levelTier.level4)
                AnalyzeTile(t);
            else
                AnalyzeTile_Lvl4Modifier(t);

            //considerings first linking 2 x 2s then going through to activate walls

            //add starting tile resources
            if (t.tileStatus == Tile.TileStatus.startingRoom)
            {
                for (int posResourceCount = 0; posResourceCount < t.presetTile.GetComponent<PresetTileInfo>().possiblePresetItems.Length; posResourceCount++)
                {
                    _possibleItems.Add(t.presetTile.GetComponent<PresetTileInfo>().possiblePresetItems[posResourceCount]);
                }

                playerSpawn = t.presetTile.GetComponent<PresetTileInfo>().playerSpawn;
                //SPAWN PLAYER
                _play = Instantiate(playerPref, Vector3.zero, playerSpawn.transform.rotation);
                _play.transform.GetChild(2).GetComponent<Player>().PlayerCamRot = playerSpawn.transform.eulerAngles.y;

                if (lightObj != null)
                {
                    lightObj.transform.rotation = Quaternion.Euler(playerSpawn.transform.eulerAngles);//new Quaternion(playerSpawn.transform.rotation);
                }

                StartCoroutine(setPlayerPosition(_play, playerSpawn.transform.position));
                //will use getcomponent in scene next time
                myLocalLevel.myPlayer = _play.transform.GetChild(2).gameObject.GetComponent<Player>();
            }
        }
        if (_debugPrints)
            myTileGeneration.PrintToLog("Finished Grid Analysis....",false);

        if (myTileGeneration.hasDoors)
        {
            ActivateLvl4Walls();
        }
    }

    //doors would be active linking tiles, just add doors on sides that don t have doors yet
    void ActivateLvl4Walls()
    {
        if (_debugPrints)
            myTileGeneration.PrintToLog("Activating Walls for walled levels...",false);

        foreach (Tile t in myTileGeneration._allActiveTiles)
        {
            if (t.tileStatus != Tile.TileStatus.startingRoom)
            {
                GameObject wall;
                    
                if (t.doors[0] != null && t.upNeighbor != null && t.upNeighbor.doors[1] != null && t.doors[0] != t.upNeighbor.doors[1])
                {
                    if(t.doors[0].tag == "Door" && t.upNeighbor.doors[1].tag == "Door")
                    {
                        t.ReSyncDoors();
                    }

                    if (t.upNeighbor.tileStatus != Tile.TileStatus.startingRoom)
                    {
                        if ((t.doors[0].tag == "Door" && t.doors[0].GetComponent<DoorBehavior>().notInUse) || (t.upNeighbor.doors[1].tag == "Door" && 
                            t.upNeighbor.doors[1].GetComponent<DoorBehavior>().notInUse))
                        {
                            wall = Instantiate(myLevelAsset.levelWall, t.gameObject.transform.position, t.gameObject.transform.rotation);
                            wall.transform.parent = t.transform;
                            wall.transform.localPosition = new Vector3(-12.5f, 5, 0);
                            wall.transform.eulerAngles = new Vector3(-90, 0, -90);
                            wall.name = "WallPlaceholder_LVL4WALLFUNCTION_0";
                            t.doors[0] = wall;
                        }
                    }
                }

                if (t.doors[1] != null && t.downNeighbor != null && t.downNeighbor.doors[0] != null && t.doors[1] != t.downNeighbor.doors[0])
                {
                    if (t.doors[1].tag == "Door" && t.downNeighbor.doors[0].tag == "Door")
                    {
                        t.ReSyncDoors();
                    }

                    if (t.downNeighbor.tileStatus != Tile.TileStatus.startingRoom)
                    {
                        if ((t.doors[1].tag == "Door" && t.doors[1].GetComponent<DoorBehavior>().notInUse) || (t.downNeighbor.doors[0].tag == "Door" && 
                            t.downNeighbor.doors[0].GetComponent<DoorBehavior>().notInUse))
                        {
                            wall = Instantiate(myLevelAsset.levelWall, t.gameObject.transform.position, t.gameObject.transform.rotation);
                            wall.transform.parent = t.transform;
                            wall.transform.localPosition = new Vector3(12.5f, 5, 0);
                            wall.transform.eulerAngles = new Vector3(-90, 0, 90);
                            wall.name = "WallPlaceholder_LVL4WALLFUNCTION_1";
                            t.doors[1] = wall;
                        }
                    }
                }

                if (t.doors[2] != null && t.leftNeighbor != null && t.leftNeighbor.doors[3] != null && t.doors[2] != t.leftNeighbor.doors[3])
                {
                    if (t.doors[2].tag == "Door" && t.leftNeighbor.doors[3].tag == "Door")
                    {
                        t.ReSyncDoors();
                    }

                    if (t.leftNeighbor.tileStatus != Tile.TileStatus.startingRoom)
                    {
                        if ((t.doors[2].tag == "Door" && t.doors[2].GetComponent<DoorBehavior>().notInUse) || (t.leftNeighbor.doors[3].tag == "Door" && 
                            t.leftNeighbor.doors[3].GetComponent<DoorBehavior>().notInUse))
                        {
                            wall = Instantiate(myLevelAsset.levelWall, t.gameObject.transform.position, t.gameObject.transform.rotation);
                            wall.transform.parent = t.transform;
                            wall.transform.localPosition = new Vector3(0, 5, -12.5f);
                            wall.transform.eulerAngles = new Vector3(-90, 0, -180);
                            wall.name = "WallPlaceholder_LVL4WALLFUNCTION_2";
                            t.doors[2] = wall;
                        }
                    }
                }

                if (t.doors[3] != null && t.rightNeighbor != null && t.rightNeighbor.doors[2] != null && t.doors[3] != t.rightNeighbor.doors[2]) 
                {
                    if (t.doors[3].tag == "Door" && t.rightNeighbor.doors[2].tag == "Door")
                    {
                        t.ReSyncDoors();
                    }

                    if (t.rightNeighbor.tileStatus != Tile.TileStatus.startingRoom)
                    {
                        if ((t.doors[3].tag == "Door" && t.doors[3].GetComponent<DoorBehavior>().notInUse) || (t.rightNeighbor.doors[2].tag == "Door" && 
                            t.rightNeighbor.doors[2].GetComponent<DoorBehavior>().notInUse))
                        {
                            wall = Instantiate(myLevelAsset.levelWall, t.gameObject.transform.position, t.gameObject.transform.rotation);
                            wall.transform.parent = t.transform;
                            wall.transform.localPosition = new Vector3(0, 5, 12.5f);
                            wall.transform.eulerAngles = new Vector3(-90, 0, 0);
                            wall.name = "WallPlaceholder_LVL4WALLFUNCTION_3";
                            t.doors[3] = wall;
                        }
                    }
                }
            }
        }

        if (_debugPrints)
            myTileGeneration.PrintToLog("Synching Big Tiles....",false);

        //remove access walls on big tiles
        foreach (GameObject tile in _bigTilesList)
        {
            //child 0 should not have wall on right side or down side
            //child 1 should not have wall on left side or down side
            //child 2 should not have wall on left side or up side
            //child 3 should not have wall on right side or up side
            Tile tileSub;
            tileSub = tile.transform.GetChild(0).GetComponent<Tile>();

            if (tileSub.doors[1] != null)//== myLevelAsset.levelWall)
                Destroy(tileSub.doors[1]);
            if (tileSub.doors[3] != null)// == myLevelAsset.levelWall)
                Destroy(tileSub.doors[3]);
            tileSub = tile.transform.GetChild(1).GetComponent<Tile>();

            if (tileSub.doors[1] != null)//== myLevelAsset.levelWall)
                Destroy(tileSub.doors[1]);
            if (tileSub.doors[2] != null)//== myLevelAsset.levelWall)
                Destroy(tileSub.doors[2]);
            tileSub = tile.transform.GetChild(2).GetComponent<Tile>();

           if (tileSub.doors[0] != null)//== myLevelAsset.levelWall)
                Destroy(tileSub.doors[0]);
            if (tileSub.doors[2] != null)//== myLevelAsset.levelWall)
                Destroy(tileSub.doors[2]);
            tileSub = tile.transform.GetChild(3).GetComponent<Tile>();

            if (tileSub.doors[0] != null) //== myLevelAsset.levelWall)
                Destroy(tileSub.doors[0]);
            if (tileSub.doors[3] != null) //== myLevelAsset.levelWall)
                Destroy(tileSub.doors[3]);  
        }
    }

    //unity can be weird and having a slight delay on the player positive makes the timing easier to account for
    IEnumerator setPlayerPosition(GameObject playerObj, Vector3 spawnPos)
    {
        Player.Instance.rb.detectCollisions = false;
        Player.Instance.rb.useGravity = false;
        yield return new WaitForSeconds(0.1f);
        Player.Instance.transform.position = spawnPos;
        Player.Instance.spawnPoint = spawnPos;
        Player.Instance.rb.detectCollisions = true;
        Player.Instance.rb.useGravity = true;
    }
    
    void ActivateSecretRoom()
    {
        if (_debugPrints)
            myTileGeneration.PrintToLog("St up Secret Room....",false);

        //picks secret room
        //spawn it in AT THE SAME ROTATION OF THE SECRETROOM GAMEOBJECT
        GameObject preset = null;
        preset = Instantiate(myLevelAsset.secretRoomAssets[Random.Range(0, myLevelAsset.secretRoomAssets.Count)], myTileGeneration.secretRoom.transform.position, myTileGeneration.secretRoom.transform.rotation);
        preset.transform.parent = myTileGeneration.secretRoom.transform;

        //rotate asset properly
        Quaternion assetRot = new Quaternion(0, 0, 0, 0);
        Tile secret = myTileGeneration.secretRoom.GetComponent<Tile>();
        //replace wall on neighbor
        if (secret.upNeighbor != null)
        {
            //assetRot = new Quaternion(secret.transform.rotation.x, secret.transform.rotation.y + 90, secret.transform.rotation.z, secret.transform.rotation.w);
            preset.transform.localEulerAngles = new Vector3(preset.transform.localEulerAngles.x, preset.transform.localEulerAngles.y + 90, preset.transform.localEulerAngles.z);
            Destroy(secret.upNeighbor.doors[1]);
        }
        else if(secret.downNeighbor != null)
        {
            //assetRot = new Quaternion(secret.transform.rotation.x, secret.transform.rotation.y - 90, secret.transform.rotation.z, secret.transform.rotation.w);
            preset.transform.localEulerAngles = new Vector3(preset.transform.localEulerAngles.x, preset.transform.localEulerAngles.y - 90, preset.transform.localEulerAngles.z);
            Destroy(secret.downNeighbor.doors[0]);
        }
        else if(secret.leftNeighbor != null)
        {
            //assetRot = new Quaternion(secret.transform.rotation.x, secret.transform.rotation.y, secret.transform.rotation.z, secret.transform.rotation.w);
           // preset.transform.localEulerAngles = new Vector3(preset.transform.localEulerAngles.x, preset.transform.localEulerAngles.y + 90, preset.transform.localEulerAngles.z);
            Destroy(secret.leftNeighbor.doors[3]);

        }
        else if (secret.rightNeighbor != null)
        {
            //assetRot = new Quaternion(secret.transform.rotation.x, secret.transform.rotation.y + 180, secret.transform.rotation.z, secret.transform.rotation.w);
            preset.transform.localEulerAngles = new Vector3(preset.transform.localEulerAngles.x, preset.transform.localEulerAngles.y + 180, preset.transform.localEulerAngles.z);
            Destroy(secret.rightNeighbor.doors[2]);
        }
        //preset.transform.rotation = assetRot;

        myTileGeneration.secretRoom.layer = 22;

        if (preset.TryGetComponent<PresetTileInfo>(out PresetTileInfo mPresetTileInfo))
        {
            for (int posResourceCount = 0; posResourceCount < mPresetTileInfo.possiblePresetItems.Length; posResourceCount++)
            {
                _possibleItems.Add(mPresetTileInfo.possiblePresetItems[posResourceCount]);
            }
            for (int posEnemyCount = 0; posEnemyCount < mPresetTileInfo.enemiesOnPreset.Length; posEnemyCount++)
            {
                _possibleEnemiesInLevel.Add(mPresetTileInfo.enemiesOnPreset[posEnemyCount]);
            }
        }
    }

    #region Objective Spawn
    //=============================================================================================================
    //=============================================================================================================
    /// <summary>
    /// - objective will spawn on boss tile
    /// - if final tile is on a 2 x 2, get that tile and spawn an objective from it
    ///  - UNSURE: if this should be what picks the objective or if localLevel handles that
    ///
    /// </summary>
    public void ActivateObjectives()
    {
        if (_debugPrints)
            myTileGeneration.PrintToLog("Activating Objectives....",false);

        if (myLocalLevel.thisLevelTier != levelTier.level4)
        {
            parents = new List<GameObject>();
            //picks random objective in awake in LocalLevel script
            if (myLocalLevel.objective != 1)
            {
                GameObject obj = Objectives.Instance.SetObjectiveRef(myLocalLevel.objective, endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn).gameObject;
                obj.transform.rotation = playerSpawn.transform.rotation;
                obj.transform.parent = endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn.transform.parent;

                _possibleObjectives.Remove(endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn);
                objectivesInLevel.Add(obj);

                //based on objective, we may need to get some more objectives throughout level. Will randomly pick 2 more (if there are not 2 more then just add whatever is available (so 1))
                //if objective is certain types (ie type 3), choose more objectives and add to list
                
                //make sure we can spawn 2 more objectives!
                for (int objCount = 0; objCount < 2; objCount++)
                {
                    //if this item is null, need to get another one

                    //randomly pick an objective (or item?)
                    _possibleObjectives = reshuffle(_possibleObjectives);

                    //run check to see if this is an adequate location to use, otherwise rechoose
                    for (int indexO = 0; indexO < _possibleObjectives.Count; indexO++)
                    {
                        if (_possibleObjectives[indexO])
                        {
                            GameObject objMulti = Objectives.Instance.SetObjectiveRef(myLocalLevel.objective, _possibleObjectives[indexO]).gameObject;

                            objMulti.transform.rotation = playerSpawn.transform.rotation;
                            objMulti.transform.parent = _possibleObjectives[indexO].transform.parent;
                            parents.Add(objMulti.transform.parent.parent.gameObject);
                            objectivesInLevel.Add(objMulti);

                            //Destroy(_possibleObjectives[indexO]);
                            GameObject tempToDelete = _possibleObjectives[indexO];

                            _possibleObjectives.Remove(_possibleObjectives[indexO]);
                            Destroy(tempToDelete);
                            break;
                        }
                    }
                }
            }
            else if (myLocalLevel.objective == 1)
            {
                //spawn in enemy
                Objectives.Instance.SetObjectiveRef(myLocalLevel.objective, null);
                _possibleObjectives.Remove(endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn);

                GameObject objectiveEnemy = objectiveEnemy = myLevelAsset.EnemyMINIBOSSPrefab.dozerEnemy;
                int choice = Random.Range(1, 3);
                switch (myLocalLevel.thisLevelTier)
                {
                    case levelTier.level1:
                        switch (choice)
                        {
                            case 1:
                                objectiveEnemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                break;
                            case 2:
                                objectiveEnemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                break;
                            case 3:
                                objectiveEnemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                break;
                            default:
                                break;
                        }
                        break;
                    case levelTier.level2:
                        switch (choice)
                        {
                            case 1:
                                objectiveEnemy = myLevelAsset.EnemyMINIBOSSPrefab.stalkerEnemy;
                                break;
                            case 2:
                                objectiveEnemy = myLevelAsset.EnemyMINIBOSSPrefab.wardenEnemy;
                                break;
                            case 3:
                                objectiveEnemy = myLevelAsset.EnemyMINIBOSSPrefab.spitterEnemy;
                                break;
                            default:
                                break;
                        }
                        break;
                    case levelTier.level3:
                        switch (choice)
                        {
                            case 1:
                                objectiveEnemy = myLevelAsset.EnemyMINIBOSSPrefab.shadowEnemy;
                                break;
                            case 2:
                                objectiveEnemy = myLevelAsset.EnemyMINIBOSSPrefab.fuglyEnemy;
                                break;
                            case 3:
                                objectiveEnemy = myLevelAsset.EnemyMINIBOSSPrefab.dozerEnemy;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                
                GameObject enemy = Instantiate(objectiveEnemy, endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn.transform);
                enemy.GetComponent<BaseEnemy>().isObjectiveEnemy = true;
                enemy.name = "OBJECTIVE_ENEMY";
                enemy.transform.parent = endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn.transform.parent;
                enemiesInLevel.Add(enemy);
            }
        }
        else
        {
            Objectives.Instance.UpdateFinalObjective(3);
            //FOR FINAL LEVEL, ONLY ONE OBJECTIVE, LOCATE LAST ARRAY
            Objectives.Instance.SetObjectiveRef(myLocalLevel.objective, null);
            //spawns in a placeholder detection, when player gets within range of this obj, objective changes to survive and boss spawns. (after boss dies then objective changes to activate last array)
            endObjTile.GetComponent<Boss_PresetTileInfo>().lastArrayInteractable.transform.rotation = playerSpawn.transform.rotation;
            endObjTile.GetComponent<Boss_PresetTileInfo>().craftingTableOutside.transform.rotation = playerSpawn.transform.rotation;
            Vector3 rot = new Vector3(endObjTile.GetComponent<Boss_PresetTileInfo>().textCanvas.transform.eulerAngles.x, playerSpawn.transform.eulerAngles.y, endObjTile.GetComponent<Boss_PresetTileInfo>().textCanvas.transform.eulerAngles.z);
            
            endObjTile.GetComponent<Boss_PresetTileInfo>().textCanvas.transform.eulerAngles = rot;
            //REMEMBER, to make sure  the player can complete objective, this has to be turned on after boss dies
            endObjTile.GetComponent<Boss_PresetTileInfo>().lastArrayInteractable.GetComponent<BoxCollider>().isTrigger = false;
            GameObject bossDetection = Instantiate(myLevelAsset.bossDetection, endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn.transform);
            bossDetection.GetComponent<BossSpawn>().tile = endObjTile.GetComponent<Boss_PresetTileInfo>();

            _possibleObjectives.Remove(endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn);
            bossDetection.name = "BossPlaceholder";
            bossDetection.transform.parent = endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn.transform.parent;
        }
    }

    //=============================================================================================================
    //=============================================================================================================
    #endregion

    #region Preset Spawning
    //=============================================================================================================
    //=============================================================================================================
    /// <summary>
    /// - Analyze Tile, looking at an individual tile, for asset choosing and spawning
    /// - also checks if 4 tiles can be linked, links them if they are and decide weather to use this link to spawn big asset
    /// </summary>
    /// <param name="tile">Tile being analyzed</param>

    void AnalyzeTile(Tile tile)
    {
        //will first see if we can link tiles
        _tArray = new Tile[4];
        List<GameObject> bigTileDoors = new List<GameObject>();
        bool obj = false;
        //check neighbors, first up, then left, then right then down, a little unoptimized
        if (!tile.checkFor4Some)
        {
            Tile t;
            if (tile.upNeighbor != null && tile.upNeighbor.tileStatus != Tile.TileStatus.nullRoom && !tile.upNeighbor.checkFor4Some && 
                tile.upNeighbor.tileStatus != Tile.TileStatus.secretRoom)
            {

                bigTileDoors.Add(tile.doors[0]);
                t = tile.upNeighbor;
                
                _tArray[0] = t;
                if (t.rightNeighbor != null && t.rightNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.rightNeighbor.checkFor4Some && 
                    t.rightNeighbor.tileStatus != Tile.TileStatus.secretRoom)
                {
                    bigTileDoors.Add(t.doors[3]);
                    t = t.rightNeighbor;
                    _tArray[1] = t;
                    if (t.downNeighbor != null && t.downNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.downNeighbor.checkFor4Some && 
                        t.downNeighbor.tileStatus != Tile.TileStatus.secretRoom)
                    {
                        bigTileDoors.Add(t.doors[1]);
                        t = t.downNeighbor;
                        _tArray[2] = t;
                        if (t.leftNeighbor != null && t.leftNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.leftNeighbor.checkFor4Some && 
                            t.leftNeighbor.tileStatus != Tile.TileStatus.secretRoom)
                        {
                            bigTileDoors.Add(t.doors[2]);
                            //all of these tiles can be linked
                            t = t.leftNeighbor;
                            _tArray[3] = t;

                            //random chance we don t use this 4 some tile and have original be single
                            if (Random.value <= twoBYtwo_SpawnChance)
                            {
                                fourSomeCount++;
                                GameObject fourSomeTile = new GameObject("BigTile_" + fourSomeCount);
                                _bigTilesList.Add(fourSomeTile);
                                _av = Vector3.zero;
                                fourSomeTile.transform.parent = this.transform;

                                bool canSpawnMiniBoss = false;

                                foreach (Tile tile2 in _tArray)
                                {
                                    _av += tile2.transform.position;
                                }
                                _av = _av / 4;
                                foreach(GameObject door in bigTileDoors)
                                {
                                    Destroy(door);
                                }

                                fourSomeTile.transform.position = _av;
                                foreach (Tile tile3 in _tArray)
                                {
                                    //could remove doors/walls here as well

                                    tile3.levelAssetPlaced = true;

                                    if(tile3.tileStatus == Tile.TileStatus.boss)
                                    {
                                        obj = true;
                                    }

                                    if (tile3.presetNum != -1)
                                    {
                                        //remove its possible items from _possileItems if it has already assigned preset tile
                                        assetCountArray[tile3.presetNum] -= 1;
                                        if (tile3.presetTile.TryGetComponent<PresetTileInfo>(out PresetTileInfo mPresetTileInfo))
                                        {
                                            foreach (GameObject item in mPresetTileInfo.GetComponent<PresetTileInfo>().possiblePresetItems)
                                            {
                                                _possibleItems.Remove(item);
                                                _possibleObjectives.Remove(item);
                                            }

                                            //Note to self maybe consider running for loop to remove possible enemies as wells - Added last night 3/10
                                            foreach (GameObject enemy in mPresetTileInfo.GetComponent<PresetTileInfo>().enemiesOnPreset)
                                            {
                                                _possibleEnemiesInLevel.Remove(enemy);
                                                if (miniBossesInLevel.Remove(enemy))
                                                {
                                                    currentMiniBossCount--;
                                                    possibleminiBossCount--;
                                                }
                                            }

                                            if (mPresetTileInfo.objectiveSpawn != null)
                                            {
                                                _possibleObjectives.Remove(mPresetTileInfo.objectiveSpawn);
                                            }

                                            _possibleTileObjectivesInLevel.Remove(tile3.presetTile);
                                        }
                                        if (tile3.endOfBranchPath)
                                        {
                                            canSpawnMiniBoss = true;
                                        }
                                    }

                                    if (tile3.presetTile != null)
                                    {
                                        Destroy(tile3.presetTile.gameObject);
                                    }
                                    tile3.checkFor4Some = true;
                                    tile3.transform.parent = fourSomeTile.transform;
                                }
                                SpawnLevelBigAsset(fourSomeTile, obj, canSpawnMiniBoss);
                                return;
                            }
                        }
                        else
                        {
                            _tArray[0] = null;
                            _tArray[1] = null;
                            _tArray[2] = null;
                        }
                    }
                    else
                    {
                        _tArray[0] = null;
                        _tArray[1] = null;
                    }
                }
                else
                {
                    _tArray[0] = null;
                }
            }
        }
            if (!tile.levelAssetPlaced )
            {
                SpawnLevelSmallAsset(tile);
            }
    }

    void AnalyzeTile_Lvl4Modifier(Tile tile)
    {
        //will first see if we can link tiles
        _tArray = new Tile[4];
        List<GameObject> bigTileDoors = new List<GameObject>();
        bool obj = false;
        //check neighbors, first up, then left, then right then down
        if (!tile.checkFor4Some && tile.tileStatus != Tile.TileStatus.boss)
        {
            Tile t;
            if (tile.upNeighbor != null && tile.upNeighbor.tileStatus != Tile.TileStatus.nullRoom && !tile.upNeighbor.checkFor4Some && tile.upNeighbor.tileStatus 
                != Tile.TileStatus.secretRoom && tile.upNeighbor.tileStatus != Tile.TileStatus.boss)
            {
                bigTileDoors.Add(tile.doors[0]);
                t = tile.upNeighbor;

                _tArray[0] = t;
                if (t.rightNeighbor != null && t.rightNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.rightNeighbor.checkFor4Some && t.rightNeighbor.tileStatus
                    != Tile.TileStatus.secretRoom && t.rightNeighbor.tileStatus != Tile.TileStatus.boss)
                {
                    bigTileDoors.Add(t.doors[3]);
                    t = t.rightNeighbor;
                    _tArray[1] = t;
                    if (t.downNeighbor != null && t.downNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.downNeighbor.checkFor4Some && t.downNeighbor.tileStatus
                        != Tile.TileStatus.secretRoom && t.downNeighbor.tileStatus != Tile.TileStatus.boss)
                    {
                        bigTileDoors.Add(t.doors[1]);
                        t = t.downNeighbor;
                        _tArray[2] = t;
                        if (t.leftNeighbor != null && t.leftNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.leftNeighbor.checkFor4Some && t.leftNeighbor.tileStatus
                            != Tile.TileStatus.secretRoom && t.leftNeighbor.tileStatus != Tile.TileStatus.boss)
                        {
                            bigTileDoors.Add(t.doors[2]);
                            //all of these tiles can be linked
                            t = t.leftNeighbor;
                            _tArray[3] = t;

                            //random chance we don t use this 4 some tile and have original be single
                            if (Random.value <= twoBYtwo_SpawnChance)
                            {
                                fourSomeCount++;
                                GameObject fourSomeTile = new GameObject("BigTile_" + fourSomeCount);
                                _bigTilesList.Add(fourSomeTile);
                                _av = Vector3.zero;
                                fourSomeTile.transform.parent = this.transform;

                                bool canSpawnMiniboss = false;

                                foreach (Tile tile2 in _tArray)
                                {
                                    _av += tile2.transform.position;
                                }
                                _av = _av / 4;
                                foreach (GameObject door in bigTileDoors)
                                {
                                    Destroy(door);
                                }

                                fourSomeTile.transform.position = _av;
                                foreach (Tile tile3 in _tArray)
                                {
                                    tile3.levelAssetPlaced = true;

                                    if (tile3.tileStatus == Tile.TileStatus.boss)
                                    {
                                        obj = true;
                                    }

                                    if (tile3.presetNum != -1)
                                    {
                                        //remove its possible items from _possileItems if it has already assigned preset tile
                                        assetCountArray[tile3.presetNum] -= 1;
                                        if (tile3.presetTile.TryGetComponent<PresetTileInfo>(out PresetTileInfo mPresetTileInfo))
                                        {
                                            foreach (GameObject item in mPresetTileInfo.GetComponent<PresetTileInfo>().possiblePresetItems)
                                            {
                                                _possibleItems.Remove(item);
                                                _possibleObjectives.Remove(item);
                                            }

                                            //Note to self maybe consider running for loop to remove possible enemies as wells - Added last night 3/10
                                            foreach (GameObject enemy in mPresetTileInfo.GetComponent<PresetTileInfo>().enemiesOnPreset)
                                            {
                                                _possibleEnemiesInLevel.Remove(enemy);
                                                if (miniBossesInLevel.Remove(enemy))
                                                {
                                                    currentMiniBossCount--;
                                                    possibleminiBossCount--;
                                                }
                                            }

                                            if (mPresetTileInfo.objectiveSpawn != null)
                                            {
                                                _possibleObjectives.Remove(mPresetTileInfo.objectiveSpawn);
                                            }
                                            
                                            _possibleTileObjectivesInLevel.Remove(tile3.presetTile);
                                        }
                                    }

                                    if (tile3.presetTile != null)
                                    {
                                        Destroy(tile3.presetTile.gameObject);
                                    }
                                    if(tile3.endOfBranchPath)
                                    {
                                        canSpawnMiniboss = true;
                                    }

                                    tile3.checkFor4Some = true;
                                    tile3.transform.parent = fourSomeTile.transform;
                                }
                                SpawnLevelBigAsset(fourSomeTile, obj, canSpawnMiniboss);
                                return;
                            }
                        }
                        else
                        {
                            _tArray[0] = null;
                            _tArray[1] = null;
                            _tArray[2] = null;
                        }
                    }
                    else
                    {
                        _tArray[0] = null;
                        _tArray[1] = null;
                    }
                }
                else
                {
                    _tArray[0] = null;
                }
            }
        }
        if (!tile.levelAssetPlaced)
        {
            SpawnLevelSmallAsset(tile);
        }
    }

    /// <summary>
    /// - spawn level asset, called in analyze tile. Spawns in and adds to assetCount array
    /// - will determine position and rotation of where asset goes on tile
    /// </summary>
    /// <param name="tile"> Tile being analyzed and spawned on </param>
    void SpawnLevelSmallAsset(Tile tile)
    {
        //will pick the least used preset but for now it will be random
        int index = FindLowestMagnatudeSMOL();//Random.Range(0, myLevelAsset.presetTileAssets.Count);

        GameObject preset = null;
        if (tile.tileStatus != Tile.TileStatus.boss)
        {
            preset = Instantiate(myLocalLevel.presetTileAssets[index], tile.transform.position, myLocalLevel.presetTileAssets[index].transform.rotation);
            tile.presetNum = index;
            preset.transform.parent = tile.transform.parent;
            tile.presetTile = preset;
            assetCountArray[index] += 1;

            if (tile.tileStatus != Tile.TileStatus.startingRoom)
            {
                int choice = Random.Range(1, 4);
                switch (choice)
                {
                    case 1:
                        //local angle = 0
                        tile.presetTile.transform.localEulerAngles = new Vector3(0, 0, 0);
                        break;
                    case 2:
                        tile.presetTile.transform.localEulerAngles = new Vector3(0, 90, 0);
                        //local angle = 90
                        break;
                    case 3:
                        tile.presetTile.transform.localEulerAngles = new Vector3(0, 180, 0);
                        //local angle = 180;
                        break;
                    case 4:
                        tile.presetTile.transform.localEulerAngles = new Vector3(0, 270, 0);
                        //local angle = 270;
                        break;
                    default:
                        break;
                }
            }

            //check for miniboss
            if (currentMiniBossCount <= _miniBossCap && tile.endOfBranchPath)
            {
                if(preset.GetComponent<PresetTileInfo>().enemiesOnPreset.Length > 0)
                {
                    //if enemies can be picked from pick one
                    int indexEnemies = Random.Range(0, preset.GetComponent<PresetTileInfo>().enemiesOnPreset.Length);
                    preset.GetComponent<PresetTileInfo>().enemiesOnPreset[indexEnemies].GetComponent<PossibleEnemy>().canBeMiniBoss = true;
                    _possibleEnemiesInLevel.Remove(preset.GetComponent<PresetTileInfo>().enemiesOnPreset[indexEnemies]);
                    possibleminiBossCount++;
                }
            }
            tile.levelAssetPlaced = true;
        }
        else
        {
            GameObject tileObj = myLocalLevel.presetObjectiveTiles[Random.Range(0, myLocalLevel.presetObjectiveTiles.Count)];
            //if  this tile is the final room, make sure it has an objective
            if (myLocalLevel.thisLevelTier == levelTier.level4)
            {
                tileObj = myLevelAsset.bossTileLastArray;
            }
            Vector3 rotation = Vector3.zero;
            //rotate tile based on if neighbor
            if(tile.upNeighbor != null && tile.upNeighbor.pathNumber == tile.pathNumber - 1)
            {
                //unused atm, found more optimal way, this will remain here until I can verify 
            }
            else if(tile.downNeighbor != null && tile.downNeighbor.pathNumber == tile.pathNumber - 1)
            {
                rotation = new Vector3(tile.transform.localEulerAngles.x, 180, tile.transform.localEulerAngles.z);
            }
            else if(tile.leftNeighbor != null && tile.leftNeighbor.pathNumber == tile.pathNumber - 1)
            {
                rotation = new Vector3(tile.transform.localEulerAngles.x, -90, tile.transform.localEulerAngles.z);
            }
            else if(tile.rightNeighbor != null && tile.rightNeighbor.pathNumber == tile.pathNumber - 1)
            {
                rotation = new Vector3(tile.transform.localEulerAngles.x, 90, tile.transform.localEulerAngles.z);
            }

            preset = Instantiate(tileObj, tile.transform.position, tile.transform.rotation);
            
            preset.transform.parent = tile.transform.parent;
            preset.transform.localEulerAngles = rotation;
            tile.presetTile = preset;
            int indexOfRandomTile = Random.Range(0, myLocalLevel.presetObjectiveTiles.Count);
            int tileIndex = myLocalLevel.presetTileAssets.IndexOf(myLocalLevel.presetObjectiveTiles[indexOfRandomTile]);
            //Debug.Log(indexOfRandomTile);
            assetCountArray[tileIndex] += 1;
            tile.presetNum = tileIndex;
            tile.levelAssetPlaced = true;
            endObjTile = preset;
          
            //FINAL ROOM OBJECTIVE PLACED
        }

        //Check this tile for any possible locations of resources AND ENEMIES
        //add those spots to the possible resources in level
        //later we can go through as activate these resources (maybe look at how far each of is from other active ones to ensure mass distribution - no clusters of resources)
        if (preset.TryGetComponent<PresetTileInfo>(out PresetTileInfo mPresetTileInfo))
        {
            for (int posResourceCount = 0; posResourceCount < mPresetTileInfo.possiblePresetItems.Length; posResourceCount++)
            {
                _possibleItems.Add(mPresetTileInfo.possiblePresetItems[posResourceCount]);
            }
            for (int posEnemyCount = 0; posEnemyCount < mPresetTileInfo.enemiesOnPreset.Length; posEnemyCount++)
            {
                _possibleEnemiesInLevel.Add(mPresetTileInfo.enemiesOnPreset[posEnemyCount]);
            }
            if(mPresetTileInfo.objectiveSpawn != null)
            {
                _possibleTileObjectivesInLevel.Add(preset);
                _possibleObjectives.Add(mPresetTileInfo.objectiveSpawn);
            }
        }
    }

    /// <summary>
    /// - spawns bigger 4 tile asset
    /// </summary>
    /// <param name="bigTile"> The parent of the 4 linked tiles being analyzed and spawned on. </param>
    void SpawnLevelBigAsset(GameObject bigTile, bool hasObj, bool canSpawnMiniBoss)
    {
        //will pick the least used preset but for now it will be random
        int index = Random.Range(0, myLocalLevel.presetBigTileAssets.Count);
        GameObject preset;
        if (!hasObj)
        {
            preset = Instantiate(myLocalLevel.presetBigTileAssets[index], bigTile.transform.position, bigTile.transform.rotation);
            preset.transform.parent = bigTile.transform;

            bigAssetCountArray[index] += 1;
        }
        else
        {
            preset = Instantiate(myLocalLevel.presetBigObjectiveTiles[Random.Range(0, myLocalLevel.presetBigObjectiveTiles.Count)], bigTile.transform.position, bigTile.transform.rotation);
            preset.transform.parent = bigTile.transform;

            bigAssetCountArray[1] += 1;
            endObjTile = preset;
            if (_debugPrints)
                myTileGeneration.PrintToLog("BIG ASSET WITH OBJ",false);
        }

        //random rotation on tile set
        int choice = Random.Range(1, 4);

        switch (choice)
        {
            case 1:
                //local angle = 0
                preset.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case 2:
                preset.transform.localEulerAngles = new Vector3(0, 90, 0);
                //local angle = 90
                break;
            case 3:
                preset.transform.localEulerAngles = new Vector3(0, 180, 0);
                //local angle = 180;
                break;
            case 4:
                preset.transform.localEulerAngles = new Vector3(0, 270, 0);
                //local angle = 270;
                break;
            default:
                break;
        }

        if (canSpawnMiniBoss)
        {
            if (currentMiniBossCount <= _miniBossCap && preset.GetComponent<PresetTileInfo>().enemiesOnPreset.Length > 0)
            {
                //if enemies can be picked from pick one
                int indexEnemies = Random.Range(0, preset.GetComponent<PresetTileInfo>().enemiesOnPreset.Length);
                preset.GetComponent<PresetTileInfo>().enemiesOnPreset[indexEnemies].GetComponent<PossibleEnemy>().canBeMiniBoss = true;
               // miniBossesInLevel.Add(preset.GetComponent<PresetTileInfo>().enemiesOnPreset[indexEnemies]);
                _possibleEnemiesInLevel.Remove(preset.GetComponent<PresetTileInfo>().enemiesOnPreset[indexEnemies]);
                possibleminiBossCount++;
            }
        }

        if (preset.TryGetComponent<PresetTileInfo>(out PresetTileInfo mPresetTileInfo))
        {
            for (int posResourceCount = 0; posResourceCount < mPresetTileInfo.possiblePresetItems.Length; posResourceCount++)
            {
                _possibleItems.Add(mPresetTileInfo.possiblePresetItems[posResourceCount]);
            }
            for(int posEnemyCount = 0; posEnemyCount < mPresetTileInfo.enemiesOnPreset.Length; posEnemyCount++)
            {
                _possibleEnemiesInLevel.Add(mPresetTileInfo.enemiesOnPreset[posEnemyCount]);
            }
            if (mPresetTileInfo.objectiveSpawn != null)
            {
                _possibleTileObjectivesInLevel.Add(preset);
                _possibleObjectives.Add(mPresetTileInfo.objectiveSpawn);
            }
        }
    }

    /// <summary>
    /// Finds the least popular asset or if there are multiple assets at the same number, choose a random one
    ///
    /// CURRENTLY NO MAGATUDE CHECK FOR BIG TILE PRESETS SINCE THEY DONT SPAWN AS MUCH :)
    /// </summary>
    /// <returns> Least Popular asset index to spawn</returns>
    int FindLowestMagnatudeSMOL()
    {
        _magAssetCount = new List<List<int>>();

        for (int firstNumCount = 0; firstNumCount <= 8; firstNumCount++)
        {
            List<int> temp = new List<int>();
            for (int c = 0; c < assetCountArray.Length; c++)
            {
                if (assetCountArray[c] == firstNumCount)
                {
                    temp.Add(c);
                }
            }
            _magAssetCount.Add(temp);
        }

        //start from first num, and see if its empty, if it is move on to next. Else use random index for second num and return this value
        for(int listFinder = 0; listFinder < _magAssetCount.Count; listFinder++)
        {
            if(_magAssetCount[listFinder].Count != 0)
            {
                int ranIndex = Random.Range(0, _magAssetCount[listFinder].Count);
                return _magAssetCount[listFinder][ranIndex];
            }
        }

        return 0;//Random.Range(0, myLevelAsset.presetTileAssets.Count);
    }

    //=============================================================================================================
    //=============================================================================================================
    #endregion

    //reshuffle list
    List<GameObject> reshuffle(List<GameObject> ar)
    {
        for (int t = 0; t < ar.Count; t++)
        {
            GameObject tmp = ar[t];
            int r = Random.Range(t, ar.Count);
            ar[t] = ar[r];
            ar[r] = tmp;
        }
        return ar;
    }

    #region Items
    //=============================================================================================================
    //=============================================================================================================
    void ActivateLevelKey()
    {
        if (_debugPrints)
            myTileGeneration.PrintToLog("Setting up Level Keys....",false);

        int keyInt = Random.Range(0, _possibleItems.Count);

        GameObject keyObj = Instantiate(myLevelAsset.emptyObj, _possibleItems[keyInt].transform.position, playerSpawn.transform.rotation);
        keyObj.GetComponent<WorldItem>().worldItemData = myLevelAsset.keyData;
        keyObj.transform.parent = _possibleItems[keyInt].transform.parent;

        GameObject tempToDelete = _possibleItems[keyInt];
        tempToDelete.GetComponent<PossibleItem>().inUse = true;
        _possibleItems.RemoveAt(keyInt);
        _possibleObjectives.Remove(tempToDelete);
        Destroy(tempToDelete);
        itemsInLevelList.Add(keyObj);
    }

    void ActivateItems()
    {
        if (_debugPrints)
            myTileGeneration.PrintToLog("Activating Items....",false);

        //shuffle _possibleItems
        _possibleItems = reshuffle(_possibleItems);

        switch (myLocalLevel.thisLevelTier)
        {
            case levelTier.level1:
                collectables = tier1CollectableCap;
                break;
            case levelTier.level2:
                collectables = tier2CollectableCap;
                break;
            case levelTier.level3:
                collectables = tier3CollectableCap;
                break;
            case levelTier.level4:
                collectables = tier3CollectableCap;
                break;
            default:
                break;
        }

        //check weight of possible item
        //highly favor that weight but could still spawn other 2 types
        //normal %:
        //resources = 50%
        //item = 30%
        //weapon = 20%
        //weight adds 30 to weapon
        //adds 30 to item
        //adds 30 to resource
        int pItemC;
        for (pItemC = 0; pItemC < _possibleItems.Count && pItemC < collectables; pItemC++)
        {
            if (_possibleItems[pItemC].GetComponent<PossibleItem>().inUse)
            {
                Debug.Log("LevelAssetSpawn: Do not use. Please Change.");
            }
            switch (_possibleItems[pItemC].GetComponent<PossibleItem>().objectWeight)
            {
                case ObjectWeightType.None:
                    //randomly picks one of three types
                    //default %
                    if (Random.value > 0.80)
                    {
                        //15% for weapon
                        int wIndex = Random.Range(0, myLevelAsset.weaponList.Count);
                        GameObject weaponD = Instantiate(myLevelAsset.emptyObj, _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        weaponD.GetComponent<WorldItem>().worldItemData = myLevelAsset.weaponDataList[wIndex];
                        //GameObject weapon = Instantiate(myLevelAsset.weaponList[wIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        //_possibleItems[pItemC].transform.rotation);
                        weaponD.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        weaponsInLevelList.Add(weaponD);
                    }
                    else if (Random.value > 0.7)
                    {
                        //35% for item
                        int iIndex = Random.Range(0, myLevelAsset.itemList.Count);
                        if (iIndex != myLevelAsset.itemList.Count - 1)
                        {
                            GameObject itemD = Instantiate(myLevelAsset.emptyObj, _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                            itemD.GetComponent<WorldItem>().worldItemData = myLevelAsset.itemDataList[iIndex];
                            itemD.transform.parent = _possibleItems[pItemC].transform.parent;
                            itemsInLevelList.Add(itemD);
                        }
                        else
                        {
                            GameObject item = Instantiate(myLevelAsset.itemList[iIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                            //_possibleItems[pItemC].transform.rotation);
                            item.transform.parent = _possibleItems[pItemC].transform.parent;
                            itemsInLevelList.Add(item);
                        }
                        Destroy(_possibleItems[pItemC]);

                    }
                    else // if(Random.value > 0.6)
                    {
                        //50% chance its a resource
                        int rIndex = Random.Range(0, myLevelAsset.resourcesList.Count);
                        GameObject resource = Instantiate(myLevelAsset.resourcesList[rIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        //_possibleItems[pItemC].transform.rotation);
                        resource.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        resourcesInLevelList.Add(resource);
                    }
                    break;
                case ObjectWeightType.Weapon:
                    //weapon = 45
                    //resource = 45
                    //item = 10
                    if (Random.value > 0.55)
                    {
                        int wIndex = Random.Range(0, myLevelAsset.weaponList.Count);
                        GameObject weaponD = Instantiate(myLevelAsset.emptyObj, _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        weaponD.GetComponent<WorldItem>().worldItemData = myLevelAsset.weaponDataList[wIndex];

                        //GameObject weapon = Instantiate(myLevelAsset.weaponList[wIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        //_possibleItems[pItemC].transform.rotation);
                        weaponD.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        weaponsInLevelList.Add(weaponD);
                    }
                    else if (Random.value > 0.55)
                    {
                        int rIndex = Random.Range(0, myLevelAsset.resourcesList.Count);
                        GameObject resource = Instantiate(myLevelAsset.resourcesList[rIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        // _possibleItems[pItemC].transform.rotation);
                        resource.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        resourcesInLevelList.Add(resource);
                    }
                    else // if (Random.value > 0.80)
                    {
                        int iIndex = Random.Range(0, myLevelAsset.itemList.Count);
                        if (iIndex != myLevelAsset.itemList.Count - 1)
                        {
                            GameObject itemD = Instantiate(myLevelAsset.emptyObj, _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                            itemD.GetComponent<WorldItem>().worldItemData = myLevelAsset.itemDataList[iIndex];
                            itemD.transform.parent = _possibleItems[pItemC].transform.parent;
                            itemsInLevelList.Add(itemD);
                        }
                        else
                        {
                            GameObject item = Instantiate(myLevelAsset.itemList[iIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                            //_possibleItems[pItemC].transform.rotation);
                            item.transform.parent = _possibleItems[pItemC].transform.parent;
                            itemsInLevelList.Add(item);
                        }
                        Destroy(_possibleItems[pItemC]);
                    }
                    break;
                case ObjectWeightType.Item:
                    //item = 55
                    //resource = 35
                    //weapon = 10
                    if (Random.value > 0.45)
                    {
                        int iIndex = Random.Range(0, myLevelAsset.itemList.Count);
                        if (iIndex != myLevelAsset.itemList.Count - 1)
                        {
                            GameObject itemD = Instantiate(myLevelAsset.emptyObj, _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                            itemD.GetComponent<WorldItem>().worldItemData = myLevelAsset.itemDataList[iIndex];
                            itemD.transform.parent = _possibleItems[pItemC].transform.parent;
                            itemsInLevelList.Add(itemD);
                        }
                        else
                        {
                            GameObject item = Instantiate(myLevelAsset.itemList[iIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                            //_possibleItems[pItemC].transform.rotation);
                            item.transform.parent = _possibleItems[pItemC].transform.parent;
                            itemsInLevelList.Add(item);
                        }
                        Destroy(_possibleItems[pItemC]);
                    }
                    else if (Random.value > 0.65)
                    {
                        int rIndex = Random.Range(0, myLevelAsset.resourcesList.Count);
                        GameObject resource = Instantiate(myLevelAsset.resourcesList[rIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        //_possibleItems[pItemC].transform.rotation);
                        resource.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        resourcesInLevelList.Add(resource);
                    }
                    else
                    {
                        int wIndex = Random.Range(0, myLevelAsset.weaponList.Count);
                        GameObject weaponD = Instantiate(myLevelAsset.emptyObj, _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        weaponD.GetComponent<WorldItem>().worldItemData = myLevelAsset.weaponDataList[wIndex];

                        //GameObject weapon = Instantiate(myLevelAsset.weaponList[wIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        //_possibleItems[pItemC].transform.rotation);
                        weaponD.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        weaponsInLevelList.Add(weaponD);
                    }
                    break;
                case ObjectWeightType.Resource:
                    //resource = 80
                    //item = 30
                    //weapon = 10
                    if (Random.value > 0.20)
                    {
                        int rIndex = Random.Range(0, myLevelAsset.resourcesList.Count);
                        GameObject resource = Instantiate(myLevelAsset.resourcesList[rIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        //_possibleItems[pItemC].transform.rotation);
                        resource.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        resourcesInLevelList.Add(resource);
                    }
                    else if (Random.value > 0.60)
                    {
                        int iIndex = Random.Range(0, myLevelAsset.itemList.Count);
                        if (iIndex != myLevelAsset.itemList.Count - 1)
                        {
                            GameObject itemD = Instantiate(myLevelAsset.emptyObj, _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                            itemD.GetComponent<WorldItem>().worldItemData = myLevelAsset.itemDataList[iIndex];
                            itemD.transform.parent = _possibleItems[pItemC].transform.parent;
                            itemsInLevelList.Add(itemD);
                        }
                        else
                        {
                            GameObject item = Instantiate(myLevelAsset.itemList[iIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                            //_possibleItems[pItemC].transform.rotation);
                            item.transform.parent = _possibleItems[pItemC].transform.parent;
                            itemsInLevelList.Add(item);
                        }
                        Destroy(_possibleItems[pItemC]);
                    }
                    else
                    {
                        int wIndex = Random.Range(0, myLevelAsset.weaponList.Count);
                        GameObject weaponD = Instantiate(myLevelAsset.emptyObj, _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        weaponD.GetComponent<WorldItem>().worldItemData = myLevelAsset.weaponDataList[wIndex];

                        //GameObject weapon = Instantiate(myLevelAsset.weaponList[wIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);
                        //_possibleItems[pItemC].transform.rotation);
                        weaponD.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        weaponsInLevelList.Add(weaponD);
                    }
                    break;
                default:
                    break;
            }
            Destroy(_possibleItems[pItemC]);
        }
        for (int lastItems = pItemC; lastItems < _possibleItems.Count; lastItems++)
        {
            _possibleObjectives.Remove(_possibleItems[lastItems]);
            Destroy(_possibleItems[lastItems]);
        }
        //resources can either spawn at random or based on distance from any other existing resource
        //when activated that gameobject will be removed from the possibleResources and added to resourcesInLevel List
        //when item is activated can either be a weapon, resource or other item
    }

    //=============================================================================================================
    //=============================================================================================================
    #endregion


    #region Enemies

    void ActivateEnemies()
    {
        if (_debugPrints)
            myTileGeneration.PrintToLog("Activating Enemies....",false);

        _possibleEnemiesInLevel = reshuffle(_possibleEnemiesInLevel);
        if(possibleminiBossCount < _miniBossCap)
        {
            int enemyIndex = Random.Range(0, _possibleEnemiesInLevel.Count);
            _possibleEnemiesInLevel[enemyIndex].GetComponent<PossibleEnemy>().canBeMiniBoss = true;
        }

        switch (myLocalLevel.thisLevelTier)
        {
            case levelTier.level1:
                ActivateEnemiesTier1();
                break;
            case levelTier.level2:
                ActivateEnemiesTier2();
                break;
            case levelTier.level3:
                ActivateEnemiesTier3();
                break;
            case levelTier.level4:
                ActivateEnemiesTier3();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// enemy spawning depends on tier
    /// - each tier has different enemies
    /// - each tier has different % spawn chance for each enemy
    /// - each possible enemy has a predetermined type set
    /// - runs through each possible enemy, determines who should spawn, and if this enemy type is a miniboss
    /// - if a type doest spawn on that tier, sets a random lower difficult type
    /// - mini bosses are 50 - 50 if there are mini spawns to spare
    /// 
    /// NOTE: This can also be better optimized, for future overhauls or adjustments
    ///     - USE TEMPLATE FUNCTION FOR OVERHAUL OF THIS SYSTEM
    ///     - having predetermined enemy types to read may still be more efficient (level designer gets to choose spawn rates, this just reads them)
    /// 
    /// TO DO: condense into one function, takes tier as param
    /// 
    /// </summary>

    //frame work for activation rework
    void ActivateEnemiesT(levelTier thisLevelTier)
    {

        //set stats
        switch (thisLevelTier)
        {
            case levelTier.level1:

                break;
            case levelTier.level2:

                break;
            case levelTier.level3:

                break;
            case levelTier.level4:

                break;
            default:
                break;
        }

        for (enemyCount = 0; enemyCount < _possibleEnemiesInLevel.Count && tier1EnemyCap > enemyCount; enemyCount++)
        {
            if (_possibleEnemiesInLevel[enemyCount].TryGetComponent<PossibleEnemy>(out PossibleEnemy mPossibleEnemy))
            {
                GameObject enemy = myLevelAsset.enemyPrefab.dogEnemy;

                switch (mPossibleEnemy.type)
                {
                    case EnemyWeightType.None:
                        break;
                    case EnemyWeightType.Outcast:
                        break;
                    case EnemyWeightType.Dog:
                        break;
                    case EnemyWeightType.Warden:
                        break;
                    case EnemyWeightType.Stalker:
                        break;
                    case EnemyWeightType.Splitter:
                        break;
                    case EnemyWeightType.Shadow:
                        break;
                    case EnemyWeightType.LostOne:
                        break;
                    case EnemyWeightType.Fugly:
                        break;
                    case EnemyWeightType.Dozer:
                        break;
                    default:
                        break;
                }



                //spawn in this enemy
                enemy = Instantiate(enemy, mPossibleEnemy.transform.position, mPossibleEnemy.transform.rotation);
                enemiesInLevel.Add(enemy);
                enemy.transform.parent = mPossibleEnemy.transform.parent;

                Destroy(_possibleEnemiesInLevel[enemyCount]);
                if (mPossibleEnemy.canBeMiniBoss)
                {
                    enemy.name += "_MINIBOSS";
                    miniBossesInLevel.Add(enemy);
                    currentMiniBossCount++;
                }
                enemy.name += "_" + (enemiesInLevel.Count - 1).ToString();
            }
        }
        for (int deleteCount = enemyCount; deleteCount < _possibleEnemiesInLevel.Count; deleteCount++)
        {
            Destroy(_possibleEnemiesInLevel[deleteCount]);
        }
    }

    void ActivateEnemiesTier1()
    {
        //each level has a range of enemies it spawns
        for(enemyCount = 0; enemyCount < _possibleEnemiesInLevel.Count && tier1EnemyCap > enemyCount; enemyCount++)
        {
            if(_possibleEnemiesInLevel[enemyCount].TryGetComponent<PossibleEnemy>(out PossibleEnemy mPossibleEnemy))
            {
                GameObject enemy = myLevelAsset.enemyPrefab.dogEnemy;
                int choice = 0;
                switch (mPossibleEnemy.type)
                {
                    case EnemyWeightType.None:
                        //picks between outcasts, dog or lost ones
                        choice = Random.Range(0, 3);

                        switch (choice)
                        {
                            case 0:
                                //outcast
                                if (Random.value <= 0.5f)
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                }
                                else
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                }
                                break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.Outcast:
                        if (Random.value <= 0.5f)
                        {
                            if (!mPossibleEnemy.canBeMiniBoss)
                                enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                            else
                                enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                        }
                        else
                        {
                            if (!mPossibleEnemy.canBeMiniBoss)
                                enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                            else
                                enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                        }
                        //tier 1
                        break;
                    case EnemyWeightType.Dog:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.dogEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                        //tier 1
                        break;
                    case EnemyWeightType.Warden:
                        //other types not in tier 1 will default to random tier 1
                        choice = Random.Range(0, 3);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                if (Random.value <= 0.5f)
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                }
                                else
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                };
                                break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.Stalker:
                        choice = Random.Range(0, 3);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                if (Random.value <= 0.5f)
                                    enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                else
                                    enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.Splitter:
                        choice = Random.Range(0, 3);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                if (Random.value <= 0.5f)
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                }
                                else
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                }
                                    break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.Shadow:
                        choice = Random.Range(0, 3);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                if (Random.value <= 0.5f)
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                }
                                else
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                }
                                    break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.LostOne:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                        //tier 1
                        break;
                    case EnemyWeightType.Fugly:
                        choice = Random.Range(0, 3);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                if (Random.value <= 0.5f)
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                }
                                else
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                }
                                break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.Dozer:
                        choice = Random.Range(0, 3);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                if (Random.value <= 0.5f)
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                }
                                else
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                }
                                break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }

                //spawn in this enemy
                enemy = Instantiate(enemy, mPossibleEnemy.transform.position, mPossibleEnemy.transform.rotation);
                enemiesInLevel.Add(enemy);
                enemy.transform.parent = mPossibleEnemy.transform.parent;

                Destroy(_possibleEnemiesInLevel[enemyCount]);
                if (mPossibleEnemy.canBeMiniBoss)
                {
                    enemy.name += "_MINIBOSS";
                    miniBossesInLevel.Add(enemy);
                    currentMiniBossCount++;
                }
                enemy.name += "_" + (enemiesInLevel.Count - 1).ToString();
            }
        }
        for(int deleteCount = enemyCount; deleteCount < _possibleEnemiesInLevel.Count; deleteCount++)
        {
            Destroy(_possibleEnemiesInLevel[deleteCount]);
        }

        //remove extra spots
    }
    void ActivateEnemiesTier2()
    {
        //includes tier 1 plus splitters, wardens and stalkers
        for (enemyCount = 0; enemyCount < _possibleEnemiesInLevel.Count && tier2EnemyCap > enemyCount; enemyCount++)
        {
            if (_possibleEnemiesInLevel[enemyCount].TryGetComponent<PossibleEnemy>(out PossibleEnemy mPossibleEnemy))
            {
                
                GameObject enemy = myLevelAsset.enemyPrefab.dogEnemy;
                int choice = 0;
                switch (mPossibleEnemy.type)
                {
                    case EnemyWeightType.None:
                        //picks between outcasts, dog or lost ones
                        choice = Random.Range(0, 6);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                if (Random.value <= 0.5f)
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                }
                                else
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                }
                                break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            case 3:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.spitterEnemy;
                                break;
                            case 4:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.wardenEnemy;
                                break;
                            case 5:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.stalkerEnemy;
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.Outcast:
                        //enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                        if (Random.value <= 0.5f)
                        {
                            if (!mPossibleEnemy.canBeMiniBoss)
                                enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                            else
                                enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                        }
                        else
                        {
                            if (!mPossibleEnemy.canBeMiniBoss)
                                enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                            else
                                enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                        }
                        //tier 1
                        break;
                    case EnemyWeightType.Dog:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.dogEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                        //tier 1
                        break;
                    case EnemyWeightType.Warden:
                        //other types not in tier 1 will default to random tier 1
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.wardenEnemy;
                        break;
                    case EnemyWeightType.Stalker:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.stalkerEnemy;
                        break;
                    case EnemyWeightType.Splitter:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.spitterEnemy;
                        break;
                    case EnemyWeightType.Shadow:
                        choice = Random.Range(0, 6);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                // enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                if (Random.value <= 0.5f)
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                }
                                else
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                }
                                break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            case 3:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.spitterEnemy;
                                break;
                            case 4:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.wardenEnemy;
                                break;
                            case 5:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.stalkerEnemy;
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.LostOne:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                        //tier 1
                        break;
                    case EnemyWeightType.Fugly:
                        choice = Random.Range(0, 6);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                //enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                if (Random.value <= 0.5f)
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                }
                                else
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                }
                                break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            case 3:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.spitterEnemy;
                                break;
                            case 4:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.wardenEnemy;
                                break;
                            case 5:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.stalkerEnemy;
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.Dozer:
                        choice = Random.Range(0, 6);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                //enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                if (Random.value <= 0.5f)
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                }
                                else
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                }
                                break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            case 3:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.spitterEnemy;
                                break;
                            case 4:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.wardenEnemy;
                                break;
                            case 5:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.stalkerEnemy;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }

                //spawn in this enemy
                enemy = Instantiate(enemy, mPossibleEnemy.transform.position, mPossibleEnemy.transform.rotation);
                enemiesInLevel.Add(enemy);
                enemy.transform.parent = mPossibleEnemy.transform.parent;

                Destroy(_possibleEnemiesInLevel[enemyCount]);
                if (mPossibleEnemy.canBeMiniBoss)
                {
                    //Debug.Log("check");
                    enemy.name += "_MINIBOSS";
                    miniBossesInLevel.Add(enemy);
                    currentMiniBossCount++;
                }
                enemy.name += "_" + (enemiesInLevel.Count - 1).ToString();
            }
        }
        for (int deleteCount = enemyCount; deleteCount < _possibleEnemiesInLevel.Count; deleteCount++)
        {
            Destroy(_possibleEnemiesInLevel[deleteCount]);
        }
    }
    void ActivateEnemiesTier3()
    {
        //all of them
        for (enemyCount = 0; enemyCount < _possibleEnemiesInLevel.Count && tier3EnemyCap > enemyCount; enemyCount++)
        {
            if (_possibleEnemiesInLevel[enemyCount].TryGetComponent<PossibleEnemy>(out PossibleEnemy mPossibleEnemy))
            {
                GameObject enemy = myLevelAsset.enemyPrefab.dogEnemy;
                int choice;
                switch (mPossibleEnemy.type)
                {
                    case EnemyWeightType.None:
                        //picks between outcasts, dog or lost ones
                        choice = Random.Range(0, 9);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                if (Random.value <= 0.5f)
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                                }
                                else
                                {
                                    if (!mPossibleEnemy.canBeMiniBoss)
                                        enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                                    else
                                        enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                                }
                                break;
                            case 1:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            case 3:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.spitterEnemy;
                                break;
                            case 4:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.wardenEnemy;
                                break;
                            case 5:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.stalkerEnemy;
                                break;
                            case 6:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.shadowEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.shadowEnemy;
                                break;
                            case 7:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.fuglyEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.fuglyEnemy;
                                break;
                            case 8:
                                if (!mPossibleEnemy.canBeMiniBoss)
                                    enemy = myLevelAsset.enemyPrefab.dozerEnemy;
                                else
                                    enemy = myLevelAsset.EnemyMINIBOSSPrefab.dozerEnemy;
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.Outcast:
                        if (Random.value <= 0.5f)
                        {
                            if (!mPossibleEnemy.canBeMiniBoss)
                                enemy = myLevelAsset.enemyPrefab.outcastEnemyRanged;
                            else
                                enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyRanged;
                        }
                        else
                        {
                            if (!mPossibleEnemy.canBeMiniBoss)
                                enemy = myLevelAsset.enemyPrefab.outcastEnemyMelee;
                            else
                                enemy = myLevelAsset.EnemyMINIBOSSPrefab.outcastEnemyMelee;
                        }
                        break;
                    case EnemyWeightType.Dog:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.dogEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.dogEnemy;
                        break;
                    case EnemyWeightType.Warden:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.wardenEnemy;
                        break;
                    case EnemyWeightType.Stalker:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.stalkerEnemy;
                        break;
                    case EnemyWeightType.Splitter:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.spitterEnemy;
                        break;
                    case EnemyWeightType.Shadow:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.shadowEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.shadowEnemy;
                        break;
                    case EnemyWeightType.LostOne:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.lostOneEnemy;
                        break;
                    case EnemyWeightType.Fugly:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.fuglyEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.fuglyEnemy;
                        break;
                    case EnemyWeightType.Dozer:
                        if (!mPossibleEnemy.canBeMiniBoss)
                            enemy = myLevelAsset.enemyPrefab.dozerEnemy;
                        else
                            enemy = myLevelAsset.EnemyMINIBOSSPrefab.dozerEnemy;
                        break;
                    default:
                        break;
                }

                //spawn in this enemy
                enemy = Instantiate(enemy, mPossibleEnemy.transform.position, mPossibleEnemy.transform.rotation);
                enemiesInLevel.Add(enemy);
                enemy.transform.parent = mPossibleEnemy.transform.parent;
                Destroy(_possibleEnemiesInLevel[enemyCount]);
                
                if (mPossibleEnemy.canBeMiniBoss)
                {
                    //Debug.Log("check");
                    enemy.name += "_MINIBOSS";
                    miniBossesInLevel.Add(enemy);
                    currentMiniBossCount++;
                }
                enemy.name += "_" + (enemiesInLevel.Count - 1).ToString();
            }
        }
        for (int deleteCount = enemyCount; deleteCount < _possibleEnemiesInLevel.Count; deleteCount++)
        {
            Destroy(_possibleEnemiesInLevel[deleteCount]);
        }
    }

    #endregion
}
