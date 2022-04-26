using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using AStarSharp;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    public class Room
    {
        public String type;
        public bool mandatory;

        public Room(String roomType, bool isMandatory)
        {
            type = roomType;
            mandatory = isMandatory;
        }
    }

    public class Building
    {
        public Vector2Int center;
        public Vector2Int widthHeight;

        public Building(Vector2Int newCenter, Vector2Int newWidthHeight)
        {
            center = newCenter;
            widthHeight = newWidthHeight;
        }
    }
    
    public class Roof
    {
        public List<Vector2Int> positions;
        public Tilemap tiles;
        public TilemapRenderer tileRenderer;

        public Roof()
        {
            positions = new List<Vector2Int>();
            GameObject roofObject = BoardManager.CreateTilemap("Roof", new Vector3(0, -0.25f, 0));
            tiles = roofObject.GetComponent<Tilemap>();
            tileRenderer = roofObject.GetComponent<TilemapRenderer>();
        }
    }

    public class Connections
    {
        public bool up;
        public bool down;
        public bool left;
        public bool right;

        public Connections(bool isUp, bool isDown, bool isLeft, bool isRight) {
            up = isUp;
            down = isDown;
            left = isLeft;
            right = isRight;
        }
    }

    public int columns;
    public int rows;
    public Count objectCount;
    public Count enemyCount;
    public Count buildingCount;
    public Vector2Int buildingRoomGrid;
    public Count roomLengthCount;
    public Count buildingDistance;
    public Count pathRadius;
    public Count pondRadius;

    public GameObject[] players;
    private GameObject[] enemies;
    public GameObject[] forestEnemies;
    public GameObject[] winterEnemies;
    private GameObject[] trees;
    public GameObject[] forestTrees;
    public GameObject[] winterTrees;
    private GameObject[] natureObjects;
    public GameObject[] forestNatureObjects;
    public GameObject[] winterNatureObjects;
    public GameObject[] desertNatureObjects;
    public GameObject[] volcanicNatureObjects;
    private GameObject[] streetObjects;
    public GameObject[] forestStreetObjects;
    public GameObject[] winterStreetObjects;
    public GameObject[] desertStreetObjects;

    public GameObject wall;
    public GameObject basicDoor;
    public GameObject keyDoor;
    public GameObject triggerDoor;
    public GameObject[] storage;
    public GameObject[] shelves;
    public GameObject[] bar;
    public GameObject[] atBack;
    public GameObject table;
    public GameObject chair;
    public GameObject bed;
    public GameObject stool;
    public GameObject stove;

    public RuleTile emptyTile;
    private RuleTile groundTile;
    public RuleTile dirtTile;
    public RuleTile sandTile;
    public RuleTile lavaRockTile;
    public RuleTile grassTile;
    public RuleTile snowTile;
    public RuleTile roofTile;
    public RuleTile floorTile;
    private RuleTile pathTile;
    public RuleTile brickPathTile;
    public RuleTile cobblestonePathTile;
    public RuleTile sandstonePathTile;
    private RuleTile waterTile;
    public RuleTile pondTile;
    public RuleTile iceTile;
    public RuleTile holeTile;
    public RuleTile lavaTile;
    public RuleTile flowerTile;
    public RuleTile mushroomTile;
    public RuleTile waterFloraTile;
    
    public List<Vector2Int> spawnPositions;

    private List<Vector3Int> gridPositions;
    private List<List<Node>> Grid;
    private Dictionary<String, Room[]> buildings;
    private List<Roof> roofs;
    private List<Vector2Int> pathPositions;
    private Tilemap bottomTilemap;
    private Tilemap grassTilemap;
    private RuleTile middleTile;
    private Tilemap overGroundTilemap;
    private Tilemap plantTilemap;
    private Tilemap floorTilemap;

    public List<String> biomes;
    private String biome;

    public Material leaf;
    public Material snowflake;
    public Material sand;
    public Material ember;

    void InitializeList()
    {
        gridPositions = new List<Vector3Int>();
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                gridPositions.Add(new Vector3Int(x, y, y));
            }
        }
    }

    private void BoardSetup()
    {

        Grid = new List<List<Node>>();
        bottomTilemap = CreateTilemap("Bottom").GetComponent<Tilemap>();
        grassTilemap = CreateTilemap("Grass").GetComponent<Tilemap>();
        overGroundTilemap = CreateTilemap("OverGround").GetComponent<Tilemap>();
        plantTilemap = CreateTilemap("Plants").GetComponent<Tilemap>();
        floorTilemap = CreateTilemap("Floor").GetComponent<Tilemap>();
        roofs = new List<Roof>();
        pathPositions = new List<Vector2Int>();

        biome = biomes[Random.Range(0, biomes.Count)];

        switch (biome)
        {
        case "Forest":
            groundTile = dirtTile;
            middleTile = grassTile;
            enemies = forestEnemies;
            trees = forestTrees;
            natureObjects = forestNatureObjects;
            streetObjects = forestStreetObjects;
            pathTile = brickPathTile;
            waterTile = pondTile;
            UpdateParticles(leaf);
            break;
        case "Winter":
            groundTile = dirtTile;
            middleTile = snowTile;
            enemies = winterEnemies;
            trees = winterTrees;
            natureObjects = winterNatureObjects;
            streetObjects = winterStreetObjects;
            pathTile = cobblestonePathTile;
            waterTile = iceTile;
            UpdateParticles(snowflake);
            break;
        case "Desert":
            groundTile = sandTile;
            middleTile = emptyTile;
            enemies = forestEnemies;
            natureObjects = desertNatureObjects;
            streetObjects = desertStreetObjects;
            pathTile = sandstonePathTile;
            waterTile = holeTile;
            UpdateParticles(sand);
            break;
        case "Volcanic":
            groundTile = lavaRockTile;
            middleTile = emptyTile;
            enemies = winterEnemies;
            natureObjects = volcanicNatureObjects;
            waterTile = lavaTile;
            UpdateParticles(ember);
            break;
        default:
            groundTile = dirtTile;
            middleTile = grassTile;
            enemies = forestEnemies;
            trees = forestTrees;
            natureObjects = forestNatureObjects;
            streetObjects = forestStreetObjects;
            pathTile = brickPathTile;
            waterTile = pondTile;
            UpdateParticles(leaf);
            break;
        }

        for (int x = -1; x <= columns; x++)
        {
            if (x >= 0 && x < columns)
                Grid.Add(new List<Node>());
            for (int y = -1; y <= rows; y++)
            {
                bottomTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
                if (x >= 0 && x < columns && y >= 0 && y < rows)
                {
                    Grid[x].Add(new Node(new Vector2(x, y), true));
                }
            }
        }
    }

    private void UpdateParticles(Material material) {
        GameObject.Find("Particles").GetComponent<ParticleSystem>().GetComponent<Renderer>().material = material;
    }

    private static GameObject CreateTilemap(String name, Vector3 tileAnchor = default(Vector3)) {
        GameObject newObject = new GameObject(name);
        Tilemap tiles = newObject.AddComponent<Tilemap>();
        tiles.tileAnchor = tileAnchor;
        TilemapRenderer tileRenderer = newObject.AddComponent<TilemapRenderer>();
        tileRenderer.sortingLayerName = name;
        tiles.transform.SetParent(GameManager.instance.GetComponent<Grid>().transform);
        return newObject;
    }

    Node GetNode(Vector2 tile)
    {
        return Grid[(int)tile.x][(int)tile.y];
    }

    Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    GameObject RandomObject(GameObject[] objects)
    {
        return objects[Random.Range(0, objects.Length)];
    }

    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        int objectCount = Random.Range(minimum, maximum + 1);

        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = RandomPosition();
            GameObject tileChoice = RandomObject(tileArray);
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    void LayoutObjectAtCorners(GameObject[] tileArray)
    {
        for (int i = 0; i < gridPositions.Count; i++)
        {
            Vector3Int position = gridPositions[i];
            double spawnChance = Math.Pow((Math.Max(position.x, columns - position.x) * Math.Max(position.y, rows - position.y)), 3) / Math.Pow((columns * rows), 3);
            if (Random.value < spawnChance)
            {
                GameObject tileChoice = RandomObject(tileArray);
                Instantiate(tileChoice, position, Quaternion.identity);
                PlaceTiles(grassTilemap, middleTile, (Vector2Int) position, 3);
                gridPositions.RemoveAt(i);
            }
        }
    }

    private void SpawnPlayers()
    {
        int i = 0;
        foreach (GameObject player in players) {
            if (i >= spawnPositions.Count)
                return;
            Vector3Int position = new Vector3Int(spawnPositions[i].x, spawnPositions[i].y, spawnPositions[i].y);
            Instantiate(player, position, Quaternion.identity);
            PlaceTiles(grassTilemap, middleTile, (Vector2Int) position, 3);
            gridPositions.Remove(position);
            pathPositions.Add(spawnPositions[i]);
            i++;
        }
    }

    private void SpawnBuildings()
    {
        int targetCount = Random.Range(buildingCount.minimum, buildingCount.maximum + 1);
        List<Building> buildings = new List<Building>() { new Building(new Vector2Int(columns/2,rows/2), new Vector2Int(0,0)) };

        while (targetCount > 0) {
            targetCount--;
            int[,] rooms = GenerateBuildingLayout();
            int roomLength = Random.Range(roomLengthCount.minimum, roomLengthCount.maximum + 1);

            bool placed = false;
            for (int i = 0; i < buildings.Count; i++) {
                for (int j = 0; j < 8; j++) {
                    Vector2Int center = buildings[i].center;
                    if (j == 1 || j == 2 || j == 3)
                        center = center + new Vector2Int(buildings[i].widthHeight.x/2 + Random.Range(buildingDistance.minimum, buildingDistance.maximum + 1), 0);
                    else if (j == 5 || j == 6 || j == 7)
                        center = center + new Vector2Int(0 - buildings[i].widthHeight.x/2 - Random.Range(buildingDistance.minimum, buildingDistance.maximum + 1), 0);
                    if (j == 7 || j == 0 || j == 1)
                        center = center + new Vector2Int(0, buildings[i].widthHeight.y/2 + Random.Range(buildingDistance.minimum, buildingDistance.maximum + 1));
                    else if (j == 3 || j == 4 || j == 5)
                        center = center + new Vector2Int(0, 0 - buildings[i].widthHeight.y/2 - Random.Range(buildingDistance.minimum, buildingDistance.maximum + 1));
                    placed = CheckBuilding(center, rooms, roomLength);
                    if (placed) {
                        pathPositions.Add(LayoutBuilding(center, rooms, roomLength));
                        buildings.Add(new Building(center, GetBuildingWidthHeight(rooms, roomLength)));
                        if (buildings[0].widthHeight == new Vector2Int(0,0))
                            buildings.RemoveAt(0);
                        break;
                    }
                }
                if (placed)
                    break;
            }
        }
    }

    private int[,] GenerateBuildingLayout()
    {
        int room = 1;
        int[,] rooms = new int[buildingRoomGrid.x, buildingRoomGrid.y];
        Vector2Int next = new Vector2Int(Random.Range(0, buildingRoomGrid.x), Random.Range(0, buildingRoomGrid.y));
        rooms[next.x, next.y] = room;

        List<Vector2Int> added = new List<Vector2Int>();
        List<Vector2Int> directions = new List<Vector2Int>() { new Vector2Int(0,1), new Vector2Int(0,-1), new Vector2Int(1,0), new Vector2Int(-1,0) };

        for (int i = 0; i < buildingRoomGrid.x * buildingRoomGrid.y; i++) {
            added.Add(next);
            Vector2Int randomRoom = added[Random.Range(0, added.Count)];
            next = randomRoom + directions[Random.Range(0, directions.Count)];
            if (!added.Contains(next) && InRange(next, new Vector2(0,0), buildingRoomGrid)) {
                bool merge = Random.Range(0, 2) == 0;
                if (!merge) {
                    room++;
                    rooms[next.x, next.y] = room;
                } else {
                    try {
                        rooms[next.x, next.y] = rooms[randomRoom.x, randomRoom.y];
                    } catch {
                        //Debug.Log(next.x + ", " + next.y + " | " + randomRoom.x + ", " + randomRoom.y);
                    }
                }
            }
        }

        return rooms;
    }

    private bool RoomHasNeighbor(int x, int y, int[,] rooms) {
        bool neighbor = false;
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector2Int coords = new Vector2Int(x + i, y + j);
                if (((i == 0 && j != 0) || (j == 0 && i != 0)) && InRange(coords, new Vector2(0,0), buildingRoomGrid))
                {
                    if (rooms[coords.x, coords.y] > 0)
                        neighbor = true;
                    //Debug.Log("Room: (" + x + ", " + y + ") -> " + rooms[x, y] + "; Other: (" + coords.x + ", " + coords.y + ") -> " + rooms[coords.x, coords.y]);
                }
            }
        }
        return neighbor;
    }

    private List<Vector2Int> GetUnmergedNeighbors (int x, int y, int[,] rooms) {
        List<Vector2Int> unmergedNeighbors = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector2Int coords = new Vector2Int(x + i, y + j);
                if (((i == 0 && j != 0) || (j == 0 && i != 0)) && InRange(coords, new Vector2(0,0), buildingRoomGrid))
                {
                    if (rooms[coords.x, coords.y] != 0 && !RoomHasMerged(coords.x, coords.y, rooms))
                        unmergedNeighbors.Add(coords);
                }
            }
        }
        return unmergedNeighbors;
    }

    private List<Vector2Int> GetMergedNeighbors (int x, int y, int[,] rooms) {
        List<Vector2Int> mergedNeighbors = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector2Int coords = new Vector2Int(x + i, y + j);
                if (((i == 0 && j != 0) || (j == 0 && i != 0)) && InRange(coords, new Vector2(0,0), buildingRoomGrid))
                {
                    if (rooms[x,y] > 0 && rooms[coords.x, coords.y] == rooms[x,y])
                        mergedNeighbors.Add(coords);
                }
            }
        }
        return mergedNeighbors;
    }

    private bool RoomHasMerged(int x, int y, int[,] rooms) {
        bool merged = false;
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                Vector2Int coords = new Vector2Int(x + i, y + j);
                if (((i == 0 && j != 0) || (j == 0 && i != 0)) && InRange(coords, new Vector2(0,0), buildingRoomGrid))
                {
                    if (rooms[coords.x, coords.y] == rooms[x, y])
                        merged = true;
                }
            }
        }
        return merged;
    }

    private Vector2Int GetBuildingWidthHeight(int[,] rooms, int roomLength)
    {
        int minX = buildingRoomGrid.x;
        int maxX = 0;
        int minY = buildingRoomGrid.y;
        int maxY = 0;
        for (int x = 0; x < buildingRoomGrid.x; x++) {
            for (int y = 0; y < buildingRoomGrid.y; y++) {
                if (rooms[x, y] > 0) {
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
            }
        }
        int width = (maxX - minX + 1) * roomLength - (maxX - minX);
        int height = (maxY - minY + 1) * roomLength - (maxY - minY);
        return new Vector2Int(width, height);
    }

    private Vector2Int GetBuildingMinXY(int[,] rooms, int roomLength)
    {
        int minX = buildingRoomGrid.x;
        int minY = buildingRoomGrid.y;
        for (int x = 0; x < buildingRoomGrid.x; x++) {
            for (int y = 0; y < buildingRoomGrid.y; y++) {
                if (rooms[x, y] > 0) {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                }
            }
        }
        return new Vector2Int(minX, minY);
    }

    private bool CheckBuilding(Vector2Int center, int[,] rooms, int roomLength)
    {
        Vector2Int widthHeight = GetBuildingWidthHeight(rooms, roomLength);
        Vector2Int minXY = GetBuildingMinXY(rooms, roomLength);
        Vector2Int bottomLeft = new Vector2Int(center.x - widthHeight.x/2, center.y - widthHeight.y/2);
        //Debug.Log(widthHeight.x + ", " + widthHeight.y + " " + bottomLeft.x + ", " + bottomLeft.y + " " + (bottomLeft.x + widthHeight.x) + ", " + (bottomLeft.y + widthHeight.y));
        List<int> completed = new List<int>() { 0 };

        for (int x = 0; x < buildingRoomGrid.x; x++) {
            for (int y = 0; y < buildingRoomGrid.y; y++) {
                if (completed.Contains(rooms[x,y]))
                    continue;
                List<Vector2Int> mergedRoom = GetMergedNeighbors(x, y, rooms);
                mergedRoom.Add(new Vector2Int(x,y));
                if (!CheckRoom(bottomLeft, mergedRoom, roomLength, minXY))
                    return false;
                completed.Add(rooms[x,y]);
            }
        }

        return true;
    }

    private bool CheckRoom(Vector2Int bottomLeft, List<Vector2Int> mergedRoom, int roomLength, Vector2Int minXY)
    {
        foreach (Vector2Int cell in mergedRoom)
        {
            Vector2Int newCell = cell - minXY;
            Vector2Int start = new Vector2Int(bottomLeft.x + (newCell.x * roomLength), bottomLeft.y + (newCell.y * roomLength));
            Vector2Int end = new Vector2Int(bottomLeft.x + ((newCell.x + 1) * roomLength), bottomLeft.y + ((newCell.y + 1) * roomLength));
            Vector2Int offset = new Vector2Int(buildingRoomGrid.x/2 - newCell.x - 1, buildingRoomGrid.y/2 - newCell.y - 1);
            Vector2Int spacing = new Vector2Int(buildingDistance.minimum, buildingDistance.minimum);
            start = start + offset - spacing;
            end = end + offset + spacing;
            //Debug.Log("Start: " + start + ", End: " + end);

            for (int x = start.x; x < end.x; x++) {
                for (int y = start.y; y < end.y; y++) {
                    Vector3Int position = new Vector3Int(x, y, y);
                    if (!gridPositions.Contains(position)) {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private Vector2Int LayoutBuilding(Vector2Int center, int[,] rooms, int roomLength)
    {
        Roof roof = new Roof();
        roofs.Add(roof);

        Vector2Int widthHeight = GetBuildingWidthHeight(rooms, roomLength);
        Vector2Int minXY = GetBuildingMinXY(rooms, roomLength);
        Vector2Int bottomLeft = new Vector2Int(center.x - widthHeight.x/2, center.y - widthHeight.y/2);
        List<int> completed = new List<int>() { 0 };
        Vector2Int placedFrontDoor = new Vector2Int(0,0);

        for (int x = 0; x < buildingRoomGrid.x; x++) {
            for (int y = 0; y < buildingRoomGrid.y; y++) {
                if (completed.Contains(rooms[x,y]))
                    continue;
                List<Vector2Int> mergedRoom = GetMergedNeighbors(x, y, rooms);
                mergedRoom.Add(new Vector2Int(x,y));
                placedFrontDoor = LayoutRoom(bottomLeft, rooms, mergedRoom, roomLength, minXY, placedFrontDoor, roof);
                completed.Add(rooms[x,y]);
            }
        }

        return placedFrontDoor;
    }

    private Vector2Int LayoutRoom(Vector2Int bottomLeft, int[,] rooms, List<Vector2Int> mergedRoom, int roomLength, Vector2Int minXY, Vector2Int placedFrontDoor, Roof roof)
    {
        foreach (Vector2Int cell in mergedRoom)
        {
            Connections cells = new Connections(false, false, false, false);
            foreach (Vector2Int other in mergedRoom)
            {
                if (cell + new Vector2Int(-1, 0) == other)
                    cells.left = true;
                if (cell + new Vector2Int(1, 0) == other)
                    cells.right = true;
                if (cell + new Vector2Int(0, 1) == other)
                    cells.up = true;
                if (cell + new Vector2Int(0, -1) == other)
                    cells.down = true;
            }

            Connections otherRooms = new Connections(false, false, false, false);
            for (int x = 0; x < buildingRoomGrid.x; x++)
            {
                for (int y = 0; y < buildingRoomGrid.y; y++)
                {
                    if (rooms[x, y] == 0)
                        continue;
                    Vector2Int other = new Vector2Int(x, y);
                    if (cell + new Vector2Int(-1, 0) == other)
                        otherRooms.left = true;
                    if (cell + new Vector2Int(1, 0) == other)
                        otherRooms.right = true;
                    if (cell + new Vector2Int(0, 1) == other)
                        otherRooms.up = true;
                    if (cell + new Vector2Int(0, -1) == other)
                        otherRooms.down = true;
                }
            }

            Vector2Int newCell = cell - minXY;
            Vector2Int start = new Vector2Int(bottomLeft.x + (newCell.x * roomLength), bottomLeft.y + (newCell.y * roomLength));
            Vector2Int end = new Vector2Int(bottomLeft.x + ((newCell.x + 1) * roomLength), bottomLeft.y + ((newCell.y + 1) * roomLength));
            Vector2Int offset = new Vector2Int(buildingRoomGrid.x / 2 - newCell.x - 1, buildingRoomGrid.y / 2 - newCell.y - 1);
            start = start + offset;
            end = end + offset;

            for (int x = start.x; x < end.x; x++)
            {
                for (int y = start.y; y < end.y; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, y);

                    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(new Vector2(x, y), 0.5f);
                    bool visited = false;
                    foreach (Collider2D hitCollider in hitColliders)
                    {
                        Wall wall = hitCollider.GetComponent<Wall>();
                        if (wall != null)
                        {
                            if (x == (start.x * 2 + roomLength) / 2 || x == (end.x * 2 - roomLength) / 2 || y == (start.y * 2 + roomLength) / 2 || y == (end.y * 2 - roomLength) / 2)
                            {
                                GameObject.Destroy(wall.gameObject);
                                Instantiate(Random.Range(0, 4) == 0 ? keyDoor : basicDoor, position, Quaternion.identity);
                                gridPositions.Remove(position);
                                gridPositions.Remove(position - new Vector3Int(0, 1, 1));
                                gridPositions.Remove(position + new Vector3Int(0, 1, 1));
                                gridPositions.Remove(position - new Vector3Int(1, 0, 0));
                                gridPositions.Remove(position + new Vector3Int(1, 0, 0));
                            }
                            visited = true;
                        }
                    }
                    if (visited)
                        continue;

                    if (otherRooms.up || cells.up || y != end.y - 1)
                        floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);

                    if ((!cells.left && x == start.x) || (!cells.right && x == end.x - 1) || (!cells.up && y == end.y - 1) || (!cells.down && y == start.y))
                    {
                        if (placedFrontDoor == new Vector2Int(0, 0) && gridPositions.Contains(position - new Vector3Int(0, 1, 1)) && !otherRooms.down && y == start.y && (x == (start.x * 2 + roomLength) / 2 || x == (end.x * 2 - roomLength) / 2))
                        {
                            Instantiate(basicDoor, position, Quaternion.identity);
                            placedFrontDoor = new Vector2Int(position.x, position.y);
                            gridPositions.Remove(position);
                            gridPositions.Remove(position - new Vector3Int(0, 1, 1));
                            gridPositions.Remove(position + new Vector3Int(0, 1, 1));
                        }
                        else
                        {
                            GameObject newWall = Instantiate(wall, position, Quaternion.identity);
                            if (gridPositions.Contains(position - new Vector3Int(0, 1, 1)) && !otherRooms.down && y == start.y)
                                newWall.GetComponent<Wall>().IsFront();
                        }
                    }
                    else if (x > start.x && x < end.x - 1)
                    {
                        if (!cells.up && y == end.y - 2 && (!otherRooms.up || (x != (start.x * 2 + roomLength) / 2 && x != (end.x * 2 - roomLength) / 2)))
                        {
                            if (Random.Range(0, 3) > 0)
                            {
                                Instantiate(RandomObject(atBack), position, Quaternion.identity);
                                gridPositions.Remove(position);
                            }
                        }
                    }

                    Grid[position.x][position.y] = new Node(new Vector2(position.x, position.y), true, 2);
                    roof.positions.Add(new Vector2Int(x, y));
                    roof.tiles.SetTile(new Vector3Int(x, y + 1, 0), roofTile);
                }
            }

            if (Random.Range(0, 2) == 0)
            {
                Vector2Int tablePos = new Vector2Int(-2, -2);
                while (!gridPositions.Contains(new Vector3Int(tablePos.x, tablePos.y, tablePos.y)))
                {
                    tablePos = new Vector2Int(Random.Range(start.x + 1, end.x - 1), Random.Range(start.y + 1, end.y - 2));
                }
                PlaceTable(tablePos, start, end);
            }

            for (int x = start.x; x < end.x; x++)
            {
                for (int y = start.y; y < end.y; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, y);
                    if (gridPositions.Contains(position))
                        gridPositions.Remove(position);
                }
            }
        }

        return placedFrontDoor;
    }

    private void SpawnPaths()
    {
        List<List<Node>> pathGrid = new List<List<Node>>();
        for (int x = 0; x < columns; x++)
        {
            pathGrid.Add(new List<Node>());
            for (int y = 0; y < rows; y++)
            {
                pathGrid[x].Add(new Node(new Vector2(x, y), Grid[x][y].Weight == 1, 100));
            }
        }
        Astar astar = new Astar(pathGrid);

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        visited.Add(pathPositions[0]);
        pathPositions.Remove(pathPositions[0]);
        while (pathPositions.Count > 0)
        {
            Vector2Int start = pathPositions[0];
            pathPositions.Remove(start);
            Stack<Node> stack = astar.FindPath(start, visited.ToList());
            List<Node> path;
            if (stack == null)
                break;
            path = stack.ToList();
            for (int i = 0; i < path.Count; i++)
            {
                Node node = path[i];
                Vector2Int newPos = new Vector2Int((int)node.Position.x, (int)node.Position.y);
                if (!visited.Contains(newPos))
                {
                    int radius = Random.Range(pathRadius.minimum, pathRadius.maximum + 1);
                    Vector2Int prevPos = (i == 0) ? new Vector2Int(-2, -2) : new Vector2Int((int)path[i - 1].Position.x, (int)path[i - 1].Position.y);
                    Vector2Int nextPos = (i == path.Count - 1) ? new Vector2Int(-2, -2) : new Vector2Int((int)path[i + 1].Position.x, (int)path[i + 1].Position.y);
                    Connections connections = new Connections(
                        newPos + new Vector2Int(0, 1) == prevPos || newPos + new Vector2Int(0, 1) == nextPos,
                        newPos + new Vector2Int(0, -1) == prevPos || newPos + new Vector2Int(0, -1) == nextPos,
                        newPos + new Vector2Int(-1, 0) == prevPos || newPos + new Vector2Int(-1, 0) == nextPos,
                        newPos + new Vector2Int(1, 0) == prevPos || newPos + new Vector2Int(1, 0) == nextPos);
                    PlaceTiles(overGroundTilemap, pathTile, newPos, radius, true, connections);
                    pathGrid[newPos.x][(int)newPos.y] = new Node(new Vector2(newPos.x, newPos.y), true, 0);
                    visited.Add(newPos);
                }
                astar = new Astar(pathGrid);
            }
        }
    }

    void BuildWall(Wall wall, Vector2Int start, Vector2Int stop)
    {
        if (start.y == stop.y)
        {
            int x = start.x;
            while (x != stop.x + 1)
            {
                Instantiate(wall, new Vector3(x, start.y, start.y), Quaternion.identity);
                x += (stop.x > start.x) ? 1 : -1;
            }
        }
        else if (start.x == stop.x)
            {
                int y = start.y;
                while (y != stop.y + 1)
                {
                    Instantiate(wall, new Vector3(start.x, y, y), Quaternion.identity);
                    y += (stop.y > start.y) ? 1 : -1;
                }
            }
    }

    void SpawnPonds()
    {
        for (int i = 0; i < gridPositions.Count; i++)
        {
            Vector3Int position = gridPositions[i];
            float power = (biome == "Volcanic") ? 0.2f : 3;
            double spawnChance = Math.Pow((Math.Max(position.x, columns - position.x) * Math.Max(position.y, rows - position.y)), power) / Math.Pow((columns * rows), power);
            if (Random.value*50 < spawnChance)
            {
                PlaceTiles(overGroundTilemap, waterTile, (Vector2Int) position, Random.Range(pondRadius.minimum, pondRadius.maximum + 1), false, null, true);
            }
        }
    }

    String GetRoomType(String type)
    {
        Room[] rooms = buildings[type];
        return rooms[Random.Range(0, rooms.Length)].type;
    }

    void PlaceTable(Vector2Int coords, Vector2Int start, Vector2Int end)
    {
        Vector3Int position = new Vector3Int(coords.x, coords.y, coords.y);
        if (!gridPositions.Contains(position))
            return;
        Instantiate(table, position, Quaternion.identity);
        Vector3Int chairPosition = position + new Vector3Int(1,0,0);
        if (gridPositions.Contains(chairPosition) && chairPosition.x > start.x && chairPosition.x < end.x - 1 && chairPosition.y > start.y && chairPosition.y < end.y - 2)
            //Instantiate(chair, chairPosition, Quaternion.identity);
            Instantiate(stool, chairPosition, Quaternion.identity);
        chairPosition = position + new Vector3Int(-1,0,0);
        if (gridPositions.Contains(chairPosition) && chairPosition.x > start.x && chairPosition.x < end.x - 1 && chairPosition.y > start.y && chairPosition.y < end.y - 2)
            //Instantiate(chair, chairPosition, Quaternion.identity);
            Instantiate(stool, chairPosition, Quaternion.identity);
        chairPosition = position + new Vector3Int(0,1,1);
        if (gridPositions.Contains(chairPosition) && chairPosition.x > start.x && chairPosition.x < end.x - 1 && chairPosition.y > start.y && chairPosition.y < end.y - 2)
            //Instantiate(chair, chairPosition, Quaternion.identity);
            Instantiate(stool, chairPosition, Quaternion.identity);
        chairPosition = position + new Vector3Int(0,-1,-1);
        if (gridPositions.Contains(chairPosition) && chairPosition.x > start.x && chairPosition.x < end.x - 1 && chairPosition.y > start.y && chairPosition.y < end.y - 2)
            //Instantiate(chair, chairPosition, Quaternion.identity);
            Instantiate(stool, chairPosition, Quaternion.identity);
    }

    void PlaceTiles(Tilemap tilemap, RuleTile tile, Vector2Int position, int radius, bool path = false, Connections connections = null, bool pond = false)
    {
        for (int x = 1 - radius; x <= radius - 1; x++) {
            for (int y = 1 - radius; y <= radius - 1; y++) {
                Vector3Int newPos = new Vector3Int(position.x + x, position.y + y, 0);
                Vector3Int newPosGrid = new Vector3Int(newPos.x, newPos.y, newPos.y);
                Vector2 gridSize = new Vector2(columns, rows);
                if (newPos.x < -1 || newPos.x > columns || newPos.y < -1 || newPos.y > rows)
                    continue;

                bool center = x == 0 && y == 0;
                bool up = x == 0 && y > 0;
                bool down = x == 0 && y < 0;
                bool left = x < 0 && y == 0;
                bool right = x > 0 && y == 0;
                bool connected = center || ((connections == null) ? false :
                    (up && connections.up) || (down && connections.down) || (left && connections.left) || (right && connections.right));

                if (path && !connected && gridPositions.Contains(newPosGrid) && Random.Range(0,20) == 0)
                    Instantiate(RandomObject(streetObjects), newPosGrid, Quaternion.identity);
                
                List<Vector3Int> added = new List<Vector3Int>();
                if ((path || !pond || gridPositions.Contains(newPosGrid) || newPos.x < 0 || newPos.x >= columns || newPos.y < 0 || newPos.y >= rows) && 
                        (!path || connected || Random.Range(0,3) > 0) && (!pond || 0.01 > (Math.Pow((Math.Max(x, 0 - x) * Math.Max(y, 0 - y)), 2) / Math.Pow((2*radius * 2*radius), 2)))) {
                    tilemap.SetTile(newPos, tile);
                    added.Add(newPosGrid);
                    if (pond && newPos.x >= 0 && newPos.x < columns && newPos.y >= 0 && newPos.y < rows) {
                        Grid[newPos.x][newPos.y] = new Node(new Vector2(newPos.x, newPos.y), false);
                        grassTilemap.SetTile(newPos, middleTile);
                    }
                    if (!path && gridPositions.Contains(newPosGrid) && ((pond && Random.Range(0,3) == 0) || (!center && Random.Range(0,30) == 0)))
                        if (pond) {
                            bool onEdge = x == 1 - radius || x == radius - 1 || y == 1 - radius || y == radius - 1;
                            for (int i = -1; i <= 1; i++) {
                                for (int j = -1; j <= 1; j++) {
                                    Vector3Int neighbor = new Vector3Int(newPos.x + i, newPos.y + j, newPos.y + j);
                                    // Debug.Log(neighbor.x + ", " + neighbor.y + "; " + added.Contains(neighbor) + " " + gridPositions.Contains(neighbor) + " " + (!added.Contains(neighbor) && !gridPositions.Contains(neighbor)));
                                    if (0.01 <= (Math.Pow((Math.Max(x + i, 0 - x - i) * Math.Max(y + j, 0 - y - j)), 2) / Math.Pow((2*radius * 2*radius), 2)))
                                        onEdge = true;
                                }
                            }
                            if (!onEdge && biome == "Forest")
                                plantTilemap.SetTile(newPos, waterFloraTile);
                        }
                        else if (biome == "Forest")
                            plantTilemap.SetTile(newPos, Random.Range(0,5) > 0 ? flowerTile : mushroomTile);
                    if ((path || pond) && gridPositions.Contains(newPosGrid))
                        gridPositions.Remove(newPosGrid);
                }
            }
        }
    }

    private bool InRange(Vector2 position, Vector2 start, Vector2 end, bool endExclusive = true) {
        if (endExclusive)
            return position.x >= start.x && position.y >= start.y && position.x < end.x && position.y < end.y;
        else
            return position.x >= start.x && position.y >= start.y && position.x <= end.x && position.y <= end.y;

    }

    public List<List<Node>> SetupScene(int level)
    {
        BoardSetup();
        InitializeList();
        SpawnPlayers();
        if (biome != "Volcanic") {
            SpawnBuildings();
            SpawnPaths();
        }
        SpawnPonds();
        if (biome == "Desert") {
            LayoutObjectAtRandom(natureObjects, objectCount.minimum*5, objectCount.maximum*5);
        } else {
            if (biome != "Volcanic")
                LayoutObjectAtCorners(trees);
            LayoutObjectAtRandom(natureObjects, objectCount.minimum, objectCount.maximum);
        }
        LayoutObjectAtRandom(enemies, enemyCount.minimum, enemyCount.maximum);
        return Grid;
    }

    public List<Roof> GetRoofs() {
        return roofs;
    }
}