using System;
using System.Collections.Generic;
using MYTYKit.Components;
using UnityEditor;
using MYTYKit.Controllers;
using MYTYKit.MotionAdapters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MYTYKit
{
    public static class SpriteControllerFixProcessor
    {
        public static void ConvertSprite1DController(this Migration migration)
        {
            var sprite1dControllers = Object.FindObjectsOfType<Sprite1DRangeController>();
            
            foreach (var controller in sprite1dControllers)
            {
                var go = controller.gameObject;
                var msrController = go.GetComponent<Sprite1DRangeControllerMSR>();
                if (msrController != null) continue;
                msrController = go.AddComponent<Sprite1DRangeControllerMSR>();
                msrController.min = controller.min;
                msrController.max = controller.max;
                msrController.value = controller.value;
                msrController.intervals = new List<Sprite1DRangeControllerMSR.Interval>();
                msrController.spriteObjects = new List<MYTYSpriteResolver>();

                foreach (var interval in controller.intervals)
                {
                    msrController.intervals.Add(new Sprite1DRangeControllerMSR.Interval()
                    {
                        label = interval.label,
                        max = interval.max,
                        min = interval.min,
                    });
                }

                foreach (var spriteResolver in controller.spriteObjects)
                {
                    var spriteGo = spriteResolver.gameObject;
                    var mytySR = spriteGo.GetComponent<MYTYSpriteResolver>();
                    if (mytySR != null)
                    {
                        msrController.spriteObjects.Add(mytySR);
                    }
                }
                
                FixAdapterLinkedController(controller, msrController);
                
                EditorUtility.SetDirty(msrController);
                Object.DestroyImmediate(controller);
            }
        }
        public static void ConvertSprite2DController(this Migration migration)
        {
            var sprite2dControllers = Object.FindObjectsOfType<Sprite2DNearstController>();
            
            foreach (var controller in sprite2dControllers)
            {
                var go = controller.gameObject;
                var msrController = go.GetComponent<Sprite2DNearestControllerMSR>();
                if (msrController != null) continue;
                msrController = go.AddComponent<Sprite2DNearestControllerMSR>();
                msrController.topRight = controller.topRight;
                msrController.bottomLeft = controller.bottomLeft;
                msrController.value = controller.value;
                msrController.labels = new List<Sprite2DNearestControllerMSR.Label2D>();
                msrController.spriteObjects = new List<MYTYSpriteResolver>();

                foreach (var label2D in controller.labels)
                {
                    msrController.labels.Add(new Sprite2DNearestControllerMSR.Label2D()
                    {
                        label = label2D.label,
                        point = label2D.point
                    });
                }
                foreach (var spriteResolver in controller.spriteObjects)
                {
                    var spriteGo = spriteResolver.gameObject;
                    var mytySR = spriteGo.GetComponent<MYTYSpriteResolver>();
                    if (mytySR != null)
                    {
                        msrController.spriteObjects.Add(mytySR);
                    }
                }
                FixAdapterLinkedController(controller, msrController);
                EditorUtility.SetDirty(msrController);
                Object.DestroyImmediate(controller);
            }

            var sprite2dControllerMSRs = Object.FindObjectsOfType<Sprite2DNearstControllerMSR>();
            
            foreach (var controller in sprite2dControllerMSRs)
            {
                var go = controller.gameObject;
                var msrController = go.GetComponent<Sprite2DNearestControllerMSR>();
                if (msrController != null) continue;
                msrController = go.AddComponent<Sprite2DNearestControllerMSR>();
                msrController.topRight = controller.topRight;
                msrController.bottomLeft = controller.bottomLeft;
                msrController.value = controller.value;
                msrController.labels = new List<Sprite2DNearestControllerMSR.Label2D>();
                msrController.spriteObjects = new List<MYTYSpriteResolver>();

                foreach (var label2D in controller.labels)
                {
                    msrController.labels.Add(new Sprite2DNearestControllerMSR.Label2D()
                    {
                        label = label2D.label,
                        point = label2D.point
                    });
                }
                foreach (var spriteResolver in controller.spriteObjects)
                {
                    var spriteGo = spriteResolver.gameObject;
                    var mytySR = spriteGo.GetComponent<MYTYSpriteResolver>();
                    if (mytySR != null)
                    {
                        msrController.spriteObjects.Add(mytySR);
                    }
                }
                
                
                FixAdapterLinkedController(controller, msrController);
                EditorUtility.SetDirty(msrController);
                Object.DestroyImmediate(controller);
            }
            
        }

        static void FixAdapterLinkedController(MYTYController oldCon, MYTYController newCon)
        {
            var jointMapper = Object.FindObjectsOfType<JointRotationMapper>();
            foreach (var adapter in jointMapper)
            {
                foreach (var item in adapter.configuration )
                {
                    if (item.targetController == null)
                    {
                        Debug.Log("null controller in joint mapper : "+adapter.name);
                        continue;
                    }
                    if (item.targetController == oldCon)
                    {
                        item.targetController = newCon;
                    }
                }
            }
            
            var parametricReducer = Object.FindObjectsOfType<ParametricReducer>();
            foreach (var adapter in parametricReducer)
            {
                foreach (var item in adapter.configuration )
                {
                    if (item.controller == null)
                    {
                        Debug.Log("null controller in parametric reducer: "+adapter.name);
                        continue;
                    }
                    if (item.controller == oldCon)
                    {
                        item.controller = newCon;
                    }
                }
            }
            
            var pointsReducer = Object.FindObjectsOfType<PointsReducer>();
            foreach (var adapter in pointsReducer)
            {
                foreach (var item in adapter.configuration )
                {
                    if (item.targetController == null)
                    {
                        Debug.Log("null controller in points reducer: "+adapter.name);
                        continue;
                    }
                    if (item.targetController == oldCon)
                    {
                        item.targetController = newCon;
                    }
                }
                
            }
        }
    }
}