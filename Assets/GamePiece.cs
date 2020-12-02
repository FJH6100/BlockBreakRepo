using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    private int x;
    public int X
    {
        get { return x; }
        set
        {
            if (IsMovable())
                x = value;
        }
    }

    private int y;
    public int Y
    {
        get { return y; }
        set
        {
            if (IsMovable())
                y = value;
        }
    }

    private VVGrid.PieceType type;
    public VVGrid.PieceType Type
    {
        get { return type; }
    }

    private VVGrid grid;
    public VVGrid GridRef
    {
        get { return grid; }
    }

    private MovablePiece movable;
    public MovablePiece Movable
    {
        get { return movable; }
    }

    private ColorPiece color;
    public ColorPiece Color
    {
        get { return color; }
    }

    private ClearablePiece clearable;
    public ClearablePiece Clearable
    {
        get { return clearable; }
    }

    public void Init(int _x, int _y, VVGrid _grid, VVGrid.PieceType _type)
    {
        x = _x;
        y = _y;
        grid = _grid;
        type = _type;
    }

    void Awake()
    {
        movable = GetComponent<MovablePiece>();
        color = GetComponent<ColorPiece>();
        clearable = GetComponent<ClearablePiece>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        grid.PressPiece(this);
    }

    private void OnMouseUp()
    {
        grid.ReleasePiece();
    }

    public bool IsMovable()
    {
        return movable != null;
    }

    public bool IsColored()
    {
        return color != null;
    }

    public bool IsClearable()
    {
        return clearable != null;
    }
}
