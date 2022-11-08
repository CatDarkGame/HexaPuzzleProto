using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타일 컨트롤 시나리오 클래스
/// </summary>
public class TileController : MonoBehaviour
{
	[SerializeField] private float _dragDirectonOffset = 30.0f;
	[SerializeField] private float _dragThresholdDistance = 15.0f;

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

	// 드래그 시나리오
	private IEnumerator Cor_TileDrag()
    {
		bool isOverThreshold = false;
		Vector3 mousePos = Input.mousePosition;
		while (_touchMode == TouchMode.Drag)
        {
			mousePos = Input.mousePosition;
			float startToEndDistance = Vector3.Distance(_startMousePos, mousePos);

			if (startToEndDistance >= _dragThresholdDistance)
			{
				_touchMode = TouchMode.None;
				isOverThreshold = true;
			}

			yield return null;
		}
		
		if(_touchMode==TouchMode.None)
        {
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
					float distance = Vector3.Distance(destPos, mousePos);
					if (distance < checkDestDistance)
					{
						directionIndex = (TileDirection)i;
						checkDestDistance = distance;
					}
				}

				yield return Cor_TileSwap(_selectTile, directionIndex);
				yield return Cor_ReloadTiles();
				
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

	// 타일 스왑 시나리오
	private IEnumerator Cor_TileSwap(Tile selectTile, TileDirection direction)
    {
		// 2개의 타일 스왑 & 직선 매칭 검사
		Tile swapedTile = GridMap.inst.TileSwap(selectTile, direction);
		if (swapedTile == null) yield break;
		List<Tile> matchingList_merge = new List<Tile>();
		List<Tile> matchTileList_select = GridUtill.GetMatchTile_Line(GridMap.inst, selectTile.GetInfo().GetGridIndex(), 3);
		List<Tile> matchTileList_swaped = GridUtill.GetMatchTile_Line(GridMap.inst, swapedTile.GetInfo().GetGridIndex(), 3);
		GridUtill.MergeTileList(matchingList_merge, matchTileList_select);
		GridUtill.MergeTileList(matchingList_merge, matchTileList_swaped);
		int matchCount = matchingList_merge.Count;

		// 타일 스왑 애니메이션 실행
		selectTile.Animation_MoveIndex(swapedTile.GetInfo().GetGridIndex(), selectTile.GetInfo().GetGridIndex(), 0.1f);
		swapedTile.Animation_MoveIndex(selectTile.GetInfo().GetGridIndex(), swapedTile.GetInfo().GetGridIndex(), 0.1f);

		yield return new WaitForSeconds(0.15f);
		if (matchCount == 0)
		{
			GridMap.inst.TileSwap(swapedTile, direction);
			selectTile.Animation_MoveIndex(swapedTile.GetInfo().GetGridIndex(), selectTile.GetInfo().GetGridIndex(), 0.1f);
			swapedTile.Animation_MoveIndex(selectTile.GetInfo().GetGridIndex(), swapedTile.GetInfo().GetGridIndex(), 0.1f);
			yield break;
		}

		// 매칭된 타일들 제거
		for (int i = 0; i < matchingList_merge.Count; i++)
		{
			if (matchingList_merge[i] == null) continue;
			bool isDie = matchingList_merge[i].Hit();
			if (isDie)
			{
				Vector3 gridIndex = matchingList_merge[i].GetInfo().GetGridIndex();
				GridMap.inst.KillTile(matchingList_merge[i]);
			}
		}

		yield return null;
    }

	// 타일 드랍 & 신규 생성 시나리오
	private IEnumerator Cor_ReloadTiles()
	{
		List<Vector3> emptyTileList = GridMap.inst.GetEmptyIndex();

		// 세로 방향 타일 드랍
		List<Tile> pullTiles = new List<Tile>();
		for (int i = 0; i < emptyTileList.Count; i++)
		{
			Vector3 gridIndex = emptyTileList[i];
			Tile tile = GridMap.inst.GetTileFromGridIndex(gridIndex);
			List<Tile> tiles = GridMap.inst.PullDownTiles(gridIndex);
			GridUtill.MergeTileList(pullTiles, tiles);
		}
		for (int i = 0; i < pullTiles.Count; i++)
		{
			Tile tile = pullTiles[i];
			if (tile == null) continue;
			Vector3 startPos = tile.transform.localPosition;
			Vector3 destPos = GridMap.inst.GridIndexToPosition(tile.GetInfo().GetGridIndex());
			tile.Animation_MovePos(startPos, destPos);
		}
		pullTiles.Clear();

		// 대각선 방향 타일 드랍
		int pingpongCount = 0;
		for (int i = 0; i < GridMap.inst.MapWidth * 2; i++)
		{
			Vector3 gridIndex = GridUtill.GetGridIndex_Y_Min(GridMap.inst, new Vector3(0, 0, 0));
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
		for (int k = pullTiles.Count - 1; k >= 0; k--)
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

		// 신규 생성 타일 대각선 드랍
		for (int k = 0; k < emptyTileList.Count; k++)
		{
			Vector3 gridIndex = GridUtill.GetGridIndex_Y_Min(GridMap.inst, new Vector3(0, 0, 0));
			Tile checkTopTile = GridMap.inst.GetTileFromGridIndex(gridIndex);
			if (checkTopTile != null) break;
			Tile tile = GridMap.inst.CreateTileObject(gridIndex, GridMap.inst.GetTileShapeRandom());

			int[,] moveDirMatrix = new int[3, 3] {  { 0, -1, 1 },
													{ -1, 0, 1 },
													{ 1, -1, 0 }};
			if (k % 2 == 1)
			{
				moveDirMatrix = new int[3, 3] {     { 0, -1, 1 },
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
				if (isEmpty == false) break;
				tile.SetGridIndex(checkIndex);
				Vector3 startPos = GridMap.inst.GridIndexToPosition(gridIndex);
				Vector3 destPos = GridMap.inst.GridIndexToPosition(checkIndex);
				tile.Animation_MovePos(startPos, destPos, 0.3f);
				yield return new WaitForSeconds(0.3f);
			}
		}

		// 전체 타일 매칭 체크
		List<Tile> allTileList = GridMap.inst.GetTileList();
		List<Tile> matchingList_merge = new List<Tile>();
		for (int i = 0; i < allTileList.Count; i++)
		{
			Tile tile = allTileList[i];
			if (tile == null) continue;
			List<Tile> matchTiles = GridUtill.GetMatchTile_Line(GridMap.inst, tile.GetInfo().GetGridIndex(), 3);
			GridUtill.MergeTileList(matchingList_merge, matchTiles);
		}
		for (int i = 0; i < matchingList_merge.Count; i++)
		{
			if (matchingList_merge[i] == null) continue;
			bool isDie = matchingList_merge[i].Hit();
			if (isDie)
			{
				Vector3 gridIndex = matchingList_merge[i].GetInfo().GetGridIndex();
				GridMap.inst.KillTile(matchingList_merge[i]);
			}
		}

		yield return new WaitForSeconds(0.3f);
		if (matchingList_merge.Count > 0)
		{
			yield return StartCoroutine(Cor_ReloadTiles());
		}

		yield return null;
	}

}


