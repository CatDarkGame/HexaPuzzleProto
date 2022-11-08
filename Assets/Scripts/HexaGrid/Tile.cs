using UnityEngine;
using System.Collections.Generic;
using System.Collections;


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


public class Tile : MonoBehaviour
{
	public bool _debug = false;

	[SerializeField] private TileInfo _Info;

	[SerializeField] private SpriteRenderer _spriteRenderer = null;
	public Sprite[] _shapeImages = null;

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
		if (_hp <= 0) isDie = true;
		return isDie;
    }

	public void Animation_MoveIndex(Vector3 gridIndex_start, Vector3 gridIndex_dest, float durationTime = 0.1f)
    {
		StartCoroutine(Cor_Animation_MoveIndex(gridIndex_start, gridIndex_dest, durationTime));
    }

	IEnumerator Cor_Animation_MoveIndex(Vector3 gridIndex_start, Vector3 gridIndex_dest, float durationTime = 0.1f)
    {
		Vector3 startPos = GridMap.inst.GridIndexToPosition(gridIndex_start);
		Vector3 destPos = GridMap.inst.GridIndexToPosition(gridIndex_dest);

		for (float i = 0.0f; i <= durationTime; i += Time.deltaTime)
        {
			float amount = i / durationTime * 1.0f;
			transform.localPosition = Vector3.Lerp(startPos, destPos, amount);
			yield return null;
        }
		transform.localPosition = destPos;
		yield return null;
    }

	public void Animation_MovePos(Vector3 startPos, Vector3 destPos, float durationTime = 0.1f)
	{
		StartCoroutine(Cor_Animation_MovePos(startPos, destPos, durationTime));
	}

	IEnumerator Cor_Animation_MovePos(Vector3 startPos, Vector3 destPos, float durationTime = 0.1f)
	{
		for (float i = 0.0f; i <= durationTime; i += Time.deltaTime)
		{
			float amount = i / durationTime * 1.0f;
			transform.localPosition = Vector3.Lerp(startPos, destPos, amount);
			yield return null;
		}
		transform.localPosition = destPos;
		yield return null;
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

