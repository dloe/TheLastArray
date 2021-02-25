using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileGeneration))]
public class TileGenerationInspector : Editor
{
    TileGeneration myTileGeneration;
    string showDebugPathActive = "Debug Path Active";

    private void Awake()
    {
        myTileGeneration = (TileGeneration)target;
    }

    public override void OnInspectorGUI()
    {
        
        EditorGUILayout.LabelField("Show Level Path");
        

        if(myTileGeneration.debugPathOn)
        {
            showDebugPathActive = "Debug Path Active";
        }
        else
            showDebugPathActive = "Debug Path Inactive";

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
    }
}


public class TileGeneration : MonoBehaviour
{
    /// <summary>
    /// On game load:
    ///     - spawns the tiles
    ///     - each level has a given amount of space to give in terms of room size
    ///     - cant have levels be to big
    ///     - starts with starting room then works out choosing each puzzle peices with given space
    ///     -  ****once grid is made, we make a path from start to end. Then we make sure all the rooms are connected to main path****
    ///     
    /// </summary>

    //gets these values from somewhere
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

    //does not include the path
    //private int totalSingleTilesInLevel = 3;
    [HideInInspector]
    public GameObject _playerSpawnPreset;
    Tile _startTile;
    Tile _endTile;
    int _pathNumber = 0;

    [Space(10)]
    [Header("List of Level Path Tiles")]
    public List<Tile> levelPath = new List<Tile>();
    //public List<Tile> inactiveRooms = new List<Tile>();
    [HideInInspector]
    public List<Tile> _allActiveTiles = new List<Tile>();

    List<Tile> backtrackTempHistory = new List<Tile>();

    [Space(10)]
    [Header("Amount of branches from main path")]
    //adding random rooms
    public int branchCount;
    [Header("Amount of extra single rooms added")]
    public int fillerRooms;
    
    List<Tile> _avalibleTileSpots = new List<Tile>();
    List<Tile> _branch = new List<Tile>();

    bool _startLine = false;
    
    [HideInInspector]
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

    private void Awake()
    {
        _lr = gameObject.AddComponent<LineRenderer>();
        _lr.widthMultiplier = 0.5f;
        // lineRenderer.positionCount = 20;
        _lr = GetComponent<LineRenderer>();
    }


    // Start is called before the first frame update
    void Start()
    {
        _distanceBetweenNodes = myLevelAssetsData.tileSize/2;

        //test = reshuffle(test);
        CreateGrid();

        
    }

    // Update is called once per frame
    void Update()
    {
        if(_startLine && debugPathOn)
            SetLineRenderer();

        _lr.enabled = debugPathOn;
    }

    void SetLineRenderer()
    {
        
        _lr = GetComponent<LineRenderer>();
        for(int t = 0; t < levelPath.Count; t++)
        {
            _lr.SetPosition(t, levelPath[t].transform.position);
        }

        //turn on colored spheres on each tile

        //Debug.Log("running");
    }

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
    
    void CreateGrid()
    {
        _grid2DArray = new GameObject[_levelWidth, _levelHeight];

        //runs throug grid
        for (int rows = 0; rows < _levelWidth; rows++)
        {
            for (int col = 0; col < _levelHeight; col++)
            {
                GameObject nodeTile = new GameObject("Grid_Node");

               // nodeTile.AddComponent<NodeRef>();

                GameObject tilePlaceholderRef = Instantiate(tilePlaceholder, nodeTile.transform.position, nodeTile.transform.rotation);
                tilePlaceholderRef.name = "Tile_" + rows.ToString() + ":" + col.ToString();
                tilePlaceholderRef.transform.parent = nodeTile.transform;
                tilePlaceholderRef.GetComponent<Tile>().posOnGrid = new Vector2(rows, col);
                tilePlaceholderRef.GetComponent<Tile>().hasDoors = hasDoors;

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
                //Debug.Log("Assiging nodeTile: " + nodeTile.name);
                _grid2DArray[rows, col] = nodeTile;
                //inactiveRooms.Add(nodeTile.GetComponent<Tile>());
            }
        }
        AssignNeighbors();
    }

