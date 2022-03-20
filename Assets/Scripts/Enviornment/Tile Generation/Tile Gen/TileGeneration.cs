using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(TileGeneration))]
public class TileGenerationInspector : Editor
{

    //Temporary turned to false
    TileGeneration myTileGeneration;
    string showDebugPathActive = "Debug Path Inactive";

    private void OnEnable()
    {
        myTileGeneration = (TileGeneration)target;
        //myTileGeneration.debugPathOn = false;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Show Level Path");
        

        if(!myTileGeneration.debugPathOn)
        {
            showDebugPathActive = "Debug Path Inactive";
        }
        else
            showDebugPathActive = "Debug Path Active";

        if (GUILayout.Button(showDebugPathActive))
        {
            
            if(myTileGeneration.debugPathOn)
            {
                
                myTileGeneration.debugPathOn = false;
            }
            else
            {
                
                myTileGeneration.debugPathOn = true;
            }
        }

        EditorGUILayout.Space(20);
        base.OnInspectorGUI();

        bool somethingChanged = EditorGUI.EndChangeCheck();

        if (somethingChanged)
        {
            EditorUtility.SetDirty(myTileGeneration);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif


public class TileGeneration : MonoBehaviour
{
    /// <summary>
    /// Tile Genaraation Script
    /// Dylan Loe
    /// 
    /// Last Updated: 5/9/2021
    /// 
    /// Notes:
    ///  - Main System for Tile Genaration. 
    ///  - Occurs On Game Load:
    ///     - spawns the tiles
    ///     - each level has a given amount of space to give in terms of room size
    ///     - cant have levels be to big
    ///     - starts with starting room then works out choosing each puzzle peices with given space
    ///     -  ****once grid is made, we make a path from start to end. Then we make sure all the rooms are connected to main path****
    ///     
    /// </summary>

    //gets from level data (set in inspector)
    //so far ive only test with width and height being the same number, same number recommended (might need to make slight adjustments to one line to test both being different)
    [Header("Grid Size")]
    public int _levelWidth = 4;
    public int _levelHeight = 4;

    [Space(10)]
    //tile prefab
    public GameObject tilePlaceholder;

    //for visible nodes
    GameObject[,] _grid2DArray;

    //depends on size of indivual tiles to ensure no overlap
    float _distanceBetweenNodes = 50.0f;

    //player spawn not included the path
    [HideInInspector]
    public GameObject _playerSpawnPreset;
    Tile _startTile;
    Tile _endTile;
    int _pathNumber = 0;

    [Space(10)]
    [Header("List of Level Path Tiles")]
    public List<Tile> levelPath = new List<Tile>();
    [HideInInspector]
    public List<Tile> _allActiveTiles = new List<Tile>();
    //a temporary list to keep record of backtracked tiles for generation (no repeats)
    List<Tile> backtrackTempHistory = new List<Tile>();

    [Space(10)]
    [Header("Amount of branches from main path")]
    //adding random rooms
    public int branchCount;
    [Header("Amount of extra single rooms added")]
    public int fillerRooms;
    //all availble tile spots
    public List<Tile> _avalibleTileSpots = new List<Tile>();
    //divergent path branch
    List<Tile> _branch = new List<Tile>();

    bool _startLine = false;
    
    [HideInInspector]
    //used for path visibility (only seen in game view not player view)
    public bool debugPathOn = false;
    LineRenderer _lr;

    [Space(10)]
    [Header("Level Asset Script")]
    public LevelAssetSpawn myLevelAssetSpawn;
    [Header("Level Asset Data Obj")]
    public LevelAssetsData myLevelAssetsData;
    [Space(10)]
    [Header("Will level have doors generate?")]
    public bool hasDoors = false;
    //local level data asset
    private LocalLevel myLocalLevel;
    //some levels have a secret room
    public GameObject secretRoom;

    int failsafeCount = 0;

    //for setup of spawn room (level orientation)
    enum spawnRoomSide
    {
        none,
        right,
        left,
        up,
        down
    }
    spawnRoomSide _side;

    //used in secret room generation
    public class TileInfo
    {
        public TileInfo()
        {
            tile = null;
            n = new List<int>();
        }
        public Tile tile;
        public List<int> n;
    }
    //used for secret room gen, neighbor tracker, made of tileinfo objs
    List<TileInfo> _posSRNeighbors;



    private void Awake()
    {
        //establish level type
        myLocalLevel = FindObjectOfType<LocalLevel>();
    }

    void Start()
    {
        if (debugPathOn)
        {
            _lr = gameObject.AddComponent<LineRenderer>();
            _lr.widthMultiplier = 0.5f;
            _lr = GetComponent<LineRenderer>();
        }
            
        _distanceBetweenNodes = myLevelAssetsData.tileSize/2;
        CreateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if (_startLine && debugPathOn)
        {
            SetLineRenderer();
            _lr.enabled = debugPathOn;
        }
    }

    //basic setup for line renderer on debug mode
    void SetLineRenderer()
    {
        _lr = GetComponent<LineRenderer>();
        for(int t = 0; t < levelPath.Count; t++)
        {
            _lr.SetPosition(t, levelPath[t].transform.position);
        }
        //turn on colored spheres on each tile
    }

    #region Initial Generation Main
    //=============================================================================================================
    //                                      Initial Generation Setup and Implementation
    //=============================================================================================================

    /// <summary>
    /// - Possible setups for chooseing start and end rooms:
    ///         - can either start from one corner (1 1/4) and have player move across path of other corner (opposite 1/4)
    ///                 - will incorperate most tiles
    ///                 - give game a tile limit for grid size so its not to big, once it generates a path the extra rooms and tiles can vary as game gets more difficult
    ///         - have player start in center and have path be a corner
    ///                 - Will not be able to use low amount of tiles
    /// - Will need to choose a starting room and ending room
    /// - recursively move across grid until we reach destination 
    ///         - path cant overlap with already marked section, will use enum to keep track of which grids are already part of path
    ///         - color code it with node ref class
    /// - once path is generated can start connecting random rooms to parts of path, path will be saved out in an array of vector2s to each represent x and y cords on tile
    /// </summary>


    //grid creation, set values from levelWidth and levelHeight
    void CreateGrid()
    {
        _grid2DArray = new GameObject[_levelWidth, _levelHeight];

        //runs throug grid
        for (int rows = 0; rows < _levelWidth; rows++)
        {
            for (int col = 0; col < _levelHeight; col++)
            {
                GameObject nodeTile = new GameObject("Grid_Node");

                GameObject tilePlaceholderRef = Instantiate(tilePlaceholder, nodeTile.transform.position, nodeTile.transform.rotation);
                tilePlaceholderRef.name = "Tile_" + rows.ToString() + ":" + col.ToString();
                tilePlaceholderRef.transform.parent = nodeTile.transform;
                tilePlaceholderRef.GetComponent<Tile>().posOnGrid = new Vector2(rows, col);
                tilePlaceholderRef.GetComponent<Tile>().hasDoors = hasDoors;
                //small issue were tile no longer automatically deltes doros on start
                //if(!hasDoors)
                    //tilePlaceholderRef.GetComponent<Tile>().RemoveDoors();

                //DOORS
                if (hasDoors)
                {
                    tilePlaceholderRef.GetComponent<Tile>().LabelDoors();
                }
                
                nodeTile.transform.parent = this.gameObject.transform;

                //assigning transforms in the world
                float x = col * _distanceBetweenNodes;
                float y = rows * _distanceBetweenNodes;
                nodeTile.transform.position = new Vector3(x, 0, y);

                //renaming
                nodeTile.name = "Grid_Node_" + rows.ToString() + ":" + col.ToString();
                _grid2DArray[rows, col] = nodeTile;
            }
        }
        AssignNeighbors();
    }

    //run through grid, establish and link whos next to who
    void AssignNeighbors()
    {
        for (int rows = 0; rows < _levelWidth; rows++)
        {
            for (int col = 0; col < _levelHeight; col++)
            {
                Tile tile = _grid2DArray[rows, col].transform.GetChild(0).GetComponent<Tile>();
                int newRef;
 
                //assign this tiles neightbors
                if (rows < _levelWidth - 1)
                {
                     newRef = rows + 1;
                     tile.rightNeighbor = _grid2DArray[newRef, col].transform.GetChild(0).GetComponent<Tile>();
                 }
                 if (rows > 0)
                 {
                     newRef = rows - 1;
                     tile.leftNeighbor = _grid2DArray[newRef, col].transform.GetChild(0).GetComponent<Tile>();
                 }
                 if(col < _levelHeight - 1)
                 {
                    newRef = col + 1;
                    tile.downNeighbor = _grid2DArray[rows, newRef].transform.GetChild(0).GetComponent<Tile>();
                }
                 if(col > 0)
                 {
                    newRef = col - 1;
                    tile.upNeighbor = _grid2DArray[rows, newRef].transform.GetChild(0).GetComponent<Tile>();
                }
            }
        }
        // Debug.Log("Grid Created Drawn and neighbors assigned...");
        ChooseStartEndRooms();
    }

    /// <summary>
    /// - each tile has 4 possible directions the path can go (up, down, left and right)
    /// - creates list of all avalible spots right outside of path
    ///     - chooses one of these tiles to start branch
    ///     - goes random directions extending branch
    ///     - reapeat random amount of times
    ///     - adds individual rooms as well
    /// - chooses random direction to start possible path
    ///     - if path doesnt have any possible directions that are not already on the array, we backtrack one
    /// </summary>
    void GeneratePath()
    {
        //starts at _startTile
        levelPath.Add(_startTile);
        //mark start as part of path and checked
        _startTile.checkedForPath = true;
        _startTile.pathNumber = _pathNumber;
        _pathNumber++;
        _startTile.tileStatus = Tile.TileStatus.startingRoom;

        int[] nsToCheck = new int[] { 1, 2, 3, 4 };
        nsToCheck = reshuffle(nsToCheck);

        for (int count = 0; count < nsToCheck.Length; count++)
        {
            switch (nsToCheck[count])
            {
                case 1:
                    if (_startTile.upNeighbor != null)
                    {                        
                        _startTile.upNeighbor.previousTile = _startTile;
                        count = 4;
                        CheckTile(_startTile.upNeighbor, levelPath);
                    }
                    break;
                case 2:
                    if (_startTile.downNeighbor != null)
                    {
                        _startTile.downNeighbor.previousTile = _startTile;
                        count = 4;
                        CheckTile(_startTile.downNeighbor, levelPath);
                    }
                    break;
                case 3:
                    if (_startTile.leftNeighbor != null)
                    {
                        _startTile.leftNeighbor.previousTile = _startTile;
                        count = 4;
                        CheckTile(_startTile.leftNeighbor, levelPath);
                    }
                    break;
                case 4:
                    if (_startTile.rightNeighbor != null)
                    {
                        _startTile.rightNeighbor.previousTile = _startTile;
                        count = 4;
                        CheckTile(_startTile.rightNeighbor, levelPath);
                    }
                    break;
                default:
                    break;
            }
        
        }

        foreach ( Tile t in levelPath)
        {
            if(t.tileStatus != Tile.TileStatus.boss && t.tileStatus != Tile.TileStatus.startingRoom)
            {
                t.ShadePath();
                t.partOfPath = true;
                t.pathNumber = _pathNumber;
                _pathNumber++;
            }
        }
        _endTile.pathNumber = _pathNumber;
        //check each of this tiles sides, 

        levelPath = levelPath.Distinct().ToList();
        _allActiveTiles = _allActiveTiles.Distinct().ToList();

        //label path in scene from list
        if (debugPathOn)
            this.GetComponent<LineRenderer>().positionCount = levelPath.Count;

        //add random rooms to dungeon
        AddRandomRooms();

        //add Start Room (outside of grid)
        CreateSpawnRoom();

        //set up secret room
        if (myLocalLevel.thisLevelTier > levelTier.level2)
        {
            SetUpSecretRoom();
        }
        
        if (hasDoors)
        {
            ActivateAllDoors();
        }
 
        FinalTileSetup();

        _startLine = true;

        //start asset spawning 
        myLevelAssetSpawn.PopulateGrid(); 
    }


    /// <summary>
    /// - makes copy of current list 
    /// - Checks given tile
    ///     - is this tile on list?
    ///     - does it have neighbors that are not already added?
    /// - if tile is able to be added, add it, call this recursively with random side
    /// - if next tile shows a neighbor to boss room, then we done
    /// 
    /// </summary>
    void CheckTile(Tile tile, List<Tile> current)
    {
        //failsafe to stop possible infinate loop, causes being looked at
        if (failsafeCount == _levelHeight * _levelWidth * 2)
        {
            //lets say we dont make it to the boss room for some logical error that i couldnt find, simply make the end of the levelPath array the boss room. FAILSAFE 
            _endTile = levelPath[levelPath.Count - 1];
            _endTile.tileStatus = Tile.TileStatus.boss;
            _endTile.ShadeBoosRoom();

            return;
        }
        else
        {
            failsafeCount++;
        }

        //checks this tile
        //if it has no neighbors or all neighbors are marked as checked go back to previous. mark this tile as checked and unmark path status as path
        if ((tile.rightNeighbor == null || tile.rightNeighbor.checkedForPath) && (tile.leftNeighbor == null || tile.leftNeighbor.checkedForPath) && (tile.upNeighbor == null || tile.upNeighbor.checkedForPath) && (tile.downNeighbor == null || tile.downNeighbor.checkedForPath))
        {
            tile.checkedForPath = true;
            tile.ShadeNull();
            //Debug.Log("BACKTRACKING " + tile.posOnGrid.x + " " + tile.posOnGrid.y + " BACKTRACKING...");

            //adds to backtrack history, eventually these tiles will be unchecked so a different path may go through them
            backtrackTempHistory.Add(tile);

            //remove from list if it is on it
            current.Remove(tile);
            CheckTile(tile.previousTile, current);
        }
        //else if next room is boss room, add this tile and boos room. Mark as path
        else if (tile.rightNeighbor != null && tile.rightNeighbor.tileStatus == Tile.TileStatus.boss)
        {
            //final room
            tile.checkedForPath = true;
            current.Add(tile);
            current.Add(tile.rightNeighbor);
        }
        else if (tile.leftNeighbor != null && tile.leftNeighbor.tileStatus == Tile.TileStatus.boss)
        {
            //Found final room
            tile.checkedForPath = true;
            current.Add(tile);
            current.Add(tile.leftNeighbor);
        }
        else if (tile.upNeighbor != null && tile.upNeighbor.tileStatus == Tile.TileStatus.boss)
        {
            //Found final room
            tile.checkedForPath = true;
            current.Add(tile);
            current.Add(tile.upNeighbor);
        }
        else if (tile.downNeighbor != null && tile.downNeighbor.tileStatus == Tile.TileStatus.boss)
        {
            //Found final room
            tile.checkedForPath = true;
            current.Add(tile);
            current.Add(tile.downNeighbor);
        }
        //else check random neighbor, mark this as path
        else
        {
            int[] nsToCheck = new int[] { 1, 2, 3, 4 };
            nsToCheck = reshuffle(nsToCheck);

            for (int count = 0; count < nsToCheck.Length; count++)
            {
                switch (nsToCheck[count])
                {
                    case 1:
                        if (tile.upNeighbor != null && tile.upNeighbor.tileStatus != Tile.TileStatus.startingRoom && !tile.upNeighbor.checkedForPath && tile.upNeighbor.tileStatus == Tile.TileStatus.nullRoom)
                        {
                            tile.upNeighbor.previousTile = tile;
                            current.Add(tile);
                            tile.checkedForPath = true;
                            count = 5;
                            ClearHistory();
                            CheckTile(tile.upNeighbor, current);
                        }
                        break;
                    case 2:
                        if (tile.downNeighbor != null && tile.downNeighbor.tileStatus != Tile.TileStatus.startingRoom && !tile.downNeighbor.checkedForPath && tile.downNeighbor.tileStatus == Tile.TileStatus.nullRoom)
                        {
                            tile.downNeighbor.previousTile = tile;
                            current.Add(tile);
                            tile.checkedForPath = true;
                            count = 5;
                            ClearHistory();
                            CheckTile(tile.downNeighbor, current);
                        }
                        break;
                    case 3:
                        if (tile.leftNeighbor != null && tile.leftNeighbor.tileStatus != Tile.TileStatus.startingRoom && !tile.leftNeighbor.checkedForPath && tile.leftNeighbor.tileStatus == Tile.TileStatus.nullRoom)
                        {
                            tile.leftNeighbor.previousTile = tile;
                            current.Add(tile);
                            tile.checkedForPath = true;
                            count = 5;
                            ClearHistory();
                            CheckTile(tile.leftNeighbor, current);
                        }
                        break;
                    case 4:
                        if (tile.rightNeighbor != null && tile.rightNeighbor.tileStatus != Tile.TileStatus.startingRoom && !tile.rightNeighbor.checkedForPath && tile.rightNeighbor.tileStatus == Tile.TileStatus.nullRoom)
                        {
                            tile.rightNeighbor.previousTile = tile;
                            current.Add(tile);
                            tile.checkedForPath = true;
                            count = 5;
                            ClearHistory();
                            CheckTile(tile.rightNeighbor, current);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    //Clears history of backtrack history, called exclusively from CheckTile
    void ClearHistory()
    {
        foreach (Tile tile in backtrackTempHistory)
        {
            tile.checkedForPath = false;
        }
    }

    //=============================================================================================================
    //=============================================================================================================
    #endregion

    #region Tiles Gen Support
    //=============================================================================================================
    //                                  Adding the Single, random and branches
    //=============================================================================================================

    void AddRandomRooms()
    {
        //chooses random tile, then sees if we can add a room to it
        //if we can we add room otherwise we check again
        //cant add rooms to boss room

        //copy level tiles to all active tiles
        _allActiveTiles = new List<Tile>(levelPath);
        
        //run through each active tile in level to see if where branches can start
        //populate _avalibleTileSpots 
        for(int tileC = 0; tileC < _allActiveTiles.Count - 1; tileC++)
        {
            Tile current = _allActiveTiles[tileC];

            if (current.tileStatus != Tile.TileStatus.boss)
            {
                if (current.upNeighbor != null && current.upNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.upNeighbor))
                {
                    _avalibleTileSpots.Add(current.upNeighbor);
                }
                if (current.downNeighbor != null && current.downNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.downNeighbor))
                {
                    _avalibleTileSpots.Add(current.downNeighbor);
                }
                if (current.leftNeighbor != null && current.leftNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.leftNeighbor))
                {
                    _avalibleTileSpots.Add(current.leftNeighbor);
                }
                if (current.rightNeighbor != null && current.rightNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.rightNeighbor))
                {
                    _avalibleTileSpots.Add(current.rightNeighbor);
                }
            }
        }

        //default to rooms left over/row count
        branchCount = Random.Range(1, ((_grid2DArray.Length - levelPath.Count) / _levelWidth) + 1);

        //start with making branches
        //change 1 to branchCount after im done debugging
        for(int branch = 0; branch < branchCount; branch++)
        {
            //wont add any more rooms if there are no spaces left to add meaning grid2DArray.Length - levelPath.Count <= 0

            //first we find where this branch will start, need to find how many tiles this branch will be
            //for now imma use the length or level
            //if at any point the path has no where to go, we move on to next branch or exit completely
            int branchlength = Random.Range(1, _levelWidth + 1);
            //not sure we want to back track for branches, just end it when the reach a dead end
            
            Tile startingTile = _avalibleTileSpots[Random.Range(0, _avalibleTileSpots.Count)];

            _avalibleTileSpots.RemoveAll(obj => obj == startingTile);

            CheckTileBranch(startingTile, _branch, branchlength);

            //DOORS
            if (hasDoors)
            {
                _branch[0].ActivateDoorToPath();
                _branch[0].name += "_StartOfBranch";
            }
            //activate these rooms
            for ( int t = 0; t < _branch.Count; t++)
            {
                if (_branch[t].tileStatus == Tile.TileStatus.room)
                {
                    _branch[t].ShadeActiveRoom();
                    _branch[t].description = "Branch " + t.ToString();
                    _branch[t].pathNumber = t;

                    //add these tiles to the active tiles array
                    if(!_allActiveTiles.Contains(_branch[t]))
                        _allActiveTiles.Add(_branch[t]);
                    else if(_avalibleTileSpots.Contains(_branch[t]))
                        _avalibleTileSpots.Remove(_branch[t]);

                    //if this tile is on the active tile list, remove it so we dont see it again later
                }
                if(t == _branch.Count - 1)
                {
                    //mark as end of branch for possible miniboss spawning
                    _branch[t].endOfBranchPath = true;
                }
            }

            //---------------------
            //       DOORS
            //---------------------
            if (hasDoors)
            {
                for (int t = 1; t < _branch.Count; t++)
                {
                    if (_branch[t].tileStatus == Tile.TileStatus.room)
                    {
                        _branch[t].ActivateDoorsBranch();
                    }
                }
            }
            //start of this branch will have the door connecting to previous tile, the rest of the path will not have a door connecting to anything part of the path
            //tiles on this branch can only connect to existing tiles in the list

            //once we make branch, we go back through and remake the avalible tile spots
            RemakeAvalibleSpots();
            _branch.Clear();
        }

        //now that branches are done, we can add some single rooms around for further population
        AddSingleRooms();
    }

    void AddSingleRooms()
    {
        //when we add a room, remove from _avalibleTileSpots, add to _allActiveTileSpots

        //default to half the rooms left over
        fillerRooms = Random.Range(1, (_grid2DArray.Length - _allActiveTiles.Count) - ((_grid2DArray.Length - _allActiveTiles.Count)/4));

        for (int tileCount = 0; tileCount < fillerRooms; tileCount++)
        {
            if(_grid2DArray.Length - _allActiveTiles.Count >= _grid2DArray.Length/ (_levelWidth * 2))
            { 
                Tile current = _avalibleTileSpots[Random.Range(0, _avalibleTileSpots.Count)];
                if (current.tileStatus == Tile.TileStatus.nullRoom)
                {
                    current.ShadeActiveRoom();
                    current.tileStatus = Tile.TileStatus.room;
                    //remove from _avalible
                    _avalibleTileSpots.Remove(current);
                    _allActiveTiles.Add(current);
                    current.description = "random room";
                    //activate doors
                    //remake avaliblespots

                    //DOORS
                    if (hasDoors)
                    {
                        current.ActivateDoorsRandom();
                    }
                    RemakeAvalibleSpots();
                }
            }
            else
            {
                //stop adding random tiles
                return;
            }
        }
    }

    void RemakeAvalibleSpots()
    {
        //Remaking avalible spots
        for (int tileC = 0; tileC < _allActiveTiles.Count; tileC++)
        {
            Tile current = _allActiveTiles[tileC];
            if (current.tileStatus != Tile.TileStatus.boss)
            {
                if (current.upNeighbor != null && current.upNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.upNeighbor) && !_allActiveTiles.Contains(current.upNeighbor))
                {
                    _avalibleTileSpots.Add(current.upNeighbor);
                }
                if (current.downNeighbor != null && current.downNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.downNeighbor) && !_allActiveTiles.Contains(current.downNeighbor))
                {
                    _avalibleTileSpots.Add(current.downNeighbor);
                }
                if (current.leftNeighbor != null && current.leftNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.leftNeighbor) && !_allActiveTiles.Contains(current.leftNeighbor))
                {
                    _avalibleTileSpots.Add(current.leftNeighbor);
                }
                if (current.rightNeighbor != null && current.rightNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.rightNeighbor) && !_allActiveTiles.Contains(current.rightNeighbor))
                {
                    _avalibleTileSpots.Add(current.rightNeighbor);
                }
            }
        }
    }

