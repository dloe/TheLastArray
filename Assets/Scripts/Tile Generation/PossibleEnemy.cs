using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy types
/// - Levels 1 - 4 == tier 1
/// - Levels 2 - 4 == teir 2
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
    public EnemyWeightType type;
}
