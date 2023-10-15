using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixRotation : MonoBehaviour
{
    /// <summary>
    /// Boss Rotation Behavior
    /// Dylan
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - Additional rotation behavior for the boss enemy
    /// </summary>
    public bool isBoss = false;
    float _yRot;
    void LateUpdate()
    {
        
        
        if (Mathf.FloorToInt( Player.Instance.playerImage.transform.parent.rotation.eulerAngles.y) == 0)
        {
            _yRot = 180;
            if(isBoss)
            {
                transform.parent.parent.GetComponent<BossEnemy>().imageDirMod = -1;
            }
            else
            {
                transform.parent.parent.GetComponent<BaseEnemy>().imageDirMod = -1;
            }
            
        }
        else
        {
            _yRot = -Player.Instance.playerImage.transform.parent.rotation.eulerAngles.y;
            if(_yRot == -180)
            {
                _yRot += 180;
                if(isBoss)
                {
                    transform.parent.parent.GetComponent<BossEnemy>().imageDirMod = -1;
                }
                else
                {
                    transform.parent.parent.GetComponent<BaseEnemy>().imageDirMod = -1;
                }
                
            }
            else
            {
                if (isBoss)
                {
                    transform.parent.parent.GetComponent<BossEnemy>().imageDirMod = 1;
                }
                else
                {
                    transform.parent.parent.GetComponent<BaseEnemy>().imageDirMod = 1;
                }
            }
            
        }
        
        transform.rotation = Quaternion.Euler(Player.Instance.transform.parent.rotation.eulerAngles.x, _yRot, Player.Instance.transform.parent.rotation.eulerAngles.z);
    }
}
