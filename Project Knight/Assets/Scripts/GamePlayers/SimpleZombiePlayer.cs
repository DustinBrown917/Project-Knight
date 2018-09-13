using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleZombiePlayer : GamePlayer {
    [SerializeField]
    //The number of pieces the player can deploy in a turn.
    protected int deploysPerTurn = 4;

    [SerializeField]
    //The direction that the player's pieces will move.
    private Directions direction;

    [SerializeField]
    //How far the player's in play pieces move every turn.
    private int moveRange = 1;

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /*************************************************** Behaviours *****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// Sets the player's ready status to ready.
    /// </summary>
    public override void SetUp()
    {
        ReadyUp();
    }

    /// <summary>
    /// Starts the player's turn.
    /// </summary>
    public override void TakeTurn()
    {
        StartTurn();
        MoveAllPieces(direction, moveRange);  
    }

    /// <summary>
    /// Move all of this player's pieces in play \tiles\ distance in \direction\ direction.
    /// </summary>
    /// <param name="direction">The direction to move the pieces.</param>
    /// <param name="tiles">The number of tiles to move the pieces.</param>
    protected void MoveAllPieces(Directions direction, int tiles)
    {
        int rowMod = 0;
        int colMod = 0;

        switch (direction)
        {
            case Directions.UP:
                rowMod = tiles;
                break;
            case Directions.DOWN:
                rowMod = -tiles;
                break;
            case Directions.LEFT:
                colMod = -tiles;
                break;
            case Directions.RIGHT:
                colMod = tiles;
                break;
        }

        if(piecesInPlay.Count > 0) {
            NoMorePiecesMoving += MovementPhase_NoMorePiecesMoving;
        } else {
            InitiateDeployment();
            return;
        }
        
        for (int i = 0; i < piecesInPlay.Count; i++)  {
            if (gameBoard.IsValidTile(piecesInPlay[i].OccupiedTile.Row + rowMod, piecesInPlay[i].OccupiedTile.Column + colMod))  {
                MoveGamePieceInPlay(piecesInPlay[i], gameBoard.GetTile(piecesInPlay[i].OccupiedTile.Row + rowMod, piecesInPlay[i].OccupiedTile.Column + colMod));
            } else {
                MoveGamePieceToPool(piecesInPlay[i]);
                i--;
            }
        }
    }

    /// <summary>
    /// Used to determine when pieces in play have stopped moving.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void MovementPhase_NoMorePiecesMoving(object sender, NoMorePiecesMovingArgs e)
    {
        InitiateDeployment();
        NoMorePiecesMoving -= MovementPhase_NoMorePiecesMoving;
    }

    /// <summary>
    /// Begins the deployment of pieces to the board.
    /// </summary>
    protected virtual void InitiateDeployment()
    {
        DeployPieces(deploysPerTurn);
    }

    /// <summary>
    /// Deploys pieces to the game board within the player's deployment zone.
    /// </summary>
    /// <param name="count">The number of pieces to deploy.</param>
    private void DeployPieces(int count)
    {
        List<GameTile> deployableTiles = GetTilesInDeployment(true, true);

        for (int i = 0; i < count; i++)
        {
            if (piecesInPool.Count == 0 || deployableTiles.Count == 0) { break; }

            GameTile tile = deployableTiles[UnityEngine.Random.Range(0, deployableTiles.Count)];
            GamePiece p = MoveGamePieceToPlay(tile);
            
            //This means the player is out of pieces to deploy.
            if(p == null)
            {
                if(i == 0) { EndTurn(); }
                break;
            }
            deployableTiles.Remove(tile);
        }

        NoMorePiecesMoving += DeploymentPhase_DeploymentFinished; 
    }

    /// <summary>
    /// Signals the end of the players deployment.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void DeploymentPhase_DeploymentFinished(object sender, NoMorePiecesMovingArgs e)
    {
        NoMorePiecesMoving -= DeploymentPhase_DeploymentFinished;
        EndTurn();
    }

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /**************************************************** Enumerators ***************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/

    protected enum Directions
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }
}
