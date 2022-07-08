using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiggedMask2D : MonoBehaviour
{
    public GameObject bone;
    public GameObject sprite;

    [SerializeField] private SpriteMask mask;

    private Vector3 offset;
    private Quaternion rotOffset;
    
    
    // Start is called before the first frame update
    void Start()
    {
        var renderer = sprite.GetComponent<SpriteRenderer>();
        offset = gameObject.transform.position - bone.transform.position;
        rotOffset = Quaternion.Inverse(bone.transform.rotation);
        mask.sprite = renderer.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.rotation =  bone.transform.rotation *rotOffset;
        gameObject.transform.position =  bone.transform.rotation *rotOffset*offset + bone.transform.position;
    }

    public void Fit()
    {
        var renderer = sprite.GetComponent<SpriteRenderer>();
        gameObject.transform.position = renderer.bounds.center;
        if (mask == null) mask = gameObject.AddComponent<SpriteMask>();
        mask.sprite = renderer.sprite;
    }
}
