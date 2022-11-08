
// 타일 종류
public enum TileShape
{
	Blue,
	Green,
	Orange,
	Purple,
	Red,
	Yellow,

	ToyTops,

	MAX,
};

// 
public enum TileDirection
{
	LeftTop,
	Top,
	RightTop,
	LeftBottom,
	Bottom,
	RightBottom,
};

public enum GridAxis
{
	X,	// 우상 대각선
	Y,	// 세로
	Z,	// 좌상 대각선
};

public enum TouchMode
{
	None,
	Drag,
};