    void AssignNeighbors()
    {
        for (int rows = 0; rows < _levelWidth; rows++)
        {
            for (int col = 0; col < _levelHeight; col++)
            {

                //Debug.Log(rows.ToString() + " " + col.ToString());
                //Debug.Log();
                Tile tile = _grid2DArray[rows, col].transform.GetChild(0).GetComponent<Tile>();
                //Debug.Log(grid2DArray[rows, col].transform.GetChild(0).name);
                // Debug.Log(grid2DArray[rows, col].name);
                int newRef;
 
                //assign this tiles neightbors
                if (rows < _levelWidth - 1)
                {
                     newRef = rows + 1;
                    // Debug.Log("Row: " + newRef + " " + _levelWidth);
                   //  Debug.Log(" right neighbor = " + grid2DArray[newRef, col].transform.GetChild(0).name);
                     tile.rightNeighbor = _grid2DArray[newRef, col].transform.GetChild(0).GetComponent<Tile>();
                 }
                 if (rows > 0)
                 {
                     newRef = rows - 1;
                    // Debug.Log("Row: " + newRef + " " + _levelWidth);
                     //Debug.Log(" left neighbor = " + grid2DArray[newRef, col].transform.GetChild(0).name);
                     tile.leftNeighbor = _grid2DArray[newRef, col].transform.GetChild(0).GetComponent<Tile>();
                 }
                 if(col < _levelHeight - 1)
                 {
                    newRef = col + 1;
                    //Debug.Log("Col: " + newRef + " " + _levelHeight);
                   // Debug.Log(" down neighbor = " + grid2DArray[newRef, col].transform.GetChild(0).name);
                    tile.downNeighbor = _grid2DArray[rows, newRef].transform.GetChild(0).GetComponent<Tile>();
                    //  tile.
                }
                 if(col > 0)
                 {
                    newRef = col - 1;
                    //Debug.Log("Col: " + newRef + " " + _levelHeight);
                    //Debug.Log(" up neighbor = " + grid2DArray[newRef, col].transform.GetChild(0).name);
                    tile.upNeighbor = _grid2DArray[rows, newRef].transform.GetChild(0).GetComponent<Tile>();
                }
            }
        }
       // Debug.Log("Grid Created Drawn and neighbors assigned...");
        MainPathCreation();
    }

    /// <summary>
    /// - Possible setups:
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
    void MainPathCreation()
    {
       // StartCoroutine(Delay());
        ChooseStartEndRooms();
        //GeneratePath();

       


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

        //Debug.Log("starting first");
        //Debug.Log(levelPath[0].posOnGrid.x + " " + levelPath[0].posOnGrid.y);


        int[] nsToCheck = new int[] { 1, 2, 3, 4 };
        nsToCheck = reshuffle(nsToCheck);

        for (int count = 0; count < nsToCheck.Length; count++)
        {
            switch (nsToCheck[count])
            {
                case 1:
                    if (_startTile.upNeighbor != null)
                    {
                        //Debug.Log("Checking starting neighbor...");
                        _startTile.upNeighbor.previousTile = _startTile;
                        count = 4;
                        CheckTile(_startTile.upNeighbor, levelPath);
                    }
                    break;
                case 2:
                    if (_startTile.downNeighbor != null)
                    {
                        //Debug.Log("Checking starting neighbor...");
                        _startTile.downNeighbor.previousTile = _startTile;
                        count = 4;
                        CheckTile(_startTile.downNeighbor, levelPath);
                    }
                    break;
                case 3:
                    if (_startTile.leftNeighbor != null)
                    {
                        //Debug.Log("Checking starting neighbor...");
                        _startTile.leftNeighbor.previousTile = _startTile;
                        count = 4;
                        //canGo = true;
                        CheckTile(_startTile.leftNeighbor, levelPath);
                    }
                    break;
                case 4:
                    if (_startTile.rightNeighbor != null)
                    {
                        //Debug.Log("Checking starting neighbor...");
                        _startTile.rightNeighbor.previousTile = _startTile;
                        count = 4;
                        //canGo = true;
                        CheckTile(_startTile.rightNeighbor, levelPath);
                    }
                    break;
                default:
                    //canGo = true;
                    break;
            }
        
        }

        //Debug.Log("finished while");
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

        //label path in scene from list
        //Debug.Log("Path Finished");
        this.GetComponent<LineRenderer>().positionCount = levelPath.Count;

        //add random rooms to dungeon
        AddRandomRooms();
        //Debug.Log("Added Random Rooms");

        //add Start Room (outside of grid)
        CreateSpawnRoom();


        //this will be removed eventaully
        if (hasDoors)
        {
            ActivateAllDoors();
        }
 


        FinalTileSetup();


        

        _startLine = true;

        //start asset spawning 
        myLevelAssetSpawn.PopulateGrid();
        
        
    }
    
