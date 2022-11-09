using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

[System.Serializable]
public struct TileInfo
{
	public int x;
	public int y;
	public int z;
	public TileShape shape;

	public TileInfo(int x, int y, int z)
	{
		this.x = x; this.y = y; this.z = z;
		shape = TileShape.MAX;
	}

	public static TileInfo operator +(TileInfo one, TileInfo two)
	{
		return new TileInfo(one.x + two.x, one.y + two.y, one.z + two.z);
	}

	public Vector3 GetGridIndex()
    {
		return new Vector3(x, y, z);
    }

	public void SetGridIndex(Vector3 gridIndex)
    {
		x = (int)gridIndex.x;
		y = (int)gridIndex.y;
		z = (int)gridIndex.z;
	}

	public override bool Equals(object obj)
	{
		if (obj == null) return false;
		TileInfo o = (TileInfo)obj;
		if ((System.Object)o == null) return false;
		return ((x == o.x) && (y == o.y) && (z == o.z));
	}

	public override int GetHashCode()
	{
		return (x.GetHashCode() ^ (y.GetHashCode() + (int)(Mathf.Pow(2, 32) / (1 + Mathf.Sqrt(5)) / 2) + (x.GetHashCode() << 6) + (x.GetHashCode() >> 2)));
	}

	public override string ToString()
	{
		return string.Format("[" + x + "," + y + "," + z + "]");
	}
}

/// <summary>
/// 타일 오브젝트 클래스
/// </summary>
public class Tile : MonoBehaviour
{
	// 타일 움직임 버퍼 구조체
	public struct TileMovePosBuffer
	{
		public Vector3 destPos;	// 목적지 좌표
		public float durationTime;	// 이동 시간
		public AnimationCurve curve;	// 이동 애니메이션
		public Action onComplete;	// 이동 완료시 콜백

		public TileMovePosBuffer(Vector3 destPos, float durationTime, AnimationCurve curve, Action onComplete)
		{
			this.destPos = destPos;
			this.durationTime = durationTime;
			this.curve = curve;
			this.onComplete = onComplete;
		}
	};

	public bool _debug = false;

	[SerializeField] private TileInfo _Info;

	[SerializeField] private SpriteRenderer _spriteRenderer = null;
	public Sprite[] _shapeImages = null;
	[SerializeField] private Animator _animator = null;

	private List<TileMovePosBuffer> _movePosBuffer = new List<TileMovePosBuffer>();
	private Coroutine _coroutine_ProcessMovePosBuffer;

	private int _hp = 1;
	public int GetHp { get { return _hp; } }

	public TileInfo GetInfo()
    {
		return _Info;
    }

	public void Init(TileShape shape, Vector3 gridIndex)
    {
		_Info = new TileInfo((int)gridIndex.x, (int)gridIndex.y, (int)gridIndex.z);
		_hp = 1;
		if (shape == TileShape.ToyTops) _hp = 2;
		SetShape(shape);
	}

	public void SetGridIndex(Vector3 gridIndex)
    {
		_Info.SetGridIndex(gridIndex);
	}

	public bool Hit(int damage = 1)
    {
		bool isDie = false;
		_hp -= damage;
		if (_hp <= 0)
		{
			EffectMng.inst.CreateEffect("Explosion", transform.position);
			isDie = true;

			StageMng.inst.ScoreUp(30);
			if (GetShape() == TileShape.ToyTops)
            {
				StageMng.inst.ScoreUp(15);
				StageMng.inst.ToyTopDown();
			}
		}
		if(GetShape()==TileShape.ToyTops && _hp ==1)
        {
			GameObject fxObj = EffectMng.inst.CreateEffect("Fire", transform.position);
			fxObj.transform.SetParent(_spriteRenderer.transform);
		}
		return isDie;
    }


	public void SetShape(TileShape shape)
    {
		_Info.shape = shape;

		Sprite shapeImage = _shapeImages[(int)_Info.shape];
		_spriteRenderer.sprite = shapeImage;
	}

	public TileShape GetShape()
	{
		return _Info.shape;
	}

