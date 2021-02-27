using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresetTileInfo : MonoBehaviour
{
    //stores info of possible resources and enemy locations to be accessed when tile spawns
    public GameObject objectiveSpawn;
    [Space(10)]
    public GameObject[] possiblePresetItems;

    public GameObject[] enemiesOnPreset;

    
}
