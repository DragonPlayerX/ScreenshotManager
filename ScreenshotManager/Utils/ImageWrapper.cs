using UnityEngine;
using UnityEngine.UI;

namespace ScreenshotManager.Utils
{
    public class ImageWrapper
    {

        public RawImage Image;
        public Image Mask;

        public ImageWrapper(GameObject imageObject)
        {
            Image = imageObject.GetComponent<RawImage>();
            Mask = imageObject.transform.parent.GetComponent<Image>();
        }
    }
}
