using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LevelAssetData", menuName = "ScritableObjects/LevelAssetData", order = 1)]
public class LevelAssetsData : ScriptableObject
{
    public List<GameObject> levelAssets;

    public List<GameObject> AssetsSpawned;

    public GameObject[] EnterenceRooms;


}
