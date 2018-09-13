using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayer : MonoBehaviour {

    
    public static GamePlayer ThisPlayersTurn { get; protected set; }

    [SerializeField]
    private Color _playerTint = Color.white;
    public Color PlayerTint { get { return _playerTint; } }

    [SerializeField]
    protected int _turnsCompleted = 0;
    public int TurnsCompleted { get { return _turnsCompleted; } }

    public bool Ready { get; protected set; }

    [SerializeField]
    protected List<GamePiece> piecesInPlay = new List<GamePiece>();
    [SerializeField]
    protected List<GamePiece> piecesInPool = new List<GamePiece>();
    [SerializeField]
    protected List<GamePiece> movingPieces = new List<GamePiece>();

    protected Transform piecesInPlayParent;
    protected Transform piecesInPoolParent;
    [SerializeField]
    protected Vector3 piecesInPoolParentPostion = new Vector3();

    [SerializeField]
    protected bool placePiecesManually = false;
    [SerializeField]
    protected int startingPieceCount = 5;
    [SerializeField]
    protected GameObject defaultPiece;

    [SerializeField]
    protected GameBoard gameBoard;

    [SerializeField]
    private GridAddress deployRangeTopLeftTile;
    [SerializeField]
    private GridAddress deployRangeBottomRightTile;
    [SerializeField]
    protected GridRange deployArea;


    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /************************************************ Unity Behaviours **************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    #region Unity Behaviours

    /// <summary>
    /// The Start method.
    /// </summary>
    protected virtual void Start()
    {
        deployHesitation = new WaitForSeconds(deployHesitationTime);
        InitializeChildTransforms();
        gameBoard = FindGameBoard();
        InitializeDeploymentZone();
        FillPoolWithDefaultPieces(); //DEBUG
    }

    #endregion

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /*************************************************** Behaviours *****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/


    /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ DEBUG ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/

    /// <summary>
    /// Ensures there is always at least some room to deploy.
    /// </summary>
    protected void InitializeDeploymentZone()
    {
        deployArea = new GridRange(deployRangeTopLeftTile, deployRangeBottomRightTile);
        if(deployArea.Area == 0) {
            throw new InvalidDeployAreaException("The deployment area contains 0 valid tiles.");
        }

    }

    /// <summary>
    /// Creates and assigns "Pieces in Play" and "Pieces in Pool" child objects.
    /// </summary>
    protected void InitializeChildTransforms()
    {
        piecesInPlayParent = new GameObject("Pieces in Play").transform;
        piecesInPlayParent.SetParent(transform);
        piecesInPoolParent = new GameObject("Pieces in Pool").transform;
        piecesInPoolParent.SetParent(transform);
        piecesInPoolParent.position = piecesInPoolParentPostion;
    }

    /// <summary>
    /// Sets the Player's board up for play.
    /// </summary>
    public virtual void SetUp()
    {
        StartCoroutine(CR_AutoDeploy());
    }

    #region Cached Variables for CR_AutoDeploy()
    [SerializeField]
    private float deployHesitationTime = 0.1f;
    private WaitForSeconds deployHesitation;
    #endregion
    /// <summary>
    /// CoRoutine to automatically place as many pieces as possible.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CR_AutoDeploy()
    {
        for(int row = (int)deployArea.BottomLeft.Row; row <= (int)deployArea.TopRight.Row; row++)
        {
            for(int col = (int)deployArea.BottomLeft.Column; col <= (int)deployArea.TopRight.Column; col++)
            {
                if(!gameBoard.IsValidTile(row, col))
                {
                    continue;
                }
                MoveGamePieceToPlay(gameBoard.GetTile(row, col));
                yield return deployHesitation;
            }
        }
        yield return new WaitForSeconds(3);
        OnAllPiecesDeployed(new AllPiecesDeployedEventArgs(this));
        ReadyUp();
    }

    /// <summary>
    /// Attempts to find the GameBoard in this scene.
    /// </summary>
    /// <returns>The GameBoard if found.</returns>
    protected GameBoard FindGameBoard()
    {
        GameBoard gb;
        if(GameBoard.Instance != null) {
            gb = GameBoard.Instance;
        }
        else {
            gb = transform.parent.Find("GameBoard").GetComponent<GameBoard>();
        }

        if(gb == null) {
            throw new NoGameBoardFoundException("The GameBoard could not be found in this scene.");
        }

        return gb;
    }

    /// <summary>
    /// Ease of use - Fills the bool with startingPieceCount default GamePieces.
    /// </summary>
    private void FillPoolWithDefaultPieces()
    {
        if(defaultPiece == null)
        {
            throw new DefaultGamePieceNotFoundException("The default GamePiece has not been set in the editor.");
        }
        for (int i = 0; i < startingPieceCount; i++)
        {
            AddNewGamePieceToPool(Instantiate(defaultPiece).GetComponent<GamePiece>());
        }
    }

    /// <summary>
    /// Adds a new new piece to the Player's pool.
    /// </summary>
    /// <param name="piece">The piece to add to the pool.</param>
    public void AddNewGamePieceToPool(GamePiece piece)
    {
        piece.SetOwner(this, true);
        piecesInPool.Add(piece);
        piece.transform.parent = piecesInPoolParent;
        piece.transform.localPosition = Vector3.zero;
    }

    /*---------------------------------------- Movement ----------------------------------------*/

    /// <summary>
    /// Adds GamePieces to the movingPieces list.
    /// </summary>
    /// <param name="piece">The GamePiece to add to the list.</param>
    protected void AddMovingPiece(GamePiece piece)
    {
        movingPieces.Add(piece);
    }

    #region Cached Variables for RemoveMovingPieces()
    private NoMorePiecesMovingArgs nmpmArgs;
    #endregion
    /// <summary>
    /// Removes GamePieces from the movingPieces collection. Raises an event when there are no moving pieces left.
    /// </summary>
    /// <param name="piece">GamePiece to be removed from the collection.</param>
    protected void RemoveMovingPiece(GamePiece piece)
    {
        if(nmpmArgs == null) { nmpmArgs = new NoMorePiecesMovingArgs(this); }
        movingPieces.Remove(piece);
        if(movingPieces.Count == 0)
        {
            OnNoMorePiecesMoving(nmpmArgs);
        }
    }

    #region Cached variables for MoveGamePieceToPool()
    private LastPieceRemovedFromPlayEventArgs lastPieceRemovedFromPlayEventArgs;
    #endregion
    /// <summary>
    /// Move a GamePiece to this player's pool.
    /// </summary>
    /// <param name="piece">The piece to be pooled.</param>
    /// <param name="animateMovement">Whether or not movement to pool should be instantaneous or animated.</param>
    public void MoveGamePieceToPool(GamePiece piece)
    {
        piecesInPlay.Remove(piece);
        piece.transform.parent = piecesInPoolParent;

        piece.MoveOffBoard(piecesInPoolParentPostion, true);
        AddMovingPiece(piece);
        piece.MoveComplete += Piece_MovedToPool;

        if(piecesInPlay.Count == 0)
        {
            if(lastPieceRemovedFromPlayEventArgs == null)
            {
                lastPieceRemovedFromPlayEventArgs = new LastPieceRemovedFromPlayEventArgs(this);
            }
            OnLastPieceRemovedFromPlay(lastPieceRemovedFromPlayEventArgs);
        }
    }

    /// <summary>
    /// Called when a piece being moved off the board reaches its destination.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void Piece_MovedToPool(object sender, GamePiece.MoveCompleteArgs e)
    {
        e.Piece.MoveComplete -= Piece_MovedToPool;
        RemoveMovingPiece(e.Piece);
        piecesInPool.Add(e.Piece);
    }

    /// <summary>
    /// Moves a GamePiece from the player's pool onto the GameBoard and into the player's piecesInPlay list.
    /// </summary>
    /// <param name="tile">The tile to place the gamepiece at.</param>
    /// <param name="atIndex">The index of the pice to move int play.</param>
    /// <returns></returns>
    public GamePiece MoveGamePieceToPlay(GameTile tile, int atIndex = -1)
    {
        if(piecesInPool.Count == 0) {
            Debug.Log("No pooled pieces to place.");
            return null;
        }
        if(atIndex >= piecesInPool.Count) {
            Debug.Log("atIndex is greater than piecesInPool uBound. atIndex: " + atIndex.ToString() + " >> piecesInPool uBound: " + (piecesInPool.Count - 1).ToString());
            return null;
        }

        GamePiece piece;
        if(atIndex < 0) {
            piece = piecesInPool[piecesInPool.Count - 1];
        }
        else {
            piece = piecesInPool[atIndex];
        }

        piece.MoveToTile(tile);
        AddMovingPiece(piece);
        piece.MoveComplete += Piece_MovedToPlay;
        piecesInPool.Remove(piece);
        piece.transform.SetParent(piecesInPlayParent);

        return piece;
    }

    /// <summary>
    /// Called when a piece being moved to play reaches its destination.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void Piece_MovedToPlay(object sender, GamePiece.MoveCompleteArgs e)
    {
        e.Piece.MoveComplete -= Piece_MovedToPlay;
        RemoveMovingPiece(e.Piece);
        piecesInPlay.Add(e.Piece);
    }

    /// <summary>
    /// Moves a piece in play to a chosen game tile.
    /// </summary>
    /// <param name="piece">The piece to move.</param>
    /// <param name="tile">The destination tile.</param>
    public virtual void MoveGamePieceInPlay(GamePiece piece, GameTile tile)
    {
        if (!piecesInPlay.Contains(piece))
        {
            throw new GamePieceNotInPlayException("The GamePiece you are trying to move is not in play. It may be moving or pooled.");
        }
        piece.MoveToTile(tile);
        piece.MoveComplete += Piece_MovedInPlay;
        AddMovingPiece(piece);
        
    }

    /// <summary>
    /// Called when a piece moved in play reaches its destination.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Piece_MovedInPlay(object sender, GamePiece.MoveCompleteArgs e)
    {
        e.Piece.MoveComplete -= Piece_MovedInPlay;
        RemoveMovingPiece(e.Piece);
    }

    /// <summary>
    /// Moves a random piece to a random tile.
    /// </summary>
    private void MakeRandomMove()
    {
        if (piecesInPlay.Count == 0)
        {
            EndTurn();
            return;
        }

        GamePiece piece = piecesInPlay[UnityEngine.Random.Range(0, piecesInPlay.Count)];
        piece.MoveComplete += Piece_RandomMoveComplete;


        GameTile tile;
        bool containsFriendly;
        do
        {
            tile = gameBoard.GetRandomTile();
            containsFriendly = false;
            if (!tile.IsOccupied) { continue; }

            foreach (GamePiece p in tile.Occupiers)
            {
                if (p.Owner == this)
                {
                    containsFriendly = true;
                    break;
                }
            }

        } while (tile.IsObstructed() || containsFriendly);

        piece.MoveToTile(tile);
        AddMovingPiece(piece);
    }

    /// <summary>
    /// Receives message that piece has finished moving and ends turn. Used in MakeRandomMove().
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Piece_RandomMoveComplete(object sender, GamePiece.MoveCompleteArgs e)
    {
        e.Piece.MoveComplete -= Piece_RandomMoveComplete;
        RemoveMovingPiece(e.Piece);
        EndTurn();
    }

    /*---------------------------------------- Movement ----------------------------------------*/

    #region Cached variables for StartTurn()
    private TurnStartedEventArgs turnStartedEventArgs;
    #endregion
    /// <summary>
    /// Signals the start of the Player's turn.
    /// </summary>
    public virtual void StartTurn()
    {
        if(turnStartedEventArgs == null)
        {
            turnStartedEventArgs = new TurnStartedEventArgs(this);
        }

        OnTurnStarted(turnStartedEventArgs);
    }

    /// <summary>
    /// Executes this players turn.
    /// </summary>
    public virtual void TakeTurn()
    {
        StartTurn();
        MakeRandomMove();
    }

    #region Cached variables for EndTurn()
    protected TurnEndedEventArgs turnEndedEventArgs;
    #endregion
    /// <summary>
    /// Signals the end of the Player's turn.
    /// </summary>
    public virtual void EndTurn()
    {
        if(turnEndedEventArgs == null)
        {
            turnEndedEventArgs = new TurnEndedEventArgs(this);
        }

        OnTurnEnded(turnEndedEventArgs);
    }

    /// <summary>
    /// Notifies that the player is ready to play.
    /// </summary>
    protected void ReadyUp()
    {
        OnReadyToPlay(new ReadyToPlayArgs(this));
    }

    /// <summary>
    /// Gets a random tile within the Player's deployment zone.
    /// </summary>
    /// <returns>TIle within deployment zone.</returns>
    protected GameTile GetRandomTileInDeploymentArea()
    {
        int row = UnityEngine.Random.Range(deployArea.BottomLeft.Row, deployArea.TopRight.Row + 1);
        int col = UnityEngine.Random.Range(deployArea.BottomLeft.Column, deployArea.TopRight.Column + 1);

        return gameBoard.GetTile(row, col);
         
    }

    /// <summary>
    /// Gets a list of GameTiles within this Player's deployment area.
    /// </summary>
    /// <param name="excludeObstructed">Whether or not GameTiles that are obstructed should be excluded from this list.</param>
    /// <param name="excludeOccupied">Whether or not GameTiles that are occupied should be excluded from the list.</param>
    /// <returns>List of GameTiles.</returns>
    protected List<GameTile> GetTilesInDeployment(bool excludeObstructed = false, bool excludeOccupied = false)
    {
        List<GameTile> deployTiles = new List<GameTile>();
        for(int row = deployArea.BottomLeft.Row; row <= deployArea.TopRight.Row; row++)
        {
            for(int col = deployArea.BottomLeft.Column; col <= deployArea.TopRight.Column; col++)
            {
                if(gameBoard.IsValidTile(row, col))
                {
                    GameTile t = gameBoard.GetTile(row, col);
                    if (excludeObstructed)
                    {
                        if (t.IsObstructed())
                        {
                            continue;
                        }
                    }
                    if (excludeOccupied)
                    {
                        if (t.IsOccupied)
                        {
                            continue;
                        }
                    }

                    deployTiles.Add(t);
                }
            }
        }

        return deployTiles;
    }



    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /****************************************************** EVENTS ******************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// Raised when the GamePlayer is ready to start playing.
    /// </summary>
    #region ReadyToPlay Event.
    public event EventHandler<ReadyToPlayArgs> ReadyToPlay;

    public class ReadyToPlayArgs : EventArgs
    {
        public GamePlayer Player { get; set; }

        public ReadyToPlayArgs(GamePlayer p)
        {
            Player = p;
        }
    }

    protected virtual void OnReadyToPlay(ReadyToPlayArgs e)
    {
        Ready = true;

        EventHandler<ReadyToPlayArgs> handler = ReadyToPlay;

        if (handler != null)
        {
            handler(this, e);
        }
    }
    #endregion

    /// <summary>
    /// Raised when player has placed all GamePieces on GameBoard.
    /// </summary>
    #region AllPiecesDeployed Event
    public event EventHandler<AllPiecesDeployedEventArgs> AllPiecesDeployed;

    public class AllPiecesDeployedEventArgs : EventArgs
    {
        public int PiecesDeployed { get; set; }
        public GamePlayer Player { get; set; }

        public AllPiecesDeployedEventArgs(GamePlayer p)
        {
            Player = p;
        }
    }

    protected virtual void OnAllPiecesDeployed(AllPiecesDeployedEventArgs e)
    {
        EventHandler<AllPiecesDeployedEventArgs> handler = AllPiecesDeployed;
        if(handler != null)
        {
            handler(this, e);
        }
    }
    #endregion

    /// <summary>
    /// Raised when player's turn starts.
    /// </summary>
    #region TurnStarted Event
    public event EventHandler<TurnStartedEventArgs> TurnStarted;

    public class TurnStartedEventArgs : EventArgs
    {
        public GamePlayer Player { get; set; }

        public TurnStartedEventArgs(GamePlayer p)
        {
            Player = p;
        }
    }

    protected virtual void OnTurnStarted(TurnStartedEventArgs e)
    {
        if(ThisPlayersTurn != null && ThisPlayersTurn != this)
        {
            ThisPlayersTurn.EndTurn();
        }
        ThisPlayersTurn = this;
        EventHandler<TurnStartedEventArgs> handler = TurnStarted;

        if(handler != null)
        {
            handler(this, e);
        }
    }
    #endregion

    /// <summary>
    /// Raised when player ends their turn.
    /// </summary>
    #region TurnEnded Event
    public event EventHandler<TurnEndedEventArgs> TurnEnded;
    
    public class TurnEndedEventArgs : EventArgs
    {
        public GamePlayer Player { get; set; }

        public TurnEndedEventArgs(GamePlayer p)
        {
            Player = p;
        }
    }

    protected virtual void OnTurnEnded(TurnEndedEventArgs e)
    {
        if(ThisPlayersTurn == this)
        {
            ThisPlayersTurn = null;
        }
        _turnsCompleted += 1;
        EventHandler<TurnEndedEventArgs> handler = TurnEnded;
        if(handler != null)
        {
            handler(this, e);
        }
    }
    #endregion

    /// <summary>
    /// Raised when the last Player's last GamePiece on the GameBoard is removed from the GameBoard.
    /// </summary>
    #region LastPieceRemovedFromPlay Event
    public event EventHandler<LastPieceRemovedFromPlayEventArgs> LastPieceRemovedFromPlay;

    public class LastPieceRemovedFromPlayEventArgs : EventArgs
    {
        public GamePlayer Player { get; set; }

        public LastPieceRemovedFromPlayEventArgs(GamePlayer p)
        {
            Player = p;
        }
    }

    protected virtual void OnLastPieceRemovedFromPlay(LastPieceRemovedFromPlayEventArgs e)
    {
        EventHandler<LastPieceRemovedFromPlayEventArgs> handler = LastPieceRemovedFromPlay;

        if(handler != null)
        {
            handler(this, e);
        }
    }

    #endregion

    /// <summary>
    /// Raised when the player has no more piece moving.
    /// </summary>
    #region NoMorePiecesMoving Event
    public event EventHandler<NoMorePiecesMovingArgs> NoMorePiecesMoving;

    public class NoMorePiecesMovingArgs : EventArgs
    {
        public GamePlayer Player { get; set; }

        public NoMorePiecesMovingArgs(GamePlayer p)
        {
            Player = p;
        }
    }

    public virtual void OnNoMorePiecesMoving(NoMorePiecesMovingArgs e)
    {
        EventHandler<NoMorePiecesMovingArgs> handler = NoMorePiecesMoving;

        if(handler != null)
        {
            handler(this, e);
        }
    }
    #endregion

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /**************************************************** EXCEPTIONS ****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// Thrown when no GameBoard can be found for the player to play on.
    /// </summary>
    public class NoGameBoardFoundException : Exception
    {
        public NoGameBoardFoundException()
        {

        }

        public NoGameBoardFoundException(string message) : base(message)
        {

        }

        public NoGameBoardFoundException(string message, Exception inner) : base(message, inner)
        {

        }
    }

    /// <summary>
    /// Thrown when no default GamePiece exists but is requested.
    /// </summary>
    public class DefaultGamePieceNotFoundException : Exception
    {
        public DefaultGamePieceNotFoundException()
        {

        }

        public DefaultGamePieceNotFoundException(string message) : base(message)
        {

        }

        public DefaultGamePieceNotFoundException(string message, Exception inner) : base(message, inner)
        {

        }
    }

    /// <summary>
    /// Thrown when the deployment zone could not be initialized properly.
    /// </summary>
    public class InvalidDeployAreaException : Exception
    {
        public InvalidDeployAreaException()
        {

        }

        public InvalidDeployAreaException(string message) : base(message)
        {

        }

        public InvalidDeployAreaException(string message, Exception inner) : base(message, inner)
        {

        }
    }

    /// <summary>
    /// Thrown when an in-play move is attempted on a gamepiece that is not in play.
    /// </summary>
    public class GamePieceNotInPlayException : Exception
    {
        public GamePieceNotInPlayException()
        {

        }

        public GamePieceNotInPlayException(string message) : base(message)
        {

        }

        public GamePieceNotInPlayException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
