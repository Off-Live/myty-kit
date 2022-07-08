using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceModel : RiggingModel
{
    public float leftEye;
    public float rightEye;

    public float leftEyeBrow;
    public float rightEyeBrow;

    public Vector2 leftPupil;
    public Vector2 rightPupil;

    public float mouthX;
    public float mouthY;

    public GameObject annotationPrefab;
    public GameObject markedPrefab;

    private GameObject[] _landmarkVis = new GameObject[478];
    private Vector3 _lmOffset = new Vector3(0, 10, 0);


    private static int[] LEFT_EYE = { 33, 133, 160, 159, 158, 144, 145, 153 };
    private static int[] RIGHT_EYE = { 263, 362, 387, 386, 385, 373, 374, 380 };

    private static int[] LEFT_EYEBROW = { 35, 244, 63, 105, 66, 229, 230, 231 };
    private static int[] RIGHT_EYEBROW = { 265, 464, 293, 334, 296, 449, 450, 451 };

    private static int[] LEFT_PUPIL = { 468, 469, 470, 471, 472 };
    private static int[] RIGHT_PUPIL = { 473, 474, 475, 476, 477 };

    private bool Contains(int[] array, int val)
    {
        for(int i = 0; i < array.Length; i++)
        {
            if (array[i] == val) return true; ;
        }
        return false;
    }

    void Start()
    {
        
    }

    private void Update()
    {
        
    }



    private void LateUpdate()
    {
        var faceLM = rawPoints;
        if (faceLM == null || faceLM.Length != 478) return;

        FaceSolver.GetEyeOpen(out leftEye, out _, true, faceLM);
        FaceSolver.GetEyeOpen(out rightEye, out _, false, faceLM);
        //FaceSolver.StabilizeBlink(out leftEye, out rightEye, leftEye, rightEye, 0.0f, false);
        //if (Mathf.Min(leftEye, rightEye) > 0.5)
        //{
        //    leftEye = rightEye;
        //}
        

        leftPupil = FaceSolver.GetPupilPosition(faceLM, true);
        rightPupil = FaceSolver.GetPupilPosition(faceLM, false);

        leftPupil = new Vector2(Mathf.Clamp(leftPupil.x / 0.3f, -1.0f,1.0f), -Mathf.Clamp(leftPupil.y / 0.3f,-1.0f,1.0f));
        rightPupil = new Vector2(Mathf.Clamp(rightPupil.x / 0.3f,-1.0f,1.0f), -Mathf.Clamp(rightPupil.y / 0.3f,-1.0f,1.0f));

        leftEyeBrow = FaceSolver.GetBrowRaise(faceLM, true);
        rightEyeBrow = FaceSolver.GetBrowRaise(faceLM, false);

        FaceSolver.CalcMouth(faceLM,
            out mouthX,
            out mouthY);
           
    }
}