    void CheckTileBranch(Tile tile, List<Tile> current, int length)
    {
        //
        if (length > 0)
        {
            current.Add(tile);
            tile.tileStatus = Tile.TileStatus.room;

            if ((tile.rightNeighbor == null || tile.rightNeighbor.tileStatus != Tile.TileStatus.nullRoom) && (tile.leftNeighbor == null || tile.leftNeighbor.tileStatus != Tile.TileStatus.nullRoom) && (tile.upNeighbor == null || tile.upNeighbor.tileStatus != Tile.TileStatus.nullRoom) && (tile.downNeighbor == null || tile.downNeighbor.tileStatus != Tile.TileStatus.nullRoom))
            {
                //if it has no avalible neighbors, we exit this branch
                length = 0;

                return;
            }
            
            int[] nsToCheck = new int[] { 1, 2, 3, 4 };
            nsToCheck = reshuffle(nsToCheck);

            for (int count = 0; count < nsToCheck.Length; count++)
            {
                bool up = true, down = true, left = true, right = true;
                switch (nsToCheck[count])
                {
                    case 0:
                        if (tile.upNeighbor != null && tile.upNeighbor.tileStatus == Tile.TileStatus.nullRoom && up == true)
                        {
                            tile.upNeighbor.previousTile = tile;
                            length--;

                            CheckTileBranch(tile.upNeighbor, current, length);
                            return;
                        }
                        else
                            up = false;
                        break;
                    case 1:
                        if (tile.downNeighbor != null && tile.downNeighbor.tileStatus == Tile.TileStatus.nullRoom && down == true)
                        {
                            tile.downNeighbor.previousTile = tile;
                            length--;
                            CheckTileBranch(tile.downNeighbor, current, length);
                            return;
                        }
                        else
                            down = false;
                        break;
                    case 2:
                        if (tile.leftNeighbor != null && tile.leftNeighbor.tileStatus == Tile.TileStatus.nullRoom && left == true)
                        {
                            tile.leftNeighbor.previousTile = tile;
                            length--;
                            CheckTileBranch(tile.leftNeighbor, current, length);
                            return;
                        }
                        else
                            left = false;
                        break;
                    case 3:
                        if (tile.rightNeighbor != null && tile.rightNeighbor.tileStatus == Tile.TileStatus.nullRoom && right == true)
                        {
                            tile.rightNeighbor.previousTile = tile;
                            length--;
                            CheckTileBranch(tile.rightNeighbor, current, length);
                            return;
                        }
                        else
                            right = false;
                        break;
                    default:
                        break;
                }
            }
        }
        return;
    }

