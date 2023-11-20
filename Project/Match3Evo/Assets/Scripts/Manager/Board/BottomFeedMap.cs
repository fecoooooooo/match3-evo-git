using UnityEngine;

public class BottomFeedMap
{
	int[,] map;
	int currentIndex = -1;

	public BottomFeedMap(int[,] map)
	{
		this.map = map;
	}

	public int[] PopRow()
	{
		currentIndex++;

		int[] row = new int[map.GetLength(0)];

		for (int i = 0; i < map.GetLength(1); ++i)
		{
			row[i] = map[currentIndex, i];
		}

		return row;
	}
}
