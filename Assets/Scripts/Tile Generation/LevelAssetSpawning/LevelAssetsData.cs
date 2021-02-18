using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LevelAssetData", menuName = "ScritableObjects/LevelAssetData", order = 1)]
public class LevelAssetsData : ScriptableObject
{
    [Header("The Small 1 Tile Preset Variations")]
    public List<GameObject> presetTileAssets;
    [Header("The Big 4 Tile Preset Variations")]
    public List<GameObject> presetBigTileAssets;
    [Header("Refs to Assets Spawned in Scene")]
    public List<GameObject> AssetsSpawned;
    [Header("Enterance Room Variations")]
    public GameObject[] EnterenceRooms;
    [Header("Level Wall Prefabs")]
    public GameObject levelWall;
    [Header("Tile Size")]
    public float tileSize = 100;
    [Header("Resource Drop Prefabs")]
    public List<GameObject> resourcesList;
    [Header("Weapon Drop Prefabs")]
    public List<GameObject> weaponList;
    [Header("Item Drop Prefabs")]
    public List<GameObject> itemList;
}
