using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSource : MonoBehaviour
{
    public WebCamTexture camTexture;
    public Renderer renderer;

    public string camDeviceName = ""; 
    // Start is called before the first frame update
    void Start()
    {
        string devName = "";
        Debug.Log("Webcam Device : " + camDeviceName);
        if (camDeviceName.Length == 0)
        {
            foreach (var dev in WebCamTexture.devices)
            {
                if (dev.name.StartsWith("MYTY") || dev.name.StartsWith("Off"))
                {
                    continue;
                }
                devName = dev.name;
                break;

            }
        }
        else
        {
            devName = camDeviceName;
        }
        
        SelectDevice(devName);
    }
    public void SelectDevice(string name)
    {
        if(camTexture!=null) camTexture.Stop();
        camTexture = new WebCamTexture(name);
        camTexture.Play();
        renderer.material.mainTexture = camTexture;
    }

}
