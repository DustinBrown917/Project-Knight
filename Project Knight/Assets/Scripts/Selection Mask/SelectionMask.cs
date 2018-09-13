using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionMask : MonoBehaviour {

    private SpriteRenderer maskBarrier;
    [SerializeField]
    private GameObject maskTile;

    private SelectionTile[,] selectionTiles;

    public int Rows { get; private set; }
    public int Columns { get; private set; }


    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /************************************************ Unity Behaviours **************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// Unity's Awake() method.
    /// </summary>
    private void Awake()
    {
        maskBarrier = transform.Find("MaskBarrier").GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Unity's OnDisable() function.
    /// </summary>
    private void OnDisable()
    {
        for(int row = 0; row < Rows; row++)
        {
            for(int col = 0; col < Columns; col++)
            {
                selectionTiles[row, col].gameObject.SetActive(false);
            }
        }
    }

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /*************************************************** Behaviours *****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// Initializes the mask to a specified number of rows and columns, off set by a specified amount.
    /// </summary>
    /// <param name="rows">The number of rows in the mask.</param>
    /// <param name="columns">The number of columns in the mask.</param>
    /// <param name="rowOffset">The number of rows to offset the mask by.</param>
    /// <param name="colOffset">The number of columns to offset the mask by.</param>
    public void Initialize(int rows, int columns, float rowOffset, float colOffset)
    {
        Rows = rows;
        Columns = columns;
        selectionTiles = new SelectionTile[rows, columns];
        maskBarrier.transform.localScale = new Vector3(columns, rows, 1);

        Transform tileHolder = transform.Find("MaskTiles");

        for(int row = 0; row < rows; row++)
        {
            for(int col = 0; col < columns; col++)
            {
                GridAddress address = new GridAddress(row, col);
                GameObject t = Instantiate(maskTile, tileHolder);
                t.transform.localPosition = new Vector3(col + colOffset, row + rowOffset, 0);
                selectionTiles[row, col] = t.GetComponent<SelectionTile>();
                selectionTiles[row, col].Address = address;
                selectionTiles[row, col].Mask = this;

                t.SetActive(false);
            }
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Creates a SelectionMask to be overlayed on a GameBoard
    /// </summary>
    /// <param name="board">the board to overlay the SelectionMask on.</param>
    public void InitializeWithGameBoard(GameBoard board)
    {
        Rows = board.Rows;
        Columns = board.Cols;
        selectionTiles = new SelectionTile[Rows, Columns];

        maskBarrier.transform.localScale = new Vector3(Columns, Rows, 1);
        Transform tileHolder = transform.Find("MaskTiles");

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                GameTile gt = board.GetTile(row, col);
                GameObject st = Instantiate(maskTile, tileHolder);

                st.transform.localPosition = gt.transform.position;
                
                selectionTiles[row, col] = st.GetComponent<SelectionTile>();
                selectionTiles[row, col].Address = gt.Address;
                selectionTiles[row, col].Mask = this;

                st.SetActive(false);
            }
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Toggles every GridAddress in the passed array as selectable if it is within the mask's dimensions.
    /// </summary>
    /// <param name="selectableAddresses">An array of GridAddresses to set as selectable.</param>
    public void SetSelectionMask(GridAddress[] selectableAddresses)
    {
        for(int i = 0; i < selectableAddresses.Length; i++)
        {
            if(IsValidTile(selectableAddresses[i].Row, selectableAddresses[i].Column))
            {
                selectionTiles[selectableAddresses[i].Row, selectableAddresses[i].Column].gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Sets a specific tile to unselectable.
    /// </summary>
    /// <param name="row">The row of the tile.</param>
    /// <param name="col">The column of the tile.</param>
    public void DisableSelectionTile(int row, int col)
    {
        if(IsValidTile(row, col))
        {
            selectionTiles[row, col].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Sets a specific tile to unselectable.
    /// </summary>
    /// <param name="address">The address of the tile.</param>
    public void DisableSelectionTile(GridAddress address)
    {
        DisableSelectionTile(address.Row, address.Column);
    }

    /// <summary>
    /// Sets a specific tile to selectable.
    /// </summary>
    /// <param name="row">The row of the tile.</param>
    /// <param name="col">The column of the tile.</param>
    public void EnableSelectionTile(int row, int col)
    {
        if (IsValidTile(row, col))
        {
            selectionTiles[row, col].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Sets a specific tile to selectable.
    /// </summary>
    /// <param name="address">The address of the tile.</param>
    public void EnableSelectionTile(GridAddress address)
    {
        EnableSelectionTile(address.Row, address.Column);
    }

    /// <summary>
    /// Gets whether or not a specific tile is active (ie. selectable)
    /// </summary>
    /// <param name="address">The address of the tile to check.</param>
    /// <returns>True if the tile is selectable, false if it is not.</returns>
    public bool GetSelectionTileActive(GridAddress address)
    {
        if(!IsValidTile(address.Row, address.Column)) {
            Debug.Log("Invalid tile selected for GetSelectionTileActive");
            return false;
        }

        return selectionTiles[address.Row, address.Column].gameObject.activeSelf;
    }

    /// <summary>
    /// Checks whether or not a tile is within the SelectionMask's dimensions.
    /// </summary>
    /// <param name="row">The row of the tile.</param>
    /// <param name="col">The column of the tile.</param>
    /// <returns>True if the tile is within the dimensions of the SelectionMask. False if not.</returns>
    public bool IsValidTile(int row, int col)
    {
        return (row >= 0 && row < Rows && col >= 0 && col < Columns);
    }

    /// <summary>
    /// Checks whether or not a tile is within the SelectionMask's dimensions.
    /// </summary>
    /// <param name="address">The address of the tile to check.</param>
    /// <returns>True if the tile is within the dimensions of the SelectionMask. False if not.</returns>
    public bool IsValidTile(GridAddress address)
    {
        return IsValidTile(address.Row, address.Column);
    }

    /// <summary>
    /// Selects the specified tile.
    /// Raises the TileSelected event using the passed tile.
    /// </summary>
    /// <param name="tile">The tile to select.</param>
    public void SelectTile(SelectionTile tile)
    {
        OnTileSelected(new TileSelectedArgs(tile.Address));
    }

    /// <summary>
    /// Selects the specified tile.
    /// Raises the TileSelected event using the passed tile.
    /// </summary>
    /// <param name="address">The address of the tile to select.</param>
    public void SelectTile(GridAddress address)
    {
        if (!IsValidTile(address))
        {
            Debug.LogError("Attempting to select invalid tile at address " + address.ToString());
            return;
        }

        if (!GetSelectionTileActive(address))
        {
            Debug.LogError("Attempting to select in active tile at address " + address.ToString());
            return;
        }

        SelectTile(selectionTiles[address.Row, address.Column]);
    }

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /****************************************************** EVENTS ******************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// Raised when a tile is selected.
    /// </summary>
    #region TileSelected Event
    public event EventHandler<TileSelectedArgs> TileSelected;

    public class TileSelectedArgs : EventArgs
    {
        public GridAddress Address { get; set; }

        public TileSelectedArgs(GridAddress address)
        {
            Address = address;
        }
    }

    private void OnTileSelected(TileSelectedArgs args)
    {
        EventHandler<TileSelectedArgs> handler = TileSelected;

        if(handler != null)
        {
            handler(this, args);
        }
    }
    #endregion
}
