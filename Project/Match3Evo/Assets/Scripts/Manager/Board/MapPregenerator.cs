using Match3_Evo;
using System;

public class MapPregenerator
{
	const int PREGENERATED_ROW_COUNT = 1000;
    System.Random seed;

    public void PregenerateToColumns(ColumnFeed[] columnFeeds)
	{
        int[,] pregeneratedMap = PreGenerateMap();

        for (int r = 0; r < PREGENERATED_ROW_COUNT; ++r)
        {
            for (int c = 0; c < GM.boardMng.columns; ++c)
                columnFeeds[c].AddField(pregeneratedMap[r,c]);
        }
    }

    private int[,] PreGenerateMap()
    {
        if (seed == null)
            seed = new System.Random(GM.GetRandom(0, int.MaxValue));

        int[,] pregeneratedMap = new int[PREGENERATED_ROW_COUNT, GM.boardMng.columns];

        for (int r = 0; r < PREGENERATED_ROW_COUNT; ++r)
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

        AddTreasureAndDns(pregeneratedMap);

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
                r = seed.Next() % PREGENERATED_ROW_COUNT;
                c = seed.Next() % GM.boardMng.columns;
            }
            while ((int)FieldVariant.SPECIAL <= pregeneratedMap[r, c]);

            pregeneratedMap[r, c] = (int)FieldVariant.DNS;
        }

        for (int i = 0; i < GM.boardMng.treasureCount; ++i)
        {
            int r;
            int c;
            do
            {
                r = seed.Next() % GM.boardMng.rows; //TODO REVERT to PREGENERATED_ROW_COUNT
                c = seed.Next() % GM.boardMng.columns;
            }
            while ((int)FieldVariant.SPECIAL <= pregeneratedMap[r, c]);

            pregeneratedMap[r, c] = (int)FieldVariant.TREASURE;
        }
    }
}
