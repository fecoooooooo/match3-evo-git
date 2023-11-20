using Match3_Evo;
using System;
using System.Collections.Generic;

[System.Serializable]
public class ColumnFeed
{
    public int column;
    public List<int> fieldTypes;

    Random seed = new System.Random(GM.GetRandom(0, int.MaxValue));

    public ColumnFeed(int _column, List<int> _colors = null)
    {
        column = _column;
        fieldTypes = new List<int>();

        if (_colors != null)
            fieldTypes.AddRange(_colors);
    }

    public int GetFieldType(FieldUI _fieldUI)
    {
        if (fieldTypes.Count > 0)
        {
            int lvResult = fieldTypes[0];

            fieldTypes.RemoveAt(0);

            return lvResult;
        }
        else
            return seed.Next() % GM.boardMng.gameParameters.TileVariantMax();
    }

    public void AddField(int _fieldType)
    {
        fieldTypes.Add(_fieldType);
    }

    internal void Evolve(int evolvingVariant, int newEvoLvl)
    {
        for (int i = 0; i < fieldTypes.Count; ++i)
        {
            int variant = Field.TypeToVariant((FieldType)fieldTypes[i]);
            int evoLvl = Field.TypeToVEvoLvl((FieldType)fieldTypes[i]);

            if (evoLvl < newEvoLvl && evolvingVariant == variant)
                fieldTypes[i] = (int)Field.EvoLvlAndVariantToType(newEvoLvl, evolvingVariant);
        }
    }
}