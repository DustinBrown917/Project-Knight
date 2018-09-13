using System;
using System.Collections.Generic;
using UnityEngine;


public class TileHolder : MonoBehaviour {

    public static TileHolder Instance { get; private set; }

    [SerializeField]
    private List<GameObject> tiles;


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
        if (Instance != this) { Instance = this; }
        CheckTiles();
    }

    #endregion

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /*************************************************** Behaviours *****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/





    /// <summary>
    /// Checks to ensure the tiles have GameTiles on them. If an item does not, it is removed.
    /// </summary>
    private void CheckTiles()
    {
        int i = 0;

        while(i < tiles.Count)
        {
            if (!tiles[i].GetComponent<GameTile>())
            {
                tiles.RemoveAt(i);
            } else
            {
                i++;
            }
        }

        if(i == 0)
        {
            throw new NoTilesLoadedException("No valid tiles were found in tile list. Double check that you've added GameObjects with GameTiles in the editor.");
        }
    }



    /// <summary>
    /// Creates an instance of the chosen tile and returns a reference to it.
    /// </summary>
    /// <param name="index">The index of the tile to be instantiated.</param>
    /// <returns>GameObject instance of the chosen tile.</returns>
    public GameObject GetTileInstance(int index)
    {
        if (index < 0)
        {
            throw new InvalidTileIndexException("Tile index cannot be less than 0. index: " + index.ToString());
        }
        else if (index >= tiles.Count)
        {
            throw new InvalidTileIndexException("Tile index cannot be greater than " + tiles.Count.ToString() + ". index: " + index.ToString());
        }

        return Instantiate(tiles[index], gameObject.transform);
    }



    /// <summary>
    /// Get method for the length of the tile list.
    /// </summary>
    /// <returns>The length of the tile list.</returns>
    public int GetListLength()
    {
        return tiles.Count;
    }







    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /**************************************************** EXCEPTIONS ****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// For when an invalid index is used to find a tile.
    /// </summary>
    public class InvalidTileIndexException : Exception
    {
        public InvalidTileIndexException()
        {

        }

        public InvalidTileIndexException(string message) : base(message)
        {

        }

        public InvalidTileIndexException(string message, Exception inner) : base(message, inner)
        {

        }
    }


    /// <summary>
    /// For when no valid tiles were found in the initial tile list.
    /// </summary>
    public class NoTilesLoadedException : Exception
    {
        public NoTilesLoadedException()
        {

        }

        public NoTilesLoadedException(string message) : base(message)
        {

        }

        public NoTilesLoadedException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}









