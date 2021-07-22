using System;
using UnityEngine;
using VRChatUtilityKit.Ui;

// Adapted from VRChatUtilityKit by loukylor

namespace ScreenshotManager.UI
{
    public class HalfButton : SingleButton
    {
        public HalfButton(string parent, Vector3 position, Vector2 pivot, string text, Action onClick, string tooltip, string buttonName, bool resize = false, Color? textColor = null) : this(GameObject.Find(parent), position, pivot, text, onClick, tooltip, buttonName, resize, textColor) { }
        public HalfButton(GameObject parent, Vector3 position, Vector2 pivot, string text, Action onClick, string tooltip, string buttonName, bool resize = false, Color? textColor = null) : base(parent, position, text, onClick, tooltip, buttonName, resize, textColor)
        {
            Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 210);
            Rect.ForceUpdateRectTransforms();
            Rect.pivot = pivot;

            RectTransform child = gameObject.transform.GetChild(0).GetComponent<RectTransform>();
            child.anchoredPosition = new Vector2(child.anchoredPosition.x, child.anchoredPosition.y / 2);
            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 210);
            child.ForceUpdateRectTransforms();
        }
    }
}
