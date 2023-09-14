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
    /// - if we want an asset like a tree that has multiple variance to be randomly choose we can attach this script
    /// 
    /// TO DO:
    /// - 
    /// 
    /// </summary>

    [Header("Select this if you want this object to be randomly selected")]
    public bool chooseVariance = false;

    [Header("Leave null if we don't want a default object to go to")]
    public GameObject overrideVariant;

    public GameObject[] variantObjects;

    //public GameObject placeHolder;

    private void Awake()
    {

        PickRandomAsset();
    }


    void PickRandomAsset()
    {
        if (chooseVariance)
        {
            GameObject spawn;
            //choose a random index from array
            if (overrideVariant == null && variantObjects.Length != 0)
            {
                this.gameObject.SetActive(false);
                spawn = variantObjects[Random.Range(0, variantObjects.Length - 1)];
                if(spawn == null)
                {
                    Destroy(this.gameObject);
                    return;
                }
            }
            else
            {
                if (overrideVariant != null)
                {
                    this.gameObject.SetActive(false);
                    spawn = overrideVariant;
                }
                else
                {
                    Destroy(this.gameObject);
                    return;
                }
            }
            GameObject spawnedObj = Instantiate(spawn, this.transform);
            spawnedObj.transform.parent = this.transform.parent;
            spawnedObj.name = "RandomlySpawnedObj_" + spawn.name;
            Destroy(this.gameObject);
        }
    }
}
