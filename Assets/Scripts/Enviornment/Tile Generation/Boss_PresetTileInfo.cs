using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_PresetTileInfo : PresetTileInfo
{
    /// <summary>
    ///  Boss Tile Preset Info
    ///  Dylan Loe
    /// 
    /// Last Updated: 4/27/21
    /// 
    /// NOTES:
    /// - Contains extra PresetTileInfo specifically for the boss room (and boss room only)
    /// 
    /// </summary>
    [Header("Boss Room Specifics")]
    public GameObject door;
    public GameObject lastArrayInteractable;
    public GameObject craftingTableOutside;
    public GameObject textCanvas;
}
