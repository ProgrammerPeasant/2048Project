using System; 
using UnityEngine;

public class Cell
{
    public event Action<int> OnValueChanged;
    public event Action<Vector2Int> OnPositionChanged;
    private Vector2Int _position;
    private int _value;

    public Vector2Int Position
    {
        get => _position;
        set
        {
            _position = value;
            OnPositionChanged?.Invoke(_position);
        }
    }

    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            OnValueChanged?.Invoke(_value);
        }
    }

    public Cell(Vector2Int pos, int val)
    {
        _position = pos;
        _value = val;
    }
}