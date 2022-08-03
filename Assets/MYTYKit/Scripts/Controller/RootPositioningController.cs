using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootPositioningController : MYTYController, IVec3Input
{
    public GameObject targetObject;
    public Vector3 displacement;

    private Vector3 m_initPos;

    public override void PostprocessAfterLoad(Dictionary<GameObject, GameObject> objMap)
    {
        throw new System.NotImplementedException();
    }

    public override void PrepareToSave()
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (targetObject == null) return;
        m_initPos = targetObject.transform.position;

    }

    private void LateUpdate()
    {
        if (targetObject == null) return;
       
        targetObject.transform.position = m_initPos + displacement;
    }

    public void SetInput(Vector3 val)
    {
        displacement = val;
    }
}
