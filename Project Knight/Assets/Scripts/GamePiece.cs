using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GamePiece : MonoBehaviour {

    private static float GamePieceZLayer = -0.01f;

    protected bool _obstacle;
    public bool IsObstacle { get { return _obstacle; } }

    private int _numberOfMoves = 0;
    public int NumberOfMoves { get { return _numberOfMoves; } }

    protected SpriteRenderer spriteRenderer;

    private GameTile _occupiedTile;
    public GameTile OccupiedTile { get { return _occupiedTile; } }

    private GamePlayer _owner;
    public GamePlayer Owner { get { return _owner; } }



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
        movements = new Queue<MovementPackage>();
    }

    /// <summary>
    /// The Start method.
    /// </summary>
    void Start()
    {

    }

    #endregion

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /*************************************************** Behaviours *****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/



    /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ DEBUG ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/


    /// <summary>
    /// Sets the owner of the piece.
    /// </summary>
    /// <param name="player">Player to own the piece.</param>
    /// <param name="usePlayerColour">True to use the player's colour.</param>
    /// <param name="useColourTransition">Smooth colour transition if true, instantaneous if false.</param>
    public virtual void SetOwner(GamePlayer player, bool usePlayerColour, bool useColourTransition = false)
    {
        _owner = player;
        if (usePlayerColour)
        {
            if (useColourTransition)
            {
                ChangeColour(player.PlayerTint, 0.5f);
            }
            else
            {
                ChangeColour(player.PlayerTint);
            }
        }
    }

    /// <summary>
    /// Instantaneously change the piece's colour.
    /// </summary>
    /// <param name="color">The colour to change the piece to.</param>
    public virtual void ChangeColour(Color color)
    {
        if(CR_ChangeColourContainer != null)
        {
            StopCoroutine(CR_ChangeColourContainer);
            CR_ChangeColourContainer = null;
        }
        spriteRenderer.color = color;
    }

    /// <summary>
    /// Changes the Piece's colour using a timed transition.
    /// </summary>
    /// <param name="color">The colour the piece will change to.</param>
    /// <param name="transitionTime">The time the transition will take.</param>
    public virtual void ChangeColour(Color color, float transitionTime)
    {
        if (CR_ChangeColourContainer != null)
        {
            StopCoroutine(CR_ChangeColourContainer);
            CR_ChangeColourContainer = null;
        }
        CR_ChangeColourContainer = StartCoroutine(CR_ChangeColour(color, transitionTime));
    }

    #region Cached Variables for CR_ChangeColour
    private Coroutine CR_ChangeColourContainer = null;
    #endregion
    /// <summary>
    /// Transitions the GamePiece's colour from its current colour to targetColour over the duration of transitionTime.
    /// </summary>
    /// <param name="targetColour">The desired colour of the GamePiece.</param>
    /// <param name="transitionTime">The time the transition should take.</param>
    /// <returns></returns>
    private IEnumerator CR_ChangeColour(Color targetColour, float transitionTime)
    {
        if(transitionTime < 0) { transitionTime = 0; }
        Color startColour = spriteRenderer.color;
        float t = 0;

        while(t < transitionTime)
        {
            spriteRenderer.color = Color.Lerp(startColour, targetColour, t / transitionTime);
            t += Time.deltaTime;
            yield return null;
        }
        CR_ChangeColourContainer = null;
    }

    /// <summary>
    /// Creates an occupation association between this GamePiece and the passed in GameTile.
    /// </summary>
    /// <param name="tile">The tile to occupy.</param>
    public virtual void OccupyTile(GameTile tile)
    {
        if (_occupiedTile != null) {
            _occupiedTile.RemoveOccupier(this);
        }

        _occupiedTile = tile;
        tile.AddOccupier(this);

        InteractWithPiecesOnTile(tile);

    }

    /// <summary>
    /// Breaks occupation association between this GamePiece and the OccupiedTile
    /// </summary>
    public virtual void RemoveOccupation()
    {
        if(_occupiedTile != null)
        {
            _occupiedTile.RemoveOccupier(this);
        }
        _occupiedTile = null;
    }

    /// <summary>
    /// Moves the GamePiece to the target GameTile's position.
    /// </summary>
    /// <param name="tile">The tile to move the GamePiece to.</param>
    public virtual void MoveToTile(GameTile tile)
    {
        MoveToTile(tile, moveSmoothTime, moveMaxSpeed, true);
    }

    /// <summary>
    /// Moves the GamePiece to the target GameTile's position.
    /// </summary>
    /// <param name="tile">The tile to move the GamePiece to.</param>
    /// <param name="smoothTime">The amount of time in seconds spent smoothing the movement.</param>
    /// <param name="maxSpeed">The maximum clamp of the speed.</param>
    /// <param name="occupyAtDestination">True of the GamePiece should occupy the destination tile when it gets there.</param>
    public virtual void MoveToTile(GameTile tile, float smoothTime, float maxSpeed, bool occupyAtDestination)
    {
        RemoveOccupation();
        //Lift
        movements.Enqueue(new MovementPackage(transform.position + liftVector, liftDampTime, liftMaxSpeed, raiseEvent: false));
        //Move
        movements.Enqueue(new MovementPackage(tile, smoothTime, maxSpeed, occupyAtDestination: false, offset: liftVector, raiseEvent:false));      
        //Place
        movements.Enqueue(new MovementPackage(tile, liftDampTime, liftMaxSpeed, occupyAtDestination, offset: new Vector3(), raiseEvent:true));        

        StartMovement();
    }

    /// <summary>
    /// Moves the GamePiece unrestricted by the GameBoard.
    /// </summary>
    /// <param name="position">The position to move the piece to.</param>
    /// <param name="interrupt">True if this should immediately halt and replace any other movement the GamePiece is undergoing.</param>
    public virtual void MoveOffBoard(Vector3 position, bool interrupt = false)
    {
        MoveOffBoard(position, moveSmoothTime, moveMaxSpeed, interrupt);
    }

    /// <summary>
    /// Moves the GamePiece unrestricted by the GameBoard.
    /// </summary>
    /// <param name="position">The position to move the piece to.</param>
    /// <param name="smoothTime">The amount of time in seconds spent smoothing the movement.</param>
    /// <param name="maxSpeed">The maximum clamp of the speed.</param>
    /// <param name="interrupt">True if this should immediately halt and replace any other movement the GamePiece is undergoing.</param>
    public virtual void MoveOffBoard(Vector3 position, float smoothTime, float maxSpeed, bool interrupt)
    {
        RemoveOccupation();

        if (interrupt)
        {
            movements.Clear();
            if(CR_Relocating != null)
            {
                StopCoroutine(CR_Relocating);
                CR_Relocating = null;
            }
        }

        MovementPackage mp = new MovementPackage(position, smoothTime, maxSpeed, false) { RaiseEvent = true };
        movements.Enqueue(mp);
        StartMovement();
    }

    /// <summary>
    /// Begins the CR_Move Coroutine.
    /// </summary>
    protected virtual void StartMovement()
    {
        if (CR_Relocating == null)
        {
            CR_Relocating = StartCoroutine(CR_Move());
        }
    }

    #region CR_Move Cached Variables
    [SerializeField]
    protected float moveSmoothTime = 0.2f;
    [SerializeField]
    protected float moveMaxSpeed = 5.0f;
    [SerializeField]
    protected float liftDampTime = 0.2f;
    [SerializeField]
    protected Vector3 liftVector = new Vector3(0,0,-1);
    [SerializeField]
    protected float liftMaxSpeed = 10;
    protected Coroutine CR_Relocating = null;
    protected Vector3 movementVelocity = Vector3.zero;
    protected Queue<MovementPackage> movements;
    #endregion
    /// <summary>
    /// Carries out the movements stored in the movements queue.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator CR_Move()
    {
        while(movements.Count > 0)
        {
            MovementPackage mp = movements.Peek();

            RemoveOccupation();

            while (Vector3.Distance(transform.position, mp.Target) > 0.01)
            {
                transform.position = Vector3.SmoothDamp(transform.position, mp.Target, ref movementVelocity, mp.SmoothTime, mp.MaxSpeed);
                yield return null;
            }

            if(mp.TargetTile != null && mp.OccupyAtDestination)
            {
                OccupyTile(mp.TargetTile);
            }

            if (mp.RaiseEvent)
            {
                OnMoveComplete(new MoveCompleteArgs(this)); //Raise MoveComplete Event
            }

            movements.Dequeue();
        }


        CR_Relocating = null;
    }

    /// <summary>
    /// Triggers Interact() on all other pieces on the occupied GameTile
    /// </summary>
    public virtual void InteractWithPiecesOnTile(GameTile t)
    {
        if(t == null) { return; }
        for(int i = 0; i < t.OccupierCount; i++)
        {
            t.GetOccupierAt(i).Interact(this);
        }
    }

    /// <summary>
    /// Interact with the passed GamePiece.
    /// </summary>
    /// <param name="other">GamePiece to interact with.</param>
    public virtual void Interact(GamePiece other)
    {
        if(other.Owner != Owner)
        {
            Owner.MoveGamePieceToPool(this);
        }
    }


    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /****************************************************** EVENTS ******************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// Raised when the GamePiece has completed a move.
    /// </summary>
    #region MoveComplete Event
    public event EventHandler<MoveCompleteArgs> MoveComplete;

    public class MoveCompleteArgs : EventArgs
    {
        public GamePiece Piece { get; set; }

        public MoveCompleteArgs(GamePiece p)
        {
            Piece = p;
        }
    }

    protected virtual void OnMoveComplete(MoveCompleteArgs e)
    {
        EventHandler<MoveCompleteArgs> handler = MoveComplete;
        if (handler != null)
        {
            handler(this, e);
        }
    }
    #endregion

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /**************************************************** SUBCLASSES ****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/


    /// <summary>
    /// Used to store and communicate information about a GamePiece's movement.
    /// </summary>
    protected class MovementPackage
    {
        public Vector3 Target { get; set; }
        public GameTile TargetTile { get; set; }
        public float SmoothTime { get; set; }
        public float MaxSpeed { get; set; }
        public bool OccupyAtDestination { get; set; }
        public bool RaiseEvent { get; set; }

        /// <summary>
        /// Construct Movement package given a location.
        /// </summary>
        /// <param name="target">The target destination.</param>
        /// <param name="smoothTime">The time in seconds that smoothing should take.</param>
        /// <param name="maxSpeed">The maximum clamp of the speed.</param>
        /// <param name="raiseEvent">True if the MoveComplete event should be raised.</param>
        public MovementPackage(Vector3 target, float smoothTime, float maxSpeed, bool raiseEvent)
        {
            Target = target;
            SmoothTime = smoothTime;
            MaxSpeed = maxSpeed;
            OccupyAtDestination = false;
            TargetTile = null;
            RaiseEvent = raiseEvent;
        }

        /// <summary>
        /// Construct MovementPackage with a GameTile as the target and the ability to offset from that tiles position.
        /// </summary>
        /// <param name="target">The target GameTile.</param>
        /// <param name="smoothTime">The time in seconds that smoothing should take.</param>
        /// <param name="maxSpeed">The maximum clamp of the speed.</param>
        /// <param name="occupyAtDestination">True if the GamePiece should interact when with other GamePieces when it arrives at the tile.</param>
        /// <param name="offset">The offset of the target position.</param>
        /// <param name="raiseEvent">True if the MoveComplete event should be raised.</param>
        public MovementPackage(GameTile target, float smoothTime, float maxSpeed, bool occupyAtDestination, Vector3 offset, bool raiseEvent)
        {
            TargetTile = target;
            Target = target.WorldPos + offset;
            SmoothTime = smoothTime;
            MaxSpeed = maxSpeed;
            OccupyAtDestination = occupyAtDestination;
            TargetTile = target;
            RaiseEvent = raiseEvent;
        }
    }
}
