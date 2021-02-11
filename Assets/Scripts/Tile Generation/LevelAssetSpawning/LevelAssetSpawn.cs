using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAssetSpawn : MonoBehaviour
{
    //ref to tile gen script
    public TileGeneration myTileGeneration;

    public LevelAssetsData myLevelAsset;

    public PlayerData myPlayerData;

    //to prevent to many of one asset spawning
    public int[] assetCountArray;

    private void Awake()
    {
       // Debug.Log("y");
        assetCountArray = new int[myLevelAsset.presetTileAssets.Count];
        //Debug.Log("n");
    }

    //populate grid with assets, called from TileGeneration once it is done setting up
    public void PopulateGrid()
    {
        Debug.Log("Populating Level with Assets...");

        //activate walls
        foreach (Tile t in myTileGeneration._allActiveTiles)
        {
            t.ActivateWalls();
            AnalyzeTile(t);
        }
    }

    public Tile[] tArray;
    public int fourSomeCount = 0;
    public Vector3 av;
    //Analyze Tile, looking at an individual tile, for asset choosing and spawning
    //also checks if 4 tiles can be linked, links them if they are and decide weather to use this link to spawn big asset
    void AnalyzeTile(Tile tile)
    {
        //will first see if we can link tiles
        tArray = new Tile[4];
        //check neighbors, first up, then left, then right then down
        if (!tile.checkFor4Some)
        {
            Tile t;
            //Debug.Log("Starting at tile: " + tile.posOnGrid.x + " " + tile.posOnGrid.y);
            if (tile.upNeighbor != null && tile.upNeighbor.tileStatus != Tile.TileStatus.nullRoom && !tile.upNeighbor.checkFor4Some)
            {

                t = tile.upNeighbor;
                tArray[0] = t;
                //Debug.Log(t.posOnGrid.x + " " + t.posOnGrid.y);
                if (t.rightNeighbor != null && t.rightNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.rightNeighbor.checkFor4Some)
                {
                    t = t.rightNeighbor;
                    tArray[1] = t;
                    //Debug.Log(t.posOnGrid.x + " " + t.posOnGrid.y);
                    if (t.downNeighbor != null && t.downNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.downNeighbor.checkFor4Some)
                    {
                        t = t.downNeighbor;
                        tArray[2] = t;
                        // Debug.Log(t.posOnGrid.x + " " + t.posOnGrid.y);
                        if (t.leftNeighbor != null && t.leftNeighbor.tileStatus != Tile.TileStatus.nullRoom && !t.leftNeighbor.checkFor4Some)
                        {
                            //all of these tiles can be linked
                            t = t.leftNeighbor;
                            // Debug.Log(t.posOnGrid.x + " " + t.posOnGrid.y);
                            tArray[3] = t;

                            //random chance we dont use this 4 some tile and have og be single
                            if (Random.value <= 0.25)
                            {
                                fourSomeCount++;
                                GameObject fourSomeTile = new GameObject("BigTile_" + fourSomeCount);
                                //fourSomeTile.transform.position = new Vector3(tArray[3].transform.position.x + 5, 0, tArray[3].transform.position.z + 5);
                                av = Vector3.zero;
                                fourSomeTile.transform.parent = this.transform;
                                foreach (Tile tile2 in tArray)
                                {
                                    Debug.Log("ap");
                                    av += tile2.transform.position;
                                    tile2.checkFor4Some = true;
                                    tile2.transform.parent = fourSomeTile.transform;
                                }
                                av = av / 4;
                                fourSomeTile.transform.position = av;
                                SpawnLevelBigAsset(fourSomeTile);
                            }
                            else
                                Debug.Log("jebait");
                        }
                        else
                        {
                            tArray[0] = null;
                            tArray[1] = null;
                            tArray[2] = null;
                        }
                    }
                    else
                    {
                        tArray[0] = null;
                        tArray[1] = null;
                    }
                }
                else
                {
                    tArray[0] = null;
                }
            }

        }
       // else
       // {
            if (!tile.levelAssetPlaced)
            {
                SpawnLevelSmallAsset(tile);
            }
       // }

    }

    /// <summary>
    /// spawn level asset, called in analyze tile. Spawns in and adds to assetCount array
    ///     - will determine position and rotation of where asset goes on tile
    /// </summary>
    void SpawnLevelSmallAsset(Tile tile)
    {
        //Debug.Log("PRE");
        //will pick the least used preset but for now it will be random
        int index = Random.Range(0, myLevelAsset.presetTileAssets.Count);
        //Debug.Log(index);
        GameObject preset = Instantiate(myLevelAsset.presetTileAssets[index], tile.transform.position, tile.transform.rotation);
        preset.transform.parent = tile.transform.parent;

        assetCountArray[index] += 1;

        tile.levelAssetPlaced = true;
    }

    void SpawnLevelBigAsset(GameObject bigTile)
    {
        Debug.Log("Spawned big boi");
    }
}
