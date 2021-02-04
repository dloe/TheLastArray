using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAssetSpawn : MonoBehaviour
{
    //ref to tile gen script
    public TileGeneration myTileGeneration;

    //LevelAsset myLevelAsset

    //to prevent to many of one asset spawning
    public int[] assetCountArray;
 
    //populate grid with assets, called from TileGeneration once it is done setting up
    public void PopulateGrid()
    {

    }

    //Analyze Tile, looking at an individual tile, for asset choosing and spawning
    void AnalyzeTile()
    {

    }

    /// <summary>
    /// spawn level asset, called in analyze tile. Spawns in and adds to assetCount array
    ///     - will determine position and rotation of where asset goes on tile
    /// </summary>
    void SpawnLevelAsset()
    {

    }
}
