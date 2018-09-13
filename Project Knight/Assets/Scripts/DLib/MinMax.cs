using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct FloatMinMax
{
    [SerializeField]
    private float _val1;
    public float Val1 { get { return _val1; } set { _val1 = value; } }

    [SerializeField]
    private float _val2;
    public float Val2 { get { return _val2; } set { _val2 = value; } }

    public float Min
    {
        get { return (_val1 > _val2) ? _val2 : _val1; }
        set {
            if (_val1 < _val2) { _val1 = value; }
            else { _val2 = value; }
        }
    }

    public float Max
    {
        get { return (_val1 > _val2) ? _val1 : _val2; }
        set {
            if (_val1 > _val2) { _val1 = value; }
            else { _val2 = value; }
        }
    }

    public float Difference { get { return (_val1 - _val2 > 0) ? _val1 - _val2 : _val2 - _val1; } }

    public FloatMinMax(float val1, float val2)
    {
        _val1 = val1;
        _val2 = val2;
    }

    /// <summary>
    /// Gets the interpolate product of t between Min and Max
    /// </summary>
    /// <param name="t">The interpolate between Min and Max</param>
    /// <returns>The float at t between Min and Max</returns>
    public float Lerp(float t)
    {
        return Mathf.Lerp(Min, Max, t);
    }

    public static FloatMinMax operator *(FloatMinMax left, FloatMinMax right)
    {
        return new FloatMinMax(left.Min * right.Min, left.Max * right.Max);
    }

    public static FloatMinMax operator *(FloatMinMax left, float right)
    {
        return new FloatMinMax(left.Min * right, left.Max * right);
    }

    public static FloatMinMax operator /(FloatMinMax left, FloatMinMax right)
    {
        return new FloatMinMax(left.Min / right.Min, left.Max / right.Max);
    }

    public static FloatMinMax operator +(FloatMinMax left, FloatMinMax right)
    {
        return new FloatMinMax(left.Min + right.Min, left.Max + right.Max);
    }

    public static FloatMinMax operator -(FloatMinMax left, FloatMinMax right)
    {
        return new FloatMinMax(left.Min - right.Min, left.Max - right.Max);
    }
}

[Serializable]
public struct IntMinMax
{
    [SerializeField]
    private int _val1;
    public int Val1 { get { return _val1; } set { _val1 = value; } }
    [SerializeField]
    private int _val2;
    public int Val2 { get { return _val2; } set { _val2 = value; } }


    public int Min {
        get { return (_val1 > _val2) ? _val2 : _val1; }
        set {
            if (_val1 < _val2) { _val1 = value; }
            else { _val2 = value; }
        }
    }

    public int Max {
        get { return (_val1 > _val2) ? _val1 : _val2; }
        set {
            if(_val1 > _val2) { _val1 = value; }
            else { _val2 = value; }
        }
    }

    public int Difference { get { return (_val1 - _val2 > 0)? _val1 - _val2 : _val2 - _val1; } }


    public IntMinMax(int val1, int val2)
    {
        _val1 = val1;
        _val2 = val2;
    }

    /// <summary>
    /// Gets the interpolate product of t between Min and Max
    /// </summary>
    /// <param name="t">The interpolate between Min and Max</param>
    /// <returns>The (rounded to nearest) int at t between Min and Max</returns>
    public int Lerp(float t)
    {
        return Mathf.RoundToInt(Mathf.Lerp(Min, Max, t));
    }
}
