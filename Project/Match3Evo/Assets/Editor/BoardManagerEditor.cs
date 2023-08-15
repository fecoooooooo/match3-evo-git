using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Match3_Evo
{
    //[CustomEditor(typeof(BoardManager))]
    public class BoardManagerEditor : Editor
    {
        private void OnEnable()
        {
            List<FieldData> data = (target as BoardManager).FieldDatas;
            // data.ForEach(x => { x.bubbleAnimation.Sort((y, z) => y.name.CompareTo(z.name)); x.wobbleAnimation.Sort((y, z) => y.name.CompareTo(z.name)); });
            EditorUtility.SetDirty(target);
        }
    }
}