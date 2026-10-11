using UnityEngine;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Controllers;

// keeps the barter grid centred on the window's viewport; the slot can shift after layout settles (Content's width
// follows its children's text), so re-check every frame while the grid is visible
public class BarterGridCentering : MonoBehaviour {
    private void LateUpdate() {
        if (MainViewport == null || BarterTradingTable == null) {
            return;
        }

        var slot = (RectTransform)transform;

        Vector3 viewportCentre = MainViewport.TransformPoint(MainViewport.rect.center);
        float   x              = slot.InverseTransformPoint(viewportCentre).x - slot.rect.center.x;

        var cloneRt = (RectTransform)BarterTradingTable.transform;

        // only write when it actually changed
        if (!Mathf.Approximately(cloneRt.anchoredPosition.x, x)) {
            cloneRt.anchoredPosition = new Vector2(x, 0f);
        }
    }
}