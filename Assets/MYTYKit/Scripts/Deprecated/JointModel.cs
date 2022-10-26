using UnityEngine;

public class JointModel : RiggingModel
{
    protected Quaternion _rotation;
    protected Vector3 _translation;
    protected Vector3 _up = Vector3.up;
    protected Vector3 _lookAt = Vector3.forward;
    public Quaternion rotation
    {
        get => _rotation;
    }

    public Vector3 translation
    {
        get => _translation;
    }

    public Vector3 upVector
    {
        get => _up;
    }

    public Vector3 lookAt
    {
        get => _lookAt;
    }

}
