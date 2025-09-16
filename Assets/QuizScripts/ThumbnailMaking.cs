using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class ThumbnailMaking : MonoBehaviour
{
    public Sprite outputSprite;
    public PublishPreview publishPreview;
    public WallManager wallManager;
    public SlideManager firstSlideManager; //first wall’s manager
    public int firstManagerPrevSlide;
    public int prevManager;
    public RectTransform slide;
    public float height;
    public float width;

    public IEnumerator RecordFrame()
    {
        //getting to first wall first slide for screenshot
        wallManager = FindObjectOfType<WallManager>();
        prevManager = wallManager.current;
        wallManager.WallChange(0);
        firstManagerPrevSlide = firstSlideManager.currentSlide;
        firstSlideManager.currentSlide = 0;
        firstSlideManager.ReloadSlide();


        yield return new WaitForEndOfFrame();
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        Vector3[] corners = new Vector3[4];
        slide.GetWorldCorners(corners); 
        int width = ((int)corners[3].x - (int)corners[0].x);
        int height = (int)corners[1].y - (int)corners[0].y;
        var startX = corners[0].x;
        var startY = corners[0].y;



        Texture2D newscreenshot = new Texture2D(screenshot.width, screenshot.height, TextureFormat.RGB24, false);

        newscreenshot.ReadPixels(new Rect(startX, startY, width, height),0,0);
        newscreenshot.Apply();

        Destroy(screenshot);
        Texture2D ss = TextureOps.Crop(newscreenshot, 0, 800, width, height, TextureFormat.RGBA32, default); 

        Sprite screenshotSprite = Sprite.Create(ss, new Rect(0, 0, ss.width, ss.height),Vector2.zero);

        outputSprite = screenshotSprite;

        Debug.Log("Thumbnail done");

        //going back to last screen
        firstSlideManager.currentSlide = firstManagerPrevSlide;
        firstSlideManager.ReloadSlide();
        wallManager.WallChange(prevManager);
    }

}
