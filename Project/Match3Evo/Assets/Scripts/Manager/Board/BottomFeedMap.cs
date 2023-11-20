using Match3_Evo;

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

	internal void Evolve(int evolvingVariant, int newEvoLvl)
	{
		for (int r = 0; r < map.GetLength(1); ++r)
		{
			for (int c = 0; c < map.GetLength(1); ++c)
			{
				int variant = Field.TypeToVariant((FieldType)map[r, c]);
				int evoLvl = Field.TypeToVEvoLvl((FieldType)map[r, c]);

				if (evoLvl < newEvoLvl && evolvingVariant == variant)
					map[r, c] = (int)Field.EvoLvlAndVariantToType(newEvoLvl, evolvingVariant);
			}
		}
	}
}
