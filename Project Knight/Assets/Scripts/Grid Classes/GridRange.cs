using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GridRange {

    [SerializeField]
    private GridAddress _topLeft;
    public GridAddress TopLeft { get { return _topLeft; } }

    [SerializeField]
    private GridAddress _topRight;
    public GridAddress TopRight { get { return _topRight; } }

    [SerializeField]
    private GridAddress _bottomLeft;
    public GridAddress BottomLeft { get { return _bottomLeft; } }

    [SerializeField]
    private GridAddress _bottomRight;
    public GridAddress BottomRight { get { return _bottomRight; } }

    public int Area { get { return GridAddress.Area(BottomLeft, TopRight); } }



    /// <summary>
    /// Grid Range constructor
    /// </summary>
    /// <param name="address1">The first corner of the range.</param>
    /// <param name="address2">The opposite corner of the range.</param>
    public GridRange(GridAddress address1, GridAddress address2)
    {
        int lowRow;
        int lowCol;
        int highRow;
        int highCol;

        if(address1.Row < address2.Row) {
            lowRow = address1.Row;
            highRow = address2.Row;
        } else {
            lowRow = address2.Row;
            highRow = address1.Row;
        }

        if (address1.Column < address2.Column) {
            lowCol = address1.Column;
            highCol = address2.Column;
        } else {
            lowCol = address2.Column;
            highCol = address1.Column;
        }

        _bottomLeft = new GridAddress(lowRow, lowCol);
        _topRight = new GridAddress(highRow, highCol);
        _topLeft = new GridAddress(highRow, lowCol);
        _bottomRight = new GridAddress(lowRow, highCol);
    }

    /// <summary>
    /// Checks if a given address is within this GridRange.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>True if the address is within the GridRange. False if not.</returns>
    public bool IsWithin(GridAddress address)
    {
        if(address.Row <= TopLeft.Row && address.Row >= BottomRight.Row && address.Column >= TopLeft.Column && address.Column <= BottomRight.Column) {
            return true;
        } else { return false; }
    }

    /// <summary>
    /// Checks if a given coordinate is within this GridRange.
    /// </summary>
    /// <param name="row">The row of the coordinate.</param>
    /// <param name="col">The column of the coordinate.</param>
    /// <returns>True if the coordinate is within the GridRange. False if not.</returns>
    public bool IsWithin(int row, int col)
    {
        if (row <= TopLeft.Row && row >= BottomRight.Row && col >= TopLeft.Column && col <= BottomRight.Column) {
            return true;
        }
        else { return false; }
    }
}
