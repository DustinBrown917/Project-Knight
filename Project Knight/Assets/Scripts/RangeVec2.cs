using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RangeVec2 {


    public Vector2 V1;
    public Vector2 V2;

    public float Height { get { return Mathf.Abs(V1.y - V2.y); } }
    public float Width { get { return Mathf.Abs(V1.x - V2.x); } }
    public float Area { get { return Height * Width; } }

    public RangeVec2(Vector2 v1, Vector2 v2)
    {
        V1 = v1;
        V2 = v2;
    }
    
}
