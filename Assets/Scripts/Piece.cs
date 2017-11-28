using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
    public int type;
    public int player;
    public bool isInitialPosition = true;
    public Vector2Int currentBoardPosition;
    
    void OnMouseDown()
    {
        if (! GameManager.Instance.isRunning)
        {
            return;
        }
        Vector3 clickPos = GameManager.Instance.GetClickCoordinates();
        Vector2Int boardPosition = GameManager.Instance.ConvertScreenToBoardCoords(clickPos);

        if (GameManager.Instance.isAi && GameManager.Instance.CurrentPlayer == Constants.Black) // disable mouse input for AI
        {
            return;
        }

        if (player == GameManager.Instance.CurrentPlayer) // if this is the current player
        {
            bool castling = false;
            if (GameManager.Instance.CurrentPiece != null)
            {
                Piece script = GameManager.Instance.CurrentPiece.GetComponent<Piece>();
                if (script.type == Constants.King && type == Constants.Rook) // check for castling
                {
                    castling = script.AttemptCastling(gameObject); //castling
                } else if(type == Constants.King && script.type == Constants.Rook)
                {
                    castling = AttemptCastling(GameManager.Instance.CurrentPiece);
                }
                GameManager.Instance.CurrentPiece.GetComponent<Piece>().Deselect(); // swap current piece with this one
            }
            Select();
            if (castling)
            {
                GameManager.Instance.RemoveHighlights();
                GameManager.Instance.PassTurn();
            }
        }
        else if (GameManager.Instance.CurrentPiece != null && GameManager.Instance.CurrentPiece.GetComponent<Piece>().IsValidMove(boardPosition) && // check movement for validity
            player != GameManager.Instance.CurrentPlayer)
        {
            Vector3 movePosition = GameManager.Instance.ConvertBoardToScreenCoords(boardPosition);
            GameManager.Instance.board.RemovePieceAt(boardPosition);
            GameManager.Instance.CurrentPiece.GetComponent<Piece>().Move(movePosition);
            GameManager.Instance.PassTurn();
        }
    }

    void OnMouseDrag()
    {
        /*
        GameObject board = GameObject.FindGameObjectsWithTag("Board")[0];
        Vector3 currentPosition = GameManager.Instance.GetClickCoordinates();

        if (currentPosition.x < board.transform.position.x - 4 || currentPosition.x > board.transform.position.x + 4 ||
            currentPosition.y < board.transform.position.y - 4 || currentPosition.y > board.transform.position.y + 4) //out of the board
        {
            return;
        }

        transform.position = currentPosition;
        */
    }

    public void Move(Vector3 position)
    {
        transform.position = position;
        Deselect();
        Vector2Int newBoardPosition = GameManager.Instance.ConvertScreenToBoardCoords(position);
        if (type == Constants.Pawn && isInitialPosition && Mathf.Abs(currentBoardPosition.y - newBoardPosition.y) == 2) // if it's a pawn in initial state, flag it for enpassant
        {
            GameManager.Instance.enPassantViable = gameObject;
        } else
        {
            GameManager.Instance.enPassantViable = null;
        }
        GameManager.Instance.SetText(name + " " + Constants.letters[currentBoardPosition.x] + (currentBoardPosition.y + 1) + " -> " +
            Constants.letters[newBoardPosition.x] + (newBoardPosition.y + 1), player); // set text for current movement
        GameManager.Instance.HighlightAvailableMoves(new List<Vector2>() { currentBoardPosition, newBoardPosition }); // highligh current move
        isInitialPosition = false;
        GameManager.Instance.board.RemovePiece(currentBoardPosition);
        currentBoardPosition = newBoardPosition;
        GameManager.Instance.board.SetPiece(gameObject, currentBoardPosition); // move object on board
        GameManager.Instance.board.SetLocks(GameManager.Instance.GetPieces()); // write down which squares the piece can put in check

        if (type == Constants.Pawn && ((player == Constants.White && newBoardPosition.y == 7) || (player == Constants.Black && newBoardPosition.y == 0))) // if a pawn reaches the end of the boar, promote it to queen
        {
            Promotion();
        }

        int otherPlayer = GameManager.Instance.GetOtherPlayer(player);
        if (GameManager.Instance.board.IsKingUnderCheck(otherPlayer)) // check for check and checkmate
        {
            GameManager.Instance.Check(otherPlayer);

            if (GameManager.Instance.board.CheckForCheckMate(otherPlayer))
            {
                GameManager.Instance.CheckMate(otherPlayer);
            }
        } else
        {
            GameManager.Instance.isInCheck = 0;
        }
    }

    public bool IsValidMove(Vector2Int position)
    {
        Vector2Int move = position - GameManager.Instance.CurrentPiece.GetComponent<Piece>().currentBoardPosition;
        
        if (! GameManager.Instance.IsDirectionValid(move, this)) // check direction from an array of predefined vectors
        {
            return false;
        }
        if (type != Constants.Knight && !GameManager.Instance.board.CanMoveFromTo(currentBoardPosition, position)) //do not pass through figures, except for the knight
        {
            return false;
        }
        if (GameManager.Instance.isInCheck == player && GameManager.Instance.board.TestMoveForCheck(gameObject, position)) // if the move results in check, make it invalid
        {
            return false;
        }

        return true;
    }

    public List<Vector2> GetAvailableMoves(bool free = true)
    {
        List<Vector2> availableMoves = new List<Vector2>();
        int currentType = type == Constants.Pawn && isInitialPosition ? 6 : type - 1; // get piece type, check if it's in initial state for pawns

        if (type == Constants.Pawn && !isInitialPosition) // enpassant - check lest and right for a viable pawn to do enpassant
        {
            Vector2 left = new Vector2(currentBoardPosition.x - 1, currentBoardPosition.y);
            Vector2 right = new Vector2(currentBoardPosition.x + 1, currentBoardPosition.y);
            int y = player == Constants.Black ? currentBoardPosition.y - 1 : currentBoardPosition.y + 1;

            if (GameManager.Instance.board.CoordExists(left) && GameManager.Instance.board.IsFull(left) &&
                GameManager.Instance.board.GetByCoord(left) == GameManager.Instance.enPassantViable)
            {
                availableMoves.Add(new Vector2(left.x, y));
            }
            if (GameManager.Instance.board.CoordExists(right) && GameManager.Instance.board.IsFull(right) &&
                GameManager.Instance.board.GetByCoord(right) == GameManager.Instance.enPassantViable)
            {
                availableMoves.Add(new Vector2(right.x, y));
            }
        }

        for (int i = 0; i < GameManager.Instance.pieceAvailablePos[currentType].GetLength(0); i++) //get all movements
        {
            int distance = GameManager.Instance.pieceAvailablePos[currentType][i, 3];
            int moveType = GameManager.Instance.pieceAvailablePos[currentType][i, 2];
            Vector2 possibleDir = new Vector2(GameManager.Instance.pieceAvailablePos[currentType][i, 0], GameManager.Instance.pieceAvailablePos[currentType][i, 1]);
            if (type == Constants.Pawn && player == Constants.Black) //Black Pawns can only move downwards
            {
                possibleDir *= -1;
            }
            for (int j = 1; j <= distance; j++)
            {
                if (moveType == Constants.MoveToFree && ! free)
                {
                    continue;
                }
                Vector2 newBoardPos = currentBoardPosition + j * possibleDir;
                if (!GameManager.Instance.board.CoordExists(newBoardPos)) //out of bounds
                {
                    break;
                }
                if (GameManager.Instance.board.IsFull(newBoardPos) && moveType == Constants.MoveToFree) // position is taken
                {
                    break;
                }
                GameObject newPositionObj = GameManager.Instance.board.GetByCoord(newBoardPos);
                if ((newPositionObj != null && newPositionObj.GetComponent<Piece>().player == player) || //do not move through own pieces
                    (newPositionObj == null && moveType == Constants.MoveToTaken)) // do not move sideways for pawns
                {
                    break;
                }
                if (GameManager.Instance.board.TestMoveForCheck(gameObject, newBoardPos))
                {
                    continue;
                }
                if (newPositionObj != null && newPositionObj.GetComponent<Piece>().player != player) //stop at first of other color
                {
                    availableMoves.Add(newBoardPos);
                    break;
                }
                
                availableMoves.Add(newBoardPos);
            }
        }

        return availableMoves;
    }

    // get all movements while skipping pawn's forward movement
    public List<Vector2> GetAllPossibleTakingMoves()
    {
        List<Vector2> availableMoves = new List<Vector2>();
        int currentType = type == Constants.Pawn && isInitialPosition ? 6 : type - 1; // get piece type, check if it's in initial state for pawns

        for (int i = 0; i < GameManager.Instance.pieceAvailablePos[currentType].GetLength(0); i++)
        {
            int distance = GameManager.Instance.pieceAvailablePos[currentType][i, 3];
            int moveType = GameManager.Instance.pieceAvailablePos[currentType][i, 2];
            if (moveType == Constants.MoveToFree)
            {
                continue;
            }
            Vector2 possibleDir = new Vector2(GameManager.Instance.pieceAvailablePos[currentType][i, 0], GameManager.Instance.pieceAvailablePos[currentType][i, 1]);
            if (type == Constants.Pawn && player == Constants.Black) //Black Pawns can only move downwards
            {
                possibleDir *= -1;
            }
            for (int j = 1; j <= distance; j++)
            {
                Vector2 newBoardPos = currentBoardPosition + j * possibleDir;
                if (!GameManager.Instance.board.CoordExists(newBoardPos)) //out of bounds
                {
                    break;
                }
                GameObject newPositionObj = GameManager.Instance.board.GetByCoord(newBoardPos);
                if (newPositionObj != null) 
                {
                    if (newPositionObj.GetComponent<Piece>().player == player)//do not move through own pieces
                    {
                        break;
                    }
                    if (newPositionObj.GetComponent<Piece>().player != player) //stop at first of other color
                    {
                        availableMoves.Add(newBoardPos);
                        break;
                    }
                }

                availableMoves.Add(newBoardPos);
            }
        }

        return availableMoves;
    }

    //Attemp to castle, only callable from the king
    private bool AttemptCastling(GameObject rook)
    {
        if (type != Constants.King)
        {
            return false;
        }
        Piece script = rook.GetComponent<Piece>();
        if (!GameManager.Instance.board.CanMoveFromTo(currentBoardPosition, script.currentBoardPosition))
        {
            return false;
        }
        if (!script.isInitialPosition || !isInitialPosition) // either has moved
        {
            return false;
        }

        Vector2 direction = script.currentBoardPosition - currentBoardPosition;
        direction.x =  Mathf.Sign(direction.x) * (direction.x / direction.x);
        Vector2 newPos = currentBoardPosition + 2 * direction;
        script.Move(GameManager.Instance.ConvertBoardToScreenCoords(currentBoardPosition + direction));
        Move(GameManager.Instance.ConvertBoardToScreenCoords(newPos));

        return true;
    }

    //Promote a pawn to a queen when it reaches the end of the board
    void Promotion()
    {
        string color = player == Constants.White ? "White" : "Black";
        GameObject queen = Instantiate(Resources.Load("Prefabs/" + color + "/Queen")) as GameObject;
        queen.transform.position = transform.position;
        queen.GetComponent<Piece>().currentBoardPosition = currentBoardPosition;
        GameManager.Instance.AddPiece(queen);
        GameManager.Instance.board.RemovePieceAt(currentBoardPosition);
        GameManager.Instance.board.SetPiece(queen, currentBoardPosition);
        GameManager.Instance.board.RemoveLocks(gameObject);
        GameManager.Instance.board.SetLocks(GameManager.Instance.GetPieces());
        
    }

    public void Select()
    {
        GameManager.Instance.CurrentPiece = gameObject; //select piece
        gameObject.GetComponent<Renderer>().material.shader = GameManager.Instance.selectedShader; // add outline shader
        List<Vector2> availableMoves = GetAvailableMoves();
        GameManager.Instance.HighlightAvailableMoves(availableMoves);
    }

    public void Deselect()
    {
        gameObject.GetComponent<Renderer>().material.shader = GameManager.Instance.normalShader; //remove outline shader
        GameManager.Instance.RemoveHighlights();
    }
}
