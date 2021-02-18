using System.Collections;
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
    List<GameObject> _possibleResources = new List<GameObject>();
    [Header("Resources In Level")]
    List<GameObject> resourcesInLevel = new List<GameObject>();

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

        Debug.Log(FindLowestMagnatude());
    }

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
        int index = FindLowestMagnatude();//Random.Range(0, myLevelAsset.presetTileAssets.Count);


        //Debug.Log(index);
        GameObject preset = Instantiate(myLevelAsset.presetTileAssets[index], tile.transform.position, tile.transform.rotation);
        preset.transform.parent = tile.transform.parent;
        tile.presetTile = preset;
        assetCountArray[index] += 1;

        tile.levelAssetPlaced = true;
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
    }

    //first number represents the number of times tiles in that list were spawned
    //second number represents the tile numbers that were spawned that amount of times
    public int[][] normAssetCountMagJaged;
    List<List<int>> magAssetCount;
    int FindLowestMagnatude()
    {
        magAssetCount = new List<List<int>>();

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
            magAssetCount.Add(temp);
        }

        //start from first num, and see if its empty, if it is move on to next. Else use random index for second num and return this value
        for(int listFinder = 0; listFinder < magAssetCount.Count; listFinder++)
        {
            if(magAssetCount[listFinder].Count != 0)
            {
                for(int test = 0; test < magAssetCount[listFinder].Count; test++)
                {
                    Debug.Log(magAssetCount[listFinder][test]);
                }
                int ranIndex = Random.Range(0, magAssetCount[listFinder].Count);
                return magAssetCount[listFinder][ranIndex];
            }
        }

        return 0;
    }

    //For Debuging whats in list
    void ListDebugger()
    {
        for (int count = 0; count < magAssetCount.Count; count++)
        {

            string tempS = "Spawned Tile: ";
            for (int c2 = 0; c2 < magAssetCount[count].Count; c2++)
            {
                tempS += magAssetCount[count][c2];
                tempS += " ";
            }
            tempS += " At a Count of: " + count.ToString();
            Debug.Log(tempS);
        }

        
    }
}