    //for reshuffling lists/arrays
    int[] reshuffle(int[] ar)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < ar.Length; t++)
        {
            int tmp = ar[t];
            int r = Random.Range(t, ar.Length);
            ar[t] = ar[r];
            ar[r] = tmp;
        }
        return ar;
    }
    List<int> reshuffle(List<int> ar)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < ar.Count; t++)
        {
            int tmp = ar[t];
            int r = Random.Range(t, ar.Count);
            ar[t] = ar[r];
            ar[r] = tmp;
        }
        return ar;
    }

    //for activating all doors (for main path)
    void ActivateAllDoors()
    {
      for(int pathCount = 0; pathCount < levelPath.Count; pathCount++)
      {
           levelPath[pathCount].pathNumber = pathCount;
      }

        foreach (Tile t in levelPath)
        { 
            t.ActivateDoors();
        }
        DeactivateInActiveRooms();

        //sync doors to have doors actually connect between tiles
        foreach (Tile t in _allActiveTiles)
        {
            t.SyncDoors();
        }
    }

    //initial door setup run when all doors are initially deactivated (turned on as we sync them)
    void DeactivateInActiveRooms()
    {
        //go through array
        foreach (GameObject tileP in _grid2DArray)
        {
            //get child
            if (tileP.transform.GetChild(0).GetComponent<Tile>().tileStatus == Tile.TileStatus.nullRoom)
            {
                tileP.transform.GetChild(0).GetComponent<Tile>().DeactivateDoors();
            }
        }
    }

    //=============================================================================================================
    //=============================================================================================================
    #endregion

    #region Extra Room Setup
    //=============================================================================================================
    //                  for set up of Important rooms such as Starting room, ending room, bonus room
    //=============================================================================================================

    public void SetUpSecretRoom()
    {
        //first finds where we can possible put this
        //outskirts of game map (any tile next to an edge)
        //compose list of all null neighbors of all active tiles (excluding start and end room)
        _posSRNeighbors = new List<TileInfo>();
        for (int tileC = 0; tileC < _allActiveTiles.Count - 1; tileC++)
        {
            Tile current = _allActiveTiles[tileC];
            TileInfo cInfo = new TileInfo();
            cInfo.tile = current;
            if (current.tileStatus != Tile.TileStatus.boss && current.tileStatus != Tile.TileStatus.startingRoom)
            {
                if (current.upNeighbor == null)
                {
                    cInfo.n.Add(1);
                }
                else if (current.upNeighbor != null && current.upNeighbor.tileStatus == Tile.TileStatus.nullRoom
                    && current.upNeighbor.tileStatus != Tile.TileStatus.boss && current.upNeighbor.tileStatus != Tile.TileStatus.startingRoom)
                {
                    cInfo.n.Add(1);
                }

                if (current.downNeighbor == null)
                {
                    cInfo.n.Add(2);
                }
                else if (current.downNeighbor != null && current.downNeighbor.tileStatus == Tile.TileStatus.nullRoom
                    && current.downNeighbor.tileStatus != Tile.TileStatus.boss && current.downNeighbor.tileStatus != Tile.TileStatus.startingRoom)
                {
                    cInfo.n.Add(2);
                }

                if (current.leftNeighbor == null)
                {
                    cInfo.n.Add(3);
                }
                else if (current.leftNeighbor != null && current.leftNeighbor.tileStatus == Tile.TileStatus.nullRoom
                    && current.leftNeighbor.tileStatus != Tile.TileStatus.boss && current.leftNeighbor.tileStatus != Tile.TileStatus.startingRoom)
                {
                    cInfo.n.Add(3);
                }

                if (current.rightNeighbor == null)
                {
                    cInfo.n.Add(4);
                }
                else if (current.rightNeighbor != null && current.rightNeighbor.tileStatus == Tile.TileStatus.nullRoom
                    && current.rightNeighbor.tileStatus != Tile.TileStatus.boss && current.rightNeighbor.tileStatus != Tile.TileStatus.startingRoom)
                {
                    cInfo.n.Add(4);
                }

                if (cInfo.n.Count != 0 && !_posSRNeighbors.Contains(cInfo))
                {
                    _posSRNeighbors.Add(cInfo);
                }
            }
        }

        //now randomly picks a tile that has a neighbor we can use for secret room
        int tileNum = Random.Range(0, _posSRNeighbors.Count);

        TileInfo t = _posSRNeighbors[tileNum];

        //add null neighbors to array then randomly pick neighbor and this is where the secret room goes ;)
        //shuffle array of possible locs
        t.n = reshuffle(t.n);
        int loc = Random.Range(0, t.n.Count);

        Vector3 spawnPos;
        Quaternion spawnRot;
        switch (t.n[loc])
        {
            case 1:
                //up 
                spawnPos = new Vector3(t.tile.transform.position.x - (myLevelAssetsData.tileSize / 2), t.tile.transform.position.y, t.tile.transform.position.z);
                spawnRot = new Quaternion(t.tile.transform.rotation.x, t.tile.transform.rotation.y, t.tile.transform.rotation.z, t.tile.transform.rotation.w);
                secretRoom = Instantiate(tilePlaceholder, spawnPos, spawnRot);

                secretRoom.GetComponent<Tile>().downNeighbor = t.tile;
                t.tile.upNeighbor = secretRoom.GetComponent<Tile>();

                break;
            case 2:
                //down
                spawnPos = new Vector3(t.tile.transform.position.x + (myLevelAssetsData.tileSize / 2), t.tile.transform.position.y, t.tile.transform.position.z);
                spawnRot = new Quaternion(t.tile.transform.rotation.x, t.tile.transform.rotation.y, t.tile.transform.rotation.z, t.tile.transform.rotation.w);
                secretRoom = Instantiate(tilePlaceholder, spawnPos, spawnRot);

                secretRoom.GetComponent<Tile>().upNeighbor = t.tile;
                t.tile.downNeighbor = secretRoom.GetComponent<Tile>();
                break;
            case 3:
                //left
                spawnPos = new Vector3(t.tile.transform.position.x, t.tile.transform.position.y, t.tile.transform.position.z - (myLevelAssetsData.tileSize / 2));
                spawnRot = new Quaternion(t.tile.transform.rotation.x, t.tile.transform.rotation.y, t.tile.transform.rotation.z, t.tile.transform.rotation.w);
                secretRoom = Instantiate(tilePlaceholder, spawnPos, spawnRot);

                secretRoom.GetComponent<Tile>().rightNeighbor = t.tile;
                t.tile.leftNeighbor = secretRoom.GetComponent<Tile>();
                break;
            case 4:
                //right
                spawnPos = new Vector3(t.tile.transform.position.x, t.tile.transform.position.y, t.tile.transform.position.z + (myLevelAssetsData.tileSize / 2));
                spawnRot = new Quaternion(t.tile.transform.rotation.x, t.tile.transform.rotation.y, t.tile.transform.rotation.z, t.tile.transform.rotation.w);
                secretRoom = Instantiate(tilePlaceholder, spawnPos, spawnRot);

                secretRoom.GetComponent<Tile>().leftNeighbor = t.tile;
                t.tile.rightNeighbor = secretRoom.GetComponent<Tile>();
                break;
            default:
                Debug.Log("ERROR: the array on n is null");
                break;
        }
        secretRoom.name = "SecretRoom";
        secretRoom.transform.parent = this.transform;
        secretRoom.GetComponent<Tile>().ShadeSecret();
        secretRoom.GetComponent<Tile>().ActivateWalls();
        //SecretRoom added
    }

    void CreateSpawnRoom()
    {
        GameObject startingNode = new GameObject();
        startingNode.name = "StartingNode";
        startingNode.transform.parent = this.transform;
        Vector3 spawnPos;
        GameObject tile = null;

        //depending on start tile cords, we add starting room
        switch (_side)
        {
            case spawnRoomSide.right:
                spawnPos = new Vector3(_startTile.transform.position.x - (myLevelAssetsData.tileSize / 2), _startTile.transform.position.y, _startTile.transform.position.z);
                tile = Instantiate(tilePlaceholder, spawnPos, _startTile.transform.rotation);
                _playerSpawnPreset = Instantiate(myLocalLevel.presetStartingTile, spawnPos, _startTile.transform.rotation);
                tile.GetComponent<Tile>().downNeighbor = _allActiveTiles[0];
                _allActiveTiles[0].upNeighbor = tile.GetComponent<Tile>();
                _playerSpawnPreset.transform.localEulerAngles = new Vector3(tile.transform.localEulerAngles.x, -90, tile.transform.localEulerAngles.z);
                break;
            case spawnRoomSide.left:
                spawnPos = new Vector3(_startTile.transform.position.x + (myLevelAssetsData.tileSize / 2), _startTile.transform.position.y, _startTile.transform.position.z);
                tile = Instantiate(tilePlaceholder, spawnPos, _startTile.transform.rotation);
                _playerSpawnPreset = Instantiate(myLocalLevel.presetStartingTile, spawnPos, _startTile.transform.rotation);
                tile.GetComponent<Tile>().upNeighbor = _allActiveTiles[0];
                _allActiveTiles[0].downNeighbor = tile.GetComponent<Tile>();
                _playerSpawnPreset.transform.localEulerAngles = new Vector3(tile.transform.localEulerAngles.x, 90, tile.transform.localEulerAngles.z);
                break;
            case spawnRoomSide.up:
                spawnPos = new Vector3(_startTile.transform.position.x, _startTile.transform.position.y, _startTile.transform.position.z + (myLevelAssetsData.tileSize / 2));
                tile = Instantiate(tilePlaceholder, spawnPos, _startTile.transform.rotation);
                _playerSpawnPreset = Instantiate(myLocalLevel.presetStartingTile, spawnPos, _startTile.transform.rotation);
                tile.GetComponent<Tile>().leftNeighbor = _allActiveTiles[0];
                _allActiveTiles[0].rightNeighbor = tile.GetComponent<Tile>();
                break;
            case spawnRoomSide.down:
                spawnPos = new Vector3(_startTile.transform.position.x, _startTile.transform.position.y, _startTile.transform.position.z - (myLevelAssetsData.tileSize / 2));
                tile = Instantiate(tilePlaceholder, spawnPos, _startTile.transform.rotation);
                _playerSpawnPreset = Instantiate(myLocalLevel.presetStartingTile, spawnPos, _startTile.transform.rotation);
                tile.GetComponent<Tile>().rightNeighbor = _allActiveTiles[0];
                _allActiveTiles[0].leftNeighbor = tile.GetComponent<Tile>();
                _playerSpawnPreset.transform.localEulerAngles = new Vector3(tile.transform.localEulerAngles.x, 180, tile.transform.localEulerAngles.z);
                break;
            default:
                break;
        }
        tile.transform.parent = startingNode.transform;
        tile.GetComponent<Tile>().tileStatus = Tile.TileStatus.startingRoom;
        tile.GetComponent<Tile>().ShadeStarting();

        tile.GetComponent<Tile>().levelAssetPlaced = true;
        _playerSpawnPreset.name = "PlayerBeginningSpawnTile";
        _playerSpawnPreset.transform.parent = startingNode.transform;
        tile.GetComponent<Tile>().presetTile = _playerSpawnPreset;

        //add to front of allActiveTiles
        _allActiveTiles[0].tileStatus = Tile.TileStatus.path;
        _allActiveTiles[0].ShadePath();
        _allActiveTiles.Insert(0, tile.GetComponent<Tile>());
        levelPath.Insert(0, tile.GetComponent<Tile>());
        if (debugPathOn)
            _lr.positionCount = levelPath.Count;
    }


    void ChooseStartEndRooms()
    {
        //first we get the start room and end room
        int startX = 0; //= Random.Range(0, _levelWidth);
        int startY = 0; //= Random.Range(0, _levelHeight);

        //can either be (0,x), (x, 0), (max, x), (x, max)
        int num = Random.Range(1, 5);
        switch (num)
        {
            case 1:
                startX = 0;
                startY = Random.Range(0, _levelHeight);
                _side = spawnRoomSide.down;
                break;
            case 2:
                startX = Random.Range(0, _levelWidth);
                startY = 0;
                _side = spawnRoomSide.right;
                break;
            case 3:
                startX = _levelWidth - 1;
                startY = Random.Range(0, _levelHeight);
                _side = spawnRoomSide.up;
                break;
            case 4:
                startX = Random.Range(0, _levelWidth);
                startY = _levelHeight - 1;
                _side = spawnRoomSide.left;
                break;
            default:
                Debug.Log(num);
                Debug.Log("No side picked?");
                break;
        }

        _startTile = _grid2DArray[startX, startY].transform.GetChild(0).GetComponent<Tile>();
        _startTile.tileStatus = Tile.TileStatus.startingRoom;
        _startTile.ShadeStarting();

        //end tile must be opposite side of start to ensure max tile coverage between the two
        int endX, endY, endXF, endYF;
        //flipped values
        endX = _levelWidth - startX - 1;
        endY = _levelHeight - startY - 1;

        //add a little variation so boss room can anywhere in that quarter
        int xBuffer = _levelWidth / 2;
        endXF = endX + Random.Range(-xBuffer + 1, xBuffer - 1);
        int yBuffer = _levelHeight / 2;
        endYF = endY + Random.Range(-yBuffer + 1, yBuffer - 1);

        //should always try to keep a minimum distance from start (the xBuffer), cant be on same x as buffer

        while (endXF > _levelWidth - 1 || endXF == startX || endXF < 0)
        {
            endXF = endX + Random.Range(-xBuffer + 1, xBuffer);
        }
        while (endYF > _levelHeight - 1 || endYF == startY || endYF < 0)
        {
            endYF = endY + Random.Range(-yBuffer + 1, yBuffer);
        }
        //yield return new WaitForSeconds(0.1f);
        //in case the start point is towards the middle and the end point is also in the middle, really close to each other
        //check if Mathf.Abs(endXF - startX) < xbuffer && Mathf.Abs(endYF - startY)
        //NORMALLY THIS IS A WHILE, but there is some edge case causing issues - will investigate later
        // NOTE: this can be drastically optimized
        int oldX, oldY;
        while (Mathf.Abs(endXF - startX) < xBuffer && Mathf.Abs(endYF - startY) < yBuffer)
        {
            oldX = endXF;
            oldY = endYF;
            //rerolling
            //yield return new WaitForSeconds(0.25f);
            //chooses then either reroll x or y (50-50 chance to reroll either one
            if (Random.value < 0.5f)
            {
                endXF = endX + Random.Range(-xBuffer + 1, xBuffer);
                while (endXF > _levelWidth - 1 || endXF == startX || endXF < 0 || endXF == oldX)
                {
                    oldX = endXF;
                    endXF = endX + Random.Range(-xBuffer + 1, xBuffer);
                    // break;
                }
            }
            else
            {
                endYF = endY + Random.Range(-yBuffer + 1, yBuffer);
                while (endYF > _levelHeight - 1 || endYF == startY || endYF < 0 || endYF == oldY)
                {
                    oldY = endYF;
                    endYF = endY + Random.Range(-yBuffer + 1, yBuffer);
                    // break;
                }
            }
        }
        _endTile = _grid2DArray[endXF, endYF].transform.GetChild(0).GetComponent<Tile>();
        _endTile.tileStatus = Tile.TileStatus.boss;
        _endTile.ShadeBoosRoom();

        //Debug.Log("Generating Main Path...");
        GeneratePath();
    }

    /// <summary>
    /// - final setup for tiles
    ///     - remove null rooms
    /// </summary>
    void FinalTileSetup()
    {
        //must go though all active tiles 
        foreach (GameObject tile in _grid2DArray)
        {
            if (tile.transform.GetChild(0).GetComponent<Tile>().tileStatus == Tile.TileStatus.nullRoom)
            {
                Destroy(tile);
            }
            //turn on walls at borders of path handled in levelassetspawn
        }
    }

    //=============================================================================================================
    //=============================================================================================================
    #endregion
}




