
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

// 그리드 방향 정보
public enum GridDirection
{
	LeftTop,
	Top,
	RightTop,
	LeftBottom,
	Bottom,
	RightBottom,

	MAX,
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
