using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAssetSpawn : MonoBehaviour
{
    /// <summary>
    /// Random Asset Spawn
    /// Dylan Loe
    /// 
    /// Updated: 9/10/23
    /// 
    /// 
    /// Notes:
    /// - For adding more variance on specific assets
    /// - if we want an asset like a tree that has multiple variance to be randomly choose we can attatch this script
    /// 
    /// TO DO:
    /// - 
    /// 
    /// </summary>

    public bool chooseVariance = false;

    public GameObject overrideVariant;

    public GameObject[] variantObjects;

    public GameObject placeHolder;

    private void Awake()
    {
        GameObject spawn;
        //choose a random index from array
        if (overrideVariant == null && variantObjects.Length != 0)
        {
            placeHolder.SetActive(false);
            spawn = variantObjects[Random.Range(0, variantObjects.Length - 1)];
            Instantiate(spawn, placeHolder.transform);
        }
        else
        {
            if (overrideVariant != null)
            {
                placeHolder.SetActive(false);
                spawn = overrideVariant;
                Instantiate(spawn, placeHolder.transform);
            }
        }
    }

}
