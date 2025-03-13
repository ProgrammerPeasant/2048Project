using TMPro; 
using UnityEngine; 
using UnityEngine.UI;

public class CellView : MonoBehaviour { [Header("отображение числа")] 
    [SerializeField] private TMP_Text valueText;
    [Header("изменение цвета фона")]
    [SerializeField] private Image bgImage;

    [Header("интерполяция")]
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.yellow;

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
        double displayNumber = Mathf.Pow(2, newValue);
        valueText.text = displayNumber.ToString();

        float t = Mathf.InverseLerp(1, 11, newValue);
        if (bgImage != null)
            bgImage.color = Color.Lerp(startColor, endColor, t);
    }

    // private void UpdatePosition(Vector2Int newPos)
    // {
    //     //Vector3 gridStartPosition = new Vector3(65, -65, 0);
    //     
    //     float cellSize = 130f;
    //     float globalX =  newPos.x * cellSize;
    //     float globalY =  -newPos.y * cellSize;        
    //     transform.position = new Vector3(globalX, globalY, 0f);
    // }
    
    public void UpdatePosition(Vector2Int newPos)
    {
        float cellSize = 130f;
        float globalX = newPos.x * cellSize;
        float globalY = -newPos.y * cellSize;

         Debug.Log($"Updating position to: {new Vector3(globalX, globalY, 0f)} for cell at {newPos}");

        transform.position = new Vector3(globalX, globalY, 0f);
    }
}