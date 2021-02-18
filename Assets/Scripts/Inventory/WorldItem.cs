using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WorldItem : MonoBehaviour
{

    public Item.ItemType itemType;

}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldItem))]
public class WorldItemEditor : Editor
{
    WorldItem worldItem;

    private void OnEnable()
    {
        worldItem = (WorldItem)target;
    }

    public override void OnInspectorGUI()
    {
        worldItem.itemType = (Item.ItemType)EditorGUILayout.EnumPopup("Item Type", worldItem.itemType);
    }
}
#endif