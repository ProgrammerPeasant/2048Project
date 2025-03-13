using System; 
using System.Collections.Generic; 
using System.IO; 
using System.Runtime.Serialization.Formatters.Binary; 
using UnityEngine; 
using TMPro;

public class GameField : MonoBehaviour
{
    [Header("Параметры игрового поля")] [SerializeField]
    private int width = 4;

    [SerializeField] private int height = 4;

    [Header("Ссылки на объекты из сцены")] [SerializeField]
    private Transform gridObject;

    [Header("Ссылки на префаб и контейнер для клеток")] [SerializeField]
    private CellView cellViewPrefab;

    [SerializeField] private Transform cellsParent;

    [Header("UI Score")] [SerializeField] private TMP_Text scoreText;
    [Header("UI High score")] [SerializeField] private TMP_Text highScoreText;

    private List<Cell> cells = new List<Cell>();

    private List<Vector2> cellPositions = new List<Vector2>();

    private Dictionary<Cell, CellView> cellViews = new Dictionary<Cell, CellView>();

    private int bestScore = 0;
    private string saveFileName = "saveGame.dat";

    private void Awake()
    {
        GetCellPositionsFromScene();
        LoadBestScore();
        if (!LoadGame())
        {
            CreateNewCell();
            CreateNewCell();
            UpdateScore();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

// Считываю позиции из ячеек в Grid
    private void GetCellPositionsFromScene()
    {
        cellPositions.Clear();

        if (gridObject == null)
        {
            Debug.LogError("Объект 'Grid' не назначен");
            return;
        }

        foreach (Transform rowTransform in gridObject)
        {
            foreach (Transform cellTransform in rowTransform)
            {
                cellPositions.Add(cellTransform.position);
            }
        }

        if (cellPositions.Count == 0)
            Debug.LogWarning("Не найдено ни одного объекта 'Cell' в иерархии под 'Grid'");
        else
            Debug.Log("Найдено позиций Cell: " + cellPositions.Count);
    }

// Находит случайную незанятую позицию
    public Vector2Int GetEmptyPosition()
    {
        List<Vector2Int> freePositions = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool occupied = false;
                foreach (var cell in cells)
                {
                    if (cell.Position.x == x && cell.Position.y == y)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (!occupied)
                    freePositions.Add(new Vector2Int(x, y));
            }
        }

        if (freePositions.Count == 0)
            return new Vector2Int(-1, -1);

        int randIndex = UnityEngine.Random.Range(0, freePositions.Count);
        return freePositions[randIndex];
    }

// Создаёт новую клетку с вероятностью 20% значение 4, иначе 2
    public void CreateNewCell()
    {
        Vector2Int emptyPos = GetEmptyPosition();
        if (emptyPos.x < 0 || emptyPos.y < 0)
        {
            Debug.LogWarning("Нет свободных позиций на поле!");
            return;
        }

        int positionIndex = emptyPos.y * width + emptyPos.x;
        if (positionIndex < 0 || positionIndex >= cellPositions.Count)
        {
            Debug.LogError("Неверный индекс позиции!");
            return;
        }

        Vector3 spawnPosition = cellPositions[positionIndex];
        int value = (UnityEngine.Random.value < 0.8f) ? 1 : 2; // 1 => 2, 2 => 4
        Cell newCell = new Cell(emptyPos, value);
        cells.Add(newCell);

        CellView view = Instantiate(cellViewPrefab, cellsParent);
        view.Init(newCell);
        view.transform.position = spawnPosition;
        cellViews[newCell] = view;
    }

    private Cell GetCellAt(Vector2Int pos)
    {
        foreach (Cell cell in cells)
        {
            if (cell.Position == pos)
                return cell;
        }

        return null;
    }

    public void Move(Vector2Int direction)
    {
        bool moved = false;
        List<Cell> cellsToRemove = new List<Cell>();

        // left/right
        if (direction == Vector2Int.left || direction == Vector2Int.right)
        {
            for (int row = 0; row < height; row++)
            {
                List<Cell> line = new List<Cell>();
                if (direction == Vector2Int.left)
                {
                    for (int col = 0; col < width; col++)
                    {
                        Cell cell = GetCellAt(new Vector2Int(col, row));
                        if (cell != null)
                            line.Add(cell);
                    }
                }
                else // вправо
                {
                    for (int col = width - 1; col >= 0; col--)
                    {
                        Cell cell = GetCellAt(new Vector2Int(col, row));
                        if (cell != null)
                            line.Add(cell);
                    }
                }

                List<Cell> mergedLine = new List<Cell>();
                int i = 0;
                while (i < line.Count)
                {
                    if (i < line.Count - 1 && line[i].Value == line[i + 1].Value)
                    {
                        line[i].Value++;
                        mergedLine.Add(line[i]);
                        cellsToRemove.Add(line[i + 1]);
                        i += 2;
                        moved = true;
                    }
                    else
                    {
                        mergedLine.Add(line[i]);
                        i++;
                    }
                }

                // сдвиг к краю
                if (direction == Vector2Int.left)
                {
                    for (int col = 0; col < mergedLine.Count; col++)
                    {
                        Vector2Int newPos = new Vector2Int(col, row);
                        if (mergedLine[col].Position != newPos)
                        {
                            mergedLine[col].Position = newPos;
                            moved = true;
                        }
                    }
                }
                else // вправо
                {
                    for (int index = 0; index < mergedLine.Count; index++)
                    {
                        Vector2Int newPos = new Vector2Int(width - 1 - index, row);
                        if (mergedLine[index].Position != newPos)
                        {
                            mergedLine[index].Position = newPos;
                            moved = true;
                        }
                    }
                }
            }
        }
        // up/down
        else if (direction == Vector2Int.up || direction == Vector2Int.down)
        {
            for (int col = 0; col < width; col++)
            {
                List<Cell> line = new List<Cell>();
                if (direction == Vector2Int.up)
                {
                    for (int row = 0; row < height; row++)
                    {
                        Cell cell = GetCellAt(new Vector2Int(col, row));
                        if (cell != null)
                            line.Add(cell);
                    }
                }
                else // вниз
                {
                    for (int row = height - 1; row >= 0; row--)
                    {
                        Cell cell = GetCellAt(new Vector2Int(col, row));
                        if (cell != null)
                            line.Add(cell);
                    }
                }

                List<Cell> mergedLine = new List<Cell>();
                int i = 0;
                while (i < line.Count)
                {
                    if (i < line.Count - 1 && line[i].Value == line[i + 1].Value)
                    {
                        line[i].Value++;
                        mergedLine.Add(line[i]);
                        cellsToRemove.Add(line[i + 1]);
                        i += 2;
                        moved = true;
                    }
                    else
                    {
                        mergedLine.Add(line[i]);
                        i++;
                    }
                }

                if (direction == Vector2Int.up)
                {
                    for (int row = 0; row < mergedLine.Count; row++)
                    {
                        Vector2Int newPos = new Vector2Int(col, row);
                        if (mergedLine[row].Position != newPos)
                        {
                            mergedLine[row].Position = newPos;
                            moved = true;
                        }
                    }
                }
                else // вниз
                {
                    for (int index = 0; index < mergedLine.Count; index++)
                    {
                        Vector2Int newPos = new Vector2Int(col, height - 1 - index);
                        if (mergedLine[index].Position != newPos)
                        {
                            mergedLine[index].Position = newPos;
                            moved = true;
                        }
                    }
                }
            }
        }

        foreach (Cell cell in cellsToRemove)
        {
            if (cellViews.ContainsKey(cell))
            {
                Destroy(cellViews[cell].gameObject);
                cellViews.Remove(cell);
            }

            cells.Remove(cell);
        }

        if (moved)
        {
            UpdateScore();
            CreateNewCell();
            CheckGameOver();
        }
    }
    
    private void UpdateScore()
    {
        highScoreText.text = "Highest " + bestScore;
        int score = 0;
        foreach (Cell cell in cells)
        {
            score += (int)Mathf.Pow(2, cell.Value);
        }

        if (scoreText != null)
            scoreText.text = "Current " + score;
    }

    private void CheckGameOver()
    {
        if (GetEmptyPosition().x != -1)
            return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = GetCellAt(new Vector2Int(x, y));
                if (cell == null) continue;
                Vector2Int[] dirs = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
                foreach (Vector2Int dir in dirs)
                {
                    Cell neighbor = GetCellAt(new Vector2Int(x + dir.x, y + dir.y));
                    if (neighbor != null && neighbor.Value == cell.Value)
                        return;
                }
            }
        }

