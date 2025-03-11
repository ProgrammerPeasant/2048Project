using UnityEngine; 
using TMPro;

public class CellView : MonoBehaviour { 
    [Header("UI-элемент для отображения текста (числа)")] 
    [SerializeField] private TMP_Text valueText;

    private Cell cell;
    public void Init(Cell cell)
    {
        this.cell = cell;

        cell.OnValueChanged += UpdateValue;
        cell.OnPositionChanged += UpdatePosition;

        UpdateValue(cell.Value);
        UpdatePosition(cell.Position);
    }

    private void UpdateValue(int newValue)
    {
        // По заданию нужно показывать Math.Pow(Value, 2)
        // Но будьте внимательны: Value — 1 или 2, а отображать нужно 1^2 = 1, 2^2 = 4, и т.д.
        // Если хотите именно «2» и «4», то используйте newValue * 2 и так далее.
        // Ниже — точная реализация по условию (Value^2):
        double displayNumber = Mathf.Pow(2, newValue);
        valueText.text = displayNumber.ToString();
    }

    private void UpdatePosition(Vector2Int newPos)
    {
        // Тут можно вычислить реальный UI-localPosition.
        // Например, если каждый слот 100×100, и (0,0) в левом верхнем углу,
        // а ось Y идёт вниз, можем сделать так:
        float cellSize = 130f; 
        float offsetX = newPos.x * cellSize;
        float offsetY = -newPos.y * cellSize;

        transform.localPosition = new Vector3(offsetX, offsetY, 0f);
    }
}