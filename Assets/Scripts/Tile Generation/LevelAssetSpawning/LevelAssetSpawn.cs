using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAssetSpawn : MonoBehaviour
{
    /// <summary>
    /// REQURIES:
    ///     - a LevelInfo Prefab
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
    List<GameObject> _possibleItems = new List<GameObject>();
    [Header("Resources In Level")]
    public List<GameObject> resourcesInLevelList = new List<GameObject>();
    [Header("Items In Level")]
    public List<GameObject> itemsInLevelList = new List<GameObject>();
    [Header("Weapons In Level")]
    public List<GameObject> weaponsInLevelList = new List<GameObject>();

    List<GameObject> _possibleObjectives = new List<GameObject>();

    [Header("Enemies In Level")]
    public List<GameObject> enemiesInLevel = new List<GameObject>();
    List<GameObject> _possibleEnemiesInLevel = new List<GameObject>();

    [Header("number of possible drop prefabs")]
    public int dontSpawnCount = 3;

    GameObject endObjTile;
    [Header("Objectives In Level")]
    public List<GameObject> objectivesInLevel = new List<GameObject>();
    List<GameObject> _possibleTileObjectivesInLevel = new List<GameObject>();

    int tier1EnemyCap = 15;
    int tier2EnemyCap = 22;
    int tier3EnemyCap = 30;
    int tier1CollectableCap = 25;
    int tier2CollectableCap = 20;
    int tier3CollectableCap = 50;

    //first number represents the number of times tiles in that list were spawned
    //second number represents the tile numbers that were spawned that amount of times
    List<List<int>> _magAssetCount;
    [Header("Amount of enemies spawned in level")]
    public int enemyCount;
    
    [Header("Ref to player data obj")]
    public GameObject playerPref;
    [HideInInspector]
    public GameObject playerSpawn;

    private void Awake()
    {
       // Debug.Log("y");
        assetCountArray = new int[myLevelAsset.presetTileAssets.Count];
        bigAssetCountArray = new int[myLevelAsset.presetBigTileAssets.Count];

        //Debug.Log("n");
    }

    private void Start()
    {
       // _myLocalLevel = FindObjectOfType<LocalLevel>();
    }
    /// <summary>
    /// Populate grid with assets, called from TileGeneration once it is done setting up
    /// </summary>
    public void PopulateGrid()
    {
        //Debug.Log("Populating Level with Assets...");

        //activate walls
        foreach (Tile t in myTileGeneration._allActiveTiles)
        {
            t.ActivateWalls();
            AnalyzeTile(t);

            //add starting tile resources
            if(t.tileStatus == Tile.TileStatus.startingRoom)
            {
                for (int posResourceCount = 0; posResourceCount < t.presetTile.GetComponent<PresetTileInfo>().possiblePresetItems.Length; posResourceCount++)
                {
                    //co++;
                    // Debug.Log(co + " " + mPresetTileInfo.possiblePresetItems[posResourceCount].name);
                    _possibleItems.Add(t.presetTile.GetComponent<PresetTileInfo>().possiblePresetItems[posResourceCount]);
                }
                playerSpawn = t.presetTile.GetComponent<PresetTileInfo>().playerSpawn;
                //SPAWN PLAYER
                GameObject play = Instantiate(playerPref, Vector3.zero, playerSpawn.transform.rotation);
                Debug.Log("Player Spawn set");
                StartCoroutine(setPlayerPosition(play, playerSpawn.transform.position));
                
                myLocalLevel.myPlayer = play.transform.GetChild(0).gameObject.GetComponent<Player>();
            }
        }

        //ActivateSecretRoom();

        myLocalLevel.ChooseObjective();
        //ACTIVATE OBJECTIVES
        ActivateObjectives();
        
        //SPAWN IN RESOURCES
        ActivateItems();

        //ACTIVATE ENEMIES
        ActivateEnemies();

        
    }

    IEnumerator setPlayerPosition(GameObject playerObj, Vector3 spawnPos)
    {
        yield return new WaitForSeconds(0.1f);
        Player.Instance.transform.position = spawnPos;
    }

    void ActivateSecretRoom()
    {
        //picks secret room
        //spawn it in AT THE SAME ROTATION OF THE SECRETROOM GAMEOBJECT
        GameObject preset = null;
        preset = Instantiate(myLevelAsset.secretRoomAssets[Random.Range(0, myLevelAsset.secretRoomAssets.Count)], myTileGeneration.secretRoom.transform.position, myTileGeneration.secretRoom.transform.rotation);

        if (preset.TryGetComponent<PresetTileInfo>(out PresetTileInfo mPresetTileInfo))
        {

            // Debug.Log(preset.name);
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

    /// <summary>
    /// - objective will spawn on boss tile
    /// - if final tile is on a 2 x 2, get that tile and spawn an objective from it
    ///  - UNSURE: if this should be what picks the objective or if localLevel handles that
    ///
    /// </summary>
    public void ActivateObjectives()
    {
        //picks random objective in awake in LocalLevel script
        if (myLocalLevel.objective != 1)
        {
            GameObject obj = Objectives.Instance.SetObjectiveRef(myLocalLevel.objective, endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn).gameObject;
            obj.transform.rotation = playerSpawn.transform.rotation;
            obj.transform.parent = endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn.transform.parent;

            objectivesInLevel.Add(obj);
            _possibleObjectives.Remove(endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn);

            //based on objective, we may need to get some more objectives throughout level. Will randomly pick 2 more (if there are not 2 more then just add whatever is availbile (so 1))
            //if objective is certain types (ie type 3), choose more objectives and add to list

                //make sure we can spawn 2 more objectives!
                for (int objCount = 0; objCount < 2; objCount++)
                {
                    if (_possibleTileObjectivesInLevel.Count > 0)
                    {
                        //randomly pick an objective (or item?)
                        int indexO = Random.Range(0, _possibleObjectives.Count);
                      //  Debug.Log(indexO);
                        GameObject objMulti = Objectives.Instance.SetObjectiveRef(myLocalLevel.objective, _possibleObjectives[indexO]).gameObject;
                        objMulti.transform.rotation = playerSpawn.transform.rotation;
                        objMulti.transform.parent = _possibleObjectives[indexO].transform.parent;
                        objectivesInLevel.Add(objMulti);
                        _possibleItems.Remove(_possibleObjectives[indexO]);
                        _possibleObjectives.Remove(_possibleObjectives[indexO]);
                        
                        //Debug.Log("Added Objective");
                    }
                }      
        }
        else if (myLocalLevel.objective == 1)
        {
            //spawn in enemy
            Objectives.Instance.SetObjectiveRef(myLocalLevel.objective, null);
           // eObj.transform.parent = endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn.transform.parent;
           // objectivesInLevel.Add(eObj);
            _possibleObjectives.Remove(endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn);
            GameObject enemy = Instantiate(myLevelAsset.enemyPrefab.dozerEnemy, endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn.transform);
            enemy.GetComponent<BaseEnemy>().isObjectiveEnemy = true;
            enemy.name = "OBJECTIVE_ENEMY";
            enemy.transform.parent = endObjTile.GetComponent<PresetTileInfo>().objectiveSpawn.transform.parent;
            enemiesInLevel.Add(enemy);
        }
        //update Objectives object with objetive info
        //Debug.Log("activated objs");
    }
    #endregion

    #region Preset Spawning
    /// <summary>
    /// - Analyze Tile, looking at an individual tile, for asset choosing and spawning
    /// - also checks if 4 tiles can be linked, links them if they are and decide weather to use this link to spawn big asset
    /// </summary>
    /// <param name="tile">Tile being analyzed</param>
    void AnalyzeTile(Tile tile)
    {
        //will first see if we can link tiles
        _tArray = new Tile[4];
        bool obj = false;
        //check neighbors, first up, then left, then right then down
        if (!tile.checkFor4Some)
        {
            Tile t;
            //Debug.Log("Starting at tile: " + tile.posOnGrid.x + " " + tile.posOnGrid.y);
            if (tile.upNeighbor != null && tile.upNeighbor.tileStatus != Tile.TileStatus.nullRoom && !tile.upNeighbor.checkFor4Some && tile.upNeighbor.tileStatus != Tile.TileStatus.secretRoom)
            {

                t = tile.upNeighbor;
                _tArray[0] = t;
                //Debug.Log(t.posOnGrid.x + " " + t.posOnGrid.y);
                if (t.rightNeighbor != null && t.rightNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.rightNeighbor.checkFor4Some && t.rightNeighbor.tileStatus != Tile.TileStatus.secretRoom)
                {
                    t = t.rightNeighbor;
                    _tArray[1] = t;
                    //Debug.Log(t.posOnGrid.x + " " + t.posOnGrid.y);
                    if (t.downNeighbor != null && t.downNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.downNeighbor.checkFor4Some && t.downNeighbor.tileStatus != Tile.TileStatus.secretRoom)
                    {
                        t = t.downNeighbor;
                        _tArray[2] = t;
                        // Debug.Log(t.posOnGrid.x + " " + t.posOnGrid.y);
                        if (t.leftNeighbor != null && t.leftNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.leftNeighbor.checkFor4Some && t.leftNeighbor.tileStatus != Tile.TileStatus.secretRoom)
                        {
                            //all of these tiles can be linked
                            t = t.leftNeighbor;
                            // Debug.Log(t.posOnGrid.x + " " + t.posOnGrid.y);
                            _tArray[3] = t;

                            //random chance we dont use this 4 some tile and have og be single
                            if (Random.value <= 0.25)
                            {
                                fourSomeCount++;
                                GameObject fourSomeTile = new GameObject("BigTile_" + fourSomeCount);
                                _av = Vector3.zero;
                                fourSomeTile.transform.parent = this.transform;
                                foreach (Tile tile2 in _tArray)
                                {
                                    _av += tile2.transform.position;
                                }
                                _av = _av / 4;
                                //Debug.Log(av);
                                fourSomeTile.transform.position = _av;
                                foreach (Tile tile3 in _tArray)
                                {
                                    //Debug.Log("ap");
                                    tile3.levelAssetPlaced = true;

                                    if(tile3.tileStatus == Tile.TileStatus.boss)
                                    {
                                        //Debug.Log("big tile has objectvie");
                                        obj = true;
                                    }

                                    if (tile3.presetNum != -1)
                                    {
                                       // Debug.Log(tile3.presetNum);
                                        //remove its possible items from _possileItems if it has already assigned preset tile
                                        assetCountArray[tile3.presetNum] -= 1;
                                        if (tile3.presetTile.TryGetComponent<PresetTileInfo>(out PresetTileInfo mPresetTileInfo))
                                        {
                                            foreach (GameObject item in mPresetTileInfo.GetComponent<PresetTileInfo>().possiblePresetItems)
                                            {
                                                _possibleItems.Remove(item);
                                                _possibleObjectives.Remove(item);
                                            }
                                            _possibleTileObjectivesInLevel.Remove(tile3.presetTile);
                                            
                                        }
                                    }

                                    if(tile3.presetTile != null)
                                        Destroy(tile3.presetTile.gameObject);


                                    tile3.checkFor4Some = true;
                                    tile3.transform.parent = fourSomeTile.transform;
                                }

                                SpawnLevelBigAsset(fourSomeTile, obj);



                                return;
                            }
                            //else
                                //Debug.Log("jebait");
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
               // Debug.Log(tile.posOnGrid.x + " " + tile.posOnGrid.y);
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
        //Debug.Log("PRE");
        //will pick the least used preset but for now it will be random
        int index = FindLowestMagnatudeSMOL();//Random.Range(0, myLevelAsset.presetTileAssets.Count);

        GameObject preset = null;
        // Debug.Log(index);
        if (tile.tileStatus != Tile.TileStatus.boss)
        {
            preset = Instantiate(myLevelAsset.presetTileAssets[index], tile.transform.position, tile.transform.rotation);
            tile.presetNum = index;
            preset.transform.parent = tile.transform.parent;
            tile.presetTile = preset;
            assetCountArray[index] += 1;

            tile.levelAssetPlaced = true;
        }
        else
        {
            //if  this tile is the final room, make sure it has an objective
            preset = Instantiate(myLevelAsset.presetObjectiveTiles[Random.Range(0, myLevelAsset.presetObjectiveTiles.Count)], tile.transform.position, tile.transform.rotation);

            preset.transform.parent = tile.transform.parent;
            tile.presetTile = preset;
            int tileIndex = myLevelAsset.presetTileAssets.IndexOf(myLevelAsset.presetObjectiveTiles[Random.Range(0, myLevelAsset.presetObjectiveTiles.Count)]);
            //Debug.Log(tileIndex);
            assetCountArray[tileIndex] += 1;
            tile.presetNum = tileIndex;
            tile.levelAssetPlaced = true;
            endObjTile = preset;
            //_tileObjectivesInLevel.Add(tile);
            //Debug.Log("FINAL ROOM OBJECTIVE PLACED");
        }

        //Check this tile for any possible locations of resources AND ENEMIES
        //add those spots to the possible resources in level
        //later we can go through as activate these resources (maybe look at how far each of is from other active ones to ensure mass distribution - no clusters of resources)
        if (preset.TryGetComponent<PresetTileInfo>(out PresetTileInfo mPresetTileInfo))
        {

           // Debug.Log(preset.name);
            for (int posResourceCount = 0; posResourceCount < mPresetTileInfo.possiblePresetItems.Length; posResourceCount++)
            {
                //co++;
               // Debug.Log(co + " " + mPresetTileInfo.possiblePresetItems[posResourceCount].name);
                _possibleItems.Add(mPresetTileInfo.possiblePresetItems[posResourceCount]);
               // mPresetTileInfo.possiblePresetItems[posResourceCount].GetComponent<PossibleItem>().itemIndex = _possibleItems.Count - 1;
                if (tile.tileStatus != Tile.TileStatus.startingRoom)
                    _possibleObjectives.Add(mPresetTileInfo.possiblePresetItems[posResourceCount]);
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
    //public int co = 0;
    /// <summary>
    /// - spawns bigger 4 tile asset
    /// </summary>
    /// <param name="bigTile"> The parent of the 4 linked tiles being analyzed and spawned on. </param>
    void SpawnLevelBigAsset(GameObject bigTile, bool hasObj)
    {
        //Debug.Log("Spawned big boi");
        //will pick the least used preset but for now it will be random
        int index = Random.Range(0, myLevelAsset.presetBigTileAssets.Count);
        GameObject preset;
        if (!hasObj)
        {
            preset = Instantiate(myLevelAsset.presetBigTileAssets[index], bigTile.transform.position, bigTile.transform.rotation);
            preset.transform.parent = bigTile.transform;

            bigAssetCountArray[index] += 1;
        }
        else
        {
            preset = Instantiate(myLevelAsset.presetBigTileAssets[1], bigTile.transform.position, bigTile.transform.rotation);
            preset.transform.parent = bigTile.transform;

            bigAssetCountArray[1] += 1;
            endObjTile = preset;
            //Debug.Log("BIG ASSET WITH OBJ");
        }


        if (preset.TryGetComponent<PresetTileInfo>(out PresetTileInfo mPresetTileInfo))
        {
            for (int posResourceCount = 0; posResourceCount < mPresetTileInfo.possiblePresetItems.Length; posResourceCount++)
            {
                //co++;
                //Debug.Log(co + " " + mPresetTileInfo.possiblePresetItems[posResourceCount].name);
                _possibleItems.Add(mPresetTileInfo.possiblePresetItems[posResourceCount]);
               // mPresetTileInfo.possiblePresetItems[posResourceCount].GetComponent<PossibleItem>().itemIndex = _possibleItems.Count;
                //if()
                _possibleObjectives.Add(mPresetTileInfo.possiblePresetItems[posResourceCount]);
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
               // for(int test = 0; test < _magAssetCount[listFinder].Count; test++)
               // {
                //    Debug.Log(_magAssetCount[listFinder][test]);
                //}
                int ranIndex = Random.Range(0, _magAssetCount[listFinder].Count);
                return _magAssetCount[listFinder][ranIndex];
            }
        }

        return 0;//Random.Range(0, myLevelAsset.presetTileAssets.Count);
    }

    #endregion

    //reshuffle list
    List<GameObject> reshuffle(List<GameObject> ar)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
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
    public int collectables;
    void ActivateItems()
    {

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
        //adds 30 to resoruce
        int pItemC;
        for(pItemC = 0; pItemC < _possibleItems.Count && pItemC < collectables; pItemC++)
        {
            switch (_possibleItems[pItemC].GetComponent<PossibleItem>().objectWeight)
            {
                case ObjectWeightType.None:
                    //randomly picks one of three types
                    //default %
                    if(Random.value > 0.80)
                    {
                        //15% for weapon
                        //Debug.Log("15");
                        int wIndex = Random.Range(0, myLevelAsset.weaponList.Count);
                        GameObject weapon = Instantiate(myLevelAsset.weaponList[wIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);//_possibleItems[pItemC].transform.rotation);
                        weapon.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        weaponsInLevelList.Add(weapon);
                    }
                    else if (Random.value > 0.7)
                    {
                        //35% for item
                        //Debug.Log("45");
                        int iIndex = Random.Range(0, myLevelAsset.itemList.Count);
                        GameObject item = Instantiate(myLevelAsset.itemList[iIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);//_possibleItems[pItemC].transform.rotation);
                        item.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        itemsInLevelList.Add(item);
                    }
                    else // if(Random.value > 0.6)
                    {
                        //50% chance its a resource
                        //Debug.Log("40");
                        int rIndex = Random.Range(0, myLevelAsset.resourcesList.Count);
                        GameObject resource = Instantiate(myLevelAsset.resourcesList[rIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);//_possibleItems[pItemC].transform.rotation);
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
                        GameObject weapon = Instantiate(myLevelAsset.weaponList[wIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation); //_possibleItems[pItemC].transform.rotation);
                        weapon.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        weaponsInLevelList.Add(weapon);
                    }
                    else if (Random.value > 0.55)
                    {
                        int rIndex = Random.Range(0, myLevelAsset.resourcesList.Count);
                        GameObject resource = Instantiate(myLevelAsset.resourcesList[rIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);// _possibleItems[pItemC].transform.rotation);
                        resource.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        resourcesInLevelList.Add(resource);

                    }
                    else // if (Random.value > 0.80)
                    {
                        int iIndex = Random.Range(0, myLevelAsset.itemList.Count);
                        GameObject item = Instantiate(myLevelAsset.itemList[iIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);//_possibleItems[pItemC].transform.rotation);
                        item.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        itemsInLevelList.Add(item);
                    }
                    break;
                case ObjectWeightType.Item:
                    //item = 55
                    //resource = 35
                    //weapon = 10
                    if (Random.value > 0.45)
                    {
                        int iIndex = Random.Range(0, myLevelAsset.itemList.Count);
                        GameObject item = Instantiate(myLevelAsset.itemList[iIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);//_possibleItems[pItemC].transform.rotation);
                        item.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        itemsInLevelList.Add(item);
                    }
                    else if (Random.value > 0.65)
                    {
                        int rIndex = Random.Range(0, myLevelAsset.resourcesList.Count);
                        GameObject resource = Instantiate(myLevelAsset.resourcesList[rIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);//_possibleItems[pItemC].transform.rotation);
                        resource.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        resourcesInLevelList.Add(resource);
                    }
                    else
                    {
                        int wIndex = Random.Range(0, myLevelAsset.weaponList.Count);
                        GameObject weapon = Instantiate(myLevelAsset.weaponList[wIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation); //_possibleItems[pItemC].transform.rotation);
                        weapon.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        weaponsInLevelList.Add(weapon);
                    }
                    break;
                case ObjectWeightType.Resource:
                    //resource = 80
                    //item = 30
                    //weapon = 10
                    if (Random.value > 0.20)
                    {
                        int rIndex = Random.Range(0, myLevelAsset.resourcesList.Count);
                        GameObject resource = Instantiate(myLevelAsset.resourcesList[rIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);//_possibleItems[pItemC].transform.rotation);
                        resource.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        resourcesInLevelList.Add(resource);
                    }
                    else if (Random.value > 0.60)
                    {
                        int iIndex = Random.Range(0, myLevelAsset.itemList.Count);
                        GameObject item = Instantiate(myLevelAsset.itemList[iIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation);//_possibleItems[pItemC].transform.rotation);
                        item.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        itemsInLevelList.Add(item);
                    }
                    else
                    {
                        int wIndex = Random.Range(0, myLevelAsset.weaponList.Count);
                        GameObject weapon = Instantiate(myLevelAsset.weaponList[wIndex], _possibleItems[pItemC].transform.position, playerSpawn.transform.rotation); //_possibleItems[pItemC].transform.rotation);
                        weapon.transform.parent = _possibleItems[pItemC].transform.parent;
                        Destroy(_possibleItems[pItemC]);
                        weaponsInLevelList.Add(weapon);
                    }
                    break;
                default:
                    break;
            }
        }
        //Debug.Log("Removing unused drops");
        for(int lastItems = pItemC; lastItems < _possibleItems.Count; lastItems++)
        {
            //Debug.Log("Destroying " + _possibleItems[lastItems].name);
            Destroy(_possibleItems[lastItems]);
        }
        //resources can either spawn at random or based on distance from any other existing resource
        //when activated that gameobject will be removed from the possibleResources and added to resourcesInLevel List
        //when item is activated can either be a weapon, resource or other item
        Debug.Log(pItemC);
    }

    /// <summary>
    /// NOT IN USE
    /// - Randomly picked objs to be items, resources, weapon
    /// </summary>
    /// //get these values from scriptable obj. Total amount we spawn in (will replace theses)
    int _weaponsInLevel = 1;
    int _itemsInLevel = 4;
    int __resourcesInLevel = 10;
    void oldS()
    {
        //first spawn in X amount of weapons
        for (int weaponCount = 0; weaponCount < _weaponsInLevel; weaponCount++)
        {
            if (_possibleItems.Count != 0)
            {
                int index = Random.Range(0, _possibleItems.Count);
                GameObject weaponTemp = _possibleItems[index];
                //spawn in actual weapon item prefab from scritable item
                _possibleItems.RemoveAt(index);
                int wIndex = Random.Range(0, myLevelAsset.weaponList.Count);
                GameObject weapon = Instantiate(myLevelAsset.weaponList[wIndex], weaponTemp.transform.position, weaponTemp.transform.rotation);
                weapon.transform.parent = weaponTemp.transform.parent;
                Destroy(weaponTemp);
                weaponsInLevelList.Add(weapon);
            }
        }

        //then spawn in Y amount of items
        for (int itemCount = 0; itemCount < _itemsInLevel; itemCount++)
        {
            if (_possibleItems.Count != 0)
            {
                int index = Random.Range(0, _possibleItems.Count);
                GameObject itemTemp = _possibleItems[index];
                _possibleItems.RemoveAt(index);
                int iIndex = Random.Range(0, myLevelAsset.itemList.Count);
                GameObject item = Instantiate(myLevelAsset.itemList[iIndex], itemTemp.transform.position, itemTemp.transform.rotation);
                item.transform.parent = itemTemp.transform.parent;
                Destroy(itemTemp);
                itemsInLevelList.Add(item);
            }
        }


        //then spawn in Z amount of resources
        for (int resourceCount = 0; resourceCount < __resourcesInLevel; resourceCount++)
        {
            if (_possibleItems.Count != 0)
            {
                int index = Random.Range(0, _possibleItems.Count);
                GameObject resourceTemp = _possibleItems[index];
                _possibleItems.RemoveAt(index);
                int rIndex = Random.Range(0, myLevelAsset.resourcesList.Count);
                GameObject resource = Instantiate(myLevelAsset.resourcesList[rIndex], resourceTemp.transform.position, resourceTemp.transform.rotation);
                resource.transform.parent = resourceTemp.transform.parent;
                Destroy(resourceTemp);
                resourcesInLevelList.Add(resource);
            }
        }
    }
    #endregion


    #region Enemies

    void ActivateEnemies()
    {
        _possibleEnemiesInLevel = reshuffle(_possibleEnemiesInLevel);
        //Debug.Log(myLevelAsset.levelTier);
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
            default:
                break;
        }
        //Debug.Log(enemyCount);
    }
    /// <summary>
    /// enemy spawning depends on tier
    /// </summary>
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
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.Outcast:
                        enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                        //tier 1
                        break;
                    case EnemyWeightType.Dog:
                        enemy = myLevelAsset.enemyPrefab.dogEnemy;
                        //tier 1
                        break;
                    case EnemyWeightType.Warden:
                        //other types not in tier 1 will default to random tier 1
                        choice = Random.Range(0, 3);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
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
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
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
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
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
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.LostOne:
                        enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                        //tier 1
                        break;
                    case EnemyWeightType.Fugly:
                        choice = Random.Range(0, 3);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
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
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
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
                //_possibleEnemiesInLevel.RemoveAt(enemyCount);
                //Debug.Log(enemyCount);
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
                int choice;
                switch (mPossibleEnemy.type)
                {
                    case EnemyWeightType.None:
                        //picks between outcasts, dog or lost ones
                        choice = Random.Range(0, 6);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            case 3:
                                enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                                break;
                            case 4:
                                enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                                break;
                            case 5:
                                enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.Outcast:
                        enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                        //tier 1
                        break;
                    case EnemyWeightType.Dog:
                        enemy = myLevelAsset.enemyPrefab.dogEnemy;
                        //tier 1
                        break;
                    case EnemyWeightType.Warden:
                        //other types not in tier 1 will default to random tier 1
                        enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                        break;
                    case EnemyWeightType.Stalker:
                        enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                        break;
                    case EnemyWeightType.Splitter:
                        enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                        break;
                    case EnemyWeightType.Shadow:
                        choice = Random.Range(0, 6);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            case 3:
                                enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                                break;
                            case 4:
                                enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                                break;
                            case 5:
                                enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.LostOne:
                        enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                        //tier 1
                        break;
                    case EnemyWeightType.Fugly:
                        choice = Random.Range(0, 6);
                        switch (choice)
                        {
                            case 0:
                                //outcast
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            case 3:
                                enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                                break;
                            case 4:
                                enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                                break;
                            case 5:
                                enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
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
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            case 3:
                                enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                                break;
                            case 4:
                                enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                                break;
                            case 5:
                                enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
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
                //_possibleEnemiesInLevel.RemoveAt(enemyCount);
                //Destroy(mPossibleEnemy.gameObject);

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
                                enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                                break;
                            case 1:
                                enemy = myLevelAsset.enemyPrefab.dogEnemy;
                                //dog
                                break;
                            case 2:
                                enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                                //lost ones
                                break;
                            case 3:
                                enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                                break;
                            case 4:
                                enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                                break;
                            case 5:
                                enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                                break;
                            case 6:
                                enemy = myLevelAsset.enemyPrefab.shadowEnemy;
                                break;
                            case 7:
                                enemy = myLevelAsset.enemyPrefab.fuglyEnemy;
                                break;
                            case 8:
                                enemy = myLevelAsset.enemyPrefab.dozerEnemy;
                                break;
                            default:
                                break;
                        }
                        break;
                    case EnemyWeightType.Outcast:
                        enemy = myLevelAsset.enemyPrefab.outcastEnemy;
                        break;
                    case EnemyWeightType.Dog:
                        enemy = myLevelAsset.enemyPrefab.dogEnemy;
                        break;
                    case EnemyWeightType.Warden:
                        enemy = myLevelAsset.enemyPrefab.wardenEnemy;
                        break;
                    case EnemyWeightType.Stalker:
                        enemy = myLevelAsset.enemyPrefab.stalkerEnemy;
                        break;
                    case EnemyWeightType.Splitter:
                        enemy = myLevelAsset.enemyPrefab.spitterEnemy;
                        break;
                    case EnemyWeightType.Shadow:
                        enemy = myLevelAsset.enemyPrefab.shadowEnemy;
                        break;
                    case EnemyWeightType.LostOne:
                        enemy = myLevelAsset.enemyPrefab.lostOneEnemy;
                        break;
                    case EnemyWeightType.Fugly:
                        enemy = myLevelAsset.enemyPrefab.fuglyEnemy;
                        break;
                    case EnemyWeightType.Dozer:
                        enemy = myLevelAsset.enemyPrefab.dozerEnemy;
                        break;
                    default:
                        break;
                }

                //spawn in this enemy
                enemy = Instantiate(enemy, mPossibleEnemy.transform.position, mPossibleEnemy.transform.rotation);
                enemiesInLevel.Add(enemy);
                enemy.transform.parent = mPossibleEnemy.transform.parent;
                Destroy(_possibleEnemiesInLevel[enemyCount]);
                //_possibleEnemiesInLevel.RemoveAt(enemyCount);
                //Destroy(mPossibleEnemy.gameObject);

            }
        }
        for (int deleteCount = enemyCount; deleteCount < _possibleEnemiesInLevel.Count; deleteCount++)
        {
            Destroy(_possibleEnemiesInLevel[deleteCount]);
        }
    }
    #endregion

    //For Debuging whats in list - NOT IN USE
    void ListDebugger()
    {
        for (int count = 0; count < _magAssetCount.Count; count++)
        {

            string tempS = "Spawned Tile: ";
            for (int c2 = 0; c2 < _magAssetCount[count].Count; c2++)
            {
                tempS += _magAssetCount[count][c2];
                tempS += " ";
            }
            tempS += " At a Count of: " + count.ToString();
            Debug.Log(tempS);
        }


    }



}
