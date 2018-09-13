using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GridAddress : IEquatable<GridAddress> {
    [SerializeField]
    private int _row;
    public int Row { get { return _row; } }

    [SerializeField]
    private int _column;
    public int Column { get { return _column; } }

    public static int Area(GridAddress a1, GridAddress a2)
    {
        int height = Mathf.Abs(a1.Row - a2.Row) + 1;
        int width = Mathf.Abs(a1.Column - a2.Column) + 1;

        return height * width;
    }

    public GridAddress( int row, int column)
    {
        _row = row;
        _column = column;
    }

    public override string ToString()
    {
        return "Grid Address (" + _row + "," + _column + ")";
    }

    public bool Equals(GridAddress other)
    {
        return (other.Row == Row && other.Column == Column);
    }
}
