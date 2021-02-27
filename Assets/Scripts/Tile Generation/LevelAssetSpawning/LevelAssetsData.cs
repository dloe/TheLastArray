using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyPrefab
{
    public GameObject outcastEnemy;
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
    [Header("Level Tier")]
    public int levelTier = 0;
    [Space(10)]
    [Header("Starting Tile Prefabs")]
    public List<GameObject> presetStartingTileAssets;
    [Header("The Small 1 Tile Preset Variations")]
    public List<GameObject> presetTileAssets;
    [Header("The Big 4 Tile Preset Variations")]
    public List<GameObject> presetBigTileAssets;
    [Space(10)]
    public EnemyPrefab enemyPrefab;
    [Space(10)]
    [Header("Tile Prefabs with objectives")]
    public List<GameObject> presetObjectiveTiles;
}

