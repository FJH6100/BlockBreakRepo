using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VVGrid : MonoBehaviour
{

    public enum PieceType
    {
        EMPTY,
        NORMAL,
        ROW_CLEAR,
        COLUMN_CLEAR,
        ALL_CLEAR,
        COUNT
    };
    public int xDim;
    public int yDim;
    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    }
    private Dictionary<PieceType, GameObject> piecePrefabDict;
    public PiecePrefab[] piecePrefabs;
    private GamePiece[,] pieces;
    public float fillTime = .2f;
    private bool inverse = false;

    private GamePiece pressedPiece;
    // Start is called before the first frame update
    void Start()
    {
        piecePrefabDict = new Dictionary<PieceType, GameObject>();
        foreach (PiecePrefab p in piecePrefabs)
        {
            if (!piecePrefabDict.ContainsKey(p.type))
                piecePrefabDict.Add(p.type, p.prefab);
        }

        pieces = new GamePiece[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                SpawnNewPiece(x, y, PieceType.NORMAL);
            }
        }
    }

    public bool ClearPiece(int x, int y)
    {
        if (pieces[x, y].IsClearable())
        {
            pieces[x, y].Clearable.Clear();
            SpawnNewPiece(x, y, PieceType.EMPTY);
            //ClearObstacles(x, y);
            return true;
        }
        return false;
    }

    public IEnumerator Fill()
    {
            while (FillStep())
            {
                inverse = !inverse;
                yield return new WaitForSeconds(fillTime);
            }
    }

    public bool FillStep()
    {
        bool movedPiece = false;
        for (int y = yDim - 2; y >= 0; y--)
        {
            for (int loopX = 0; loopX < xDim; loopX++)
            {
                int x = loopX;
                if (inverse)
                    x = xDim - 1 - loopX;
                GamePiece piece = pieces[x, y];
                if (piece.IsMovable())
                {
                    GamePiece pieceBelow = pieces[x, y + 1];
                    if (pieceBelow.Type == PieceType.EMPTY)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.Movable.Move(x, y + 1, fillTime);
                        pieces[x, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                    }
                    else
                    {
                        for (int diag = -1; diag <= 1; diag++)
                        {
                            if (diag != 0)
                            {
                                int diagX = x + diag;

                                if (inverse)
                                {
                                    diagX = x - diag;
                                }

                                if (diagX >= 0 && diagX < xDim)
                                {
                                    GamePiece diagonalPiece = pieces[diagX, y + 1];

                                    if (diagonalPiece.Type == PieceType.EMPTY)
                                    {
                                        bool hasPieceAbove = true;

                                        for (int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            GamePiece pieceAbove = pieces[diagX, aboveY];

                                            if (pieceAbove.IsMovable())
                                            {
                                                break;
                                            }
                                            else if (!pieceAbove.IsMovable() && pieceAbove.Type != PieceType.EMPTY)
                                            {
                                                hasPieceAbove = false;
                                                break;
                                            }
                                        }

                                        if (!hasPieceAbove)
                                        {
                                            Destroy(diagonalPiece.gameObject);
                                            piece.Movable.Move(diagX, y + 1, fillTime);
                                            pieces[diagX, y + 1] = piece;
                                            SpawnNewPiece(x, y, PieceType.EMPTY);
                                            movedPiece = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        for (int x = 0; x < xDim; x++)
        {
            GamePiece pieceBelow = pieces[x, 0];
            if (pieceBelow.Type == PieceType.EMPTY)
            {
                PieceType type = PieceType.NORMAL;
                Destroy(pieceBelow.gameObject);
                GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type], GetWorldPosition(x, -1, -1), Quaternion.identity);
                newPiece.transform.parent = transform;

                pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                pieces[x, 0].Init(x, -1, this, type);
                pieces[x, 0].Movable.Move(x, 0, fillTime);
                pieces[x, 0].Color.SetColor(Random.Range(0, pieces[x, 0].Color.numColors));
                movedPiece = true;
            }
        }
        return movedPiece;
    }

    public void ClearPiece(GamePiece piece)
    {
        if (piece.IsMovable())
        {
            List<GamePiece> adjacentPieces = new List<GamePiece>();
            adjacentPieces.Add(piece);
            if (piece.Type == PieceType.NORMAL)
                adjacentPieces = GetMatch(piece, adjacentPieces);
            if (adjacentPieces.Count > 1)
            {
                PieceType specialPieceType = PieceType.COUNT;
                if (adjacentPieces.Count == 5)
                    specialPieceType = PieceType.ROW_CLEAR;
                else if (adjacentPieces.Count == 6)
                    specialPieceType = PieceType.COLUMN_CLEAR;
                else if (adjacentPieces.Count >= 7)
                    specialPieceType = PieceType.ALL_CLEAR;
                foreach (GamePiece p in adjacentPieces)
                {
                    ClearPiece(p.X, p.Y);
                }
                if (specialPieceType != PieceType.COUNT)
                {
                    Destroy(pieces[piece.X, piece.Y]);
                    GamePiece newPiece = SpawnNewPiece(piece.X, piece.Y, specialPieceType);

                    if ((specialPieceType == PieceType.ROW_CLEAR || specialPieceType == PieceType.COLUMN_CLEAR || specialPieceType == PieceType.ALL_CLEAR)
                        && newPiece.IsColored() && adjacentPieces[0].IsColored())
                        newPiece.GetComponent<SpriteRenderer>().color = adjacentPieces[0].GetComponent<SpriteRenderer>().color;
                }

                pressedPiece = null;

                StartCoroutine(Fill());
            }
            if (piece.Type == PieceType.ROW_CLEAR)
                ClearRow(piece.Y);
            if (piece.Type == PieceType.COLUMN_CLEAR)
                ClearColumn(piece.X);
            if (piece.Type == PieceType.ALL_CLEAR)
                ClearColor(piece.GetComponent<SpriteRenderer>().color);
        }
    }

    public void PressPiece(GamePiece piece)
    {
        pressedPiece = piece;
    }
    public void ReleasePiece()
    {
        ClearPiece(pressedPiece);
    }

    public List<GamePiece> GetMatch(GamePiece piece, List<GamePiece> neighbors)
    {
        if (piece.IsColored())
        {
            List<GamePiece> newPieces = new List<GamePiece>();
            //North
            if (IsValid(piece.X, piece.Y - 1, piece.GetComponent<SpriteRenderer>().color))
            {
                if (!neighbors.Contains(pieces[piece.X, piece.Y - 1]))
                    newPieces.Add(pieces[piece.X, piece.Y - 1]);
            }
            //East
            if (IsValid(piece.X + 1, piece.Y, piece.GetComponent<SpriteRenderer>().color))
            {
                if (!neighbors.Contains(pieces[piece.X + 1, piece.Y]))
                    newPieces.Add(pieces[piece.X + 1, piece.Y]);
            }
            //South
            if (IsValid(piece.X, piece.Y + 1, piece.GetComponent<SpriteRenderer>().color))
            {
                if (!neighbors.Contains(pieces[piece.X, piece.Y + 1]))
                    newPieces.Add(pieces[piece.X, piece.Y + 1]);
            }
            //West
            if (IsValid(piece.X - 1, piece.Y, piece.GetComponent<SpriteRenderer>().color))
            {
                if (!neighbors.Contains(pieces[piece.X - 1, piece.Y]))
                    newPieces.Add(pieces[piece.X - 1, piece.Y]);
            }
            if (newPieces.Count != 0)
            {
                neighbors.AddRange(newPieces);
                foreach (GamePiece p in newPieces)
                {
                    List<GamePiece> n = GetMatch(p, neighbors);  
                }
            }
        }

        return neighbors;
    }

    bool IsValid(int value1, int value2, Color thisColor)
    {
        if ((value1 > -1 && value1 < xDim) && (value2 > -1 && value2 < yDim))
        {
            if (pieces[value1, value2].GetComponent<SpriteRenderer>().color.Equals(thisColor))
                return true;
            return false;
        }
        else
            return false;
    }

    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        Quaternion rot;
        if (type == PieceType.ROW_CLEAR)
            rot = Quaternion.Euler(0, 0, 90);
        else
            rot = Quaternion.identity;
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type], GetWorldPosition(x, y, -1), rot);
        newPiece.name = "Piece(" + x + "," + y + ")";
        newPiece.transform.parent = transform;

        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x, y, this, type);
        if (pieces[x,y].IsColored())
            pieces[x, y].Color.SetColor(Random.Range(0, pieces[x, 0].Color.numColors));

        return pieces[x, y];
    }

    public Vector3 GetWorldPosition(int x, int y, float z)
    {
        return new Vector3((transform.position.x - xDim / 2f + x + .5f)*.6f, (transform.position.y + yDim / 2f - y - .5f)*.6f, z);
    }

    public void ClearRow(int row)
    {
        Debug.Log(row);
        for (int i = 0; i < xDim; i++)
        {
            if (pieces[row,i].IsClearable())
            {
                ClearPiece(i,row);
            }
        }
        pressedPiece = null;
        StartCoroutine(Fill());
    }

    public void ClearColumn(int column)
    {
        Debug.Log(column);
        for (int i = 0; i < yDim; i++)
        {
            if (pieces[i, column].IsClearable())
            {
                ClearPiece(column,i);
            }
        }
        pressedPiece = null;
        StartCoroutine(Fill());
    }
    
    public void ClearColor(Color thisColor)
    {
        for (int y = 0; y < yDim; y++)
        {
            for (int x = 0; x < xDim; x++)
            {
                if (pieces[x,y].IsColored() && pieces[x, y].GetComponent<SpriteRenderer>().color.Equals(thisColor))
                {
                    ClearPiece(x, y);
                }
            }
        }
        pressedPiece = null;
        StartCoroutine(Fill());
    }
}
