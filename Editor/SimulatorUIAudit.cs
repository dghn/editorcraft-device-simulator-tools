using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DeviceSimulatorTools.Editor
{
    [Serializable]
    internal sealed class SimulatorAuditItem
    {
        public string path, objectId, message;
        public Rect bounds;
        // Left, top, right, bottom distances in simulated screen pixels.
        public Vector4 safeOverflow, screenOverflow;
        public bool skipped;
        [NonSerialized] internal RectTransform target;
    }

    [Serializable]
    internal sealed class SimulatorReviewFrame
    {
        public string device, deviceGuid, image, thumbnail;
        public ScreenOrientation orientation;
        public int width, height, checkedCount, skippedCount;
        public Rect safeArea;
        public List<SimulatorAuditItem> issues = new List<SimulatorAuditItem>();
    }

    [Serializable]
    internal sealed class SimulatorReviewReport
    {
        public string createdUtc, status, root, scope;
        public bool audit;
        public float tolerance;
        public string limitations = "Sequential snapshots of a running application. Geometric bounds only; masks, occlusion, text overflow and physical hardware are not validated. Safe areas depend on the device profile. Skipped items are not passes.";
        public List<string> exclusions = new List<string>();
        public List<SimulatorReviewFrame> frames = new List<SimulatorReviewFrame>();
    }

    internal static class SimulatorUIAudit
    {
        internal static string HierarchyPath(Transform target)
        {
            var names = new List<string>();
            for (var t = target; t != null; t = t.parent) names.Add(t.name);
            names.Reverse();
            return string.Join("/", names);
        }
        internal static Vector4 Overflow(Rect bounds, Rect boundary, float tolerance)
        {
            float Amount(float n) => n > tolerance ? n : 0;
            return new Vector4(Amount(boundary.xMin - bounds.xMin), Amount(bounds.yMax - boundary.yMax),
                Amount(bounds.xMax - boundary.xMax), Amount(boundary.yMin - bounds.yMin));
        }
        internal static bool TryBounds(RectTransform target, out Rect bounds, out string reason)
        {
            bounds = default;
            var canvas = target.GetComponentInParent<Canvas>();
            canvas = canvas != null ? canvas.rootCanvas : null;
            if (canvas == null || !canvas.isActiveAndEnabled) { reason = "No active Canvas."; return false; }
            if (canvas.renderMode == RenderMode.WorldSpace) { reason = "World-space Canvas is not supported."; return false; }
            if (canvas.targetDisplay != 0) { reason = "Only the primary display is supported."; return false; }
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && (camera == null || !camera.isActiveAndEnabled || camera.targetTexture != null || camera.targetDisplay != 0))
            { reason = "Screen-space Canvas needs an active primary-display camera without a target texture."; return false; }
            var corners = new Vector3[4]; target.GetWorldCorners(corners);
            if (camera != null && corners.Any(p => camera.WorldToScreenPoint(p).z <= 0))
            { reason = "UI is behind the Canvas camera."; return false; }
            var points = corners.Select(p => RectTransformUtility.WorldToScreenPoint(camera, p)).ToArray();
            if (points.Any(p => float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsInfinity(p.x) || float.IsInfinity(p.y)))
            { reason = "UI has non-finite screen coordinates."; return false; }
            bounds = Rect.MinMaxRect(points.Min(p => p.x), points.Min(p => p.y), points.Max(p => p.x), points.Max(p => p.y));
            reason = null; return true;
        }
        internal static void Inspect(RectTransform root, RectTransform[] exclusions, bool includeContainers,
            float tolerance, SimulatorReviewFrame frame)
        {
            if (root == null || !root.gameObject.scene.IsValid() || !root.gameObject.activeInHierarchy)
                throw new InvalidOperationException("The audit root is no longer an active scene object.");
            Canvas.ForceUpdateCanvases();
            foreach (var target in root.GetComponentsInChildren<RectTransform>(false))
            {
                if (exclusions.Any(e => e != null && (target == e || target.IsChildOf(e)))) continue;
                var graphics = target.GetComponents<Graphic>();
                var selectable = target.GetComponent<Selectable>();
                if (!includeContainers && !graphics.Any(g => g.isActiveAndEnabled) && (selectable == null || !selectable.isActiveAndEnabled)) continue;
                // CanvasRenderer accounts for inherited CanvasGroup alpha, without treating
                // clipping/culling as a pass (off-screen graphics can be culled).
                var renderer = target.GetComponent<CanvasRenderer>();
                if (renderer != null && renderer.GetInheritedAlpha() <= .001f) continue;
                var item = new SimulatorAuditItem { path = HierarchyPath(target), target = target,
                    objectId = GlobalObjectId.GetGlobalObjectIdSlow(target).ToString() };
                if (!TryBounds(target, out item.bounds, out item.message))
                {
                    item.skipped = true; frame.skippedCount++; frame.issues.Add(item); continue;
                }
                // A stale layout must not be reported as fitting this device.
                var canvas = target.GetComponentInParent<Canvas>().rootCanvas;
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay &&
                    (Mathf.Abs(canvas.pixelRect.width - frame.width) > 1 || Mathf.Abs(canvas.pixelRect.height - frame.height) > 1))
                {
                    item.skipped = true; item.message = "Canvas dimensions have not settled to the Simulator screen.";
                    frame.skippedCount++; frame.issues.Add(item); continue;
                }
                frame.checkedCount++;
                item.safeOverflow = Overflow(item.bounds, frame.safeArea, tolerance);
                item.screenOverflow = Overflow(item.bounds, new Rect(0, 0, frame.width, frame.height), tolerance);
                if (item.safeOverflow != Vector4.zero || item.screenOverflow != Vector4.zero) frame.issues.Add(item);
            }
        }
    }
}
