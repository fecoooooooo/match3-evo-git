using UnityEngine;

namespace Match3_Evo
{
    [CreateAssetMenu(fileName = "BoardOverride", menuName = "Tools/BoardOverride")]
    public class BoardOverride : ScriptableObject
    {
        public Field[] overrides;

        public void Override()
        {
            for (int i = 0; i < overrides.Length; i++)
            {
                GM.boardMng.Fields[overrides[i].rowIndex, overrides[i].columnIndex].fieldVariant = overrides[i].fieldVariant;
                GM.boardMng.Fields[overrides[i].rowIndex, overrides[i].columnIndex].fieldUI.Initialize(GM.boardMng.Fields[overrides[i].rowIndex, overrides[i].columnIndex]);
            }
        }
    }
}