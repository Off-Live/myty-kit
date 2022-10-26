using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MYTYKit
{
    public class CameraSource : MonoBehaviour
    {
        public WebCamTexture camTexture;
        public Renderer previewRenderer;

        public string camDeviceName = "";

        // Start is called before the first frame update
        void Start()
        {
            string devName = "";
            Debug.Log("Webcam Device : " + camDeviceName);
            if (!WebCamTexture.devices.Select(_ => _.name).Contains(camDeviceName)) camDeviceName = "";
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
            if (camTexture != null) camTexture.Stop();
            camTexture = new WebCamTexture(name);
            camTexture.Play();
            previewRenderer.material.mainTexture = camTexture;
        }

    }
}
