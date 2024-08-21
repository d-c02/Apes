using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static System.Reflection.Metadata.BlobBuilder;

public partial class map : GridMap
{
    enum Blocks { Center, Corner, Ramp, Sand, Water, InnerCorner, DoubleCornerJoin};

    const int topLevel = 4;

    const int bottomLevel = 0;

    int[] maxLevelRadii = { 15, 12, 9, 6, 3 }; //Ascending

    const int mapSize = 50;

    int minInitialPointRadius = 1;
    int maxInitialPointRadius = 2;

    int MinNewPointsPerLevel = 1;
    int MaxNewPointsPerLevel = 4;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //GenerateMap();
        int y = 0;
        for (int x = -mapSize / 2 - maxLevelRadii[y]; x <= mapSize / 2 + maxLevelRadii[y]; x++)
        {
            for (int z = -mapSize / 2 - maxLevelRadii[y]; z <= mapSize / 2 + maxLevelRadii[y]; z++)
            {
                FillSimpleGaps(x, y, z);
            }
        }

        for (int x = -mapSize / 2 - maxLevelRadii[y]; x <= mapSize / 2 + maxLevelRadii[y]; x++)
        {
            for (int z = -mapSize / 2 - maxLevelRadii[y]; z <= mapSize / 2 + maxLevelRadii[y]; z++)
            {
                CreateRamp(x, y, z);
            }
        }
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

    private void GenerateMap()
    {

        //Define necessary variables
        List<Vector2I> points = new List<Vector2I>();
        List<int> pointRadii = new List<int>();
        Random rnd = new Random();
        Vector3I Coords = new Vector3I(0, 0, 0);
        int maxX = int.MaxValue;
        int minX = int.MinValue;
        int maxZ = int.MinValue;
        int minZ = int.MaxValue;

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
                    if (GetCellItem(Coords) == GridMap.InvalidCellItem)
                    {
                        SetCellItem(Coords, (int)Blocks.Sand);
                    }
                }
            }
        }
    }

}
