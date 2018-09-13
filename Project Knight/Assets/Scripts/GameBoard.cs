using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class GameBoard : MonoBehaviour {

    public static GameBoard Instance { get; private set; }

    public SelectionMask SelectMask { get; private set; }

    [SerializeField, Range(5, 100)]
    private int _rows = 5;
    public int Rows { get { return _rows; } }

    [SerializeField, Range(5, 100)]
    private int _cols = 5;
    public int Cols { get { return _cols; } }

    [SerializeField]
    private Color[] boardColours;


    private GameTile[,] board;

    private bool built = false;


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
        Instance = this;

        board = new GameTile[_cols, _rows];

        SelectMask = transform.Find("SelectionMask").GetComponent<SelectionMask>();
        
    }

    /// <summary>
    /// The Start method.
    /// </summary>
    private void Start()
    {
        BuildBoard();
        //SelectMask.Initialize(_rows, _cols, -(_rows / 2f) + 0.5f, -(_cols / 2f) + 0.5f);
        SelectMask.InitializeWithGameBoard(this);
        SelectMask.TileSelected += SelectMask_TileSelected;
    }

    #endregion

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /*************************************************** Behaviours *****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/



    /// <summary>
    /// Builds the board to a playable state.
    /// </summary>
    /// <returns>True if build was successful. False if not.</returns>
    private bool BuildBoard()
    {
        //StartCoroutine(DEBUG_Build());
        //return true;
        if (_rows < 1)
        {
            throw new InvalidBoardDimensionException("Invalid row count: " + _rows.ToString());
        }
        if (_cols < 1)
        {
            throw new InvalidBoardDimensionException("Invalid column count: " + _cols.ToString());
        }
        if (built)
        {
            Debug.LogWarning("Attempted to build an already built board. Abandoning build.");
            return false;
        }

        /* Variables for board creation */
        Vector2 startPos = new Vector2(-(_cols / 2f) + 0.5f, -(_rows / 2f) + 0.5f);
        Vector2 currentPos = startPos;
        int colourIndex = 0;
        int colourShift = 0;

        for (int row = 0; row < board.GetLength(0); row++)
        {
            /* colourShift cycles through the starting index for each columns colour
             * this ensures staggered colour patterns on the board */
            colourIndex = colourShift;
            for (int col = 0; col < board.GetLength(1); col++)
            {
                /* Get the instance of a tile and then place it */
                board[row, col] = TileHolder.Instance.GetTileInstance(0).GetComponent<GameTile>();
                currentPos.x = startPos.x + col;
                currentPos.y = startPos.y + row;
                board[row, col].Place(currentPos, row, col);

                if (boardColours.Length > 0)
                {
                    board[row, col].SetColour(boardColours[colourIndex]);
                    colourIndex = (colourIndex == boardColours.Length - 1) ? 0 : colourIndex + 1;
                }
            }
            if (colourShift < boardColours.Length - 1)
            {
                colourShift++;
            }
            else
            {
                colourShift = 0;
            }
        }

        return built = true;
    }

    /// <summary>
    /// Gets a random tile from the board.
    /// </summary>
    /// <returns>A random GameTile from this board.</returns>
    public GameTile GetRandomTile()
    {
        int colIndex = UnityEngine.Random.Range(0, Cols);
        int rowIndex = UnityEngine.Random.Range(0, Rows);
        GameTile tile = GetTile(rowIndex, colIndex);
        return tile;
    }

    /// <summary>
    /// Returns the tile at the selected row,col index.
    /// </summary>
    /// <param name="row">The row the desired tile is in.</param>
    /// <param name="col">The column the desired tile is in.</param>
    /// <returns>The GameTile at the designated index.</returns>
    public GameTile GetTile(int row, int col)
    {
        #region Error Catching
        if (row < 0)
        {
            throw new InvalidBoardDimensionException("row index must be greater than 0. Current row index: " + row.ToString() + ".");
        }
        
        if(row >= board.GetLength(0))
        {
            throw new InvalidBoardDimensionException("row index must be less than " + board.GetLength(0) + ". Current row index: " + row.ToString() + ".");
        }

        if (col < 0)
        {
            throw new InvalidBoardDimensionException("col index must be greater than 0. Current col index: " + col.ToString() + ".");
        }
        
        if(col >= board.GetLength(1))
        {
            throw new InvalidBoardDimensionException("col index must be less than " + board.GetLength(1) + ". Current col index: " + col.ToString() + ".");
        }
        #endregion

        return board[row, col];
    }

    /// <summary>
    /// Find whether or not a given index is valid for this board.
    /// </summary>
    /// <param name="col">The column index.</param>
    /// <param name="row">The row index.</param>
    /// <returns>True if index is valid, false if not.</returns>
    public bool IsValidTile(int row, int col)
    {
        if(col < 0 || col >= board.GetLength(1) || row < 0 || row >= board.GetLength(0))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Shows the selection mask.
    /// </summary>
    public void ShowSelectionMask()
    {
        SelectMask.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the selection mask.
    /// </summary>
    public void HideSelectionMask()
    {
        SelectMask.gameObject.SetActive(false);
    }

    public void SetSelectionMask(GridAddress[] selectableAddresses)
    {
        SelectMask.SetSelectionMask(selectableAddresses);
    }

    public void SetSelectionMask(List<GameTile> tiles)
    {
        GridAddress[] addresses = new GridAddress[tiles.Count];

        for(int i = 0; i < tiles.Count; i++)
        {
            addresses[i] = tiles[i].Address;
        }
        SelectMask.SetSelectionMask(addresses);
    }

    public void ToggleSelectionTile(GridAddress address)
    {
        if (SelectMask.GetSelectionTileActive(address))
        {
            SelectMask.DisableSelectionTile(address);
        }
        else
        {
            SelectMask.EnableSelectionTile(address);
        }
    }

    private void SelectMask_TileSelected(object sender, SelectionMask.TileSelectedArgs e)
    {
        OnGameTileSelected(new GameTileSelectedArgs(board[e.Address.Row, e.Address.Column]));
    }

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /****************************************************** EVENTS ******************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    public event EventHandler<GameTileSelectedArgs> GameTileSelected;
    
    public class GameTileSelectedArgs : EventArgs
    {
        public GameTile Tile { get; set; }

        public GameTileSelectedArgs(GameTile selectedTile)
        {
            Tile = selectedTile;
        }
    }

    private void OnGameTileSelected(GameTileSelectedArgs args)
    {
        EventHandler<GameTileSelectedArgs> handler = GameTileSelected;

        if(handler != null)
        {
            handler(this, args);
        }
    }



    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /**************************************************** EXCEPTIONS ****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// Thrown when a board dimension is less than 1.
    /// </summary>
    public class InvalidBoardDimensionException : Exception
    {
        public InvalidBoardDimensionException()
        {

        }

        public InvalidBoardDimensionException(string message) : base(message)
        {

        }

        public InvalidBoardDimensionException(string message, Exception inner) : base(message, inner)
        {

        }
    }

    /// <summary>
    /// To be thrown when attempting to access a GameTile that doesn't exist on the board.
    /// </summary>
    public class InvalidBoardIndexException : Exception
    {
        public InvalidBoardIndexException()
        {

        }

        public InvalidBoardIndexException(string message) : base(message)
        {

        }

        public InvalidBoardIndexException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}

