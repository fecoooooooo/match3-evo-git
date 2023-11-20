using Match3_Evo;
using System;

public class MapPregenerator
{
	const int PREGENERATED_ROW_COUNT_TOP_PART = 1000;
	const int PREGENERATED_ROW_COUNT_BOTTOM_PART = 100;
    const int HIGHER_EVO_FILL_STEP = 3;
    Random seed;

    public void PregenerateToColumns(ColumnFeed[] columnFeeds)
	{
        int[,] pregeneratedMap = PreGenerateMap(PREGENERATED_ROW_COUNT_TOP_PART);

        for (int r = 0; r < PREGENERATED_ROW_COUNT_TOP_PART; ++r)
        {
            for (int c = 0; c < GM.boardMng.columns; ++c)
                columnFeeds[c].AddField(pregeneratedMap[r,c]);
        }
    }

    public void PregenerateBottomFeedMap(out BottomFeedMap bottomFeedMap)
	{
        int[,] pregeneratedMap = PreGenerateMap(PREGENERATED_ROW_COUNT_BOTTOM_PART);

        AddTreasureAndDns(pregeneratedMap);
        AddHigherEvoLvlTiles(pregeneratedMap);

        string s = "";
        for(int i = 0; i < pregeneratedMap.GetLength(0); ++i)
		{
            for(int j = 0; j < pregeneratedMap.GetLength(1); ++j)
			{
                s += pregeneratedMap[i, j].ToString();
			}
            s += "\n";
		}

        bottomFeedMap = new BottomFeedMap(pregeneratedMap);
    }

	private int[,] PreGenerateMap(int rowCount)
    {
        if (seed == null)
            seed = new Random(GM.GetRandom(0, int.MaxValue));

        int[,] pregeneratedMap = new int[rowCount, GM.boardMng.columns];

        for (int r = 0; r < rowCount; ++r)
        {
            for (int c = 0; c < GM.boardMng.columns; ++c)
            {
                bool matchWithThisField;
                do
                {
                    pregeneratedMap[r, c] = seed.Next() % GM.boardMng.gameParameters.TileVariantMax();

                    bool yPrevFieldMatch = 0 <= r - 1 && pregeneratedMap[r - 1,c]  == pregeneratedMap[r,c];
                    bool yPrevPrevFieldMatch = 0 <= r - 2 && pregeneratedMap[r - 1,c]  == pregeneratedMap[r,c];

                    bool xPrevFieldMatch = 0 <= c - 1 && pregeneratedMap[r,c - 1] == pregeneratedMap[r,c];
                    bool xPrevPrevFieldMatch = 0 <= c - 2 && pregeneratedMap[r,c - 2] == pregeneratedMap[r,c];

                    matchWithThisField = (yPrevFieldMatch && yPrevPrevFieldMatch) || (xPrevFieldMatch && xPrevPrevFieldMatch);
                }
                while (matchWithThisField);
            }
        }

        return pregeneratedMap;
    }

	private void AddTreasureAndDns(int[,] pregeneratedMap)
	{
        for (int i = 0; i < GM.boardMng.dnsCount; ++i)
        {
            int r;
            int c;
            do
            {
                r = seed.Next() % pregeneratedMap.GetLength(0);
                c = seed.Next() % GM.boardMng.columns;
            }
            while ((int)FieldType.STARTER_TYPE <= pregeneratedMap[r, c]);

            pregeneratedMap[r, c] = (int)FieldType.DNS;
        }

        for (int i = 0; i < GM.boardMng.treasureCount; ++i)
        {
            int r;
            int c;
            do
            {
                r = seed.Next() % pregeneratedMap.GetLength(0);
                c = seed.Next() % GM.boardMng.columns;
            }
            while ((int)FieldType.STARTER_TYPE <= pregeneratedMap[r, c]);

            pregeneratedMap[r, c] = (int)FieldType.TREASURE;
        }
    }

    private void AddHigherEvoLvlTiles(int[,] pregeneratedMap)
    {
        FieldType[] fieldVariants = new FieldType[] { FieldType.V1_E0, FieldType.V2_E0, FieldType.V3_E0, FieldType.V4_E0 };
        int fieldVariantIndex = -1;
        int amountToAdd = fieldVariants.Length;

        for(int i = 0; i < pregeneratedMap.GetLength(0); i += HIGHER_EVO_FILL_STEP)
		{
            fieldVariantIndex++;
            if (fieldVariantIndex == fieldVariants.Length)
            {
                fieldVariantIndex = 0;
                amountToAdd += fieldVariants.Length;
            }

            bool foundAndReplaced = false;
            int rowIndex = i;

            do
            {
                for (int j = 0; j < GM.boardMng.columns; ++j)
                {
                    if ((FieldType)pregeneratedMap[rowIndex, j] == fieldVariants[fieldVariantIndex])
                    {
                        int evolvedVariant = pregeneratedMap[rowIndex, j] + amountToAdd;
                        pregeneratedMap[rowIndex, j] = Math.Min(evolvedVariant, (int)FieldType.LAST_NORMAL);
                        foundAndReplaced = true;
                    }
                }

                rowIndex++;

            } while (foundAndReplaced == false);
		}
    }


}
