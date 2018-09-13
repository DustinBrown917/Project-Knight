using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionTile : MonoBehaviour {

    public SelectionMask Mask { get; set; }
	public GridAddress Address { get; set; }

    public void OnMouseDown()
    {
        Mask.SelectTile(this);
    }
}
