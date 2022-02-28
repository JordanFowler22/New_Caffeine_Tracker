using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;
using ZXing;

public class BarcodeManager : MonoBehaviour
{
    private WebCam _webCam; //Is this how i make a webcam? Look into that
    

    public void ScanBarcodeButton()
    {
        StartCoroutine(GetBarCode());
    }

    IEnumerator GetBarCode()
    {
        IBarcodeReader barCodeReader = new BarcodeReader();

        WebCamDevice[] devices = WebCamTexture.devices;
        
        
        
        
        
        yield return null;
    }
}