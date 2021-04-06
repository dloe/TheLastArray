using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawn : MonoBehaviour
{

    //checks for player to enter radius
    //when player enters radisu, change objective, look door behind player and spawn boss
    public GameObject Bossdoor;
   // public Objectives obj;
    public float radius = 6.25f;
    public GameObject bossObj;
    bool startCheck = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    // Update is called once per frame
    void Update()
    {
        if(startCheck)
            CheckForPlayer();
    }


    public void CheckForPlayer()
    {
        Transform[] _possiblePlayer = collidersToTransforms((Physics.OverlapSphere(transform.position, radius)));
        foreach(Transform potentialTarget in _possiblePlayer)
        {
            if(potentialTarget.gameObject.tag == "Player")
            {
                Debug.Log("begininng boss fight");
                Bossdoor.SetActive(true);
                Objectives.Instance.UpdateFinalObjective(0);

                GameObject b0ss = Instantiate(bossObj, transform.position, transform.rotation);

                //give ref to door and to last array interactable to boss for when it is killed (so player can open doors and complete objectives

                Destroy(this.gameObject);
            }
        }
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1.0f);
        startCheck = true;
    }

    private void OnDrawGizmos()
    {
           Gizmos.color = Color.red;
         Gizmos.DrawSphere(transform.position, radius);
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
