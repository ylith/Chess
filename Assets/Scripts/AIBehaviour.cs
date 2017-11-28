using System.Collections.Generic;
using UnityEngine;

public class AIBehaviour : ScriptableObject {

    public void NextMove()
    {
        List<GameObject> pieces = GameManager.Instance.GetPiecesByColor(Constants.Black);
        Vector2 bestPos = new Vector2(0, 0);
        GameObject bestPiece = pieces[0];
        int bestValue = 0;

        foreach (GameObject piece in pieces)
        {
            Piece script = piece.GetComponent<Piece>();
            List<Vector2> availableMoves = script.GetAvailableMoves();
            Board board = GameManager.Instance.board;

            foreach (Vector2 move in availableMoves) // Get all available moves, test them and report the result board values by weights
            {
                int currentValue = 0;
                Dictionary<int, List<Vector2>>[] tempLocks = (Dictionary<int, List<Vector2>>[])board.locks.Clone();
                List<GameObject> tempPieces = new List<GameObject>(GameManager.Instance.GetPieces());
                BoardArray tempBoard = new BoardArray(8, 8);
                tempBoard.boardArray = (int[,])board.BoardObj.boardArray.Clone();
                Vector2Int currentPos = script.currentBoardPosition;
                if (board.GetIdByCoord(move) != 0)
                {
                    tempPieces.Remove(board.GetByCoord(move));
                }
                board.RemovePiece(script.currentBoardPosition);
                board.SetPiece(piece, move);
                script.currentBoardPosition = new Vector2Int((int)move.x, (int)move.y);
                board.SetLocks(tempPieces);
                
                foreach (GameObject tempPiece in tempPieces)
                {
                    Piece tempScrip = tempPiece.GetComponent<Piece>();
                    if (tempScrip.player == Constants.Black)
                    {
                        currentValue += Constants.weights[tempScrip.type];
                    } else
                    {
                        currentValue -= Constants.weights[tempScrip.type];
                    }
                }
                bool isInCheck = board.IsKingUnderCheck(script.player);

                if (isInCheck)
                {
                    continue;
                }

                float rand = Random.Range(0f, 100f) / 100; // get a random 0-1 float and setup 30% chance to change move for moves with the same result

                if (bestPos == Vector2.zero) //initialize
                {
                    bestPos = move;
                    bestPiece = piece;
                    bestValue = currentValue;
                } else if (currentValue > bestValue || (currentValue == bestValue && rand > 0.3))
                {
                    bestPos = move;
                    bestPiece = piece;
                    bestValue = currentValue;
                }

                //reset
                board.locks = (Dictionary<int, List<Vector2>>[])tempLocks.Clone();
                board.BoardObj.boardArray = (int[,])tempBoard.boardArray.Clone();
                script.currentBoardPosition = currentPos;
            }
        }

        if (bestPos == Vector2.zero) // game over if error or no moves
        {
            GameManager.Instance.gameOverText.text = "Something went wrong";
            GameManager.Instance.PassTurn();
            GameManager.Instance.GameOver();
        } else
        {
            Vector3 move = GameManager.Instance.ConvertBoardToScreenCoords(bestPos);
            Vector2Int boardPos = new Vector2Int((int)bestPos.x, (int)bestPos.y);
            GameManager.Instance.board.RemovePieceAt(boardPos);
            bestPiece.GetComponent<Piece>().Move(move);
            GameManager.Instance.PassTurn();
        }
    }
}
