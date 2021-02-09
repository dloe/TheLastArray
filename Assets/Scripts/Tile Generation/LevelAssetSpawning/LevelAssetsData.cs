using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LevelAssetData", menuName = "ScritableObjects/LevelAssetData", order = 1)]
public class LevelAssetsData : ScriptableObject
{
    public List<GameObject> presetTileAssets;

    public List<GameObject> AssetsSpawned;

    public GameObject[] EnterenceRooms;

    public GameObject levelWall;

    public float tileSize = 100;
}
