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
        Debug.Log("n");
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

    //Analyze Tile, looking at an individual tile, for asset choosing and spawning
    void AnalyzeTile(Tile tile)
    {
        if(!tile.levelAssetPlaced)
        {
            SpawnLevelAsset(tile);
        }

    }

    /// <summary>
    /// spawn level asset, called in analyze tile. Spawns in and adds to assetCount array
    ///     - will determine position and rotation of where asset goes on tile
    /// </summary>
    void SpawnLevelAsset(Tile tile)
    {
        Debug.Log("PRE");
        //will pick the least used preset but for now it will be random
        int index = Random.Range(0, myLevelAsset.presetTileAssets.Count);
        Debug.Log(index);
        GameObject preset = Instantiate(myLevelAsset.presetTileAssets[index], tile.transform.position, tile.transform.rotation);

        assetCountArray[index] += 1;

        tile.levelAssetPlaced = true;
    }
}
