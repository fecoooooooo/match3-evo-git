using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RepositionPivotUtil {
    public static void RepositionPivot(RectTransform rect, Vector2 newPivot) {
        Vector2 pivotDiff = rect.pivot - newPivot;
        pivotDiff.Scale(rect.rect.size);
        pivotDiff.Scale(rect.localScale);
        rect.pivot = newPivot;
        rect.localPosition -= new Vector3(pivotDiff.x, pivotDiff.y);
    }
}
