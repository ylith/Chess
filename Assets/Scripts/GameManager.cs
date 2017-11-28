using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : ScriptableObject {
    private int currentPlayer = 1;
    private GameObject currentPiece;
    private static GameManager instance;
    private Dictionary<int, GameObject> pieces = new Dictionary<int, GameObject>();

    public bool isRunning = true;
    public GameObject selectorObj; // piece selector plane
    public Board board;
    public GameObject enPassantViable;
    public Shader selectedShader;
    public Shader normalShader;
    public Shader highlightShader;
    public Text whiteText;
    public Text blackText;
    public Text gameOverText;
    public GameObject[] kings = new GameObject[2];
    public int isInCheck = 2;
    public bool clearText = true;
    public bool isAi;
    public AIBehaviour ai;

    //Each row has a piece's possible moves, condition for move and max distance
    public int [][,] pieceAvailablePos = new int[7][,] {
        new int [,] { { 0, 1, Constants.MoveToFree, 1}, { 1, 1, Constants.MoveToTaken, 1}, { -1, 1, Constants.MoveToTaken, 1} }, //pawn
        new int [,] { { 0, 1, Constants.MoveAny, 7}, { 1, 0, Constants.MoveAny, 7}, { -1, 0, Constants.MoveAny, 7}, { 0, -1, Constants.MoveAny, 7} }, //rook
        new int [,] { { 1, 2, Constants.MoveAny, 1}, { 2, 1, Constants.MoveAny, 1}, { -1, 2, Constants.MoveAny, 1}, { 2, -1, Constants.MoveAny, 1},
            { -2, -1, Constants.MoveAny, 1}, { -1, -2, Constants.MoveAny, 1}, { -2, 1, Constants.MoveAny, 1}, { 1, -2, Constants.MoveAny, 1} }, //knight
        new int [,] { { 1, 1, Constants.MoveAny, 7}, { 1, -1, Constants.MoveAny, 7}, { -1, 1, Constants.MoveAny, 7}, { -1, -1, Constants.MoveAny, 7} }, //bishop
        new int [,] { { 0, 1, Constants.MoveAny, 7}, { 1, 0, Constants.MoveAny, 7}, { -1, 0, Constants.MoveAny, 7}, { 0, -1, Constants.MoveAny, 7},
            { 1, 1, Constants.MoveAny, 7}, { 1, -1, Constants.MoveAny, 7}, { -1, 1, Constants.MoveAny, 7}, { -1, -1, Constants.MoveAny, 7} }, //queen
        new int [,] { { 0, 1, Constants.MoveAny, 1}, { 1, 0, Constants.MoveAny, 1}, { -1, 0, Constants.MoveAny, 1}, { 0, -1, Constants.MoveAny, 1}, { 1, 1, Constants.MoveAny, 1},
            { 1, -1, Constants.MoveAny, 1}, { -1, 1, Constants.MoveAny, 1}, { -1, -1, Constants.MoveAny, 1} }, //king
        new int [,] { { 0, 1, Constants.MoveToFree, 2}, { 1, 1, Constants.MoveToTaken, 1}, { -1, 1, Constants.MoveToTaken, 1} }, //pawn move from initial position
    };

    public GameObject CurrentPiece { get { return currentPiece; } set { currentPiece = value; } }
    public static GameManager Instance {
        get {
            if (GameManager.instance == null)
            {
                GameManager.instance = ScriptableObject.CreateInstance<GameManager>();
            }

            return GameManager.instance;
        }
    }

    public int CurrentPlayer { get { return currentPlayer; } set { currentPlayer = value; } }

    public void PassTurn()
    {
        if (! isRunning)
        {
            return;
        }
        currentPiece = null;
        CurrentPlayer = CurrentPlayer == Constants.White ? Constants.Black : Constants.White;
        if (clearText)
        {
            SetText("", CurrentPlayer);
        }
        clearText = true;

        if (isAi && CurrentPlayer == Constants.Black)
        {
            ai.NextMove();
        }
    }

    // get coordinates of click
    public Vector3 GetClickCoordinates()
    {
        Vector3 currentScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.1f);
        Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenPoint);
        currentPosition.z = -0.1f;

        return currentPosition;
    }

    // transform coordinates to board position coordinates
    public Vector2Int ConvertScreenToBoardCoords(Vector3 coords)
    {
        return new Vector2Int((int)coords.x, (int)coords.y);
    }

    // transform board coordinates to screen coordinates
    public Vector3 ConvertBoardToScreenCoords(Vector2 position)
    {
        return new Vector3(position.x + 0.5f, position.y + 0.5f, -0.1f);
    }

    // keep all pieces in a dictionary by instance ID
    public void SetPieces(List<GameObject> pieceArray)
    {
        foreach (GameObject piece in pieceArray)
        {
            pieces[piece.GetInstanceID()] = piece;
        }
    }

    public void AddPiece(GameObject obj)
    {
        pieces[obj.GetInstanceID()] = obj;
    }

    public void RemovePiece(int id)
    {
        if (pieces.ContainsKey(id))
        {
            pieces.Remove(id);
        }
    }

    public List<GameObject> GetPiecesByColor(int color)
    {
        List<GameObject> objects = new List<GameObject>();

        foreach (KeyValuePair<int, GameObject> item in pieces)
        {
            if (item.Value.GetComponent<Piece>().player == color)
            {
                objects.Add(item.Value);
            }
        }

        return objects;
    }

    public GameObject GetPieceById(int id)
    {
        GameObject piece;
        if (pieces.TryGetValue(id, out piece))
        {
            return piece;
        }

        return null;
    }

    public List<GameObject> GetPieces()
    {
        return pieces.Values.ToList();
    }

    // check vector for validity based on predefined values for each piece
    public bool IsDirectionValid(Vector2 dir, Piece piece)
    {
        Vector2 endPos = piece.currentBoardPosition + dir;
        bool posIsTaken = board.IsFull(endPos);
        int type = piece.type == Constants.Pawn && piece.isInitialPosition ? 6 : piece.type - 1; // get piece type, check if it's in initial state for pawns
        
        if (piece.type == Constants.Pawn && Mathf.Abs(dir.x) == 1 && board.IsFull(new Vector2(endPos.x, piece.currentBoardPosition.y)) && // en passant check
            board.GetByCoord(new Vector2(endPos.x, piece.currentBoardPosition.y)) == enPassantViable)
        {
            board.RemovePieceAt(new Vector2Int((int)(endPos.x), piece.currentBoardPosition.y));
            return true;
        }

        for (int i = 0; i < pieceAvailablePos[type].GetLength(0); i++)
        {
            if ((!posIsTaken && pieceAvailablePos[type][i, 2] == Constants.MoveToTaken) || (posIsTaken && pieceAvailablePos[type][i, 2] == Constants.MoveToFree))
            {
                continue;
            }

            Vector2 possibleDir = new Vector2(pieceAvailablePos[type][i, 0], pieceAvailablePos[type][i, 1]);
            if (piece.type == Constants.Pawn && GameManager.Instance.CurrentPlayer == Constants.Black) //Black Pawns can only move downwards
            {
                possibleDir *= -1;
            }
            if (possibleDir.normalized == dir.normalized && dir.magnitude / possibleDir.magnitude <= pieceAvailablePos[type][i, 3]) // check direction and size of vector of direction
            {
                return true;
            }
        }

        return false;
    }

    public void Check(int player)
    {
        SetText("Check", player);
        clearText = false;
        isInCheck = player;
    }

    public void CheckMate(int player)
    {
        SetText("Checkmate", player);
        clearText = false;
        isInCheck = player;
        GameOver();
    }

    public void GameOver()
    {
        Debug.Log("Game over");
        board.GetComponent<Collider>().enabled = false;
        isRunning = false;
        gameOverText.text = "Player" + currentPlayer + " wins!";
    }

    public int GetOtherPlayer(int player)
    {
        return player % 2 + 1;
    }

    public void SetText(string text, int type)
    {
        if (type == Constants.White)
        {
            whiteText.text = text;
        } else
        {
            blackText.text = text;
        }
    }

    // Highlight movement for each selected piece
    public void HighlightAvailableMoves(List<Vector2> availableMoves)
    {
        if (availableMoves.Count == 0)
        {
            RemoveHighlights();
            return;
        }

        List<Vector4> segments = new List<Vector4>();
        for (int i = 0; i < availableMoves.Count; i++)
        {
            segments.Add(new Vector4(1 - (availableMoves[i].x + 1) * 0.125f, 1 - availableMoves[i].x * 0.125f,
                1 - (availableMoves[i].y + 1) * 0.125f, 1 - availableMoves[i].y * 0.125f));
        }

        Material mat = selectorObj.GetComponent<Renderer>().material;
        mat.shader = highlightShader;
        mat.SetVectorArray("_Segments", segments);
    }

    public void RemoveHighlights()
    {
        Material mat = selectorObj.GetComponent<Renderer>().material;
        mat.shader = null;
    }

    private void OnApplicationQuit()
    {
        GameManager.instance = null;
    }
}
