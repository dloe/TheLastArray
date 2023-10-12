using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy types
/// - Levels 1 - 4 == tier 1
/// - Levels 2 - 4 == tier 2
/// - Levels 3 - 4 == tier 3
/// </summary>
public enum EnemyWeightType
{
    None,
    Outcast,
    Dog,
    Warden,
    Stalker,
    Splitter,
    Shadow,
    LostOne,
    Fugly,
    Dozer
}

public class PossibleEnemy : MonoBehaviour
{
    /// <summary>
    /// Possible Enemy Placeholder Info
    /// Dylan Loe
    /// 
    /// last Updated: 4/25/21
    /// 
    /// Values are checked when determining what enemy should spawn here
    /// - Can be a boss (then upgrade health and damage variant)
    /// 
    /// </summary>
    public EnemyWeightType type;
    public bool canBeMiniBoss = false;
}
