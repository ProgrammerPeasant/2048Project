using TMPro; 
using UnityEngine; 
using UnityEngine.UI;

public class CellView : MonoBehaviour 
{ 
    [Header("отображение числа")] 
    [SerializeField] private TMP_Text valueText;
    [Header("изменение цвета фона")]
    [SerializeField] private Image bgImage;

    [Header("интерполяция")]
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.yellow;

    private Cell cell;
    private GameField gameField;

    public void Init(Cell cell, GameField gameField)
    {
        this.cell = cell;
        this.gameField = gameField;
        
        cell.OnValueChanged += UpdateValue;
        cell.OnPositionChanged += UpdatePosition;
        UpdateValue(cell.Value);
        UpdatePosition(cell.Position);
    }

    private void UpdateValue(int newValue)
    {
        double displayNumber = Mathf.Pow(2, newValue);
        valueText.text = displayNumber.ToString();

        float t = Mathf.InverseLerp(1, 11, newValue);
        if (bgImage != null)
            bgImage.color = Color.Lerp(startColor, endColor, t);
    }
    
    public void UpdatePosition(Vector2Int newPos)
    {
        int positionIndex = newPos.y * gameField.GridWidth + newPos.x;
        
        if (positionIndex >= 0 && positionIndex < gameField.CellPositions.Count)
        {
            Vector3 targetPosition = gameField.CellPositions[positionIndex];
            transform.position = targetPosition;
        }
        else
        {
            Debug.LogError($"Invalid position index: {positionIndex} for cell at {newPos}");
        }
    }
}