        Debug.Log("Game Over!");

        int currentScore = 0;
        foreach (Cell cell in cells)
        {
            currentScore += (int)Mathf.Pow(2, cell.Value);
        }

        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            SaveBestScore();
        }

        ResetGame();
    }

    private void ResetGame()
    {
        foreach (var kvp in cellViews)
        {
            Destroy(kvp.Value.gameObject);
        }

        cellViews.Clear();
        cells.Clear();
        UpdateScore();
        CreateNewCell();
        CreateNewCell();
    }

    private void SaveBestScore()
    {
        string path = Application.persistentDataPath + "/bestScore.dat";
        File.WriteAllText(path, bestScore.ToString());
    }

    private void LoadBestScore()
    {
        string path = Application.persistentDataPath + "/bestScore.dat";
        if (File.Exists(path))
        {
            string scoreStr = File.ReadAllText(path);
            int.TryParse(scoreStr, out bestScore);
        }
    }

    [Serializable]
    class SaveData
    {
        public List<CellData> cells;
        public int bestScore;
    }

    [Serializable]
    class CellData
    {
        public int x;
        public int y;
        public int value;
    }

    private void SaveGame()
    {
        SaveData data = new SaveData();
        data.cells = new List<CellData>();
        foreach (Cell cell in cells)
        {
            CellData cd = new CellData();
            cd.x = cell.Position.x;
            cd.y = cell.Position.y;
            cd.value = cell.Value;
            data.cells.Add(cd);
        }

        data.bestScore = bestScore;

        string path = Application.persistentDataPath + "/" + saveFileName;
        FileStream stream = new FileStream(path, FileMode.Create);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, data);
        stream.Close();
    }

    private bool LoadGame()
    {
        string path = Application.persistentDataPath + "/" + saveFileName;
        if (File.Exists(path))
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            foreach (var kvp in cellViews)
            {
                Destroy(kvp.Value.gameObject);
            }

            cellViews.Clear();
            cells.Clear();

            foreach (CellData cd in data.cells)
            {
                Vector2Int pos = new Vector2Int(cd.x, cd.y);
                Cell cell = new Cell(pos, cd.value);
                cells.Add(cell);
                int posIndex = pos.y * width + pos.x;
                if (posIndex >= 0 && posIndex < cellPositions.Count)
                {
                    Vector3 spawnPos = cellPositions[posIndex];
                    CellView view = Instantiate(cellViewPrefab, cellsParent);
                    view.Init(cell);
                    view.transform.position = spawnPos;
                    cellViews[cell] = view;
                }
            }

            bestScore = data.bestScore;
            UpdateScore();
            Debug.Log("game loaded from " + path);
            return true;
        }

        return false;
    }
}