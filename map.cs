using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using static System.Reflection.Metadata.BlobBuilder;

public partial class map : GridMap
{
    enum Blocks { Center, Corner, Ramp, InnerCorner, Sand, Water, DoubleCornerJoin, DebugNavBlue, DebugNavRed};

    const int topLevel = 4;

    const int bottomLevel = 0;

    int[] maxLevelRadii = { 15, 12, 9, 6, 3 }; //Ascending

    const int mapSize = 50;

    int minInitialPointRadius = 1;
    int maxInitialPointRadius = 2;

    int MinNewPointsPerLevel = 1;
    int MaxNewPointsPerLevel = 4;

    int maxX = int.MinValue;
    int minX = int.MaxValue;
    int maxZ = int.MinValue;
    int minZ = int.MaxValue;

    //Navigation shenanigans
    AStarGrid2D aStarGrid;

    int aStarGridxOffset;
    int aStarGridzOffset;

    Vector2I aStarGridSize;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Clear();
        GenerateMap();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("DEBUG_GENERATE_MAP"))
        {
            Clear();
            GenerateMap();
        }
    }

    //This really sucks, optimize later
    public Vector2I getRandomOpenNavCoords(bool fillPoint = false)
    {
        Random rnd = new Random();
        Vector2I Coords = Vector2I.Zero;
        int EscapeCtr = 0;
        int EscapeLimit = 500;
        while (aStarGrid.IsPointSolid(Coords))
        {
            Coords.X = rnd.Next(0, aStarGridSize.X);
            Coords.Y = rnd.Next(0, aStarGridSize.Y);
            EscapeCtr++;
            if (EscapeCtr > EscapeLimit)
            {
                return Vector2I.Zero;
            }
        }

        if (fillPoint)
        {
            //Vector3I debug_pos = new Vector3I(Coords.X - aStarGridxOffset, 6, Coords.Y - aStarGridzOffset);
            //SetCellItem(debug_pos, (int)Blocks.DebugNavRed);
            aStarGrid.SetPointSolid(Coords);
        }

        return Coords;
    }

    public Vector2 GetPointPosition(Vector2I id)
    {
        //Vector3I pos = new Vector3I((int) aStarGrid.GetPointPosition(id).X, 0, (int)aStarGrid.GetPointPosition(id).Y);
        Vector3I pos = new Vector3I(id.X - aStarGridxOffset, 0, id.Y - aStarGridzOffset);
        return new Vector2(ToGlobal(MapToLocal(pos)).X, ToGlobal(MapToLocal(pos)).Z);
    }

    public Vector2[] getPointPath(Vector2I fromID, Vector2I toID, bool allowPartialPath)
    {
        return aStarGrid.GetPointPath(fromID, toID, allowPartialPath);
    }

    public Godot.Collections.Array<Vector2I> getIdPath(Vector2I fromID, Vector2I toID, bool allowPartialPath)
    {
        return aStarGrid.GetIdPath(fromID, toID, allowPartialPath);
    }

    public Vector2I GetIDByIndex(Vector2I fromID, Vector2I toID, int index, bool allowPartialPath = false)
    {

        return aStarGrid.GetIdPath(fromID, toID, allowPartialPath)[index];
    }

    public bool IsPointSolid(Vector2I id)
    {
        return aStarGrid.IsPointSolid(id);
    }

    public void SetPointSolid(Vector2I id, bool solid = true)
    {
        /* SHOW DEBUG NAVMAP
        
        Vector3I debug_pos = new Vector3I(id.X - aStarGridxOffset, 6, id.Y - aStarGridzOffset);
        if (solid)
        {
            SetCellItem(debug_pos, (int)Blocks.DebugNavRed);
        }
        else
        {
            SetCellItem(debug_pos, (int)Blocks.DebugNavBlue);
        }
        */
        aStarGrid.SetPointSolid(id, solid);
    }

    public bool IsInBounds(Vector2I id)
    {
        return aStarGrid.IsInBoundsv(id);
    }

    /// <summary>
    /// Private methods
    /// </summary>

    private void GenerateMap()
    {

        //Define necessary variables
        List<Vector2I> points = new List<Vector2I>();
        List<int> pointRadii = new List<int>();
        Random rnd = new Random();
        Vector3I Coords = new Vector3I(0, 0, 0);

        int maxPointRadius = maxInitialPointRadius;
        int minPointRadius = minInitialPointRadius;
        for (int y = topLevel; y >= bottomLevel; y--)
        {

            maxPointRadius = maxLevelRadii[y];
            //minPointRadius *= pointRadiusMultiplier

            //Place wider points underneath previously placed points
            for (int i = 0; i < points.Count; i++)
            {
                int pointRadius = rnd.Next(pointRadii[i] + 2, maxPointRadius);

                int xShift = rnd.Next(-((pointRadius - 1) - pointRadii[i]), (pointRadius - 1) - pointRadii[i]);
                int zShift = rnd.Next(-((pointRadius - 1) - pointRadii[i]), (pointRadius - 1) - pointRadii[i]);

                for (int x = points[i].X - (pointRadius - 1); x < points[i].X + pointRadius; x++)
                {
                    for (int z = points[i].Y - (pointRadius - 1); z < points[i].Y + pointRadius; z++)
                    {
                            Coords.X = x + xShift;
                            Coords.Y = y;
                            Coords.Z = z + zShift;

                            if (x < minX)
                            {
                                minX = x;
                            }
                            if (x > maxX)
                            {
                                maxX = x;
                            }

                            if (z < minZ)
                            {
                                minZ = z;
                            }
                            if (z > maxZ)
                            {
                                maxZ = z;
                            }

                            SetCellItem(Coords, (int)Blocks.Center);
                    }
                }

                points[i] = new Vector2I(points[i].X + xShift, points[i].Y + zShift);
                pointRadii[i] = pointRadius;
            }

            //Generate new points at this level
            int newPoints = rnd.Next(MinNewPointsPerLevel, MaxNewPointsPerLevel + 1);

            for (int curPoint = 0; curPoint < newPoints; curPoint++)
            {
                int pointX = rnd.Next(-mapSize / 2, mapSize / 2); //Centered at 0, 0
                int pointZ = rnd.Next(-mapSize / 2, mapSize / 2);
                int pointRadius = rnd.Next(minPointRadius, maxPointRadius + 1);

                bool alreadyExists = false;
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].X == pointX && points[i].Y == pointZ)
                    {
                        alreadyExists = true;
                        break;
                    }
                }
                if (!alreadyExists)
                {
                    for (int x = pointX - (pointRadius - 1); x < pointX + pointRadius; x++)
                    {
                        for (int z = pointZ - (pointRadius - 1); z < pointZ + pointRadius; z++)
                        {

                            Coords.X = x;
                            Coords.Y = y;
                            Coords.Z = z;

                            if (x < minX)
                            {
                                minX = x;
                            }
                            if (x > maxX)
                            {
                                maxX = x;
                            }

                            if (z < minZ)
                            {
                                minZ = z;
                            }
                            if (z > maxZ)
                            {
                                maxZ = z;
                            }

                            SetCellItem(Coords, (int)Blocks.Center);
                        }
                    }
                    points.Add(new Vector2I(pointX, pointZ));
                    pointRadii.Add(pointRadius);
                }
            }

            if (y > bottomLevel)
            {
                //Fill in ramps and corners
                for (int x = -mapSize / 2 - maxLevelRadii[y] - 10; x <= mapSize / 2 + maxLevelRadii[y] + 10; x++)
                {
                    for (int z = -mapSize / 2 - maxLevelRadii[y] - 10; z <= mapSize / 2 + maxLevelRadii[y] + 10; z++)
                    {
                        FillSimpleGaps(x, y, z);
                    }
                }

                for (int x = -mapSize / 2 - maxLevelRadii[y] - 10; x <= mapSize / 2 + maxLevelRadii[y] + 10; x++)
                {
                    for (int z = -mapSize / 2 - maxLevelRadii[y] - 10; z <= mapSize / 2 + maxLevelRadii[y] + 10; z++)
                    {
                        CreateRamp(x, y, z);
                    }
                }
            }
            else
            {
                for (int x = -mapSize / 2 - maxLevelRadii[y]; x <= mapSize / 2 + maxLevelRadii[y]; x++)
                {
                    for (int z = -mapSize / 2 - maxLevelRadii[y]; z <= mapSize / 2 + maxLevelRadii[y]; z++)
                    {
                        GenerateSandAtPoint(x, y, z);
                    }
                }
                GenerateWater();
                GenerateNavMap();
            }
        }

        //Optimization stuff - remove unnecessary blocks
        for (int y = bottomLevel; y < topLevel; y++)
        {
            for (int x = -(mapSize * 2 + 10); x <= 10 + mapSize * 2; x++)
            {
                for (int z = -(mapSize * 2 + 10); z <= 10 + mapSize * 2; z++)
                {
                    Coords.X = x;
                    Coords.Y = y + 1;
                    Coords.Z = z;
                    if (GetCellItem(Coords) != GridMap.InvalidCellItem)
                    {
                        Coords.Y = y;
                        SetCellItem(Coords, (int) GridMap.InvalidCellItem);
                    }
                }
            }
        }
    }

    private void CreateRamp(int inx, int iny, int inz)
    {

        Vector3I Coords = new Vector3I(inx, iny, inz);
        if (GetCellItem(Coords) == GridMap.InvalidCellItem)
        {
            bool[,] CubeMap = { { false, false, false }, { false, false, false }, { false, false, false } };
            for (int x = inx - 1; x <= inx + 1; x++)
            {
                for (int z = inz - 1; z <= inz + 1; z++)
                {
                    Coords.X = x;
                    Coords.Z = z;
                    if (GetCellItem(Coords) == (int)Blocks.Center)
                    {
                        CubeMap[x - (inx - 1), z - (inz - 1)] = true;
                    }
                }
            }

            Coords.X = inx;
            Coords.Z = inz;
            //if ((CubeMap[0, 1] && CubeMap[2, 1]) || (CubeMap[1, 0] && CubeMap[1, 2]))
            //{
            //    SetCellItem(Coords, (int)Blocks.Center);
            //    return;
            //}

            if ((CubeMap[0, 1] && CubeMap[1, 0]))
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner, 22);
                return;
            }

            if ((CubeMap[1, 0] && CubeMap[2, 1]))
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner, 10);
                return;
            }

            if ((CubeMap[2, 1] && CubeMap[1, 2]))
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner, 16);
                return;
            }

            if (((CubeMap[0, 1] && CubeMap[1, 2])))
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner);
                return;
            }

            //Weird side and diag handling
            if (CubeMap[0, 2] && CubeMap[2, 1])
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner, 16);
                return;
            }

            if (CubeMap[0,1] && CubeMap[2, 0])
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner, 22);
                return;
            }

            if (CubeMap[0, 0] && CubeMap[1, 2])
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner, 0);
                return;
            }

            if (CubeMap[1, 0] && CubeMap[0, 2])
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner, 22);
                return;
            }

            if (CubeMap[0, 1] && CubeMap[2, 2])
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner, 0);
                return;
            }

            if (CubeMap[0, 0] && CubeMap[2, 1])
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner, 10);
                return;
            }

            if (CubeMap[1, 0] && CubeMap[2, 2])
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner, 10);
                return;
            }

            if (CubeMap[2, 0] && CubeMap[1, 2])
            {
                SetCellItem(Coords, (int)Blocks.InnerCorner, 16);
                return;
            }
            //Ramps

            if (CubeMap[1, 0])
            {
                SetCellItem(Coords, (int)Blocks.Ramp, 22);
                return;
            }

            if (CubeMap[0, 1])
            {
                SetCellItem(Coords, (int)Blocks.Ramp);
                return;
            }

            if (CubeMap[1, 2])
            {
                SetCellItem(Coords, (int)Blocks.Ramp, 16);
                return;
            }

            if (CubeMap[2, 1])
            {
                SetCellItem(Coords, (int)Blocks.Ramp, 10);
                return;
            }

            if ((CubeMap[0, 0] && CubeMap[2, 2]))
            {
                SetCellItem(Coords, (int) Blocks.DoubleCornerJoin, 16);
                return;
            }

            if ((CubeMap[2, 0] && CubeMap[0, 2]))
            {
                SetCellItem(Coords, (int)Blocks.DoubleCornerJoin);
                return;
            }

            if (CubeMap[2, 2])
            {
                SetCellItem(Coords, (int)Blocks.Corner, 16);
                return;
            }

            if (CubeMap[0, 2])
            {
                SetCellItem(Coords, (int)Blocks.Corner);
                return;
            }

            if (CubeMap[0, 0])
            {
                SetCellItem(Coords, (int)Blocks.Corner, 22);
                return;
            }

            if (CubeMap[2, 0])
            {
                SetCellItem(Coords, (int)Blocks.Corner, 10);
                return;
            }
        }
    }

    private void FillSimpleGaps(int inx, int iny, int inz)
    {

        Vector3I Coords = new Vector3I(inx, iny, inz);
        if (GetCellItem(Coords) == GridMap.InvalidCellItem)
        {
            bool[,] CubeMap = { { false, false, false }, { false, false, false }, { false, false, false } };
            for (int x = inx - 1; x <= inx + 1; x++)
            {
                for (int z = inz - 1; z <= inz + 1; z++)
                {
                    Coords.X = x;
                    Coords.Z = z;
                    if (GetCellItem(Coords) == (int)Blocks.Center)
                    {
                        CubeMap[x - (inx - 1), z - (inz - 1)] = true;
                    }
                }
            }

            Coords.X = inx;
            Coords.Z = inz;

            //Walls
            if ((CubeMap[0, 1] && CubeMap[2, 1]) || (CubeMap[1, 0] && CubeMap[1, 2]))
            {
                SetCellItem(Coords, (int)Blocks.Center);
                return;
            }

            if ((CubeMap[0, 0] && CubeMap[2, 2]) && (CubeMap[2, 0] && CubeMap[0, 2]))
            {
                SetCellItem(Coords, (int)Blocks.Center, 16);
                return;
            }
            //Corners
            //if ((CubeMap[0, 0] && CubeMap[2, 2]) || (CubeMap[2, 0] && CubeMap[0, 2]))
            //{
            //    SetCellItem(Coords, (int)Blocks.Center);
            //    return;
            //}

        }

    }

    private void GenerateSandAtPoint(int inx, int iny, int inz)
    {
        int sandRadius = 7;

        Vector3I Coords = new Vector3I(inx, iny, inz);
        if (GetCellItem(Coords) == (int) Blocks.Center)
        {
            for (int x = inx - sandRadius; x <= inx + sandRadius; x++)
            {
                for (int z = inz - sandRadius; z <= inz + sandRadius; z++)
                {
                    Coords.X = x;
                    Coords.Z = z;
                    if (x < minX)
                    {
                        minX = x;
                    }
                    if (x > maxX)
                    {
                        maxX = x;
                    }

                    if (z < minZ)
                    {
                        minZ = z;
                    }
                    if (z > maxZ)
                    {
                        maxZ = z;
                    }
                    if (GetCellItem(Coords) == GridMap.InvalidCellItem)
                    {
                        SetCellItem(Coords, (int)Blocks.Sand);
                    }
                }
            }
        }
    }

    private void GenerateWater()
    {
        int oceanRadius = 10;

        int y = 0;
        Vector3I Coords = new Vector3I(0, y, 0);

        for (int x = -(mapSize * 2 + oceanRadius); x <= oceanRadius + mapSize * 2; x++)
        {
            for (int z = -(mapSize * 2 + oceanRadius) ; z <= oceanRadius + mapSize * 2; z++)
            {
                Coords.X = x;
                Coords.Z = z;
                if (GetCellItem(Coords) == GridMap.InvalidCellItem)
                {
                    SetCellItem(Coords, (int)Blocks.Water);
                }
            }
        }
    }

    private void GenerateNavMap()
    {
        aStarGrid = new AStarGrid2D();
        aStarGridSize = new Vector2I();
        aStarGrid.Offset = new Vector2(minX, minZ);

        bool isDirty = aStarGrid.IsDirty();
        aStarGridxOffset = -minX;
        aStarGridzOffset = -minZ;
        aStarGrid.Region = new Rect2I(0, 0, maxX + aStarGridxOffset + 1, maxZ + aStarGridzOffset + 1);

        aStarGridSize.X = maxX + aStarGridxOffset + 1;
        aStarGridSize.Y = maxZ + aStarGridzOffset + 1;

        aStarGrid.Update();

        Vector3I Coords = new Vector3I(0, 0, 0);
        Vector2I ID = new Vector2I(0, 0);
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Coords.X = x;
                Coords.Z = z;
                ID.X = x + aStarGridxOffset;
                ID.Y = z + aStarGridzOffset;
                if (GetCellItem(Coords) == (int) Blocks.Water)
                {
                    aStarGrid.SetPointSolid(ID);
                    //SetCellItem(new Vector3I(Coords.X, 6, Coords.Z), (int)Blocks.DebugNavRed);
                }
                else
                {
                    //SetCellItem(new Vector3I(Coords.X, 6, Coords.Z), (int)Blocks.DebugNavBlue);
                }
            }
        }
        //aStarGrid.Offset = new Vector2(aStarGridxOffset, aStarGridzOffset);
    }

}
