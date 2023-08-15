using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class HierarchyTools : MonoBehaviour
{
    [MenuItem("Tools/dAlex/Group %g", false, 1)]
    static void GroupSelections()
    {
        GroupTools.Group();
    }

    [MenuItem("Tools/dAlex/Ungroup #%g", false, 1)]
    static void Ungroup()
    {
        GroupTools.Ungroup();
    }

    [MenuItem("Tools/dAlex/Collider destroyer")]
    public static void DestroyColliders()
    {
        Collider[] childColliders;

        // childColliders = myObject.GetComponentsInChildren<Collider>();
        // foreach (Collider collider in childColliders) {
        //     DestroyImmediate(collider);
        // }

        foreach (Transform t in Selection.transforms)
        {
            childColliders = t.GetComponentsInChildren<Collider>();
            foreach (Collider collider in childColliders)
            {
                DestroyImmediate(collider);
            }
        }
    }

    [MenuItem("Tools/dAlex/HierarchyTools/Sort Children By Name")]
    public static void SortChildrenByName()
    {
        foreach (Transform t in Selection.transforms)
        {
            List<Transform> children = t.Cast<Transform>().ToList();
            children.Sort((Transform t1, Transform t2) =>
            {
                return t1.name.CompareTo(t2.name);
            });

            for (int i = 0; i < children.Count; ++i)
            {
                Undo.SetTransformParent(children[i], children[i].parent, "Sort Children");
                children[i].SetSiblingIndex(i);
            }
        }
    }

    [MenuItem("Tools/dAlex/HierarchyTools/Sort Children Random")]
    public static void SortChildrenRandom()
    {
        foreach (Transform t in Selection.transforms)
        {
            List<Transform> children = t.Cast<Transform>().ToList();
            children.Sort((Transform t1, Transform t2) =>
            {
                return t1.name.CompareTo(t2.name);
            });

            for (int i = 0; i < children.Count; ++i)
            {
                children[i].SetSiblingIndex(Random.Range(0, children.Count));
            }
        }
    }

    [MenuItem("Tools/dAlex/HierarchyTools/Children Count")]
    public static void ChildrenCount()
    {
        int lvCounter = 0;
        foreach (Transform t in Selection.transforms)
        {
            List<Transform> children = t.Cast<Transform>().ToList();
            lvCounter += children.Count;
        }
        Debug.Log(lvCounter);
    }

}