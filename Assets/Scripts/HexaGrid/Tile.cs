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

	private TileInfo _Info;
	public TileInfo GetInfo()
    {
		return _Info;
    }

	public void Init(TileShape shape, Vector3 gridIndex)
    {
		_Info = new TileInfo((int)gridIndex.x, (int)gridIndex.y, (int)gridIndex.z);
		SetShape(shape);
	}


	private Coroutine _coroutine = null;
	public void Click()
	{
		if(_coroutine!=null)
        {
			StopCoroutine(_coroutine);
			_coroutine = null;
		}
		_coroutine = StartCoroutine(Cor_Click());
	}

	private IEnumerator Cor_Click()
	{
		SpriteRenderer sprite = _spriteRenderer;
		if (sprite == null) yield break;

		sprite.color = Color.red;
		yield return new WaitForSeconds(1.0f);
		sprite.color = Color.white;
		yield return null;
	}

	
	[SerializeField] private SpriteRenderer _spriteRenderer = null;
	public Sprite[] _shapeImages = null;

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

	public void Explosion()
    {
		if (_coroutine != null)
		{
			StopCoroutine(_coroutine);
			_coroutine = null;
		}
		_coroutine = StartCoroutine(Cor_Explosion());
	}

	private IEnumerator Cor_Explosion()
	{
		SpriteRenderer sprite = _spriteRenderer;
		if (sprite == null) yield break;

		sprite.color = Color.green;
		yield return new WaitForSeconds(1.0f);
		sprite.color = Color.white;
		yield return null;
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

}

