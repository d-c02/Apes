using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using static System.Reflection.Metadata.BlobBuilder;

public partial class map : GridMap
{

    //Change enum conversion logic at some point
    enum Blocks { Center, Corner, Ramp, InnerCorner, Sand, Water, DoubleCornerJoin, DebugNavBlue, DebugNavRed };

    const int m_TopLevel = 4;

    const int m_BottomLevel = 0;

    int[] m_MaxLevelRadii = { 15, 12, 9, 6, 3 }; //Ascending

    //Sort in descending order with objects with the same dimensions grouped together.
    int[,] m_Markers = { 
        {7, 7}, //Player Statue
        {5, 5}, //Insight Project Site
        {5, 5}, //Influence Project Site
        {5, 5} //Fervor Project Site
    };

    const int m_MapSize = 50;

    int m_MinInitialPointRadius = 1;
    int m_MaxInitialPointRadius = 2;

    int m_MinNewPointsPerLevel = 1;
    int m_MaxNewPointsPerLevel = 4;

    int m_MaxX = int.MinValue;
    int m_MinX = int.MaxValue;
    int m_MaxZ = int.MinValue;
    int m_MinZ = int.MaxValue;

    //Navigation shenanigans
    AStarGrid2D m_AStarGrid;

    int m_AStarGridxOffset;
    int m_AStarGridzOffset;

    Vector2I m_AStarGridSize;

