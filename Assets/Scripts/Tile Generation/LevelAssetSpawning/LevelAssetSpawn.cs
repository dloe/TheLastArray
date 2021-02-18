﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAssetSpawn : MonoBehaviour
{
    [Header("Tile Gen Script")]
    //ref to tile gen script
    public TileGeneration myTileGeneration;
    [Header("Level Asset Data Obj")]
    public LevelAssetsData myLevelAsset;
    [Header("Player Data Obj")]
    public PlayerData myPlayerData;

    //to prevent to many of one asset spawning
    [Space(10)]
    [Header("How many of which assets are spawned in")]
    public int[] assetCountArray;
    public int[] bigAssetCountArray;
   // public int[][] normAssetCountMagJaged;

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

    //get these values from scriptable obj
    int _weaponsInLevel = 1;
    int _itemsInLevel = 4;
    int __resourcesInLevel = 10;

   

    //first number represents the number of times tiles in that list were spawned
    //second number represents the tile numbers that were spawned that amount of times
    List<List<int>> _magAssetCount;

    private void Awake()
    {
       // Debug.Log("y");
        assetCountArray = new int[myLevelAsset.presetTileAssets.Count];
        bigAssetCountArray = new int[myLevelAsset.presetBigTileAssets.Count];

        //Debug.Log("n");
    }

    /// <summary>
    /// Populate grid with assets, called from TileGeneration once it is done setting up
    /// </summary>
    public void PopulateGrid()
    {
        Debug.Log("Populating Level with Assets...");

        //activate walls
        foreach (Tile t in myTileGeneration._allActiveTiles)
        {
            t.ActivateWalls();
            AnalyzeTile(t);
        }

        //SPAWN IN RESOURCES
        //ActivateItems();

        //ACTIVATE ENEMIES


    }

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
        //check neighbors, first up, then left, then right then down
        if (!tile.checkFor4Some)
        {
            Tile t;
            //Debug.Log("Starting at tile: " + tile.posOnGrid.x + " " + tile.posOnGrid.y);
            if (tile.upNeighbor != null && tile.upNeighbor.tileStatus != Tile.TileStatus.nullRoom && !tile.upNeighbor.checkFor4Some)
            {

                t = tile.upNeighbor;
                _tArray[0] = t;
                //Debug.Log(t.posOnGrid.x + " " + t.posOnGrid.y);
                if (t.rightNeighbor != null && t.rightNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.rightNeighbor.checkFor4Some)
                {
                    t = t.rightNeighbor;
                    _tArray[1] = t;
                    //Debug.Log(t.posOnGrid.x + " " + t.posOnGrid.y);
                    if (t.downNeighbor != null && t.downNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.downNeighbor.checkFor4Some)
                    {
                        t = t.downNeighbor;
                        _tArray[2] = t;
                        // Debug.Log(t.posOnGrid.x + " " + t.posOnGrid.y);
                        if (t.leftNeighbor != null && t.leftNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.leftNeighbor.checkFor4Some)
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
                                foreach (Tile tile2 in _tArray)
                                {
                                    //Debug.Log("ap");
                                    tile2.levelAssetPlaced = true;
                                    Destroy(tile2.presetTile.gameObject);
                                    tile2.checkFor4Some = true;
                                    tile2.transform.parent = fourSomeTile.transform;
                                }
                                
                                SpawnLevelBigAsset(fourSomeTile);
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


        //Debug.Log(index);
        GameObject preset = Instantiate(myLevelAsset.presetTileAssets[index], tile.transform.position, tile.transform.rotation);
        preset.transform.parent = tile.transform.parent;
        tile.presetTile = preset;
        assetCountArray[index] += 1;

        tile.levelAssetPlaced = true;


        //Check this tile for any possible locations of resources AND ENEMIES
        //add those spots to the possible resources in level
        //later we can go through as activate these resources (maybe look at how far each of is from other active ones to ensure mass distribution - no clusters of resources)
        if (preset.TryGetComponent<PresetTileInfo>(out PresetTileInfo mPresetTileInfo))
        {
            Debug.Log(preset.name);
            for (int posResourceCount = 0; posResourceCount < mPresetTileInfo.possiblePresetItems.Length; posResourceCount++)
            {
                _possibleItems.Add(mPresetTileInfo.possiblePresetItems[posResourceCount]);
            }
        }
    }

    /// <summary>
    /// - spawns bigger 4 tile asset
    /// </summary>
    /// <param name="bigTile"> The parent of the 4 linked tiles being analyzed and spawned on. </param>
    void SpawnLevelBigAsset(GameObject bigTile)
    {
        Debug.Log("Spawned big boi");
        //will pick the least used preset but for now it will be random
        int index = Random.Range(0, myLevelAsset.presetBigTileAssets.Count);


        GameObject preset = Instantiate(myLevelAsset.presetBigTileAssets[index], bigTile.transform.position, bigTile.transform.rotation);
        preset.transform.parent = bigTile.transform;

        bigAssetCountArray[index] += 1;


        if (preset.TryGetComponent<PresetTileInfo>(out PresetTileInfo mPresetTileInfo))
        {
            for (int posResourceCount = 0; posResourceCount < mPresetTileInfo.possiblePresetItems.Length; posResourceCount++)
            {
                _possibleItems.Add(mPresetTileInfo.possiblePresetItems[posResourceCount]);
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


    #region Items
    void ActivateItems()
    {
        //resources can either spawn at random or based on distance from any other existing resource
        //when activated that gameobject will be removed from the possibleResources and added to resourcesInLevel List
        //when item is activated can either be a weapon, resource or other item

        //first spawn in X amount of weapons
        for(int weaponCount = 0; weaponCount < _weaponsInLevel; weaponCount++)
        {
            if (_possibleItems.Count != 0)
            {
                int index = Random.Range(0, _possibleItems.Count);
                GameObject weaponTemp = _possibleItems[index];
                //spawn in actual weapon item prefab from scritable item
                _possibleItems.RemoveAt(index);
                int wIndex = Random.Range(0, myLevelAsset.weaponList.Count);
                GameObject weapon = Instantiate(myLevelAsset.weaponList[wIndex], weaponTemp.transform.position, weaponTemp.transform.rotation);
                Destroy(weaponTemp);
                weaponsInLevelList.Add(weapon);
            }
        }

        //then spawn in Y amount of items
        for(int itemCount = 0; itemCount < _itemsInLevel; itemCount++)
        {
            if (_possibleItems.Count != 0)
            {
                int index = Random.Range(0, _possibleItems.Count);
                GameObject itemTemp = _possibleItems[index];
                _possibleItems.RemoveAt(index);
                int iIndex = Random.Range(0, myLevelAsset.itemList.Count);
                GameObject item = Instantiate(myLevelAsset.itemList[iIndex], itemTemp.transform.position, itemTemp.transform.rotation);
                Destroy(itemTemp);
                itemsInLevelList.Add(item);
            }
        }


        //then spawn in Z amount of resources
        for(int resourceCount = 0; resourceCount < __resourcesInLevel; resourceCount++)
        {
            if (_possibleItems.Count != 0)
            {
                int index = Random.Range(0, _possibleItems.Count);
                GameObject resourceTemp = _possibleItems[index];
                _possibleItems.RemoveAt(index);
                int rIndex = Random.Range(0, myLevelAsset.resourcesList.Count);
                GameObject resource = Instantiate(myLevelAsset.resourcesList[rIndex], resourceTemp.transform.position, resourceTemp.transform.rotation);
                Destroy(resourceTemp);
                resourcesInLevelList.Add(resource);
            }
        }
    }
    #endregion

    #region Enemies
    void ActivateEnemies()
    {

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
