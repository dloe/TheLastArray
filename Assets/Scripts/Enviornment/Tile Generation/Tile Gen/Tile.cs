using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Tile : MonoBehaviour
{
    /// <summary>
    /// Tile Script
    /// Dylan Loe
    /// 
    /// Last Updated: 5/8/2021
    /// 
    /// NOTES:
    /// - Holds info for each tile (to be referenced heavily by TileGeneration
    /// - runs through a check to help prefent duplicated linked doors
    /// - Door system is only active in lvl 4 (final level), every other level simply just has to spawn in the walls
    /// 
    /// </summary>
    public enum TileStatus
    {
        nullRoom,
        path,
        room,
        startingRoom,
        secretRoom,
        boss
    };

    public enum MapType
    {
        none,
        SingleDoor,
        DoubleStraight,
        DoubleLShape,
        TripeT,
        All4
    };
    [Space(5)]
    public MapType mapType;

    //Each tile has a path to its neighbors
    [Space(5)]
    public Vector2 posOnGrid;

    [Space(5)]
    public TileStatus tileStatus;

    [Header("Tiles Neighbors - One on each side of square")]
    public Tile upNeighbor;
    public Tile downNeighbor;
    public Tile leftNeighbor;
    public Tile rightNeighbor;

    [HideInInspector]
    //last tile in path or branch
    public Tile previousTile;
    [HideInInspector]
    //has been checked in tile system 
    public bool checkedForPath = false;
    [HideInInspector]
    //tile exists on path itself
    public bool partOfPath = false;

    [HideInInspector]
    //what number does this tile have on path (-1 means not on path)
    public int pathNumber = -1;
    public bool endOfBranchPath = false;
    
    [HideInInspector]
    //is this tile connected to path (like branch or other)
    public bool connectedToPath = false;

    private Color _nodeColor = Color.red;
    [Header("Level Asset Scriptable Obj")]
    public LevelAssetsData myLevelAssetData;
    public bool levelAssetPlaced = false;

    private TileGeneration myTileGen;
    //small details about tile for debugging
    public string description = "";
    [HideInInspector]
    //checked and linked to giant preset
    public bool checkFor4Some = false;
    [Header("Tile Preset")]
    public GameObject presetTile;
    [HideInInspector]
    public int presetNum = -1;

    #region Door Variables
    /// <summary>
    /// Door vars will only be used in final level
    /// </summary>
    public int doorsActivated = 0;
    public GameObject doorRef;
    public bool hasDoors = false;
    //when door is activated, it will not spawn any blockage or enviornment where the door is located, otherwise that direction/doorway will be blocked for the player
    public GameObject[] doors;

    //visible mesh player stands on, material will be set based on which level we one
    public GameObject tileStandable;
    
    #endregion

    private void Start()
    {
        myTileGen = GameObject.Find("TileGen").GetComponent<TileGeneration>();

        SetStandableMat();
        if(!hasDoors)
        {
            foreach (GameObject door in doors)
            {
                Destroy(door);
            }
        }
    }

    void SetStandableMat()
    {
        LocalLevel myLocalLevel = GameObject.Find("LevelManager").GetComponent<LocalLevel>();

        switch (myLocalLevel.thisLevelTier)
        {
            case levelTier.level1:
                tileStandable.GetComponent<MeshRenderer>().material = myLocalLevel.myLvlAssetData.forest_mat;
                break;
            case levelTier.level2:
                tileStandable.GetComponent<MeshRenderer>().material = myLocalLevel.myLvlAssetData.urban_mat;
                break;
            case levelTier.level3:
                tileStandable.GetComponent<MeshRenderer>().material = myLocalLevel.myLvlAssetData.outskirts_mat;
                break;
            case levelTier.level4:
                tileStandable.GetComponent<MeshRenderer>().material = myLocalLevel.myLvlAssetData.plant_mat;
                break;
            default:
                break;
        }
    }

    //establish this tile as a null tile (will not be used)
    public void ShadeNull()
    {
        _nodeColor = Color.red;
        tileStatus = Tile.TileStatus.nullRoom;
    }
    //establish this tile as being part of main path that will be used throughout level
    public void ShadePath()
    {
        _nodeColor = Color.green;
        tileStatus = Tile.TileStatus.path;
    }
    //establish this tile will be active during level
    public void ShadeActiveRoom()
    {
        _nodeColor = Color.grey;
        
    }
    //establish boss room
    public void ShadeBoosRoom()
    {
        _nodeColor = Color.yellow;
        tileStatus = Tile.TileStatus.boss;
    }
    //establish this tile as being the starting tile
    public void ShadeStarting()
    {
        _nodeColor = Color.blue;
        tileStatus = Tile.TileStatus.startingRoom;
    }
    //establish this tile as being a secret tile
    public void ShadeSecret()
    {
        _nodeColor = Color.black;
        tileStatus = Tile.TileStatus.secretRoom;
    }
    //doors on this tile will be properly labeled and checked
    public void LabelDoors()
    {
        for(int i = 0; i < doors.Length; i++)
        {
            doors[i].name = doors[i].name + "-" + posOnGrid.x + "-" + posOnGrid.y;
        }
    }

    //if a neighbor is null, add a wall - specifically for tiles next to null spaces
    public void ActivateWalls()
    {
        if(upNeighbor == null || upNeighbor.tileStatus == TileStatus.nullRoom)
        {
            //tile length / 2 
            //spawn at local pos -25, 10, 0 with rotation of -90, 0, -90
            GameObject wall = Instantiate(myLevelAssetData.levelWall, transform.position, transform.rotation);
            wall.transform.parent = this.transform;
            wall.transform.localPosition = new Vector3(-12.5f, 5, 0);
            wall.transform.eulerAngles = new Vector3(-90, 0, -90);
            if(hasDoors)
                doors[0] = wall;
        }
        if(downNeighbor == null || downNeighbor.tileStatus == TileStatus.nullRoom)
        {
            //spawn at local pos 25, 10, 0 with rotation of -90, 0, 90
            GameObject wall = Instantiate(myLevelAssetData.levelWall, transform.position, transform.rotation);
            wall.transform.parent = this.transform;
            wall.transform.localPosition = new Vector3(12.5f, 5, 0);
            wall.transform.eulerAngles = new Vector3(-90, 0, 90);
            if(hasDoors)
                doors[1] = wall;
        }
        if(leftNeighbor == null || leftNeighbor.tileStatus == TileStatus.nullRoom)
        {
            //spawn at local pos 0, 10, 25 with rotation of -90, 0, -180
            GameObject wall = Instantiate(myLevelAssetData.levelWall, transform.position, transform.rotation);
            wall.transform.parent = this.transform;
            wall.transform.localPosition = new Vector3(0, 5, -12.5f);
            wall.transform.eulerAngles = new Vector3(-90, 0, -180);
            if(hasDoors)
                doors[2] = wall;
        }
        if(rightNeighbor == null || rightNeighbor.tileStatus == TileStatus.nullRoom)
        {
            //spawn at local pos 0, 10, -25 with rotation of -90, 0, 0
            GameObject wall = Instantiate(myLevelAssetData.levelWall, transform.position, transform.rotation);
            wall.transform.parent = this.transform;
            wall.transform.localPosition = new Vector3(0, 5, 12.5f);
            wall.transform.eulerAngles = new Vector3(-90, 0, 0);
            if(hasDoors)
                doors[3] = wall;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _nodeColor;
        if(myTileGen == null || myTileGen.debugPathOn)
            Gizmos.DrawSphere(transform.position, 2);
    }

    int[] reshuffle(int[] ar)
    {
        for (int t = 0; t < ar.Length; t++)
        {
            int tmp = ar[t];
            int r = Random.Range(t, ar.Length);
            ar[t] = ar[r];
            ar[r] = tmp;
        }
        return ar;
    }



    /*-------------------------------------------------------------------------------------
     *                      DOOR SYSTEM - ONLY IN LEVEL 4
     *-------------------------------------------------------------------------------------
     * 
     * - Planed to be used in last level
     * - will have doors between each tile be linked (ex: tile 1's left neighbor tile 2 both share a door (same door between tile1.leftdoor = tile2.rightdoor)
     */

    #region Doors
    /// <summary>
    /// ---------------------------------------------------------------------------------
    /// IN LVL 4, a way to make variant room sizes
    /// - UNUSED
    /// --------------------------------------------------------------------------------
    /// </summary>
    public void ChooseTileMap()
    {
        //will have a checklist of possible shapes the doosr can make, as we get though checking all the doors, we can narrow down which tile shapes it will be
        int _doorsOn = 0;
        for(int c = 0; c < doors.Length; c++)
        {
            if (doors[c].GetComponent<DoorBehavior>().isDoor)
            {
                _doorsOn++;
            }
        }
        doorsActivated = _doorsOn;

        switch (_doorsOn)
        {
            case 1:
                mapType = MapType.SingleDoor;
                break;
            case 2:
                //find if its which variant
                Determine2DoorVariant();
                break;
            case 3:
                mapType = MapType.TripeT;
                break;
            case 4:
                mapType = MapType.All4;
                break;
            default:
                break;
        }

        //make sure we choose and rotate tile map to propery mapType
        switch (mapType)
        {
            case MapType.none:
                break;
            case MapType.SingleDoor:
                //chooses random map variant made for incorperating 1 door
                //make sure room is rotated (either 90, 180, or 270) so door is facing right side
                //single door rooms most likely orientated having the front of the room being where the door is 
                break;
            case MapType.DoubleStraight:
                //double straights are oriented having front be one of the doors (not really imporant which one)
                break;
            case MapType.DoubleLShape:
                //double L shapes are oriented having front be left most door on L
                break;
            case MapType.TripeT:
                //T shaped rooms oriented having front be the middle door (so we have room on either side of front
                break;
            case MapType.All4:
                //doesnt matter (we dont rotate this room so orientation doesnt matter)
                break;
            default:
                break;
        }

    }

    void Determine2DoorVariant()
    {
        //go through doors array, check doors neighbors to see if they are actvie, if we find any neighbors that are active, then this is an L shape, else it is a straight line shape
        for (int c = 0; c < doors.Length; c++)
        {
            if (doors[c].GetComponent<DoorBehavior>().isDoor)
            {
                switch (c)
                {
                    case 0:
                        //door up
                         if (doors[1].GetComponent<DoorBehavior>().isDoor)
                         {
                            //neighbor is door, this is L shape
                            mapType = MapType.DoubleStraight;
                            return;
                         }
                         if(doors[2].GetComponent<DoorBehavior>().isDoor || doors[3].GetComponent<DoorBehavior>().isDoor)
                         {
                            mapType = MapType.DoubleStraight;
                            return;
                         }
                        break;
                    case 1:
                        //door down
                        if (doors[0].GetComponent<DoorBehavior>().isDoor)
                        {
                            //neighbor is door, this is L shape
                            mapType = MapType.DoubleStraight;
                            return;
                        }
                        if (doors[2].GetComponent<DoorBehavior>().isDoor || doors[3].GetComponent<DoorBehavior>().isDoor)
                        {
                            mapType = MapType.DoubleStraight;
                            return;
                        }
                        break;
                    case 2:
                        //door left
                        if (doors[3].GetComponent<DoorBehavior>().isDoor)
                        {
                            //neighbor is door, this is L shape
                            mapType = MapType.DoubleStraight;
                            return;
                        }
                        if (doors[0].GetComponent<DoorBehavior>().isDoor || doors[1].GetComponent<DoorBehavior>().isDoor)
                        {
                            mapType = MapType.DoubleStraight;
                            return;
                        }
                        break;
                    case 3:
                        //door left
                        if (doors[4].GetComponent<DoorBehavior>().isDoor)
                        {
                            //neighbor is door, this is L shape
                            mapType = MapType.DoubleStraight;
                            return;
                        }
                        if (doors[0].GetComponent<DoorBehavior>().isDoor || doors[1].GetComponent<DoorBehavior>().isDoor)
                        {
                            mapType = MapType.DoubleStraight;
                            return;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        mapType = MapType.none;
        Debug.Log("UUHH");
    }

    public void SyncDoors()
    {
        for (int doorC = 0; doorC < 4; doorC++)
        {
            if (doors[doorC].TryGetComponent<DoorBehavior>(out DoorBehavior mDoor))
                mDoor.GetComponent<DoorBehavior>().CheckForReplacementDoor();
        }

        //each door is resting on two tiles, this tile and its neighbor
        //if element 0 on doors in not null then there is a door between this tile and up neighbor. set element 1 of up neighbor to equal this tiles element 0
        if (doors[0] != null && upNeighbor != null && doors[0].GetComponent<DoorBehavior>().isDoor && !upNeighbor.doors[1].GetComponent<DoorBehavior>().isDoor)
        {
            upNeighbor.doors[1] = doors[0];
        }

        //if element 1 on doors in not null then there is a door between this tile and down neighbor. set element 0 of down neighbor to equal this tiles element 1
        if (doors[1] != null && downNeighbor != null && doors[1].GetComponent<DoorBehavior>().isDoor && !downNeighbor.doors[0].GetComponent<DoorBehavior>().isDoor)
        {
            downNeighbor.doors[0] = doors[1];
        }

        //if element 2 on doors in not null then there is a door between this tile and left neighbor. set element 3 of left neighbor to equal this tiles element 2
        if (doors[2] != null && leftNeighbor != null && doors[2].GetComponent<DoorBehavior>().isDoor && !leftNeighbor.doors[3].GetComponent<DoorBehavior>().isDoor)
        {
            leftNeighbor.doors[3] = doors[2];
        }

        //if element 3 on doors in not null then there is a door between this tile and right neighbor. set element 2 of up neighbor to equal this tiles element 3
        if (doors[3] != null && rightNeighbor != null && doors[3].GetComponent<DoorBehavior>().isDoor && !rightNeighbor.doors[2].GetComponent<DoorBehavior>().isDoor)
        {
            rightNeighbor.doors[2] = doors[3];
        }
    }

    /// <summary>
    /// Double check to fix small bug in which a duplicate door can rarly appear and not be removed and synced causing mass caos and walls to spawn in places they should not be spawning
    /// - fixes an edge case in which a specific angled door doesnt sync properly, for now this will have to do but a better algorithim may be the actual fix
    /// </summary>
    public void ReSyncDoors()
    {
        for (int doorC = 0; doorC < 4; doorC++)
        {
            if (doors[doorC].TryGetComponent<DoorBehavior>(out DoorBehavior mDoor))
                mDoor.GetComponent<DoorBehavior>().CheckForReplacementDoor();
        }

        //each door is resting on two tiles, this tile and its neighbor
        //if element 0 on doors in not null then there and the second door been deleted
        if (doors[0] != null && upNeighbor != null && upNeighbor.doors[1] == null)
        {
            upNeighbor.doors[1] = doors[0];
        }

        if (doors[1] != null && downNeighbor != null && downNeighbor.doors[0] == null)
        {
            downNeighbor.doors[0] = doors[1];
        }

        if (doors[2] != null && leftNeighbor != null && leftNeighbor.doors[3] == null)
        {
            leftNeighbor.doors[3] = doors[2];
        }

        if (doors[3] != null && rightNeighbor != null && rightNeighbor.doors[2] == null)
        {
            rightNeighbor.doors[2] = doors[3];
        }
    }

    //50% chance of activating door if neighbor is starting room
    static private float linkToStartingRoomDec_Branch = 0.4f;
    //have a 20% chance of looking at another neighbor and turning on door if they are just a basic room
    static private float linkToSecondNeighborDec_Branch = 0.2f;
    public void ActivateDoorsBranch()
    {
        bool firstBranchDoor = false;
        int[] doorsToCheck = new int[] { 1, 2, 3, 4 };
        doorsToCheck = reshuffle(doorsToCheck);

        for (int count = 0; count < 4; count++)
        {
            //will see which of its neighbors are paths/or active
            switch (doorsToCheck[count])
            {
                case 1:
                    //if any neighbor is boss room, or path, deactivate that door
                    if (upNeighbor == null || upNeighbor.tileStatus == TileStatus.nullRoom || upNeighbor.tileStatus == TileStatus.boss || upNeighbor.tileStatus == TileStatus.path)
                    {
                        doors[0].GetComponent<DoorBehavior>().ActivateDoor(false);
                    }
                    else
                    {
                        //50% chance of activating door if neighbor is starting room
                        if (upNeighbor.tileStatus == TileStatus.startingRoom && Random.value < linkToStartingRoomDec_Branch)
                        {
                            doorsActivated++;
                            doors[0].GetComponent<DoorBehavior>().ActivateDoor(true);
                        }
                        //checks 4 neighbors, if it has a neighbor that is that tiles previous, turn on that door
                        //have a 20% chance of looking at another neighbor and turning on door if they are just a basic room
                        else if (upNeighbor == previousTile)
                        {
                            if (firstBranchDoor == false)
                            {
                                firstBranchDoor = true;
                                doors[0].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else if (Random.value < linkToSecondNeighborDec_Branch)
                            {
                                doors[0].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else
                            {
                                doors[0].GetComponent<DoorBehavior>().ActivateDoor(false);
                            }
                        }
                    }
                    break;
                case 2:
                    //if any neighbor is boss room, or path, deactivate that door
                    if (downNeighbor == null || downNeighbor.tileStatus == TileStatus.nullRoom || downNeighbor.tileStatus == TileStatus.boss || downNeighbor.tileStatus == TileStatus.path)
                    {
                        doors[1].GetComponent<DoorBehavior>().ActivateDoor(false);
                    }
                    else
                    {
                        //50% chance of activating door if neighbor is starting room
                        if (downNeighbor.tileStatus == TileStatus.startingRoom && Random.value < linkToStartingRoomDec_Branch)
                        {
                            doors[1].GetComponent<DoorBehavior>().ActivateDoor(true);
                            doorsActivated++;
                        }
                        //checks 4 neighbors, if it has a neighbor that is that tiles previous, turn on that door
                        //have a 20% chance of looking at another neighbor and turning on door if they are just a basic room
                        else if (downNeighbor == previousTile)
                        {
                            if (firstBranchDoor == false)
                            {
                                firstBranchDoor = true;
                                doors[1].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else if (Random.value < linkToSecondNeighborDec_Branch)
                            {
                                doors[1].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else
                            {
                                doors[1].GetComponent<DoorBehavior>().ActivateDoor(false);
                            }
                        }
                    }
                    break;
                case 3:
                    //if any neighbor is boss room, or path, deactivate that door
                    if (leftNeighbor == null || leftNeighbor.tileStatus == TileStatus.nullRoom || leftNeighbor.tileStatus == TileStatus.boss || leftNeighbor.tileStatus == TileStatus.path)
                    {
                        doors[2].GetComponent<DoorBehavior>().ActivateDoor(false);
                    }
                    else
                    {
                        //50% chance of activating door if neighbor is starting room
                        if (leftNeighbor.tileStatus == TileStatus.startingRoom && Random.value < linkToStartingRoomDec_Branch)
                        {
                            doors[2].GetComponent<DoorBehavior>().ActivateDoor(true);
                            doorsActivated++;
                        }
                        //checks 4 neighbors, if it has a neighbor that is that tiles previous, turn on that door
                        //have a 20% chance of looking at another neighbor and turning on door if they are just a basic room
                        else if (leftNeighbor == previousTile)
                        {
                            if (firstBranchDoor == false)
                            {
                                firstBranchDoor = true;
                                doors[2].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else if (Random.value < linkToSecondNeighborDec_Branch)
                            {
                                doors[2].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else
                            {
                                doors[2].GetComponent<DoorBehavior>().ActivateDoor(false);
                               // doors[2] = null;
                            }
                        }
                    }
                    break;
                case 4:
                    //if any neighbor is boss room, or path, deactivate that door
                    if (rightNeighbor == null || rightNeighbor.tileStatus == TileStatus.nullRoom || rightNeighbor.tileStatus == TileStatus.boss || rightNeighbor.tileStatus == TileStatus.path)
                    {
                        doors[3].GetComponent<DoorBehavior>().ActivateDoor(false);
                    }
                    else
                    {
                        //50% chance of activating door if neighbor is starting room
                        if (rightNeighbor.tileStatus == TileStatus.startingRoom && Random.value < linkToStartingRoomDec_Branch)
                        {
                            doors[3].GetComponent<DoorBehavior>().ActivateDoor(true);
                            doorsActivated++;
                        }
                        //checks 4 neighbors, if it has a neighbor that is that tiles previous, turn on that door
                        //have a 20% chance of looking at another neighbor and turning on door if they are just a basic room
                        else if (rightNeighbor == previousTile)
                        {
                            if (firstBranchDoor == false)
                            {
                                firstBranchDoor = true;
                                doors[3].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else if (Random.value < linkToSecondNeighborDec_Branch)
                            {
                                doors[3].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else
                            {
                                doors[3].GetComponent<DoorBehavior>().ActivateDoor(false);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        //doors are now activated branch
    }

    static private float linkToRoomMutliple_RandomRoom = 0.25f;
    //for rooms not on branches or paths
    public void ActivateDoorsRandom()
    {
        bool activateGarantee = false;
        int doorsOn = 0;
        //goes through and turns on 1 door garenteed, then has a very low chance of turning on another door, and even lower chance of another door and even lower chance of another door, etc

        //bool firstBranchDoor = false;
        int[] doorsToCheck = new int[] { 1, 2, 3, 4 };
        doorsToCheck = reshuffle(doorsToCheck);

        for (int count = 0; count < 4; count++)
        {
            //will see which of its neighbors are paths/or active
            switch (doorsToCheck[count])
            {
                case 1:
                    //if any neighbor is null, boss room, or path, deactivate that door
                    if (upNeighbor == null || upNeighbor.tileStatus == TileStatus.nullRoom || upNeighbor.tileStatus == TileStatus.boss)
                    {
                        doors[0].GetComponent<DoorBehavior>().ActivateDoor(false);
                    }
                    else
                    {
                        //if its part of path, or a basic room
                        if (upNeighbor.tileStatus == TileStatus.startingRoom || upNeighbor.tileStatus == TileStatus.room || upNeighbor.tileStatus == TileStatus.path)
                        {
                            if (activateGarantee == false)
                            {
                                activateGarantee = true;
                                doorsOn++;
                                doors[0].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else if (Random.value < (float)linkToRoomMutliple_RandomRoom / doorsOn)
                            {
                                doorsOn++;
                                doors[0].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else
                            {
                                doors[0].GetComponent<DoorBehavior>().ActivateDoor(false);
                            }
                        }
                    }
                    break;
                case 2:
                    //if any neighbor is null, boss room, or path, deactivate that door
                    if (downNeighbor == null || downNeighbor.tileStatus == TileStatus.nullRoom || downNeighbor.tileStatus == TileStatus.boss)
                    {
                        doors[1].GetComponent<DoorBehavior>().ActivateDoor(false);
                    }
                    else
                    {
                        //if its part of path, or a basic room
                        if (downNeighbor.tileStatus == TileStatus.startingRoom || downNeighbor.tileStatus == TileStatus.room || downNeighbor.tileStatus == TileStatus.path)
                        {
                            if (activateGarantee == false)
                            {
                                activateGarantee = true;
                                doorsOn++;
                                doors[1].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else if (Random.value < (float)linkToRoomMutliple_RandomRoom / doorsOn)
                            {
                                doorsOn++;
                                doors[1].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else
                            {
                                doors[1].GetComponent<DoorBehavior>().ActivateDoor(false);
                            }
                        }
                    }
                    break;
                case 3:
                    if (leftNeighbor == null || leftNeighbor.tileStatus == TileStatus.nullRoom || leftNeighbor.tileStatus == TileStatus.boss)
                    {
                        doors[2].GetComponent<DoorBehavior>().ActivateDoor(false);
                    }
                    else
                    {
                        //if its part of path, or a basic room
                        if (leftNeighbor.tileStatus == TileStatus.startingRoom || leftNeighbor.tileStatus == TileStatus.room || leftNeighbor.tileStatus == TileStatus.path)
                        {
                            if (activateGarantee == false)
                            {
                                activateGarantee = true;
                                doorsOn++;
                                doors[2].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else if (Random.value < (float)linkToRoomMutliple_RandomRoom / doorsOn)
                            {
                                doorsOn++;
                                doors[2].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else
                            {
                                doors[2].GetComponent<DoorBehavior>().ActivateDoor(false);
                            }
                        }
                    }
                    break;
                case 4:
                    if (rightNeighbor == null || rightNeighbor.tileStatus == TileStatus.nullRoom || rightNeighbor.tileStatus == TileStatus.boss)
                    {
                        doors[3].GetComponent<DoorBehavior>().ActivateDoor(false);
                    }
                    else
                    {
                        //if its part of path, or a basic room
                        if (rightNeighbor.tileStatus == TileStatus.startingRoom || rightNeighbor.tileStatus == TileStatus.room || rightNeighbor.tileStatus == TileStatus.path)
                        {
                            if (activateGarantee == false)
                            {
                                activateGarantee = true;
                                doorsOn++;
                                doors[3].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else if (Random.value < (float)linkToRoomMutliple_RandomRoom / doorsOn)
                            {
                                doorsOn++;
                                doors[3].GetComponent<DoorBehavior>().ActivateDoor(true);
                                doorsActivated++;
                            }
                            else
                            {
                                doors[3].GetComponent<DoorBehavior>().ActivateDoor(false);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        doorsActivated = doorsOn;
        //doors activated via random
    }

    public void ActivateDoorToPath()
    {
        int[] doorsToCheck = new int[] { 1, 2, 3, 4 };
        doorsToCheck = reshuffle(doorsToCheck);

        //checks with neighbor is part of path and turn that door on (only if neighbor is starting room or path)
        for (int count = 0; count < 4; count++)
        {
            switch (doorsToCheck[count])
            {
                case 1:
                    //will see which of its neighbors are paths/or active
                    if (upNeighbor == null || upNeighbor.tileStatus == TileStatus.nullRoom || upNeighbor.tileStatus == TileStatus.boss || connectedToPath)
                    {
                        doors[0].GetComponent<DoorBehavior>().ActivateDoor(false);
                    }
                    else if (upNeighbor.tileStatus == TileStatus.path || upNeighbor.tileStatus == TileStatus.startingRoom)
                    {
                        doors[0].GetComponent<DoorBehavior>().ActivateDoor(true);
                        doorsActivated++;
                        connectedToPath = true;
                    }
                    break;
                case 2:
                    //dont want to add doors that create shortcuts through path
                    if (downNeighbor == null || downNeighbor.tileStatus == TileStatus.nullRoom || downNeighbor.tileStatus == TileStatus.boss || connectedToPath)
                    {
                        doors[1].GetComponent<DoorBehavior>().ActivateDoor(false);
                       // doors[1] = null;
                    }
                    else if ((downNeighbor.tileStatus == TileStatus.path || downNeighbor.tileStatus == TileStatus.startingRoom))
                    {
                        doors[1].GetComponent<DoorBehavior>().ActivateDoor(true);
                        doorsActivated++;
                        connectedToPath = true;
                    }
                    break;
                case 3:
                    if (leftNeighbor == null || leftNeighbor.tileStatus == TileStatus.nullRoom || leftNeighbor.tileStatus == TileStatus.boss || connectedToPath)
                    {
                        doors[2].GetComponent<DoorBehavior>().ActivateDoor(false);
                       // doors[2] = null;
                    }
                    else if (leftNeighbor.tileStatus == TileStatus.path || leftNeighbor.tileStatus == TileStatus.startingRoom)
                    {
                        doors[2].GetComponent<DoorBehavior>().ActivateDoor(true);
                        doorsActivated++;
                        connectedToPath = true;
                        //Debug.Log(doors[2].name);
                    }
                    break;
                case 4:
                    if (rightNeighbor == null || rightNeighbor.tileStatus == TileStatus.nullRoom || rightNeighbor.tileStatus == TileStatus.boss || connectedToPath)
                    {
                        doors[3].GetComponent<DoorBehavior>().ActivateDoor(false);
                        //doors[3] = null;
                    }
                    else if (rightNeighbor.tileStatus == TileStatus.path || rightNeighbor.tileStatus == TileStatus.startingRoom)
                    {
                        doors[3].GetComponent<DoorBehavior>().ActivateDoor(true);
                        doorsActivated++;
                        connectedToPath = true;
                    }
                    break;
                default:
                    break;
            }
        }
        //doors activated via path
    }

    public void ActivateDoors()
    {
        //will see which of its neighbors are paths/or active
        if ((upNeighbor == null || upNeighbor.tileStatus == TileStatus.nullRoom))
        {
            doors[0].GetComponent<DoorBehavior>().ActivateDoor(false);
           // doors[0] = null;
        }
        else if ((upNeighbor.tileStatus == TileStatus.path || upNeighbor.tileStatus == TileStatus.room || upNeighbor.tileStatus == TileStatus.boss))
        {
            if (pathNumber + 1 == upNeighbor.pathNumber)
            {
                doors[0].GetComponent<DoorBehavior>().ActivateDoor(true);
                doorsActivated++;
            }
            else
            {
                doors[0].GetComponent<DoorBehavior>().ActivateDoor(false);
               // doors[0] = null;
            }
        }
        else if (upNeighbor.tileStatus == TileStatus.startingRoom && (pathNumber - 1 != upNeighbor.pathNumber || pathNumber + 1 != upNeighbor.pathNumber))
        {
            //if this tile is next to starting room and not the next tile in path
            doors[0].GetComponent<DoorBehavior>().ActivateDoor(false);
           // doors[0] = null;
        }


        //dont want to add doors that create shortcuts through path
        if ((downNeighbor == null || downNeighbor.tileStatus == TileStatus.nullRoom))
        {
            doors[1].GetComponent<DoorBehavior>().ActivateDoor(false);
           // doors[1] = null;
        }
        else if ((downNeighbor.tileStatus == TileStatus.path || downNeighbor.tileStatus == TileStatus.room || downNeighbor.tileStatus == TileStatus.boss))
        {
            if (pathNumber + 1 == downNeighbor.pathNumber)
            {
                doors[1].GetComponent<DoorBehavior>().ActivateDoor(true);
                doorsActivated++;
            }
            else
            {
                doors[1].GetComponent<DoorBehavior>().ActivateDoor(false);
               // doors[1] = null;
            }
        }
        else if (downNeighbor.tileStatus == TileStatus.startingRoom && (pathNumber - 1 != downNeighbor.pathNumber || pathNumber + 1 != downNeighbor.pathNumber))
        {
            //if this tile is next to starting room and not the next tile in path
            doors[1].GetComponent<DoorBehavior>().ActivateDoor(false);
           // doors[1] = null;
        }


        if ((leftNeighbor == null || leftNeighbor.tileStatus == TileStatus.nullRoom))
        {
            doors[2].GetComponent<DoorBehavior>().ActivateDoor(false);
           // doors[2] = null;
        }
        else if ((leftNeighbor.tileStatus == TileStatus.path || leftNeighbor.tileStatus == TileStatus.room || leftNeighbor.tileStatus == TileStatus.boss))
        {
            if (pathNumber + 1 == leftNeighbor.pathNumber)
            {
                doors[2].GetComponent<DoorBehavior>().ActivateDoor(true);
                doorsActivated++;
            }
            else
            {
                doors[2].GetComponent<DoorBehavior>().ActivateDoor(false);
               // doors[2] = null;
            }
        }
        else if (leftNeighbor.tileStatus == TileStatus.startingRoom && (pathNumber - 1 != leftNeighbor.pathNumber || pathNumber + 1 != leftNeighbor.pathNumber))
        {
            //if this tile is next to starting room and not the next tile in path
            doors[2].GetComponent<DoorBehavior>().ActivateDoor(false);
            //doors[2] = null;
        }


        if ((rightNeighbor == null || rightNeighbor.tileStatus == TileStatus.nullRoom))
        {
            doors[3].GetComponent<DoorBehavior>().ActivateDoor(false);
            //doors[3] = null;
        }
        else if ((rightNeighbor.tileStatus == TileStatus.path || rightNeighbor.tileStatus == TileStatus.room || rightNeighbor.tileStatus == TileStatus.boss))
        {
            if (pathNumber + 1 == rightNeighbor.pathNumber)
            {
                doors[3].GetComponent<DoorBehavior>().ActivateDoor(true);
                doorsActivated++;
            }
            else
            {
                doors[3].GetComponent<DoorBehavior>().ActivateDoor(false);
                //doors[3] = null;
            }
        }
        else if (rightNeighbor.tileStatus == TileStatus.startingRoom && (pathNumber - 1 != rightNeighbor.pathNumber || pathNumber + 1 != rightNeighbor.pathNumber))
        {
            //if this tile is next to starting room and not the next tile in path
            doors[3].GetComponent<DoorBehavior>().ActivateDoor(false);
            //doors[3] = null;
        }
    }

    public void DeactivateDoors()
    {
        foreach (GameObject door in doors)
        {
            door.GetComponent<DoorBehavior>().ActivateDoor(false);
        }
    }
    #endregion
}

/// <summary>
/// ---------------------------
/// NOT IN USE
/// - Intended to be used to color code Tiles during generation phase before tile assets were assigned
/// - Red -- not in use
/// - green -- in use, part of path to exit
/// - blue -- in use, not necessarly part of path
/// ---------------------------
/// </summary>
public class NodeRef : MonoBehaviour
{
    private Color _nodeColor = Color.red;
    public void ShadeNull()
    {
        _nodeColor = Color.red;
    }
    public void ShadePath()
    {
        _nodeColor = Color.green;
    }
    public void ShadeActiveRoom()
    {
        _nodeColor = Color.blue;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _nodeColor;
        Gizmos.DrawSphere(transform.position, 5);
    }
}