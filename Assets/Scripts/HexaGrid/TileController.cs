using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
	public enum TouchMode
    {
		None,
		Drag,
    };

	private float _dragDirectonOffset = 30.0f;
	private float _dragThresholdDistance = 15.0f;

	private TouchMode _touchMode = TouchMode.None;
	private Vector3 _startMousePos;
	private Tile _selectTile = null;

	private Camera _mainCamera = null;

	void Start()
    {
		_mainCamera = Camera.main;
		_touchMode = TouchMode.None;
	}


	private void Update()
	{
		if (_mainCamera == null) return;

		if(_touchMode == TouchMode.None)
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

				Tile swapedTile = GridMap.inst.TileSwap(_selectTile, directionIndex);
				int matchCount_select = GridMap.inst.TileMatching(_selectTile);
				int matchCount_swaped = GridMap.inst.TileMatching(swapedTile);

				float aniTime = 0.1f;
				_selectTile.Animation_MoveIndex(swapedTile.GetInfo().GetGridIndex(), _selectTile.GetInfo().GetGridIndex(), aniTime);
				swapedTile.Animation_MoveIndex(_selectTile.GetInfo().GetGridIndex(), swapedTile.GetInfo().GetGridIndex(), aniTime);

				yield return new WaitForSeconds(aniTime);
				if (matchCount_select == 0 && matchCount_swaped == 0)
				{
					yield return new WaitForSeconds(0.1f);
					GridMap.inst.TileSwap(swapedTile, directionIndex);
					_selectTile.Animation_MoveIndex(swapedTile.GetInfo().GetGridIndex(), _selectTile.GetInfo().GetGridIndex(), aniTime);
					swapedTile.Animation_MoveIndex(_selectTile.GetInfo().GetGridIndex(), swapedTile.GetInfo().GetGridIndex(), aniTime);
				}

			}
			else
			{
				Debug.Log("Not over Th");
			}
			_selectTile = null;
		}

		yield return null;
    }
}
