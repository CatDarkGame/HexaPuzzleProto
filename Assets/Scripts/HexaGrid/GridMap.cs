using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// 그리드 타일맵 시스템 클래스
/// </summary>
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

	public Vector3 GetGridDirection(GridDirection direction)
    {
		Vector3 dir = Vector3.zero;
		int dirIndex = (int)direction;
		dir.x = _directionMatrix[dirIndex, 0];
		dir.y = _directionMatrix[dirIndex, 1];
		dir.z = _directionMatrix[dirIndex, 2];
		return dir;
	}

	[SerializeField] private int mapWidth;
	[SerializeField] private int mapHeight;
	[SerializeField] private float hexRadius = 1;

	public int MapWidth { get { return mapWidth; } }
	public int MapHeight { get { return mapHeight; } }
	public float GridRadius { get { return hexRadius; } }

	private List<Tile> _tileList = new List<Tile>();
	private int _tileCreateCount = 0;

	public GameObject tilePrefab = null;    // TODO : 오브젝트 폴링으로 대체
	
	private void Awake()
	{
		if (!inst) inst = this;

		//GenerateGrid();
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

	// 그리드 & 타일 생성
	public void GenerateGrid() 
	{
		ClearGrid();

		int width = mapWidth;
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

	// 그리드 & 타일 클리어
	public void ClearGrid() 
	{
		_tileCreateCount = 0;
		for (int i=0;i< _tileList.Count;i++)
        {
			DestroyImmediate(_tileList[i].gameObject, false);
		}
		_tileList.Clear();
	}

	/// <summary>
	/// 타일 오브젝트 생성
	/// </summary>
	/// <param name="gridIndex"></param> 그리드 인덱스(배열 넘버)
	/// <param name="tileShape"></param> 타일 종류
	/// <returns></returns>
	public Tile CreateTileObject(Vector3 gridIndex, TileShape tileShape)
	{
		Vector3 tilePostion = GridIndexToPosition(gridIndex);
		string objName = string.Format("Tile[{0}]", _tileCreateCount++); ;

		GameObject go = Instantiate(tilePrefab);	// TODO : 오브젝트 폴링으로 대체
		go.name = objName;
		go.transform.localPosition = tilePostion;
		go.transform.SetParent(transform);

		Tile tile = go.GetComponent<Tile>();
		tile.Init(tileShape, gridIndex);
		_tileList.Add(tile);
		return tile;
	}

	// 타일 오브젝트 제거
	public void KillTile(Tile tile)
    {
		if (tile == null) return;

		_tileList.Remove(tile);
		Destroy(tile.gameObject);   // TODO : 오브젝트 폴링으로 대체
	}

	// 그리드 인덱스 -> Transform 좌표 변환
	public Vector3 GridIndexToPosition(Vector3 gridIndex)
	{
		Vector3 tilePostion = Vector3.zero;

		float radius = hexRadius;
		tilePostion.x = radius * 3.0f / 2.0f * gridIndex.x;
		tilePostion.y = radius * Mathf.Sqrt(3.0f) * (gridIndex.y + gridIndex.x / 2.0f);
		return tilePostion;
	}

	// 전체 타일 리스트 참조
	public List<Tile> GetTileList()
	{
		return _tileList;
	}

	// 그리드 인덱스 -> 타일 오브젝트 참조
	public Tile GetTileFromGridIndex(Vector3 gridIndex)
	{
		for (int i = 0; i < _tileList.Count; i++)
		{
			if (_tileList[i].GetInfo().GetGridIndex() == gridIndex) return _tileList[i];
		}
		return null;
	}

	// 타일 종류 랜덤 참조
	public TileShape GetTileShapeRandom()
	{
		TileShape[] shapeRandomList = { TileShape.Red, TileShape.Green, TileShape.Orange, TileShape.Purple, TileShape.Blue, TileShape.ToyTops };
		//TileShape[] shapeRandomList = { TileShape.Red, TileShape.Green, TileShape.Orange, TileShape.Purple, TileShape.Blue };
		int randomShapeMax = shapeRandomList.Length;
		int randomShape = UnityEngine.Random.Range(0, randomShapeMax);
		return shapeRandomList[randomShape];
	}


	public List<Tile> PullDownTiles(Vector3 gridIndex)
	{
		int r1 = Mathf.Max(-((int)gridIndex.x + mapWidth), -mapWidth);
		int r2 = Mathf.Min(mapHeight, -(int)gridIndex.x + mapHeight) - 1;
		
		List<Tile> pullTiles = new List<Tile>();
		for (int i = r1; i <= r2; i++)
		{
			Vector3 checkIndex = new Vector3(gridIndex.x, i, -gridIndex.x - i);
			Tile tile = GetTileFromGridIndex(checkIndex);
			if (tile == null) continue;
			pullTiles.Add(tile);
		}

		int pullTileCount = 0;
		for (int i = r1; i <= r2; i++)
		{
			if (pullTiles.Count <= pullTileCount) break;
			Tile tile = pullTiles[pullTileCount++];
			if (tile == null) continue;
			Vector3 checkIndex = new Vector3(gridIndex.x, i, -gridIndex.x - i);
			tile.SetGridIndex(checkIndex);
		}
		
		// TODO : 장애물을 고려한 기능 추가 필요
		return pullTiles;
	}

	public bool CheckInGridArray(Vector3 gridIndex)
    {
		bool isIn = false;

		int width = mapWidth;
		int height = mapHeight;
		for (int q = -width; q <= width; q++)
		{
			int r1 = Mathf.Max(-width, -q - width);
			int r2 = Mathf.Min(height, -q + height);

			for (int r = r1; r < r2; r++)
			{
				Vector3 index = new Vector3(q, r, -q - r);
				if (gridIndex== index)
                {
					isIn = true;
					break;
                }
			}
		}
		return isIn;
	}

	public List<Vector3> GetEmptyIndex()
	{
		List<Vector3> list = new List<Vector3>();
		int width = mapWidth;
		int height = mapHeight;
		for (int q = -width; q <= width; q++)
		{
			int r1 = Mathf.Max(-width, -q - width);
			int r2 = Mathf.Min(height, -q + height);

			for (int r = r1; r < r2; r++)
			{
				Vector3 index = new Vector3(q, r, -q - r);
				Tile tile = GetTileFromGridIndex(index);
				if(tile==null)
                {
					list.Add(index);
                }
			}
		}
		return list;
	}


	public Tile TileSwap(Tile tile, GridDirection direction)
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

}

