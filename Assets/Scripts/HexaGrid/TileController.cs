using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
	private float _dragDirectonOffset = 30.0f;
	private float _dragThresholdDistance = 15.0f;

	private TouchMode _touchMode = TouchMode.None;
	private Vector3 _startMousePos;
	private Tile _selectTile = null;

	private Camera _mainCamera = null;
	private bool _blockControl = false;

	void Start()
    {
		_mainCamera = Camera.main;
		_touchMode = TouchMode.None;
		_blockControl = false;
	}

	private void Update()
	{
		if (_mainCamera == null) return;

		if(_touchMode == TouchMode.None && _blockControl==false)
        {
			if (Input.GetMouseButtonDown(0))
			{
				Vector3 mousePos = Input.mousePosition;
				Vector3 screenToWorldPos = _mainCamera.ScreenToWorldPoint(mousePos);
				int layerMask = 1 << LayerMask.NameToLayer("Tile");
				RaycastHit2D hitInfo = Physics2D.Raycast(screenToWorldPos, Vector2.zero, 1.0f, layerMask);

				if (hitInfo)
				{
					Tile tile = hitInfo.transform.GetComponent<Tile>();
					if (tile == null) return;

					_selectTile = tile;
					_startMousePos = mousePos;
					_touchMode = TouchMode.Drag;

						StartCoroutine(Cor_TileDrag());
					

					/*Vector3 gridIndex = tile.GetInfo().GetGridIndex();
					Vector3 checkIndex = GetIndex_ZRight(gridIndex);
					Tile getTile = GridMap.inst.GetTileFromGridIndex(checkIndex);
					if (getTile != null) getTile.ColorCheck();*/


				}
			}
		}

		if (_touchMode == TouchMode.Drag)
		{
			if (Input.GetMouseButtonUp(0))
			{
				_touchMode = TouchMode.None;
			}
		}
	}


	private IEnumerator Cor_TileDrag()
    {
		while(_touchMode == TouchMode.Drag)
        {
			yield return null;
		}
		
		if(_touchMode==TouchMode.None)
        {
			Vector3 endMousePos = Input.mousePosition;
			float startToEndDistance = Vector3.Distance(_startMousePos, endMousePos);

			bool isOverThreshold = false;
			if (startToEndDistance >= _dragThresholdDistance) isOverThreshold = true;

			if (isOverThreshold)
			{
				_blockControl = true;
				float checkDestDistance = 9999.0f;
				TileDirection directionIndex = 0;
				int[,] directionPos = GridMap.inst.DirectionMatrix;
				for (int i = 0; i < 6; i++)
				{
					Vector3 dir = new Vector3(directionPos[i, 0], directionPos[i, 1], directionPos[i, 2]);
					Vector3 destPos = _startMousePos + (dir * _dragDirectonOffset);
					float distance = Vector3.Distance(destPos, endMousePos);
					if (distance < checkDestDistance)
					{
						directionIndex = (TileDirection)i;
						checkDestDistance = distance;
					}
				}

				yield return Cor_TileSwap(_selectTile, directionIndex);

				_blockControl = false;
			}
			else
			{
				Debug.Log("Not over Th");
			}
			_selectTile = null;
		}

		yield return null;
    }


	private IEnumerator Cor_TileSwap(Tile selectTile, TileDirection direction)
    {
		Tile swapedTile = GridMap.inst.TileSwap(selectTile, direction);
		if (swapedTile == null) yield break;

		List<Tile> matchingList_merge = new List<Tile>();
		List <Tile> matchTileList_select = GridMap.inst.TileMatching(selectTile);
		List<Tile> matchTileList_swaped = GridMap.inst.TileMatching(swapedTile);
		GridMap.inst.MergeTileLists(matchingList_merge, matchTileList_select);
		GridMap.inst.MergeTileLists(matchingList_merge, matchTileList_swaped);

		int matchCount = matchingList_merge.Count;
		float aniTime = 0.1f;
		selectTile.Animation_MoveIndex(swapedTile.GetInfo().GetGridIndex(), selectTile.GetInfo().GetGridIndex(), aniTime);
		swapedTile.Animation_MoveIndex(selectTile.GetInfo().GetGridIndex(), swapedTile.GetInfo().GetGridIndex(), aniTime);

		yield return new WaitForSeconds(aniTime);
		if (matchCount == 0)
		{
			yield return new WaitForSeconds(0.1f);
			GridMap.inst.TileSwap(swapedTile, direction);
			_selectTile.Animation_MoveIndex(swapedTile.GetInfo().GetGridIndex(), selectTile.GetInfo().GetGridIndex(), aniTime);
			swapedTile.Animation_MoveIndex(selectTile.GetInfo().GetGridIndex(), swapedTile.GetInfo().GetGridIndex(), aniTime);
		}
		else
        {
			while(true)
            {
				List<Tile> toyTiles = new List<Tile>();
				List<Vector3> killTileIndexs = new List<Vector3>();
				for (int i = 0; i < matchingList_merge.Count; i++)
				{
					if (matchingList_merge[i] == null) continue;
					
					bool isDie = matchingList_merge[i].Hit();
					if(isDie)
                    {
						Vector3 gridIndex = matchingList_merge[i].GetInfo().GetGridIndex();
						GridMap.inst.KillTile(matchingList_merge[i]);
						killTileIndexs.Add(gridIndex);

						List<Tile> findToyTiles = GridMap.inst.TileMatching_ToyTops(matchingList_merge[i]);
						GridMap.inst.MergeTileLists(toyTiles, findToyTiles);
					}
				}

				for(int i=0;i< toyTiles.Count;i++)
                {
					if (toyTiles[i] == null) continue;
					bool isDie = toyTiles[i].Hit();
					if (isDie)
					{
						GridMap.inst.KillTile(toyTiles[i]);
						killTileIndexs.Add(toyTiles[i].GetInfo().GetGridIndex());
					}
				}

				matchingList_merge.Clear();
				yield return new WaitForSeconds(0.1f);
				
				List<Tile> pullTiles = new List<Tile>();
				for (int i = 0; i < killTileIndexs.Count; i++)
				{
					Vector3 gridIndex = killTileIndexs[i];
					List<Tile> tiles = GridMap.inst.PullDownTiles(gridIndex);
					GridMap.inst.MergeTileLists(pullTiles, tiles);
				}
				for (int i = 0; i < pullTiles.Count; i++)
				{
					Tile tile = pullTiles[i];
					if (tile == null) continue;
					Vector3 startPos = tile.transform.localPosition;
					Vector3 destPos = GridMap.inst.GridIndexToPosition(tile.GetInfo().GetGridIndex());
					tile.Animation_MovePos(startPos, destPos);
				}
				yield return new WaitForSeconds(0.3f);


				pullTiles.Clear();
				int pingpongCount = 0;
				for (int i = 0; i < GridMap.inst.MapWidth * 2; i++)
				{
					Vector3 gridIndex = GetIndex_YTop(new Vector3(0, 0, 0));
					int[,] moveDirMatrix = new int[2, 3] {  { -1, 0, 1 },
															{ 1, -1, 0 }};
					int dir = i % 2;
					if (dir == 1) pingpongCount++;
					Vector3 moveDir = new Vector3(moveDirMatrix[dir, 0], moveDirMatrix[dir, 1], moveDirMatrix[dir, 2]);
					Vector3 checkIndex = gridIndex + (moveDir * pingpongCount);
					Tile tile = GridMap.inst.GetTileFromGridIndex(checkIndex);
					if (tile == null) continue;
					pullTiles.Add(tile);
				}
				for(int k= pullTiles.Count - 1; k >= 0; k--)
                {
					Tile tile = pullTiles[k];
					if (tile == null) continue;
					int[,] moveDirMatrix = new int[3, 3] {  { 0, -1, 1 },
															{ -1, 0, 1 },
															{ 1, -1, 0 }};
					for (int i = 0; i < GridMap.inst.GetTileList().Count; i++)
					{
						Vector3 gridIndex = tile.GetInfo().GetGridIndex();
						Vector3 checkIndex = Vector3.zero;
						bool isEmpty = false;
						for (int j = 0; j < 3; j++)
						{
							if (gridIndex.x < 0 && j == 2) continue;
							if (gridIndex.x > 0 && j == 1) continue;
							Vector3 moveDir = new Vector3(moveDirMatrix[j, 0], moveDirMatrix[j, 1], moveDirMatrix[j, 2]);
							checkIndex = gridIndex + moveDir;
							if (GridMap.inst.CheckInGridArray(checkIndex) == false) continue;
							if (GridMap.inst.GetTileFromGridIndex(checkIndex) == null)
							{
								isEmpty = true;
								break;
							}
						}
						if (isEmpty == false) break;
						tile.SetGridIndex(checkIndex);
						Vector3 startPos = GridMap.inst.GridIndexToPosition(gridIndex);
						Vector3 destPos = GridMap.inst.GridIndexToPosition(checkIndex);
						tile.Animation_MovePos(startPos, destPos, 0.3f);
						yield return new WaitForSeconds(0.3f);
					}
				}


				for (int k = 0; k < killTileIndexs.Count; k++)
				{
					Tile tile = null;
					Vector3 gridIndex = GetIndex_YTop(new Vector3(0, 0, 0));
					Tile checkTopTile = GridMap.inst.GetTileFromGridIndex(gridIndex);
					if (checkTopTile != null) break;
					tile = GridMap.inst.CreateTileObject(gridIndex, GridMap.inst.GetTileShapeRandom());
					
					int[,] moveDirMatrix = new int[3, 3] {  { 0, -1, 1 },
															{ -1, 0, 1 },
															{ 1, -1, 0 }};
					if (k%2==1)
                    {
						moveDirMatrix = new int[3, 3] { { 0, -1, 1 },
														{ 1, -1, 0 },
														{ -1, 0, 1 }};
					}
					for (int i = 0; i < GridMap.inst.GetTileList().Count; i++)
					{
						gridIndex = tile.GetInfo().GetGridIndex();
						Vector3 checkIndex = Vector3.zero;
						bool isEmpty = false;
						for (int j = 0; j < 3; j++)
						{
							Vector3 moveDir = new Vector3(moveDirMatrix[j, 0], moveDirMatrix[j, 1], moveDirMatrix[j, 2]);
							checkIndex = gridIndex + moveDir;
							if (GridMap.inst.CheckInGridArray(checkIndex) == false) continue;
							if (GridMap.inst.GetTileFromGridIndex(checkIndex) == null)
							{
								isEmpty = true;
								break;
							}
						}
						if (isEmpty== false) break;
						tile.SetGridIndex(checkIndex);
						Vector3 startPos = GridMap.inst.GridIndexToPosition(gridIndex);
						Vector3 destPos = GridMap.inst.GridIndexToPosition(checkIndex);
						tile.Animation_MovePos(startPos, destPos, 0.3f);
						yield return new WaitForSeconds(0.3f);
					}
				}

				List<Tile> allTiles = GridMap.inst.GetTileList();
				for (int i = 0; i < allTiles.Count; i++)
				{
					Tile tile = allTiles[i];
					if (tile == null) continue;
					List<Tile> matchTiles = GridMap.inst.TileMatching(tile);
					GridMap.inst.MergeTileLists(matchingList_merge, matchTiles);
				}

				yield return new WaitForSeconds(0.3f);
				if (matchingList_merge.Count <= 0) break;
			}

		}
		yield return null;
    }

	private Vector3 GetIndex_YTop(Vector3 gridIndex)
	{
		int mapHeight = GridMap.inst.MapHeight;
		int r1 = Mathf.Max((int)gridIndex.x, 0);
		int indexY = mapHeight - r1 - 1;
		Vector3 checkIndex = new Vector3(gridIndex.x, indexY, -gridIndex.x - indexY);
		return checkIndex;
	}

	private Vector3 GetIndex_YBottom(Vector3 gridIndex)
	{
		int mapHeight = GridMap.inst.MapHeight;
		int r1 = Mathf.Max(-((int)gridIndex.x + mapHeight), -mapHeight);
		Vector3 checkIndex = new Vector3(gridIndex.x, r1, -gridIndex.x - r1);
		return checkIndex;
	}

	private Vector3 GetIndex_XLeft(Vector3 gridIndex)
    {
		int mapWidth = GridMap.inst.MapWidth - 1;
		//int r1 = Mathf.Min(mapWidth, -(int)gridIndex.y + mapWidth - 1); // 우측 상단
		int r1 = Mathf.Max(-mapWidth, -(int)gridIndex.y - mapWidth);
		Vector3 checkIndex = new Vector3(r1, (int)gridIndex.y, -r1 - (int)gridIndex.y);
		return checkIndex;
	}

	private Vector3 GetIndex_ZRight(Vector3 gridIndex)
    {
		int mapWidth = GridMap.inst.MapWidth - 1;
		//int r1 = Mathf.Min(mapWidth - 1, -(int)gridIndex.z + mapWidth); // 우측 상단
		int r1 = Mathf.Max(-mapWidth, -(int)gridIndex.z - mapWidth);
		Vector3 checkIndex = new Vector3(-r1 - (int)gridIndex.z, r1, (int)gridIndex.z);
		return checkIndex;
	}
}


