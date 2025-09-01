using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    public bool take;
    public Camera camerasnap;
    private void Start()
    {
        string filename = string.Format("Assets/Screenshots/capture_{0}.png", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));
        if (!Directory.Exists("Assets/Screenshots"))
        {
            Directory.CreateDirectory("Assets/Screenshots");
        }
       
    }
    private void Update()
    {
        if (take) {
            take = false;

            string filename = string.Format("Assets/Screenshots/capture_{0}.png", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));
            if (!Directory.Exists("Assets/Screenshots"))
            {
                Directory.CreateDirectory("Assets/Screenshots");
            }
            tSaveScreenshotToFile(filename);

        }
    }
    public Texture2D TakeScreenShot()
    {
        return tScreenshot();
    }

    Texture2D tScreenshot()
    {

        int resWidth = camerasnap.pixelWidth;
        int resHeight = camerasnap.pixelHeight;
        Camera camera = camerasnap;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 32);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        return screenShot;
    }

    public Texture2D tSaveScreenshotToFile(string fileName)
    {
        Texture2D screenShot = tScreenshot();
        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(fileName, bytes);
        return screenShot;
    }

}