using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [SerializeField]
    private List<GamePlayer> players = new List<GamePlayer>();

    private int currentPlayersTurn = -1;
    private int activePlayers = 0;
    private int readyPlayers = 0;

	// Use this for initialization
	void Start () {
        HookUpPlayers();
        SetUpPlayers();
	}


    private void HookUpPlayers()
    {
        for(int i = 0; i < players.Count; i++)
        {
            if(players[i] != null)
            {
                players[i].TurnEnded += Players_TurnEnded;
                players[i].ReadyToPlay += Players_ReadyToPlay;
                activePlayers++;
            }
        }
    }

    private void SetUpPlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != null)
            {
                players[i].SetUp();
            }
        }
    }

    private void Players_ReadyToPlay(object sender, GamePlayer.ReadyToPlayArgs e)
    {
        readyPlayers++;
        if(readyPlayers == activePlayers)
        {
            NextPlayersTurn();
        }
    }

    private void Players_TurnEnded(object sender, GamePlayer.TurnEndedEventArgs e)
    {
        NextPlayersTurn();
    }

    private void NextPlayersTurn()
    {
        do
        {
            if (currentPlayersTurn == players.Count - 1) {
                currentPlayersTurn = 0;
            } else {
                currentPlayersTurn++;
            }
        } while (players[currentPlayersTurn] == null);


        players[currentPlayersTurn].TakeTurn();
    }

}