    void CreateSpawnRoom()
    {
        GameObject startingNode = new GameObject();
        startingNode.name = "StartingNode";
        startingNode.transform.parent = this.transform;
        Vector3 spawnPos;
        GameObject tile = null;
        Debug.Log(_side);
        //depending on start tile cords, we add starting room
        switch (_side)
        {
            case spawnRoomSide.right:
                spawnPos = new Vector3(_startTile.transform.position.x - (myLevelAssetsData.tileSize/2), _startTile.transform.position.y, _startTile.transform.position.z);
                tile = Instantiate(tilePlaceholder, spawnPos, _startTile.transform.rotation);
                _playerSpawnPreset = Instantiate(myLevelAssetsData.presetStartingTileAssets[Random.Range(0, myLevelAssetsData.presetStartingTileAssets.Count)], spawnPos, _startTile.transform.rotation);
                tile.GetComponent<Tile>().downNeighbor = _allActiveTiles[0];
                _allActiveTiles[0].upNeighbor = tile.GetComponent<Tile>();
                _playerSpawnPreset.transform.localEulerAngles = new Vector3(tile.transform.localEulerAngles.x, -90, tile.transform.localEulerAngles.z);
                break;
            case spawnRoomSide.left:
                spawnPos = new Vector3(_startTile.transform.position.x + (myLevelAssetsData.tileSize / 2), _startTile.transform.position.y, _startTile.transform.position.z);
                tile = Instantiate(tilePlaceholder, spawnPos, _startTile.transform.rotation);
                _playerSpawnPreset = Instantiate(myLevelAssetsData.presetStartingTileAssets[Random.Range(0, myLevelAssetsData.presetStartingTileAssets.Count)], spawnPos, _startTile.transform.rotation);
                tile.GetComponent<Tile>().upNeighbor = _allActiveTiles[0];
                _allActiveTiles[0].downNeighbor = tile.GetComponent<Tile>();
                _playerSpawnPreset.transform.localEulerAngles = new Vector3(tile.transform.localEulerAngles.x, 90, tile.transform.localEulerAngles.z);
                break;
            case spawnRoomSide.up:
                spawnPos = new Vector3(_startTile.transform.position.x, _startTile.transform.position.y, _startTile.transform.position.z + (myLevelAssetsData.tileSize / 2));
                tile = Instantiate(tilePlaceholder, spawnPos, _startTile.transform.rotation);
                _playerSpawnPreset = Instantiate(myLevelAssetsData.presetStartingTileAssets[Random.Range(0, myLevelAssetsData.presetStartingTileAssets.Count)], spawnPos, _startTile.transform.rotation);
                tile.GetComponent<Tile>().leftNeighbor = _allActiveTiles[0];
                _allActiveTiles[0].rightNeighbor = tile.GetComponent<Tile>();
                break;
            case spawnRoomSide.down:
                spawnPos = new Vector3(_startTile.transform.position.x, _startTile.transform.position.y, _startTile.transform.position.z - (myLevelAssetsData.tileSize / 2));
                tile = Instantiate(tilePlaceholder, spawnPos, _startTile.transform.rotation);
                _playerSpawnPreset = Instantiate(myLevelAssetsData.presetStartingTileAssets[Random.Range(0, myLevelAssetsData.presetStartingTileAssets.Count)], spawnPos, _startTile.transform.rotation);
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
        _lr.positionCount = levelPath.Count;
    }

    void AddRandomRooms()
    {
        
        //chooses random tile, then sees if we can add a room to it
        //if we can we add room otherwise we check again
        //cant add rooms to boss room
       // Debug.Log("Adding Random Rooms");

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

       // Debug.Log("Adding branches...");
        //start with making branches
        //change 1 to branchCount after im done debugging
        for(int branch = 0; branch < branchCount; branch++)
        {
           // Debug.Log("Starting branch...");
            //wont add any more rooms if there are no spaces left to add meaning grid2DArray.Length - levelPath.Count <= 0

            //first we find where this branch will start, need to find how many tiles this branch will be
            //for now imma use the length or level
            //if at any point the path has no where to go, we move on to next branch or exit completely
            int branchlength = Random.Range(1, _levelWidth + 1);
           // Debug.Log("This branch will be " + branchlength + " Length");
            //not sure we want to back track for branches, just end it when the reach a dead end
            
            Tile startingTile = _avalibleTileSpots[Random.Range(0, _avalibleTileSpots.Count)];
            //Debug.Log("removing tile" + startingTile.posOnGrid.x + " " + startingTile.posOnGrid.y);

            _avalibleTileSpots.RemoveAll(obj => obj == startingTile);

            CheckTileBranch(startingTile, _branch, branchlength);
            //Debug.Log("ended branch");

            //DOORS
            if (hasDoors)
            {
                _branch[0].ActivateDoorToPath();
            }
            //activate these rooms
            //Debug.Log(_branch.Count);
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
            }

            //---------------------
            //DOORS
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

        //Debug.Log("Branches added");

        AddSingleRooms();
    }

    void AddSingleRooms()
    {
       // Debug.Log("Adding Single Rooms");

        //Debug.Log(_avalibleTileSpots.Count);
        //Debug.Log(grid2DArray.Length / (_levelWidth * 2));
        //when we add a room, remove from _avalibleTileSpots, add to _allActiveTileSpots

        //default to half the rooms left over
        fillerRooms = Random.Range(1, (_grid2DArray.Length - _allActiveTiles.Count) - ((_grid2DArray.Length - _allActiveTiles.Count)/4));
        //Debug.Log(fillerRooms);

        for (int tileCount = 0; tileCount < fillerRooms; tileCount++)
        {
            if(_grid2DArray.Length - _allActiveTiles.Count >= _grid2DArray.Length/ (_levelWidth * 2))
            {
               // Debug.Log("adding random tile");
                
                Tile current = _avalibleTileSpots[Random.Range(0, _avalibleTileSpots.Count)];
                //Debug.Log(current.posOnGrid.x + " " + current.posOnGrid.y);
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
                Debug.Log("stop adding random tiles");
                return;
            }
        }
        //Debug.Log("Tile Generation Finished, you can now order panda express.");
        //Debug.Log("added single rooms");
    }

    void RemakeAvalibleSpots()
    {
        //Debug.Log("Remaking avalible spots");
        for (int tileC = 0; tileC < _allActiveTiles.Count; tileC++)
        {
            
            Tile current = _allActiveTiles[tileC];
            //Debug.Log("This tile: " + current.posOnGrid.x + " " + current.posOnGrid.y);

            if (current.tileStatus != Tile.TileStatus.boss)
            {
                if (current.upNeighbor != null && current.upNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.upNeighbor) && !_allActiveTiles.Contains(current.upNeighbor))
                {
                    //Debug.Log(current.upNeighbor.posOnGrid.x + " " + current.upNeighbor.posOnGrid.y);
                    _avalibleTileSpots.Add(current.upNeighbor);
                }
                if (current.downNeighbor != null && current.downNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.downNeighbor) && !_allActiveTiles.Contains(current.downNeighbor))
                {
                    //Debug.Log("down");
                    //Debug.Log(current.downNeighbor.posOnGrid.x + " " + current.downNeighbor.posOnGrid.y);
                    _avalibleTileSpots.Add(current.downNeighbor);
                }
                if (current.leftNeighbor != null && current.leftNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.leftNeighbor) && !_allActiveTiles.Contains(current.leftNeighbor))
                {
                    //Debug.Log("left");
                    //Debug.Log(current.leftNeighbor.posOnGrid.x + " " + current.leftNeighbor.posOnGrid.y);
                    _avalibleTileSpots.Add(current.leftNeighbor);
                }
                if (current.rightNeighbor != null && current.rightNeighbor.tileStatus == Tile.TileStatus.nullRoom && !_avalibleTileSpots.Contains(current.rightNeighbor) && !_allActiveTiles.Contains(current.rightNeighbor))
                {
                    //Debug.Log("right");
                    //Debug.Log(current.rightNeighbor.posOnGrid.x + " " + current.rightNeighbor.posOnGrid.y);
                    _avalibleTileSpots.Add(current.rightNeighbor);
                }
            }
        }
    }

    void CheckTileBranch(Tile tile, List<Tile> current, int length)
    {
        
        if (length > 0)
        {
            //Debug.Log("tile: " + tile.posOnGrid.x + " " + tile.posOnGrid.y);
            current.Add(tile);
            tile.tileStatus = Tile.TileStatus.room;

            if ((tile.rightNeighbor == null || tile.rightNeighbor.tileStatus != Tile.TileStatus.nullRoom) && (tile.leftNeighbor == null || tile.leftNeighbor.tileStatus != Tile.TileStatus.nullRoom) && (tile.upNeighbor == null || tile.upNeighbor.tileStatus != Tile.TileStatus.nullRoom) && (tile.downNeighbor == null || tile.downNeighbor.tileStatus != Tile.TileStatus.nullRoom))
            {
                //Debug.Log(tile.posOnGrid.x + " " + tile.posOnGrid.y);
                //if it has no avalible neighbors, we exit this branch
                //current.Add(tile);
                length = 0;
                
               // Debug.Log(length);
                return;
            }
            // Debug.Log("check");
            
            int[] nsToCheck = new int[] { 1, 2, 3, 4 };
            nsToCheck = reshuffle(nsToCheck);

            for (int count = 0; count < nsToCheck.Length; count++)
            {
                //Debug.Log(tile.posOnGrid.x + " " + tile.posOnGrid.y);
                bool up = true, down = true, left = true, right = true;
                switch (nsToCheck[count])
                {
                    case 0:
                        if (tile.upNeighbor != null && tile.upNeighbor.tileStatus == Tile.TileStatus.nullRoom && up == true)
                        {
                            //current.Add(tile);
                            tile.upNeighbor.previousTile = tile;
                            length--;
                           // Debug.Log(length);
                           // Debug.Log("Next tile: " + tile.upNeighbor.posOnGrid.x + " " + tile.upNeighbor.posOnGrid.y);
                            CheckTileBranch(tile.upNeighbor, current, length);
                            return;
                        }
                        else
                            up = false;
                        break;
                    case 1:
                        if (tile.downNeighbor != null && tile.downNeighbor.tileStatus == Tile.TileStatus.nullRoom && down == true)
                        {
                           // Debug.Log("Next tile: " + tile.downNeighbor.posOnGrid.x + " " + tile.downNeighbor.posOnGrid.y);
                            //current.Add(tile);
                            tile.downNeighbor.previousTile = tile;
                            length--;
                            //Debug.Log(length);
                            CheckTileBranch(tile.downNeighbor, current, length);
                            return;
                        }
                        else
                            down = false;
                        break;
                    case 2:
                        if (tile.leftNeighbor != null && tile.leftNeighbor.tileStatus == Tile.TileStatus.nullRoom && left == true)
                        {
                           // Debug.Log("Next tile: " + tile.leftNeighbor.posOnGrid.x + " " + tile.leftNeighbor.posOnGrid.y);
                           // current.Add(tile);
                            tile.leftNeighbor.previousTile = tile;
                            length--;
                           // Debug.Log(length);
                            CheckTileBranch(tile.leftNeighbor, current, length);
                            return;
                        }
                        else
                            left = false;
                        break;
                    case 3:
                        if (tile.rightNeighbor != null && tile.rightNeighbor.tileStatus == Tile.TileStatus.nullRoom && right == true)
                        {
                           // Debug.Log("Next tile: " + tile.rightNeighbor.posOnGrid.x + " " + tile.rightNeighbor.posOnGrid.y);
                            //current.Add(tile);
                            tile.rightNeighbor.previousTile = tile;
                            length--;
                            //Debug.Log(length);
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
    void ActivateAllDoors()
    {
        foreach (Tile t in levelPath)
        {
            t.ActivateDoors();
        }
        DeactivateInActiveRooms();
        //Debug.Log("Activated doors");

        //sync doors to have doors actually connect between tiles
        foreach (Tile t in _allActiveTiles)
        {
            t.SyncDoors();
        }
        //Debug.Log("Synced doors");

        
    }

    


    int failsafeCount = 0;
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
            //lets say we dont make it to the boss room for some logical error that i couldnt find, simply make the end of the levelPath array the boss room
            //Debug.Log("End Point changed to end of levelpath");
            _endTile = levelPath[ levelPath.Count - 1];
            _endTile.tileStatus = Tile.TileStatus.boss;
            _endTile.ShadeBoosRoom();

            return;
        }
        else
        {
            //Debug.Log(c);
            failsafeCount++;
        }

        //Debug.Log("Current Tile checking: " + tile.posOnGrid.x + " " + tile.posOnGrid.y);
        //checks this tile
        //if it has no neighbors or all neighbors are marked as checked go back to previous. mark this tile as checked and unmark path status as path
        if((tile.rightNeighbor == null || tile.rightNeighbor.checkedForPath) && (tile.leftNeighbor== null || tile.leftNeighbor.checkedForPath) && (tile.upNeighbor == null || tile.upNeighbor.checkedForPath) && (tile.downNeighbor == null || tile.downNeighbor.checkedForPath))
        {
            tile.checkedForPath = true;
            tile.ShadeNull();
            //Debug.Log("BACKTRACKING " + tile.posOnGrid.x + " " + tile.posOnGrid.y + " BACKTRACKING...");

            //adds to backtrack history, eventually these tiles will be unchecked so a different path may go through them
            backtrackTempHistory.Add(tile);

            //remove from list if it is on it
            current.Remove(tile);
           // Debug.Log("going to: " + tile.previousTile.posOnGrid.x + " " + tile.previousTile.posOnGrid.y);
            CheckTile(tile.previousTile, current);
        }
        //else if next room is boss room, add this tile and boos room. Mark as path
        else if (tile.rightNeighbor != null && tile.rightNeighbor.tileStatus == Tile.TileStatus.boss)
        {
           // Debug.Log("Found final room...");
            tile.checkedForPath = true;
            current.Add(tile);
            current.Add(tile.rightNeighbor);
        }
        else if (tile.leftNeighbor != null && tile.leftNeighbor.tileStatus == Tile.TileStatus.boss)
        {
           // Debug.Log("Found final room...");
            tile.checkedForPath = true;
            current.Add(tile);
            current.Add(tile.leftNeighbor);
        }
        else if (tile.upNeighbor != null && tile.upNeighbor.tileStatus == Tile.TileStatus.boss)
        {
            //Debug.Log("Found final room...");
            tile.checkedForPath = true;
            current.Add(tile);
            current.Add(tile.upNeighbor);
        }
        else if (tile.downNeighbor != null && tile.downNeighbor.tileStatus == Tile.TileStatus.boss)
        {
           // Debug.Log("Found final room...");
            tile.checkedForPath = true;
            current.Add(tile);
            current.Add(tile.downNeighbor);
        }
        //else check random neighbor, mark this as path
        else {
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
                           // Debug.Log("Next tile: " + tile.upNeighbor.posOnGrid.x + " " + tile.upNeighbor.posOnGrid.y);
                            ClearHistory();
                            CheckTile(tile.upNeighbor, current);
                        }
                        break;
                    case 2:
                        if (tile.downNeighbor != null && tile.downNeighbor.tileStatus != Tile.TileStatus.startingRoom && !tile.downNeighbor.checkedForPath && tile.downNeighbor.tileStatus == Tile.TileStatus.nullRoom)
                        {
                            tile.downNeighbor.previousTile = tile;
                           // Debug.Log("Next tile: " + tile.downNeighbor.posOnGrid.x + " " + tile.downNeighbor.posOnGrid.y);
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
                           // Debug.Log("Next tile: " + tile.leftNeighbor.posOnGrid.x + " " + tile.leftNeighbor.posOnGrid.y);
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
                            //Debug.Log("Next tile: " + tile.rightNeighbor.posOnGrid.x + " " + tile.rightNeighbor.posOnGrid.y);
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

    void ClearHistory()
    {
        //if(backtrackTempHistory.)
        foreach (Tile tile in backtrackTempHistory)
        {
            tile.checkedForPath = false;
        }
    }

    enum spawnRoomSide
    {
        none,
        right,
        left,
        up,
        down
    }
    spawnRoomSide _side;
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

        //Debug.Log(startX + " " + startY);
        _startTile = _grid2DArray[startX, startY].transform.GetChild(0).GetComponent<Tile>();
        _startTile.tileStatus = Tile.TileStatus.startingRoom;
        _startTile.ShadeStarting();
        //Debug.Log(_startTile.posOnGrid.x + " " + _startTile.posOnGrid.y);

        //end tile must be opposite side of start to ensure max tile coverage between the two
        int endX, endY, endXF, endYF;
        //flipped values
        endX = _levelWidth - startX - 1;
        endY = _levelHeight - startY - 1;
        //Debug.Log("Potential end: " + endX + " " +endY);

        //add a little variation so boss room can anywhere in that quarter
        int xBuffer = _levelWidth / 2;
        endXF = endX + Random.Range(-xBuffer + 1, xBuffer - 1);
        //Debug.Log(endXF);
        int yBuffer = _levelHeight / 2;
        endYF = endY + Random.Range(-yBuffer + 1, yBuffer - 1);
        // Debug.Log(endYF);

        //should always try to keep a minimum distance from start (the xBuffer), cant be on same x as buffer

        while (endXF > _levelWidth - 1 || endXF == startX || endXF < 0)
        {
            endXF = endX + Random.Range(-xBuffer + 1, xBuffer);
        }
        while (endYF > _levelHeight - 1 || endYF == startY || endYF < 0)
        {
            endYF = endY + Random.Range(-yBuffer + 1, yBuffer);
        }
        //Debug.Log("check");
        //Debug.Log(endXF);
        //Debug.Log(endYF);
        //yield return new WaitForSeconds(0.1f);
        //in case the start point is towards the middle and the end point is also in the middle, really close to each other
        //check if Mathf.Abs(endXF - startX) < xbuffer && Mathf.Abs(endYF - startY)
        //NORMALLY THIS IS A WHILE, but there is some edge case causing issues - will investigate later
        int oldX, oldY;
        while (Mathf.Abs(endXF - startX) < xBuffer && Mathf.Abs(endYF - startY) < yBuffer)
        {
            oldX = endXF;
            oldY = endYF;
            //Debug.Log("REROLLING");
            //yield return new WaitForSeconds(0.25f);
            //chooses then either reroll x or y (50-50 chance to reroll either one
            if (Random.value < 0.5f)
            {
                endXF = endX + Random.Range(-xBuffer + 1, xBuffer);
                // for (int x = 0; x < 4; x++)
                // {
                while (endXF > _levelWidth - 1 || endXF == startX || endXF < 0 || endXF == oldX)
                {
                    oldX = endXF;
                    endXF = endX + Random.Range(-xBuffer + 1, xBuffer);
                    // break;
                }
                //}
            }
            else
            {
                endYF = endY + Random.Range(-yBuffer + 1, yBuffer);
                //for (int x = 0; x < 4; x++)
                //{
                while (endYF > _levelHeight - 1 || endYF == startY || endYF < 0 || endYF == oldY)
                {
                    oldY = endYF;
                    endYF = endY + Random.Range(-yBuffer + 1, yBuffer);
                    // break;
                }
                //}
            }
        }
        // Debug.Log("End Point: " + endXF + " " + endYF);
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
               // Debug.Log(tile.name);
                Destroy(tile);
            }
            //turn on walls at borders of path handled in levelassetspawn

        }
    }

    /// <summary>
    /// ---------------------------------------------
    /// NOT IN USE
    /// ---------------------------------------------
    /// </summary>
    IEnumerator Delay()
    {
        //first we get the start room and end room
        int startX = 0; //= Random.Range(0, _levelWidth);
        int startY = 0; //= Random.Range(0, _levelHeight);

        //can either be (0,x), (x, 0), (max, x), (x, max)
        int num = Random.Range(0, 4);
        switch (num)
        {
            case 1:
                startX = 0;
                startY = Random.Range(0, _levelHeight);
                break;
            case 2:
                startX = Random.Range(0, _levelWidth);
                startY = 0;
                break;
            case 3:
                startX = _levelWidth - 1;
                startY = Random.Range(0, _levelHeight);
                break;
            case 4:
                startX = Random.Range(0, _levelWidth);
                startY = _levelHeight - 1;
                break;
            default:
                break;
        }

        //Debug.Log(startX + " " + startY);
        _startTile = _grid2DArray[startX, startY].transform.GetChild(0).GetComponent<Tile>();
        _startTile.tileStatus = Tile.TileStatus.startingRoom;
        _startTile.ShadeStarting();
        //Debug.Log(_startTile.posOnGrid.x + " " + _startTile.posOnGrid.y);

        //end tile must be opposite side of start to ensure max tile coverage between the two
        int endX, endY, endXF, endYF;
        //flipped values
        endX = _levelWidth - startX - 1;
        endY = _levelHeight - startY - 1;
        //Debug.Log("Potential end: " + endX + " " +endY);

        //add a little variation so boss room can anywhere in that quarter
        int xBuffer = _levelWidth / 2;
        endXF = endX + Random.Range(-xBuffer + 1, xBuffer - 1);
        //Debug.Log(endXF);
        int yBuffer = _levelHeight / 2;
        endYF = endY + Random.Range(-yBuffer + 1, yBuffer - 1);
        // Debug.Log(endYF);

        //should always try to keep a minimum distance from start (the xBuffer), cant be on same x as buffer

        while (endXF > _levelWidth - 1 || endXF == startX || endXF < 0)
        {
            endXF = endX + Random.Range(-xBuffer + 1, xBuffer);
        }
        while (endYF > _levelHeight - 1 || endYF == startY || endYF < 0)
        {
            endYF = endY + Random.Range(-yBuffer + 1, yBuffer);
        }
        //Debug.Log("check");
        //Debug.Log(endXF);
        //Debug.Log(endYF);
        yield return new WaitForSeconds(0.1f);
        //in case the start point is towards the middle and the end point is also in the middle, really close to each other
        //check if Mathf.Abs(endXF - startX) < xbuffer && Mathf.Abs(endYF - startY)
        //NORMALLY THIS IS A WHILE, but there is some edge case causing issues - will investigate later
        int oldX, oldY;
        while (Mathf.Abs(endXF - startX) < xBuffer && Mathf.Abs(endYF - startY) < yBuffer)
        {
            oldX = endXF;
            oldY = endYF;
            //Debug.Log("REROLLING");
            yield return new WaitForSeconds(0.25f);
            //chooses then either reroll x or y (50-50 chance to reroll either one
            if (Random.value < 0.5f)
            {
                endXF = endX + Random.Range(-xBuffer + 1, xBuffer);
                // for (int x = 0; x < 4; x++)
                // {
                while (endXF > _levelWidth - 1 || endXF == startX || endXF < 0 || endXF == oldX)
                {
                    oldX = endXF;
                    endXF = endX + Random.Range(-xBuffer + 1, xBuffer);
                    // break;
                }
                //}
            }
            else
            {
                endYF = endY + Random.Range(-yBuffer + 1, yBuffer);
                //for (int x = 0; x < 4; x++)
                //{
                while (endYF > _levelHeight - 1 || endYF == startY || endYF < 0 || endYF == oldY)
                {
                    oldY = endYF;
                    endYF = endY + Random.Range(-yBuffer + 1, yBuffer);
                    // break;
                }
                //}
            }
        }
        // Debug.Log("End Point: " + endXF + " " + endYF);
        _endTile = _grid2DArray[endXF, endYF].transform.GetChild(0).GetComponent<Tile>();
        _endTile.tileStatus = Tile.TileStatus.boss;
        _endTile.ShadeBoosRoom();

        //Debug.Log("Generating Main Path...");
        GeneratePath();
    }
}




