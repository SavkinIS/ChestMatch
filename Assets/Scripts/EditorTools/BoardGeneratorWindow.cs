// BoardGenerator.cs
// Генерация шашечной/шахматной доски через окно редактора Unity
// Добавляет меню: Tools/Shashki/Board Generator…
// Включает простые runtime-компоненты: BoardRoot и BoardCell

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace Shashki.Editor
{
    /// <summary>
    /// Окно редактора для генерации доски из Quads.
    /// Доступно в меню: Tools/Shashki/Board Generator…
    /// </summary>
    public class BoardGeneratorWindow : EditorWindow
    {
        // Поддерживаемые размеры доски (6x6 или 8x8)
        private static readonly int[] _presetSizes = new[] { 6, 8 };
        private int _sizeIndex = 1; // По умолчанию 8x8

        // Настройки доски
        private float _cellSize = 1f;          // Размер клетки
        private float _spacing = 0f;           // Расстояние между клетками
        private bool _centerAtOrigin = true;   // Центрировать ли доску в (0,0,0)

        // Настройки коллайдеров
        private bool _createBoxColliders = true;   // Добавить BoxCollider для кликов
        private bool _removeMeshColliders = true;  // Удалить MeshCollider у Quad

        // Настройки паттерна
        private bool _darkSquaresOnly = false;     // Создавать только тёмные клетки
        private bool _checkerPattern = true;       // Включить шахматный паттерн

        // Цвета клеток
        private Color _lightColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        private Color _darkColor = new Color(0.20f, 0.20f, 0.20f, 1f);

        // Имя родителя
        private string _parentName = "Board_8x8";

        private const string _menuPath = "Tools/Shashki/Board Generator…";
        private const string _autoFolder = "Assets/_Auto/BoardMaterials";

        [MenuItem(_menuPath)]
        public static void Open()
        {
            var wnd = GetWindow<BoardGeneratorWindow>(true, "Board Generator");
            wnd.minSize = new Vector2(360, 420);
            wnd.Show();
        }

        private void OnGUI()
        {
            // Настройки размера доски
            EditorGUILayout.LabelField("Параметры доски", EditorStyles.boldLabel);
            _sizeIndex = EditorGUILayout.Popup("Размер", _sizeIndex, new[] { "6 x 6", "8 x 8" });
            int size = _presetSizes[Mathf.Clamp(_sizeIndex, 0, _presetSizes.Length - 1)];

            using (new EditorGUI.IndentLevelScope())
            {
                _cellSize = EditorGUILayout.FloatField("Размер клетки", Mathf.Max(0.01f, _cellSize));
                _spacing = EditorGUILayout.FloatField("Расстояние между клетками", Mathf.Max(0f, _spacing));
                _centerAtOrigin = EditorGUILayout.ToggleLeft("Центрировать доску в (0,0,0)", _centerAtOrigin);
            }

            // Цвета клеток
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Цвета", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                _lightColor = EditorGUILayout.ColorField("Светлая клетка", _lightColor);
                _darkColor = EditorGUILayout.ColorField("Тёмная клетка", _darkColor);
                _checkerPattern = EditorGUILayout.ToggleLeft("Использовать шахматный паттерн", _checkerPattern);
                _darkSquaresOnly = EditorGUILayout.ToggleLeft("Генерировать только тёмные клетки", _darkSquaresOnly);
            }

            // Коллайдеры
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Коллайдеры", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                _createBoxColliders = EditorGUILayout.ToggleLeft("Добавить BoxCollider", _createBoxColliders);
                _removeMeshColliders = EditorGUILayout.ToggleLeft("Удалить MeshCollider у Quad", _removeMeshColliders);
            }

            // Имя объекта-родителя
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Вывод", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                _parentName = EditorGUILayout.TextField("Имя родителя", _parentName);
            }

            EditorGUILayout.Space(10);

            // Кнопки генерации/очистки
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Сгенерировать", GUILayout.Height(32)))
                {
                    Generate(size, size);
                }

                if (GUILayout.Button("Очистить выделение", GUILayout.Height(32)))
                {
                    Selection.activeObject = null;
                }
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Создаёт новый объект с дочерними Quad-клетками. Материалы автоматически создаются в папке Assets/_Auto/BoardMaterials.",
                MessageType.Info);
        }

        private void Generate(int rows, int cols)
        {
            // Проверка: если доска с таким именем уже есть, спросить об удалении
            GameObject existing = GameObject.Find(_parentName);
            if (existing != null)
            {
                bool replace = EditorUtility.DisplayDialog(
                    "Доска уже существует",
                    $"Объект '{_parentName}' уже есть в сцене. Заменить его?", "Заменить", "Отмена");
                if (!replace) return;
                Undo.DestroyObjectImmediate(existing);
            }

            // Убедиться, что папка для материалов существует
            EnsureFolder(_autoFolder);

            // Создать или загрузить материалы для клеток
            var lightMat = GetOrCreateMaterial(Path.Combine(_autoFolder, "Board_Light.mat"), _lightColor);
            var darkMat  = GetOrCreateMaterial(Path.Combine(_autoFolder, "Board_Dark.mat"), _darkColor);

            // Создать объект-родитель
            GameObject parent = new GameObject(_parentName);
            Undo.RegisterCreatedObjectUndo(parent, "Создать доску");

            // Добавить компонент BoardRoot и записать параметры
            var root = parent.AddComponent<BoardRoot>();
            root.SetData(rows, cols, _cellSize, _spacing, _darkSquaresOnly, _checkerPattern);
            root.SetMaterials(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(lightMat)),
                AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(darkMat)));

            // Центровка доски (если включена опция)
            float step = _cellSize + _spacing;
            Vector3 originOffset = Vector3.zero;
            if (_centerAtOrigin)
            {
                originOffset = new Vector3(-(cols - 1) * 0.5f * step, 0f, (rows - 1) * 0.5f * step);
            }

            List<BoardCell> cells = new List<BoardCell>();
            
            try
            {
                EditorUtility.DisplayProgressBar("Генерация доски", "Создание клеток…", 0f);

                int total = rows * cols;
                int created = 0;

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        bool isDark = ((r + c) % 2) == 1; // тёмная клетка, если сумма индексов нечётная
                        if (_darkSquaresOnly && (!_checkerPattern || !isDark))
                        {
                            continue; // пропускаем светлые клетки, если выбрана генерация только тёмных
                        }

                        // Создание Quad клетки
                        var cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        Undo.RegisterCreatedObjectUndo(cell, "Создать клетку");
                        cell.name = $"Cell_{r}_{c}";
                        cell.transform.SetParent(parent.transform);

                        // Позиция и размер
                        Vector3 pos = new Vector3(c * step, -r * step, 0f) + originOffset;
                        cell.transform.localPosition = pos;
                        cell.transform.localRotation = Quaternion.identity;
                        cell.transform.localScale = new Vector3(_cellSize, _cellSize, 1f);

                        // Применение материала (светлый/тёмный)
                        var mr = cell.GetComponent<MeshRenderer>();
                        mr.sharedMaterial = (_checkerPattern && isDark) ? darkMat : lightMat;

                        // Настройка коллайдеров
                        if (_removeMeshColliders)
                        {
                            var meshCol = cell.GetComponent<MeshCollider>();
                            if (meshCol) Object.DestroyImmediate(meshCol);
                        }
                        if (_createBoxColliders)
                        {
                            var bc = cell.AddComponent<BoxCollider>();
                            bc.size = new Vector3(1f, 0.01f, 1f); // тонкий по Y, Quad лежит в XZ
                            bc.center = Vector3.zero;
                        }

                        // Добавление метаинформации
                        var boardCell = cell.AddComponent<BoardCell>();
                        boardCell.SetData(r, c, isDark);
                        
                        created++;
                        cells.Add(boardCell);
                        if (total > 0 && created % 8 == 0)
                        {
                            EditorUtility.DisplayProgressBar("Генерация доски",
                                $"Создано клеток: {created}/{total}", (float)created / total);
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            root.SetCells(cells);
            
            // Выделить доску в Hierarchy
            Selection.activeGameObject = parent;
            EditorGUIUtility.PingObject(parent);

            Debug.Log($"[BoardGenerator] Создана '{parent.name}' с {parent.transform.childCount} клетками ({rows}x{cols}).");
        }

        // ————————————————————————————— Вспомогательные методы —————————————————————————————

        // Убедиться, что папка существует, иначе создать её
        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            if (current != "Assets")
                throw new System.Exception("Путь должен начинаться с Assets/");

            for (int i = 1; i < parts.Length; i++)
            {
                string next = parts[i];
                string combined = string.Join("/", parts, 0, i + 1);
                if (!AssetDatabase.IsValidFolder(combined))
                {
                    AssetDatabase.CreateFolder(current, next);
                }
                current = combined;
            }
        }

        // Создать или загрузить материал с указанным цветом
        private static Material GetOrCreateMaterial(string assetPath, Color color)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null)
            {
                var shader = FindBestColorShader();
                mat = new Material(shader);
                SetMaterialColor(mat, color);
                AssetDatabase.CreateAsset(mat, assetPath);
            }
            else
            {
                // Обновляем цвет существующего материала
                var matClone = Object.Instantiate(mat);
                SetMaterialColor(matClone, color);
                EditorUtility.CopySerialized(matClone, mat);
                Object.DestroyImmediate(matClone);
            }
            AssetDatabase.SaveAssets();
            return mat;
        }

        // Поиск подходящего шейдера для материалов
        private static Shader FindBestColorShader()
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Unlit/Color");
            if (s == null) throw new System.Exception("Не найден подходящий шейдер (URP Lit, Standard или Unlit/Color)");
            return s;
        }

        // Установка цвета материала
        private static void SetMaterialColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color); // URP/HDRP
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);       // Built-in/Unlit
        }
    }
}
#endif
