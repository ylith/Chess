using UnityEngine;

public class BoardArray {
    public int[,] boardArray;
    Vector2 size;

    public BoardArray(int i, int j)
    {
        size = new Vector2(i, j);
        boardArray = new int[i, j];
    }

    public void SetArray(int[,] board)
    {
        boardArray = board;
    }

    public int GetIdByCoord(Vector2 boardPos) // get id of object at boardPos
    {
        return boardArray[(int)boardPos.x, (int)boardPos.y];
    }

    public int GetIdByCoord(int i, int j)
    {
        return boardArray[i, j];
    }

    public void SetPiece(GameObject obj, Vector2 currentBoardPosition) // add a piece to the board
    {
        boardArray[(int)currentBoardPosition.x, (int)currentBoardPosition.y] = obj.GetInstanceID();
    }

    public void SetPiece(int id, Vector2 currentBoardPosition)
    {
        boardArray[(int)currentBoardPosition.x, (int)currentBoardPosition.y] = id;
    }

    public void RemovePiece(Vector2 currentBoardPosition) //remove piece from the board
    {
        if (CoordExists(currentBoardPosition))
        {
            boardArray[(int)currentBoardPosition.x, (int)currentBoardPosition.y] = 0;
        }
    }

    public bool CoordExists(Vector2 pos)
    {
        if (pos.x >= size.x || pos.y >= size.y || pos.x < 0 || pos.y < 0)
        {
            return false;
        }

        return true;
    }

    public bool IsFull(Vector2 pos)
    {
        return CoordExists(pos) && boardArray[(int)pos.x, (int)pos.y] != 0;
    }
}
