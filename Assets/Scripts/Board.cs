using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour {
    private BoardArray board = new BoardArray(8, 8);
    private int[] takenByColor = new int[2] { 0, 0 };
    public BoardArray BoardObj { get { return board; } }
    public Dictionary<int, List<Vector2>>[] locks;

    public void Init() {
        List<GameObject> pieces = GameObject.FindGameObjectsWithTag("Piece").ToList();
        foreach (GameObject piece in pieces)
        {
            Piece script = piece.GetComponent<Piece>();
            Vector2Int boardPosition = GameManager.Instance.ConvertScreenToBoardCoords(piece.transform.position);
            board.SetPiece(piece.GetInstanceID(), boardPosition);
            script.currentBoardPosition = boardPosition;
            if (script.type == Constants.King)
            {
                GameManager.Instance.kings[script.player - 1] = piece;
            }
        }
        GameManager.Instance.SetPieces(pieces);
        SetLocks(pieces);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && GameManager.Instance.CurrentPiece != null) //right click deselect piece
        {
            GameManager.Instance.CurrentPiece.GetComponent<Piece>().Deselect();
        }
    }

    public void SetLocks(List<GameObject> pieces)
    {
        locks = new Dictionary<int, List<Vector2>>[2];
        locks[0] = new Dictionary<int, List<Vector2>>();
        locks[1] = new Dictionary<int, List<Vector2>>();

        foreach (GameObject piece in pieces)
        {
            SetLocksForPiece(piece);
        }
    }

    public GameObject GetByCoord(int i, int j)
    {
        return GameManager.Instance.GetPieceById(GetIdByCoord(i, j));
    }

    public GameObject GetByCoord(Vector2 boardPos)
    {
        return GameManager.Instance.GetPieceById(GetIdByCoord(boardPos));
    }

    public int GetIdByCoord(Vector2 boardPos)
    {
        return board.GetIdByCoord(boardPos);
    }

    public int GetIdByCoord(int i, int j)
    {
        return board.GetIdByCoord(i, j);
    }

    public bool CoordExists(int i, int j)
    {
        if (i > 7 || i > 7 || j < 0 || j < 0)
        {
            return false;
        }

        return true;
    }

    public bool CoordExists(Vector2 pos)
    {
        return board.CoordExists(pos);
    }

    public void SetPiece(GameObject obj, Vector2 currentBoardPosition)
    {
        board.SetPiece(obj, currentBoardPosition);
    }

    public void SetPiece(int id, Vector2 currentBoardPosition)
    {
        board.SetPiece(id, currentBoardPosition);
    }

    public void RemovePiece(Vector2 currentBoardPosition)
    {
        if (CoordExists(currentBoardPosition))
        {
            board.RemovePiece(currentBoardPosition);
        }
    }

    public void SetLocksForPiece(GameObject piece)
    {
        Piece script = piece.GetComponent<Piece>();
        List<Vector2> a = script.GetAllPossibleTakingMoves();
        locks[script.player - 1][piece.GetInstanceID()] = a;
    }

    public void RemoveLocks(GameObject piece)
    {
        int instanceId = piece.GetInstanceID();
        Piece script = piece.GetComponent<Piece>();
        if (locks[script.player - 1][instanceId] != null)
        {
            locks[script.player - 1][instanceId].Clear();
        }
    }

    public void RemoveLocks(int instanceId)
    {
        GameObject piece = GameManager.Instance.GetPieceById(instanceId);
        Piece script = piece.GetComponent<Piece>();
        if (locks[script.player - 1][instanceId] != null)
        {
            locks[script.player - 1][instanceId].Clear();
        }
    }

    void OnMouseDown()
    {
        Vector3 clickPos = GameManager.Instance.GetClickCoordinates();
        Vector2Int boardPosition = GameManager.Instance.ConvertScreenToBoardCoords(clickPos);

        if (GameManager.Instance.CurrentPiece == null) // if this is a piece of the current player => select
        {
            if (board.IsFull(boardPosition))
            {
                GetByCoord(boardPosition).GetComponent<Piece>().Select();
            }
        } else if (GameManager.Instance.CurrentPiece.GetComponent<Piece>().IsValidMove(boardPosition)) // if this is not a piece of the current player => attempt move
        {
            Piece script = GameManager.Instance.CurrentPiece.GetComponent<Piece>();
            Vector3 movePosition = GameManager.Instance.ConvertBoardToScreenCoords(boardPosition);
            script.Move(movePosition);
            GameManager.Instance.PassTurn();
        }
    }

    public bool IsFull(Vector2 pos)
    {
        return board.IsFull(pos);
    }
    
    public void RemovePieceAt(Vector2Int boardPosition)
    {
        if (! IsFull(boardPosition))
        {
            return;
        }
        GameObject piece = GetByCoord(boardPosition);
        Piece script = piece.GetComponent<Piece>();
        if (script.type == Constants.King)
        {
            GameManager.Instance.GameOver();
        }
        float initialPosX = script.player == Constants.White ? -0.5f : 8.5f;
        float initialPosY = takenByColor[script.player - 1] >= 8 ? takenByColor[script.player - 1] % 8 : takenByColor[script.player - 1];
        int sign = script.player == Constants.White ? -1 : 1;
        piece.transform.position = new Vector3(initialPosX + sign * Mathf.Floor(takenByColor[script.player - 1] / 8), 0.5f + initialPosY, 0.1f);
        takenByColor[script.player - 1] += 1;
        piece.GetComponent<Collider>().enabled = false;
        RemoveLocks(piece);
        RemovePiece(boardPosition);
        GameManager.Instance.RemovePiece(piece.GetInstanceID());
        //Destroy(board[position.x, position.y]);
    }

    // Check if the king is in check
    public bool IsKingUnderCheck(int player)
    {
        int otherPlayer = GameManager.Instance.GetOtherPlayer(player);
        Vector2 kingPos = GameManager.Instance.kings[player - 1].GetComponent<Piece>().currentBoardPosition;

        foreach (KeyValuePair<int, List<Vector2>> item in locks[otherPlayer - 1])
        {
            if (item.Value.Contains(kingPos))
            {
                return true;
            }
        }

        return false;
    }

    public bool CheckForCheckMate(int player)
    {
        List<GameObject> playerPieces = GameManager.Instance.GetPiecesByColor(player);

        foreach (GameObject piece in playerPieces)
        {
            Piece script = piece.GetComponent<Piece>();
            List<Vector2> availableMoves = script.GetAvailableMoves();

            foreach (Vector2 move in availableMoves)
            {
                if (! TestMoveForCheck(piece, move)) // if we can break check, return
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool TestMoveForCheck(GameObject piece, Vector2 pos)
    {
        Dictionary<int, List<Vector2>>[] tempLocks = (Dictionary<int, List<Vector2>>[])locks.Clone();
        List<GameObject> tempPieces = new List<GameObject>(GameManager.Instance.GetPieces());
        BoardArray tempBoard = new BoardArray(8, 8);
        tempBoard.boardArray = (int[,])board.boardArray.Clone();
        Piece script = piece.GetComponent<Piece>();
        Vector2Int currentPos = script.currentBoardPosition;
        if (GetIdByCoord(pos) != 0)
        {
            tempPieces.Remove(GetByCoord(pos));
        }
        board.RemovePiece(script.currentBoardPosition);
        board.SetPiece(piece, pos);
        script.currentBoardPosition = new Vector2Int((int)pos.x, (int)pos.y);
        SetLocks(tempPieces);
        //reset
        bool isInCheck = IsKingUnderCheck(script.player);
        locks = (Dictionary<int, List<Vector2>>[])tempLocks.Clone();
        board.boardArray = (int[,])tempBoard.boardArray.Clone();
        script.currentBoardPosition = currentPos;

        return isInCheck;
    }

    public bool CanMoveFromTo(Vector2Int start, Vector2Int end)
    {
        Vector2Int move = end - start;
        Vector2Int dir = Vector2Int.zero;
        dir.x = move.x != 0 ? (int)Mathf.Sign(move.x): 0;
        dir.y = move.y != 0 ? (int)Mathf.Sign(move.y) : 0;

        for (int i = 0; i < Mathf.Max(Mathf.Abs(move.x) - 1, Mathf.Abs(move.y) - 1); i++)
        {
            start += dir;
            if (!CoordExists(start) || board.IsFull(start))
            {
                return false;
            }
        }

        return true;
    }
}