	public void MovePositionByIndex(Vector3 gridIndex_start, Vector3 gridIndex_dest, float durationTime = 0.1f, AnimationCurve curve = null)
	{
		StartCoroutine(Cor_MovePositionByIndex(gridIndex_start, gridIndex_dest, durationTime));
	}

	private IEnumerator Cor_MovePositionByIndex(Vector3 gridIndex_start, Vector3 gridIndex_dest, float durationTime = 0.1f, AnimationCurve curve = null)
	{
		Vector3 startPos = GridMap.inst.GridIndexToPosition(gridIndex_start);
		Vector3 destPos = GridMap.inst.GridIndexToPosition(gridIndex_dest);

		for (float i = 0.0f; i <= durationTime; i += Time.deltaTime)
		{
			float amount = i / durationTime * 1.0f;
			if (curve != null) amount = curve.Evaluate(amount);
			transform.localPosition = Vector3.Lerp(startPos, destPos, amount);
			yield return null;
		}
		transform.localPosition = destPos;
		yield return null;
	}

	public void MovePosition(Vector3 startPos, Vector3 destPos, float durationTime = 0.1f, AnimationCurve curve = null)
	{
		StartCoroutine(Cor_MovePosition(startPos, destPos, durationTime));
	}

	private IEnumerator Cor_MovePosition(Vector3 startPos, Vector3 destPos, float durationTime = 0.1f, AnimationCurve curve = null)
	{
		for (float i = 0.0f; i <= durationTime; i += Time.deltaTime)
		{
			float amount = i / durationTime * 1.0f;
			if(curve!=null) amount = curve.Evaluate(amount);
			transform.localPosition = Vector3.Lerp(startPos, destPos, amount);
			yield return null;
		}
		transform.localPosition = destPos;
		yield return null;
	}


	public void AddMovePosBuffer(Vector3 destPos, float duration = 0.1f, AnimationCurve curve = null, Action OnComplete = null)
    {
		if (_coroutine_ProcessMovePosBuffer != null) return;
		_movePosBuffer.Add(new TileMovePosBuffer(destPos, duration, curve, OnComplete));
    }

	public bool IsMoveProcessing()
    {
		return _coroutine_ProcessMovePosBuffer == null ? false : true;
    }

	public bool ProcessMovePosBuffer(Action onComplete)
    {
		if (_movePosBuffer.Count <= 0) return false;
		if (_coroutine_ProcessMovePosBuffer != null) return false;
		_coroutine_ProcessMovePosBuffer = StartCoroutine(Cor_ProcessMovePosBuffer(onComplete));
		return true;
    }

	private IEnumerator Cor_ProcessMovePosBuffer(Action onComplete)
    {
		for(int i=0;i< _movePosBuffer.Count;i++)
        {
			Vector3 startPos = transform.localPosition;
			Vector3 destPos = _movePosBuffer[i].destPos;
			float durationTime = _movePosBuffer[i].durationTime;
			AnimationCurve curve = _movePosBuffer[i].curve;
			Action onBufferComplete = _movePosBuffer[i].onComplete;
			yield return Cor_MovePosition(startPos, destPos, durationTime, curve);
			onBufferComplete?.Invoke();
		}
		_movePosBuffer.Clear();
		onComplete?.Invoke();
		_coroutine_ProcessMovePosBuffer = null;
		yield return null;
    }

	public void PlayAnimation(string stateName)
    {
		if (_animator == null) return;
		_animator.Play(stateName, 0, 0.0f);
    }

	private void OnGUI()
	{
		if (_debug == false) return;
		string text = _Info.ToString();
		GUIStyle style = new GUIStyle("label");
		style.fontSize = 16;
		style.normal.textColor = Color.white;
		Vector3 position = Camera.main.WorldToScreenPoint(transform.position);
		Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));
		GUI.Label(new Rect(position.x - (textSize.x / 2), Screen.height - position.y - (textSize.y / 2), textSize.x * 2, textSize.y), text, style);
	}

	public void ColorCheck()
    {
		StartCoroutine(Cor_ColorCheck());
    }

	IEnumerator Cor_ColorCheck()
    {
		_spriteRenderer.color = Color.red;

		yield return new WaitForSeconds(1.0f);
		_spriteRenderer.color = Color.white;
    }

}

