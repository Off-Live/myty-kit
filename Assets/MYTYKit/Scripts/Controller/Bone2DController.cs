using System;
using System.Collections.Generic;
using UnityEngine;

public class Bone2DController : BoneController, IVec2Input
{
    public Vector2 controlPosition;
    public float xScale = 1.0f;
    public float yScale = 1.0f;

    public List<RiggingEntity> xminRig;
    public List<RiggingEntity> xmaxRig;
    public List<RiggingEntity> yminRig;
    public List<RiggingEntity> ymaxRig;

    private List<RiggingEntity> diffBuffer;



   
    void Update()
    {
        var interpList = CalcInterpolate();
        if (interpList == null) return;
        diffBuffer = CalcDiff(orgRig, interpList);

    }
   

    public override void ApplyDiff()
    {
        AccumulatePose(diffBuffer);
    }

    
    protected override List<RiggingEntity> CalcInterpolate()
    {

        if (orgRig == null || orgRig.Count == 0) return null;

        List<RiggingEntity> xmin = xminRig;
        List<RiggingEntity> xmax = xmaxRig;
        List<RiggingEntity> ymin = yminRig;
        List<RiggingEntity> ymax = ymaxRig;

        if (xmin == null || xmin.Count==0) xmin = orgRig;
        if (xmax == null || xmax.Count==0) xmax = orgRig;
        if (ymin == null || ymin.Count==0) ymin = orgRig;
        if (ymax == null || ymax.Count==0) ymax = orgRig;
        

        List<RiggingEntity> interpList;
        if (controlPosition.x >=0 && controlPosition.y >= 0)
        {
            interpList = BilinearInterp(orgRig, xmax ,ymax);   
        }else if (controlPosition.x < 0 && controlPosition.y >= 0)
        {
            interpList = BilinearInterp(orgRig, xmin, ymax);
        }else if (controlPosition.x < 0 && controlPosition.y < 0)
        {
            interpList = BilinearInterp(orgRig, xmin, ymin);
        }
        else
        {
            interpList = BilinearInterp(orgRig, xmax, ymin);
        }


        return interpList;

    }


    private List<RiggingEntity> BilinearInterp(List<RiggingEntity> originList, List<RiggingEntity> xMaxList, List<RiggingEntity> yMaxList)
    {
        var interpList = new List<RiggingEntity>();
        for(int i = 0; i < originList.Count; i++)
        {
            var origin = originList[i];
            var xMax = xMaxList[i];
            var yMax = yMaxList[i];
            RiggingEntity xyMax = new RiggingEntity();
            RiggingEntity interp = new RiggingEntity();
            
            xyMax.position = (xMax.position - origin.position) + (yMax.position - origin.position) + origin.position;
            xyMax.rotation = xMax.rotation * Quaternion.Inverse(origin.rotation) * yMax.rotation;
            xyMax.scale = (xMax.scale - origin.scale) + (yMax.scale - origin.scale) + origin.scale;

            if (xScale == 0) xScale = 1.0f;
            if (yScale == 0) yScale = 1.0f;

            var u = Math.Abs(controlPosition.x)/xScale;
            var v = Math.Abs(controlPosition.y)/yScale;

            interp.position = (1 - u) * (1 - v) * origin.position + u * (1 - v) * xMax.position + (1 - u) * v * yMax.position + u * v * xyMax.position;
            interp.scale = (1 - u) * (1 - v) * origin.scale + u * (1 - v) * xMax.scale + (1 - u) * v * yMax.scale + u * v * xyMax.scale;
            interp.rotation = Quaternion.Slerp(Quaternion.Slerp(origin.rotation, xMax.rotation, u), Quaternion.Slerp(yMax.rotation, xyMax.rotation, u), v);
            interpList.Add(interp);
        }
        return interpList;
    }

    public void SetInput(Vector2 val)
    {
        controlPosition = val;
    }
}
