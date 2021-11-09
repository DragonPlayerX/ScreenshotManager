using UnityEngine;

namespace ScreenshotManager.UI.Elements
{
    public class ElementBase
    {

        public GameObject GameObject { get; private set; }
        public RectTransform RectTransform { get; private set; }

        protected ElementBase(Transform parent, GameObject template, string gameObjectName)
        {
            if (parent != null)
                GameObject = Object.Instantiate(template, parent);
            else
                GameObject = Object.Instantiate(template, UiManager.TempUIParent);
            RectTransform = GameObject.GetComponent<RectTransform>();
            GameObject.name = gameObjectName;
        }
    }
}
