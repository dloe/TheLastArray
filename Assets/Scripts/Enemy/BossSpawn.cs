using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawn : MonoBehaviour
{
    /// <summary>
    /// Boss Spawn Event
    /// Dylan Loe
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - checks for player to enter radius
    /// - when player enters radius, change objective, look door behind player and spawn boss
    ///     - boss must be defeated for player to leave area
    /// </summary>

    public Boss_PresetTileInfo tile;
    public GameObject bossdoor;
    public float radius = 6.25f;
    public GameObject bossObj;
    bool _startCheck = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    // Update is called once per frame
    void Update()
    {
        if(_startCheck)
            CheckForPlayer();
    }


    public void CheckForPlayer()
    {
        //Transform[] _possiblePlayer = collidersToTransforms((Physics.OverlapSphere(transform.position, radius)));
        Transform[] _possiblePlayer = collidersToTransforms((Physics.OverlapBox(transform.position, new Vector3(23.5f, 15, 10))));
        foreach(Transform potentialTarget in _possiblePlayer)
        {
            if(potentialTarget.gameObject.tag == "Player")
            {
                Debug.Log("Beginning boss fight");
                bossdoor.SetActive(true);
                Objectives.Instance.UpdateFinalObjective(0);

                GameObject b0ss = Instantiate(bossObj, transform.position, transform.rotation);

                //give ref to door and to last array interactable to boss for when it is killed (so player can open doors and complete objectives
                b0ss.GetComponent<BossEnemy>().lastArray = tile.lastArrayInteractable;
                Destroy(this.gameObject);
            }
        }
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1.0f);
        _startCheck = true;
        bossdoor = tile.door;
    }

    private void OnDrawGizmos()
    {
           Gizmos.color = Color.red;
        // Gizmos.DrawSphere(transform.position, radius);
       // Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, new Vector3(23.5f,15,10));
    }

    //locate transforms from colliders found in sphere
    private Transform[] collidersToTransforms(Collider[] colliders)
    {
        Transform[] transforms = new Transform[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            transforms[i] = colliders[i].transform;
        }
        return transforms;
    }
}
