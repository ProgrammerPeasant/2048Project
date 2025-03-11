using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameField : MonoBehaviour {
    [Header("Параметры игрового поля")]
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;

    [Header("Ссылки на объекты из сцены")]
    [SerializeField] private Transform gridObject;


    [Header("Ссылки на префаб и контейнер для клеток")]
    [SerializeField] private CellView cellViewPrefab;
    [SerializeField] private Transform cellsParent;

    private List<Cell> cells = new List<Cell>();
    private List<Vector2> cellPositions = new List<Vector2>();

    private void Start()
    {
        GetCellPositionsFromScene();
        CreateCell();
        CreateCell();
        CreateCell();
        CreateCell();
        CreateCell();
        CreateCell();

    }

    // Новый метод для получения позиций "Cell" из сцены
    private void GetCellPositionsFromScene()
    {
        cellPositions.Clear(); // Очищаем список, если метод вызывается повторно

        // **Используем прямую ссылку через gridObject вместо transform.Find("Grid")**
        if (gridObject == null)
        {
            Debug.LogError("Объект 'Grid' не назначен в инспекторе");
            return;
        }

        Transform gridTransform = gridObject;

        foreach (Transform rowTransform in gridTransform)
        {
            foreach (Transform cellTransform in rowTransform)
            {
                cellPositions.Add(cellTransform.position);
            }
        }

        if (cellPositions.Count == 0)
        {
            Debug.LogWarning("Не найдено ни одного объекта 'Cell' в иерархии под 'Grid'");
        }
        else
        {
            Debug.Log("Найдено позиций Cell: " + cellPositions.Count);
        }
    }

    /// <summary>
    /// Возвращает координаты случайной пустой клетки
    /// </summary>
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

        int randIndex = Random.Range(0, freePositions.Count);
        return freePositions[randIndex];
    }

    /// <summary>
    /// Создаёт новую клетку в случайной пустой позиции на поле
    /// Значение = 1 (с вероятностью 90%) или 2 (с вероятностью 10%)
    /// Создаёт для неё префаб с CellView и подписывает на события
    /// </summary>
    public void CreateCell()
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
            Debug.LogError("Неверный индекс позиции! Проверьте логику GetEmptyPosition и GetCellPositionsFromScene.");
            return;
        }

        Vector3 spawnPosition = cellPositions[positionIndex];

        int value = (Random.value < 0.9f) ? 1 : 2;
        Cell newCell = new Cell(emptyPos, value);

        cells.Add(newCell);

        CellView view = Instantiate(cellViewPrefab, cellsParent);
        view.Init(newCell);

        view.transform.position = spawnPosition;
    }
}