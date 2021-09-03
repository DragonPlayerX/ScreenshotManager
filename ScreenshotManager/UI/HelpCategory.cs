using System;
using UnityEngine;

namespace ScreenshotManager.UI
{
    public class HelpCategory
    {

        public WideButton Button;
        public GameObject Content;

        public HelpCategory(string name, GameObject contentParent, string path, Vector3 position, Vector3 pivot, Action onClick)
        {
            Content = contentParent.transform.Find(name).gameObject;
            Content.SetActive(false);
            Button = new WideButton(path, position, pivot, name, new Action(() =>
            {
                Content.SetActive(true);
                onClick.Invoke();
            }), name + " Help", name + "Button", true);
        }
    }
}
