using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleZombieHumanPlayer : SimpleZombiePlayer {

    private List<GridAddress> selectableTiles = new List<GridAddress>();
    private int remainingDeploys;

    /*------------------------------------------------------------------------------------------------------------------*/
    /********************************************************************************************************************/
    /*************************************************** Behaviours *****************************************************/
    /********************************************************************************************************************/
    /*------------------------------------------------------------------------------------------------------------------*/


    /// <summary>
    /// Called when all of the player's pieces are done moving during the movement phase.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected override void MovementPhase_NoMorePiecesMoving(object sender, NoMorePiecesMovingArgs e)
    {
        NoMorePiecesMoving -= MovementPhase_NoMorePiecesMoving;
        if (piecesInPool.Count == 0)
        {
            EndTurn();
            return;
        }
        InitiateDeployment();
    }

    /// <summary>
    /// Begins the deployment phase of the Player's turn.
    /// </summary>
    protected override void InitiateDeployment()
    {
        gameBoard.GameTileSelected += GameBoard_GameTileSelected;
        remainingDeploys = deploysPerTurn;
        selectableTiles.Clear();
        List<GameTile> deploymentTiles = GetTilesInDeployment(true, true);

        gameBoard.ShowSelectionMask();
        gameBoard.SetSelectionMask(deploymentTiles);

        for(int i = 0; i < deploymentTiles.Count; i++)
        {
            selectableTiles.Add(deploymentTiles[i].Address);
        }
    }

    /// <summary>
    /// Deploys a piece to a chosen GridAddress on the gameboard.
    /// </summary>
    /// <param name="address">The address to deploy the piece to.</param>
    public void DeployPiece(GridAddress address)
    {
        if (!gameBoard.IsValidTile(address.Row, address.Column))
        {
            Debug.LogWarning("Invalid GridAddress passed to DeployPiece");
            return;
        }
        if (piecesInPool.Count == 0)
        {
            Debug.LogWarning("Attempting to move piece to play from pool, but pool is empty.");
            return;
        }

        MoveGamePieceToPlay(gameBoard.GetTile(address.Row, address.Column));
        RemoveSelectableTile(address);
        remainingDeploys--;
        if (remainingDeploys <= 0 || piecesInPool.Count == 0)
        {
            gameBoard.HideSelectionMask();
            NoMorePiecesMoving += DeploymentPhase_DeploymentFinished; //See SimpleZombiePlayer for this method.
        }
    }

    /// <summary>
    /// Called when the GameTileSelected event of the gameBoard occurs.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameBoard_GameTileSelected(object sender, GameBoard.GameTileSelectedArgs e)
    {
        DeployPiece(e.Tile.Address);
    }

    /// <summary>
    /// Removes a specified GridAddress from the selectableTiles list.
    /// </summary>
    /// <param name="address"></param>
    private void RemoveSelectableTile(GridAddress address)
    {
        for(int i = 0; i < selectableTiles.Count; i++)
        {
            if (selectableTiles[i].Equals(address))
            {
                selectableTiles.RemoveAt(i);
                i--;
            }
        }
        gameBoard.ToggleSelectionTile(address);
    }

    /// <summary>
    /// Ends the player's turn.
    /// </summary>
    public override void EndTurn()
    {
        gameBoard.GameTileSelected -= GameBoard_GameTileSelected;
        base.EndTurn();
    }
}
