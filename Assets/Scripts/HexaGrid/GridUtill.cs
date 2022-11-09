using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 그리드 활용 기능 클래스
/// </summary>
public static class GridUtill
{
	// 라인 형태 타일 매칭 체크
	public static List<Tile> GetMatchTile_Line(GridMap gridMap, Vector3 gridIndex, int matchCount_Min)
    {
		List<Tile>[] tileList_Axis = new List<Tile>[3];
		tileList_Axis[0] = GetTileList_Line(gridMap, gridIndex, GridAxis.X);
		tileList_Axis[1] = GetTileList_Line(gridMap, gridIndex, GridAxis.Y);
		tileList_Axis[2] = GetTileList_Line(gridMap, gridIndex, GridAxis.Z);

		Tile tile = gridMap.GetTileFromGridIndex(gridIndex);
		List<Tile> mergeMatchTIle = new List<Tile>();
		for(int k=0;k<3;k++)
        {
			List<Tile> checkTileList = new List<Tile>();
			List<Tile> tileList = tileList_Axis[k];
			for (int i = 0; i < tileList.Count; i++)
			{
				if (tileList[i] != tile) continue;
				
				for (int j = i; j >= 0; j--)
				{
					Tile checkTile = tileList[j];
					if (checkTile == null || checkTile.GetShape() != tile.GetShape()) break;
					checkTileList.Add(checkTile);
				}
				for (int j = i + 1; j < tileList.Count; j++)
				{
					Tile checkTile = tileList[j];
					if (checkTile == null || checkTile.GetShape() != tile.GetShape()) break;
					checkTileList.Add(checkTile);
				}
				if (checkTileList.Count >= matchCount_Min)
				{
					for (int j = 0; j < checkTileList.Count; j++)
					{
						if (mergeMatchTIle.Contains(checkTileList[j]) == true) continue;
						mergeMatchTIle.Add(checkTileList[j]);
					}
					break;
				}
			}
		}

		return mergeMatchTIle;
	}

	// 특정 축의 타일 리스트 참조
	public static List<Tile> GetTileList_Line(GridMap gridMap, Vector3 gridIndex, GridAxis gridAxis)
	{
		int mapWidth = gridMap.MapWidth;
		int mapHeight = gridMap.MapHeight;
		int maxLine = mapWidth + mapHeight;

		Vector3 gridIndex_Max = Vector3.zero;
		Vector3 checkDir = gridMap.GetGridDirection(GridDirection.RightTop);
		if (gridAxis==GridAxis.X)
        {
			gridIndex_Max = GetGridIndex_X_Max(gridMap, gridIndex);
			checkDir = gridMap.GetGridDirection(GridDirection.RightTop);
		}
		else if(gridAxis==GridAxis.Y)
        {
			gridIndex_Max = GetGridIndex_Y_Max(gridMap, gridIndex);
			checkDir = gridMap.GetGridDirection(GridDirection.Top);
		}
		else if(gridAxis==GridAxis.Z)
        {
			gridIndex_Max = GetGridIndex_Z_Max(gridMap, gridIndex);
			checkDir = gridMap.GetGridDirection(GridDirection.LeftTop);
		}

		List<Tile> checkTileList = new List<Tile>();
		for (int i=0; i< maxLine; i++)
        {
			Vector3 checkIndex = gridIndex_Max + (checkDir * i);

			Tile checkTile = gridMap.GetTileFromGridIndex(checkIndex);
			if (checkTile == null) break;
			checkTileList.Add(checkTile);
		}

		return checkTileList;
	}

	public static List<Tile> GetTileList_Around(GridMap gridMap, Vector3 gridIndex, int range)
	{
		List<Tile> list = new List<Tile>();
		for (int dx = -range; dx <= range; dx++)
		{
			for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++)
			{
				Vector3 findIndex = new Vector3(dx, dy, -dx - dy) + gridIndex;
				Tile getTile = gridMap.GetTileFromGridIndex(findIndex);
				if (getTile == null) continue;
				list.Add(getTile);
			}
		}
		return list;
	}

	/*
	 * private List<Tile> TileMatching_Square(Tile tile)
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

		

	 */

	// 특정 축의 끝 인덱스 참조
	public static Vector3 GetGridIndex_X_Max(GridMap gridMap, Vector3 gridIndex)
	{
		int mapSize = gridMap.MapWidth;
		int r1 = Mathf.Max(-mapSize, -(int)gridIndex.y - mapSize);
		Vector3 checkIndex = new Vector3(r1, (int)gridIndex.y, -r1 - (int)gridIndex.y);
		return checkIndex;
	}

	public static Vector3 GetGridIndex_Y_Min(GridMap gridMap, Vector3 gridIndex)
	{
		int mapSize = gridMap.MapHeight;
		int r2 = Mathf.Min(mapSize, -(int)gridIndex.x + mapSize) - 1;
		Vector3 checkIndex = new Vector3(gridIndex.x, r2, -gridIndex.x - r2);
		return checkIndex;
	}

	public static Vector3 GetGridIndex_Y_Max(GridMap gridMap, Vector3 gridIndex)
	{
		int mapSize = gridMap.MapWidth;
		int r1 = Mathf.Max(-((int)gridIndex.x + mapSize), -mapSize);
		Vector3 checkIndex = new Vector3(gridIndex.x, r1, -gridIndex.x - r1);
		return checkIndex;
	}

	public static Vector3 GetGridIndex_Z_Max(GridMap gridMap, Vector3 gridIndex)
	{
		int mapSize = GridMap.inst.MapWidth;
		int r1 = Mathf.Max(-mapSize, -(int)gridIndex.z - mapSize);
		Vector3 checkIndex = new Vector3(-r1 - (int)gridIndex.z, r1, (int)gridIndex.z);
		return checkIndex;
	}

	// 2개의 타일 리스트 중복 제거 & 합치기
	public static void MergeTileList(List<Tile> matchedTiles, List<Tile> tiles)
	{
		if (matchedTiles ==null || tiles == null) return;
		for (int i = 0; i < tiles.Count; i++)
		{
			Tile checkTile = tiles[i];
			if (checkTile == null) continue;
			if (matchedTiles.Contains(checkTile) == true) continue;
			matchedTiles.Add(checkTile);
		}
	}

}
