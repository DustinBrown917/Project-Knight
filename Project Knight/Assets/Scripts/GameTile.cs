using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class GameTile : MonoBehaviour {

    protected SpriteRenderer spriteRenderer;
    [SerializeField]
    public GridAddress Address { get; protected set; }
    public int Row { get { return Address.Row; } }
    public int Column { get { return Address.Column; } }

    public Vector3 WorldPos { get{ return transform.position; } }
    public int OccupierCount { get { return occupiers.Count; } }
    public bool IsOccupied { get { return (occupiers.Count > 0) ? true : false; } }

    [SerializeField]
    protected List<GamePiece> occupiers = new List<GamePiece>();
    public ReadOnlyCollection<GamePiece> Occupiers { get { return occupiers.AsReadOnly(); } }

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /************************************************ Unity Behaviours **************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    #region Unity Behaviours

    /// <summary>
    /// The Awake method.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// The Start method.
    /// </summary>
    private void Start()
    {

    }

    #endregion

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /*************************************************** Behaviours *****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/



    /// <summary>
    /// Adds an occupier to the GameTile.
    /// </summary>
    /// <param name="occupier">The GamePiece to add as an occupier.</param>
    public void AddOccupier(GamePiece occupier)
    {
        occupiers.Add(occupier);
    }

    /// <summary>
    /// Removes an occupier from the GameTile.
    /// </summary>
    /// <param name="occupier">The GamePiece to remove.</param>
    /// <returns>True if removed successfully, false if the GamePiece could not be removed or could not be found.</returns>
    public bool RemoveOccupier(GamePiece occupier)
    {
        return occupiers.Remove(occupier);
    }

    /// <summary>
    /// Gets the occupier at a given index.
    /// </summary>
    /// <param name="index">The index to get the GamePiece from.</param>
    /// <returns>The GamePiece at the specified index.</returns>
    public GamePiece GetOccupierAt(int index)
    {
        if(index < 0 || index > occupiers.Count)
        {
            throw new InvalidOccupierIndexException("Invalid occupier index: " + index.ToString());
        }

        return occupiers[index];
    }

    /// <summary>
    /// Gets the occupiers of this tile as an array.
    /// </summary>
    /// <returns>An array of the occupiers in this tile.</returns>
    public GamePiece[] GetOccupiers()
    {
        return occupiers.ToArray();
    }

    /// <summary>
    /// Removes an occupier at a specific index.
    /// </summary>
    /// <param name="index">The specific index to remove an occupier at.</param>
    public void RemoveOccupierAt(int index)
    {
        if (index < 0 || index > occupiers.Count)
        {
            throw new InvalidOccupierIndexException("Invalid occupier index: " + index.ToString());
        }

        occupiers.RemoveAt(index);
    }

    /// <summary>
    /// Returns whether or not any of the GamePieces that occupy this tile are Obstacles.
    /// </summary>
    /// <returns>True if the tile is obstructed, false if it is not.</returns>
    public bool IsObstructed()
    {
        for(int i = 0; i < occupiers.Count; i++)
        {
            if (occupiers[i].IsObstacle)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Place the GameTile at a location.
    /// </summary>
    /// <param name="location">The location to place the GameTile.</param>
    public void Place(Vector2 location, int row, int column)
    {
        transform.position = location;
        Address = new GridAddress(row, column);
    }

    /// <summary>
    /// Sets the tint of the gameTile spriteRenderer.
    /// </summary>
    /// <param name="color">The color to apply to the tint.</param>
    public void SetColour(Color color)
    {
        spriteRenderer.color = color;
    }






    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /**************************************************** SUBCLASSES ****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/



    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /**************************************************** EXCEPTIONS ****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// Thrown when an there is an attempt to retrieve an occupier at an index that doesn't exist.
    /// </summary>
    public class InvalidOccupierIndexException : Exception
    {
        public InvalidOccupierIndexException()
        {

        }

        public InvalidOccupierIndexException(string message) : base(message)
        {

        }

        public InvalidOccupierIndexException(string message, Exception inner) : base(message, inner)
        {

        }
    }

}
