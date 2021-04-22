using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyPrefab
{
    public GameObject outcastEnemyRanged;
    public GameObject outcastEnemyMelee;
    public GameObject dogEnemy;
    public GameObject wardenEnemy;
    public GameObject stalkerEnemy;
    public GameObject spitterEnemy;
    public GameObject shadowEnemy;
    public GameObject lostOneEnemy;
    public GameObject fuglyEnemy;
    public GameObject dozerEnemy;
}

[CreateAssetMenu(fileName = "LevelAssetData", menuName = "ScritableObjects/LevelAssetData", order = 1)]
public class LevelAssetsData : ScriptableObject
{
    
    [Header("Refs to Assets Spawned in Scene")]
    public List<GameObject> AssetsSpawned;
    public GameObject emptyObj;
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
    public List<ItemData> weaponDataList;
    [Header("Item Drop Prefabs")]
    public List<GameObject> itemList;
    public List<ItemData> itemDataList;
    [Header("Key Prefab Drop")]
    public ItemData keyData;
    [Header("Level Tier")]
    public int levelTier = 0;
    [Space(10)]
    public EnemyPrefab enemyPrefab;
    public EnemyPrefab EnemyMINIBOSSPrefab;
    [Space(10)]
    
    [Header("Boss Detection")]
    public GameObject bossDetection;
    public GameObject bossTileLastArray;

    [Header("Secret Room Varations")]
    public List<GameObject> secretRoomAssets;
    [Space(10)]
    [Header("Different varients of perset tile (by themes)")]
    [Header("Forest")]
    [Header("The Small 1 Tile Preset Variations")]
    public List<GameObject> forest_presetTileAssets;
    [Header("The Big 4 Tile Preset Variations")]
    public List<GameObject> forest_presetBigTileAssets;
    [Header("Small Tile Prefabs with objectives")]
    public List<GameObject> forest_presetObjectiveTiles;
    [Header("Big Tile Prefabs with objects")]
    public List<GameObject> forest_presetBigTileObjectives;
    [Header("Tile starting tile")]
    public GameObject forest_presetStartingTile;
    [Space(5)]
    [Header("Outskirts")]
    [Header("The Small 1 Tile Preset Variations")]
    public List<GameObject> outskirts_presetTileAssets;
    [Header("The Big 4 Tile Preset Variations")]
    public List<GameObject> outskirts_presetBigTileAssets;
    [Header("Tile Prefabs with objectives")]
    public List<GameObject> outskirts_presetObjectiveTiles;
    [Header("Big Tile Prefabs with objects")]
    public List<GameObject> outskirts_presetBigTileObjectives;
    [Header("Tile starting tile")]
    public GameObject outskirts_presetStartingTile;
}

