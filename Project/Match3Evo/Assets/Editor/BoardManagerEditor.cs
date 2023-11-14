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
            List<FieldDataEvo> data = (target as BoardManager).FieldData;
            // data.ForEach(x => { x.bubbleAnimation.Sort((y, z) => y.name.CompareTo(z.name)); x.wobbleAnimation.Sort((y, z) => y.name.CompareTo(z.name)); });
            EditorUtility.SetDirty(target);
        }
    }
}