    Dictionary<Vector2I, Stack<Vector3I>> m_ProjectLocations;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Clear();
        GenerateMap();
        //m_MinX = -4;
        //m_MaxX = 4;
        //m_MinZ = -4;
        //m_MaxZ = 4;
        GenerateNavMap();
        PlaceStructureMarkers();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

    }

    //This really sucks, optimize later
    public Vector2I getRandomOpenNavCoords(bool fillPoint = false)
    {
        Random rnd = new Random();
        Vector2I Coords = Vector2I.Zero;
        int EscapeCtr = 0;
        int EscapeLimit = 500;
        while (m_AStarGrid.IsPointSolid(Coords))
        {
            Coords.X = rnd.Next(0, m_AStarGridSize.X);
            Coords.Y = rnd.Next(0, m_AStarGridSize.Y);
            EscapeCtr++;
            if (EscapeCtr > EscapeLimit)
            {
                return Vector2I.Zero;
            }
        }

        if (fillPoint)
        {
            //Vector3I debug_pos = new Vector3I(Coords.X - m_AStarGridxOffset, 6, Coords.Y - m_AStarGridzOffset);
            //SetCellItem(debug_pos, (int)Blocks.DebugNavRed);
            m_AStarGrid.SetPointSolid(Coords);
        }

        return Coords;
    }

    public Vector2 GetPointPosition(Vector2I id)
    {
        //Vector3I pos = new Vector3I((int) m_AStarGrid.GetPointPosition(id).X, 0, (int) m_AStarGrid.GetPointPosition(id).Y);
        Vector3I pos = new Vector3I(id.X - m_AStarGridxOffset, 0, id.Y - m_AStarGridzOffset);
        //Vector3I pos = new Vector3I(id.X, 0, id.Y);
        return new Vector2(ToGlobal(MapToLocal(pos)).X, ToGlobal(MapToLocal(pos)).Z);
        //return new Vector2I(pos.X, pos.Z);
    }

    public Vector2I PosCoordsToNavCoords(Vector2I id)
    {
        return new Vector2I(id.X - m_MinX, id.Y - m_MinZ);
    }

    public Vector2[] getPointPath(Vector2I fromID, Vector2I toID, bool allowPartialPath)
    {
        return m_AStarGrid.GetPointPath(fromID, toID, allowPartialPath);
    }

    public Godot.Collections.Array<Vector2I> getIdPath(Vector2I fromID, Vector2I toID, bool allowPartialPath)
    {
        return m_AStarGrid.GetIdPath(fromID, toID, allowPartialPath);
    }

    public Vector2I GetIDByIndex(Vector2I fromID, Vector2I toID, int index, bool allowPartialPath = false)
    {

        return m_AStarGrid.GetIdPath(fromID, toID, allowPartialPath)[index];
    }

    public bool IsPointSolid(Vector2I id)
    {
        return m_AStarGrid.IsPointSolid(id);
    }

    public void SetPointSolid(Vector2I id, bool solid = true)
    {

        //SHOW DEBUG NAVMAP
        /*
        Vector3I debug_pos = new Vector3I(id.X - m_AStarGridxOffset, 6, id.Y - m_AStarGridzOffset);
        if (solid)
        {
            SetCellItem(debug_pos, (int)Blocks.DebugNavRed);
        }
        else
        {
            SetCellItem(debug_pos, (int)Blocks.DebugNavBlue);
        }
        */

        m_AStarGrid.SetPointSolid(id, solid);
    }

    public bool IsInBounds(Vector2I id)
    {
        return m_AStarGrid.IsInBoundsv(id);
    }

    public Vector3I GetProjectLocation(Vector2I Dimensions)
    {
        if (m_ProjectLocations.ContainsKey(Dimensions))
        {
            if (m_ProjectLocations[Dimensions].Count > 0)
            {
                Vector3I Location = m_ProjectLocations[Dimensions].Pop();
                //Location.X += Dimensions.X / 2;
                //Location.Y += Dimensions.Y / 2;
                /*
                for (int x = Location.X; x < Location.X + Dimensions.X; x++)
                {
                    for (int z = Location.Z; z < Location.Z +  Dimensions.Y; z++)
                    {
                        SetCellItem(new Vector3I(x, Location.Y, z), (int)Blocks.Sand);
                    }
                }

                */
                return Location;
            }
            else
            {
                throw new Exception("No location for project found!");
            }
        }
        else
        {
            throw new Exception("No location for project found!");
        }
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

        int maxPointRadius = m_MaxInitialPointRadius;
        int minPointRadius = m_MinInitialPointRadius;
        for (int y = m_TopLevel; y >= m_BottomLevel; y--)
        {

            maxPointRadius = m_MaxLevelRadii[y];
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

                            if (x < m_MinX)
                            {
                                m_MinX = x;
                            }
                            if (x > m_MaxX)
                            {
                                m_MaxX = x;
                            }

                            if (z < m_MinZ)
                            {
                                m_MinZ = z;
                            }
                            if (z > m_MaxZ)
                            {
                                m_MaxZ = z;
                            }

                            SetCellItem(Coords, (int)Blocks.Center);
                    }
                }

                points[i] = new Vector2I(points[i].X + xShift, points[i].Y + zShift);
                pointRadii[i] = pointRadius;
            }

            //Generate new points at this level
            int newPoints = rnd.Next(m_MinNewPointsPerLevel, m_MaxNewPointsPerLevel + 1);

            for (int curPoint = 0; curPoint < newPoints; curPoint++)
            {
                int pointX = rnd.Next(-m_MapSize / 2, m_MapSize / 2); //Centered at 0, 0
                int pointZ = rnd.Next(-m_MapSize / 2, m_MapSize / 2);
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

                            if (x < m_MinX)
                            {
                                m_MinX = x;
                            }
                            if (x > m_MaxX)
                            {
                                m_MaxX = x;
                            }

                            if (z < m_MinZ)
                            {
                                m_MinZ = z;
                            }
                            if (z > m_MaxZ)
                            {
                                m_MaxZ = z;
                            }

                            SetCellItem(Coords, (int)Blocks.Center);
                        }
                    }
                    points.Add(new Vector2I(pointX, pointZ));
                    pointRadii.Add(pointRadius);
                }
            }

            if (y > m_BottomLevel)
            {
                //Fill in ramps and corners
                for (int x = -m_MapSize / 2 - m_MaxLevelRadii[y] - 10; x <= m_MapSize / 2 + m_MaxLevelRadii[y] + 10; x++)
                {
                    for (int z = -m_MapSize / 2 - m_MaxLevelRadii[y] - 10; z <= m_MapSize / 2 + m_MaxLevelRadii[y] + 10; z++)
                    {
                        FillSimpleGaps(x, y, z);
                    }
                }

                for (int x = -m_MapSize / 2 - m_MaxLevelRadii[y] - 10; x <= m_MapSize / 2 + m_MaxLevelRadii[y] + 10; x++)
                {
                    for (int z = -m_MapSize / 2 - m_MaxLevelRadii[y] - 10; z <= m_MapSize / 2 + m_MaxLevelRadii[y] + 10; z++)
                    {
                        CreateRamp(x, y, z);
                    }
                }
            }
            else
            {
                for (int x = -m_MapSize / 2 - m_MaxLevelRadii[y]; x <= m_MapSize / 2 + m_MaxLevelRadii[y]; x++)
                {
                    for (int z = -m_MapSize / 2 - m_MaxLevelRadii[y]; z <= m_MapSize / 2 + m_MaxLevelRadii[y]; z++)
                    {
                        GenerateSandAtPoint(x, y, z);
                    }
                }
                GenerateWater();
                GenerateNavMap();
            }
        }

        //Optimization stuff - remove unnecessary blocks
        for (int y = m_BottomLevel; y < m_TopLevel; y++)
        {
            for (int x = -(m_MapSize * 2 + 10); x <= 10 + m_MapSize * 2; x++)
            {
                for (int z = -(m_MapSize * 2 + 10); z <= 10 + m_MapSize * 2; z++)
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

    private void PlaceStructureMarkers()
    {
        m_ProjectLocations = new Dictionary<Vector2I, Stack<Vector3I>>();
        Random rnd = new Random();
        int prevN = -1;
        int prevM = -1;
        for (int curMarker = 0; curMarker < m_Markers.GetLength(0); curMarker++)
        {
            List<Vector3I> ValidSpaces = new List<Vector3I>();
            for (int y = m_BottomLevel; y <= m_TopLevel; y++)
                {
                    for (int x = m_MinX; x < m_MaxX - m_Markers[curMarker, 0]; x++)
                    {
                        for (int z = m_MinZ; z < m_MaxZ - m_Markers[curMarker, 1]; z++)
                        {
                            bool ValidSpace = true;
                            for (int xOffset = 0; xOffset < m_Markers[curMarker, 0]; xOffset++)
                            {
                                if (!ValidSpace)
                                {
                                    break;
                                }
                                for (int zOffset = 0; zOffset < m_Markers[curMarker, 1]; zOffset++)
                                {
                                    if (GetCellItem(new Vector3I(x + xOffset, y, z + zOffset)) != (int)Blocks.Center || GetCellItem(new Vector3I(x + xOffset, y + 1, z + zOffset)) != GridMap.InvalidCellItem)
                                    {
                                        ValidSpace = false;
                                        break;
                                    }
                                }
                            }

                            if (ValidSpace)
                            {
                                ValidSpaces.Add(new Vector3I(x, y + 1, z));
                            }
                        }
                    }
                }
            
            

            //Choose out of potential spaces
            if (ValidSpaces.Count > 0)
            {
                int space = rnd.Next(0, ValidSpaces.Count);
                Vector3I pos = ValidSpaces[space];
                ValidSpaces.RemoveAt(space);
                Vector2I Dimensions = new Vector2I(m_Markers[curMarker, 0], m_Markers[curMarker, 1]);
                if (m_ProjectLocations.ContainsKey(Dimensions))
                {
                    m_ProjectLocations[Dimensions].Push(pos);
                }
                else
                {
                    m_ProjectLocations[Dimensions] = new Stack<Vector3I>();
                    m_ProjectLocations[Dimensions].Push(pos);
                }
            }
            else
            {
                throw new Exception("Not enough room for object");
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
                    if (x < m_MinX)
                    {
                        m_MinX = x;
                    }
                    if (x > m_MaxX)
                    {
                        m_MaxX = x;
                    }

                    if (z < m_MinZ)
                    {
                        m_MinZ = z;
                    }
                    if (z > m_MaxZ)
                    {
                        m_MaxZ = z;
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

        for (int x = -(m_MapSize * 2 + oceanRadius); x <= oceanRadius + m_MapSize * 2; x++)
        {
            for (int z = -(m_MapSize * 2 + oceanRadius) ; z <= oceanRadius + m_MapSize * 2; z++)
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
        m_AStarGrid = new AStarGrid2D();
        m_AStarGridSize = new Vector2I();
        m_AStarGrid.Offset = new Vector2(m_MinX, m_MinZ);

        bool isDirty = m_AStarGrid.IsDirty();
        m_AStarGridxOffset = -m_MinX;
        m_AStarGridzOffset = -m_MinZ;
        m_AStarGrid.Region = new Rect2I(0, 0, m_MaxX + m_AStarGridxOffset + 1, m_MaxZ + m_AStarGridzOffset + 1);

        m_AStarGridSize.X = m_MaxX + m_AStarGridxOffset + 1;
        m_AStarGridSize.Y = m_MaxZ + m_AStarGridzOffset + 1;

        m_AStarGrid.Update();

        Vector3I Coords = new Vector3I(0, 0, 0);
        Vector2I ID = new Vector2I(0, 0);
        for (int x = m_MinX; x <= m_MaxX; x++)
        {
            for (int z = m_MinZ; z <= m_MaxZ; z++)
            {
                Coords.X = x;
                Coords.Z = z;
                ID.X = x + m_AStarGridxOffset;
                ID.Y = z + m_AStarGridzOffset;
                if (GetCellItem(Coords) == (int) Blocks.Water)
                {
                    m_AStarGrid.SetPointSolid(ID);
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
