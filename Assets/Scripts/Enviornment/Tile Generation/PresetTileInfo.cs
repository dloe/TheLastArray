using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresetTileInfo : MonoBehaviour
{
    /// <summary>
    /// Preset Tile Info
    /// Dylan Loe
    /// 
    /// Last Updated 4/25/21
    /// 
    /// Notes:
    /// - Info that helps determine locals for possible items, enemies
    /// - marks tile if objective spawns here or if the player spawns here
    /// - NOTE: Special child obj of PresetTileInfo  for boss tile specifically
    /// 
    /// </summary>

    //stores info of possible resources and enemy locations to be accessed when tile spawns
    public GameObject objectiveSpawn;
    public GameObject playerSpawn;
    [Space(10)]
    public GameObject[] possiblePresetItems;
    public GameObject[] enemiesOnPreset;
}
