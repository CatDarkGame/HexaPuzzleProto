﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class GridMap : MonoBehaviour 
{
	public static GridMap inst;
	public bool _debug = false;

	private int[,] _directionMatrix = new int[6, 3] { { -1, 1, 0 },
												{ 0, 1, -1 },
												{ 1, 0, -1 },
												{ -1, 0, 1 },
												{ 0, -1, 1 },
												{ 1, -1, 0 }};
	public int[,] DirectionMatrix { get { return _directionMatrix; } }

	[SerializeField] private int mapWidth;
	[SerializeField] private int mapHeight;
	[SerializeField] private float hexRadius = 1;

	public int MapWidth { get { return mapWidth; } }
	public int MapHeight { get { return mapHeight; } }
	
	private List<Tile> _tileList = new List<Tile>();
	public GameObject tilePrefab = null;

	private int _tileCreateCount = 0;

	private void Awake()
	{
		if (!inst) inst = this;

		GenerateGrid();
	}

    private void OnDestroy()
    {
		ClearGrid();
	}
	
#if UNITY_EDITOR
	private void OnValidate()
	{
		for(int i=0;i< _tileList.Count;i++)
        {
			_tileList[i]._debug = _debug;
        }
	}
#endif


	public void GenerateGrid() 
	{
		ClearGrid();

		int width = mapWidth - 1;
		int height = mapHeight;
	
		for (int q = -width; q <= width; q++)
		{
			int r1 = Mathf.Max(-width, -q - width);
			int r2 = Mathf.Min(height, -q + height);

			for (int r = r1; r < r2; r++)
			{
				Vector3 gridIndex = new Vector3(q, r, -q - r);

				TileShape shape = GetTileShapeRandom();
				Tile tile = CreateTileObject(gridIndex, shape);
			}
		}
	}

	public void ClearGrid() 
	{
		_tileCreateCount = 0;
		for (int i=0;i< _tileList.Count;i++)
        {
			DestroyImmediate(_tileList[i].gameObject, false);
		}
		_tileList.Clear();
	}


	public Tile CreateTileObject(Vector3 gridIndex, TileShape tileShape)
	{
		Vector3 tilePostion = GridIndexToPosition(gridIndex);
		string objName = string.Format("Tile[{0}]", _tileCreateCount++); ;

		GameObject go = Instantiate(tilePrefab);
		go.name = name;
		go.transform.localPosition = tilePostion;
		go.transform.SetParent(transform);

		Tile tile = go.GetComponent<Tile>();
		tile.Init(tileShape, gridIndex);
		_tileList.Add(tile);
		return tile;
	}

	public Tile GetTileFromGridIndex(Vector3 gridIndex)
	{
		for (int i = 0; i < _tileList.Count; i++)
		{
			if (_tileList[i].GetInfo().GetGridIndex() == gridIndex) return _tileList[i];
		}
		return null;
	}

	public void KillTile(Tile tile)
    {
		if (tile == null) return;

		_tileList.Remove(tile);
		Destroy(tile.gameObject);
	}

	public List<Tile> PullDownTiles(Vector3 gridIndex)
	{
		int r1 = Mathf.Max(-((int)gridIndex.x + mapHeight), -mapHeight);
		int r2 = Mathf.Max((int)gridIndex.x, 0);
		List<Tile> pullTiles = new List<Tile>();
		for (int i = r1; i < mapHeight - r2; i++)
		{
			Vector3 checkIndex = new Vector3(gridIndex.x, i, -gridIndex.x -i);
			Tile tile = GetTileFromGridIndex(checkIndex);
			if (tile == null) continue;
			pullTiles.Add(tile);
		}

		int pullTileCount = 0;
		for (int i = r1; i < mapHeight - r2; i++)
		{
			if (pullTiles.Count <= pullTileCount) break;
			Tile tile = pullTiles[pullTileCount++];
			if (tile == null) continue;
			Vector3 checkIndex = new Vector3(gridIndex.x, i, -gridIndex.x - i);
			tile.SetGridIndex(checkIndex);
		}
		
		return pullTiles;
	}

	public List<Tile> ReloadTiles()
    {
		int width = mapWidth - 1;
		int height = mapHeight;
		List<Tile> reloadTiles = new List<Tile>();
		for (int q = -width; q <= width; q++)
		{
			int r1 = Mathf.Max(-width, -q - width);
			int r2 = Mathf.Min(height, -q + height);

			for (int r = r1; r < r2; r++)
			{
				Vector3 gridIndex = new Vector3(q, r, -q - r);
				Tile checkTile = GetTileFromGridIndex(gridIndex);
				if (checkTile != null) continue;
				TileShape shape = GetTileShapeRandom();
				Tile tile = CreateTileObject(gridIndex, shape);
				
				reloadTiles.Add(tile);
			}
		}
		return reloadTiles;
	}

	public TileShape GetTileShapeRandom()
    {
		TileShape[] shapeRandomList = { TileShape.Red, TileShape.Green, TileShape.Orange, TileShape.Purple, TileShape.Blue, TileShape.ToyTops };
		//TileShape[] shapeRandomList = { TileShape.Red, TileShape.Green, TileShape.Orange, TileShape.Purple, TileShape.Blue };
		int randomShapeMax = shapeRandomList.Length;
		int randomShape = UnityEngine.Random.Range(0, randomShapeMax);
		return shapeRandomList[randomShape];
	}



	public Tile TileSwap(Tile tile, TileDirection direction)
    {
		if (tile == null) return null;
		Vector3 indexVector = new Vector3(_directionMatrix[(int)direction, 0],
											_directionMatrix[(int)direction, 1],
											_directionMatrix[(int)direction, 2]);

		Vector3 findIndex = indexVector + tile.GetInfo().GetGridIndex();
		Tile getTile = GetTileFromGridIndex(findIndex);
		if (getTile == null) return null;
		getTile.SetGridIndex(tile.GetInfo().GetGridIndex());
		tile.SetGridIndex(findIndex);

		return getTile;
	}


	public List<Tile> TileMatching(Tile tile)
    {
		if (tile == null || 
			tile.GetShape() == TileShape.ToyTops) return null;

		List<Tile> matchingList_Line = new List<Tile>();
		matchingList_Line = TileMatching_Line(tile);
		
		List<Tile> matchingList_Suare = new List<Tile>();
		matchingList_Suare = TileMatching_Square(tile);

		List<Tile> matchedTiles = new List<Tile>();
		MergeTileLists(matchedTiles, matchingList_Line);
		MergeTileLists(matchedTiles, matchingList_Suare);

		return matchedTiles;
	}

	public Vector3 GridIndexToPosition(Vector3 gridIndex)
    {
		Vector3 tilePostion = Vector3.zero;

		float radius = hexRadius;
		tilePostion.x = radius * 3.0f / 2.0f * gridIndex.x;
		tilePostion.y = radius * Mathf.Sqrt(3.0f) * (gridIndex.y + gridIndex.x / 2.0f);
		return tilePostion;
	}

	public List<Tile> GetTileList()
	{
		return _tileList;
	}

	public void MergeTileLists(List<Tile> matchedTiles, List<Tile> tiles)
	{
		if (tiles == null) return;
		for (int i = 0; i < tiles.Count; i++)
		{
			Tile checkTile = tiles[i];
			if (matchedTiles.Contains(checkTile) == true) continue;
			matchedTiles.Add(checkTile);
		}
	}

	private List<Tile> TileMatching_Line(Tile tile)
    {
		int matchingCount = 3;
		List<Tile> matchingList_Vertical = GetTileMatching_Line_Vertical(tile, -1, tile.GetShape());
		if (matchingList_Vertical.Count < matchingCount) matchingList_Vertical.Clear();
		List<Tile> matchingList_HorizontalRT = GetTileMatching_Line_Diagonal_RightTop(tile, -1, tile.GetShape());
		if (matchingList_HorizontalRT.Count < matchingCount) matchingList_HorizontalRT.Clear();
		List<Tile> matchingList_HorizontalLT = GetTileMatching_Line_Diagonal_LeftTop(tile, -1, tile.GetShape());
		if (matchingList_HorizontalLT.Count < matchingCount) matchingList_HorizontalLT.Clear();


		List<Tile> matchedTiles = new List<Tile>();
		MergeTileLists(matchedTiles, matchingList_Vertical);
		MergeTileLists(matchedTiles, matchingList_HorizontalRT);
		MergeTileLists(matchedTiles, matchingList_HorizontalLT);
		
		return matchedTiles;
	}

	private List<Tile> TileMatching_Square(Tile tile)
	{
		int[,] squareMatrix_R = new int[3, 3] {	{ 1, 0, -1 },
												{ 2, -1, -1 },
												{ 1, -1, 0 }};
		int[,] squareMatrix_L = new int[3, 3] {	{ -1, 1, 0 },
												{ -2, 1, 1 },
												{ -1, 0, 1 }};
		int[,] squareMatrix_T = new int[3, 3] {	{ -1, 1, 0 },
												{ 0, 1, -1 },
												{ 1, 0, -1 }};
		int[,] squareMatrix_B = new int[3, 3] {	{ -1, 0, 1 },
												{ 0, -1, 1 },
												{ 1, -1, 0 }};
		
		int matchingCount = 4;
		List<Tile> matchingList_Square_R = GetTileMatching_Square(tile, squareMatrix_R, tile.GetShape());
		if (matchingList_Square_R.Count < matchingCount) matchingList_Square_R.Clear();
		List<Tile> matchingList_Square_L = GetTileMatching_Square(tile, squareMatrix_L, tile.GetShape());
		if (matchingList_Square_L.Count < matchingCount) matchingList_Square_L.Clear();
		List<Tile> matchingList_Square_T = GetTileMatching_Square(tile, squareMatrix_T, tile.GetShape());
		if (matchingList_Square_T.Count < matchingCount) matchingList_Square_T.Clear();
		List<Tile> matchingList_Square_B = GetTileMatching_Square(tile, squareMatrix_B, tile.GetShape());
		if (matchingList_Square_B.Count < matchingCount) matchingList_Square_B.Clear();


		List<Tile> matchedTiles = new List<Tile>();
		MergeTileLists(matchedTiles, matchingList_Square_R);
		MergeTileLists(matchedTiles, matchingList_Square_L);
		MergeTileLists(matchedTiles, matchingList_Square_T);
		MergeTileLists(matchedTiles, matchingList_Square_B);

		return matchedTiles;
	}

	public List<Tile> TileMatching_ToyTops(Tile tile)
    {
		List<Tile> toyList = new List<Tile>();
		List<Tile> tilelist = GetTile_Around(tile, 1, TileShape.ToyTops);

		for(int j=0;j<tilelist.Count;j++)
        {
			Tile findTile = tilelist[j];
			if(toyList.Contains(findTile) == false)
            {
				toyList.Add(findTile);
			}
        }
		return toyList;
	}
	
	private List<Tile> GetTileMatching_Square(Tile tile, int[,] squareMatrix, TileShape matchShape)
    {
		if (tile == null) return null;
		List<Tile> checkedTiles = new List<Tile>();
		
		checkedTiles.Add(tile);
		for (int i = 0; i < 3; i++)
		{
			Vector3 findIndex = new Vector3(squareMatrix[i, 0], squareMatrix[i, 1], squareMatrix[i, 2]) + tile.GetInfo().GetGridIndex();
			Tile getTile = GetTileFromGridIndex(findIndex);
			if (getTile == null) continue;
			if (getTile.GetShape() != matchShape) break;
			checkedTiles.Add(getTile);
		}
		return checkedTiles;
	}

	public List<Tile> GetTile_Around(Tile tile, int range, TileShape matchShape = TileShape.MAX)
    {
		if (tile == null) return null;
		bool isFilter = matchShape != TileShape.MAX;
		List<Tile> list = new List<Tile>();
		
		for (int dx = -range; dx <= range; dx++)
		{
			for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++)
			{
				Vector3 findIndex = new Vector3(dx, dy, -dx - dy) + tile.GetInfo().GetGridIndex();
				Tile getTile = GetTileFromGridIndex(findIndex);
				if (getTile == null) continue;
				if (isFilter && getTile.GetShape() != matchShape) continue;
				list.Add(getTile);
			}
		}
		return list;
	}

	private List<Tile> GetTileMatching_Line_Vertical(Tile tile, int range = -1, TileShape matchShape = TileShape.MAX)
    {
		Vector3 checkDirection = new Vector3(0, 1, -1);
		if(range < 0) range = Math.Max(mapWidth, mapHeight) + 1;

		List<Tile> list = GetTileMatching_Line_Both(tile, range, checkDirection, matchShape);
		return list;
	}

	private List<Tile> GetTileMatching_Line_Diagonal_RightTop(Tile tile, int range = -1, TileShape matchShape = TileShape.MAX)
	{
		Vector3 checkDirection = new Vector3(1, 0, -1);
		if (range < 0) range = Math.Max(mapWidth, mapHeight) + 1;

		List<Tile> list = GetTileMatching_Line_Both(tile, range, checkDirection, matchShape);
		return list;
	}

	private List<Tile> GetTileMatching_Line_Diagonal_LeftTop(Tile tile, int range = -1, TileShape matchShape = TileShape.MAX)
	{
		Vector3 checkDirection = new Vector3(1, -1, 0);
		if (range < 0) range = Math.Max(mapWidth, mapHeight) + 1;

		List<Tile> list = GetTileMatching_Line_Both(tile, range, checkDirection, matchShape);
		return list;
	}

	private List<Tile> GetTileMatching_Line_Both(Tile tile, int range, Vector3 checkDirection, TileShape matchShape = TileShape.MAX)
    {
		if (tile == null) return null;
		List<Tile> list = new List<Tile>();
		int[] pingpongDir = new int[2] { 1, -1 };
		bool isFilter = matchShape != TileShape.MAX;

		list.Add(tile);

		int count = 1;
		for (int i = 0; i < range * 2; i++)
		{
			int dirIndex = i % 2;
			int dir = pingpongDir[dirIndex];
			if (dir == 0) continue;
			int j = count * dir;
			if (dirIndex == 1) count++;

			Vector3 indexVector = checkDirection * j;

			Vector3 findIndex = indexVector + tile.GetInfo().GetGridIndex();
			Tile getTile = GetTileFromGridIndex(findIndex);
			if (getTile == null) continue;

			if (isFilter && getTile.GetShape() != matchShape)
			{
				pingpongDir[dirIndex] = 0;
				continue;
			}
			list.Add(getTile);
		}
		return list;
	}

	private List<Tile> GetTileMatching_Line(Tile tile, int range, Vector3 checkDirection, bool pingpongDir, TileShape matchShape = TileShape.MAX)
	{
		if (tile == null) return null;
		List<Tile> list = new List<Tile>();
		bool isFilter = matchShape != TileShape.MAX;
		int dir = pingpongDir == true ? 1 : -1;
		list.Add(tile);

		int count = 1;
		for (int i = 0; i < range; i++)
		{
			int j = count * dir;
			count++;

			Vector3 indexVector = checkDirection * j;

			Vector3 findIndex = indexVector + tile.GetInfo().GetGridIndex();
			Tile getTile = GetTileFromGridIndex(findIndex);
			if (getTile == null) continue;

			if (isFilter && getTile.GetShape() != matchShape)
			{
				break;
			}
			list.Add(getTile);
		}
		return list;
	}


}

