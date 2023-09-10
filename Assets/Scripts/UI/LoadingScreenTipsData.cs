using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "LoadingScreenTipsData", menuName = "ScritableObjects/LoadingScreenTipsData", order = 3)]
public class LoadingScreenTipsData : ScriptableObject
{
    [Header("Loading Screen Tips to Use")]
    public string[] loadingScreenTips;

    [Header("Loading Screen Tips to Use")]
    public Sprite[] loadingScreenBackgroundImages;
}
