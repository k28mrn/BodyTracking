using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasView : MonoBehaviour
{   
    RawImage irImage, rgbImage;
    Texture2D irTexture, rgbTexture;

    bool isSetup = false;
    // Start is called before the first frame update
    void Start()
    {
        irImage = transform.Find("IrImage").GetComponent<RawImage>();
        rgbImage = transform.Find("RgbImage").GetComponent<RawImage>();
        isSetup = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateImage(BackgroundData trackerFrameData)
    {
        //初期設定
        if (!isSetup) {
            isSetup = true;
            irTexture = new Texture2D(
                trackerFrameData.DepthImageWidth,
                trackerFrameData.DepthImageHeight,
                TextureFormat.RGBA32,
                false
            );
            irImage.texture = irTexture;

            rgbTexture = new Texture2D(
                trackerFrameData.ColorImageWidth,
                trackerFrameData.ColorImageHeight,
                TextureFormat.RGBA32,
                false
            );
            rgbImage.texture = rgbTexture;

            
        }

        irTexture.SetPixels32(trackerFrameData._DepthImage);
        irTexture.Apply();

        int width = trackerFrameData.ColorImageWidth;
        int height = trackerFrameData.ColorImageHeight;
        Color32[] pixels = trackerFrameData.ColorImage;
        
        //RGBの色入れ替える
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                byte r = pixels[i].b;
                byte b = pixels[i].r;
                pixels[i].r = r;
                pixels[i].b = b;
            }
        }

        //点をつける
        foreach (var body in trackerFrameData.Bodies)
        {
            foreach (var pos2D in body.JointPositions2DColor)
            {
                Utils.DrawRect(ref pixels, width, height, (int)pos2D.X, (int)pos2D.Y, Color.red, 10);
            }
        }



        rgbTexture.SetPixels32(pixels);
        rgbTexture.Apply();
        
    }
}
