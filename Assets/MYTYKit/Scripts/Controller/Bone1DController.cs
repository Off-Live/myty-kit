using System.Collections.Generic;
using UnityEngine;


public class Bone1DController : BoneController, IFloatInput
{
    public float controlValue;
    public List<RiggingEntity> xminRig;
    public List<RiggingEntity> xmaxRig;

    public List<RiggingEntity> diffBuffer;

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
        if (xminRig == null || xminRig.Count == 0) return null;
        if (xmaxRig == null || xmaxRig.Count == 0) return null;

        List<RiggingEntity> interpList = new();

        var u = Mathf.Clamp01(controlValue);

        for (int i = 0; i < rigTarget.Count; i++)
        {
            RiggingEntity interp = new RiggingEntity();

            interp.position = (1 - u) * xminRig[i].position + u * xmaxRig[i].position;
            interp.scale = (1 - u) * xminRig[i].scale + u * xmaxRig[i].scale;
            interp.rotation = Quaternion.Slerp(xminRig[i].rotation, xmaxRig[i].rotation, u);
            interpList.Add(interp);
        }

        return interpList;
    }

    public void SetInput(float val)
    {
        controlValue = val;
    }
}
