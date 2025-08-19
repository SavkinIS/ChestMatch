using UnityEditor;
using UnityEngine;

namespace Shashki.EditorTools
{
    public static class PieceSpawnerEditor
    {
        private const string _menuPath = "Tools/Shashki/Spawn Pieces";

        [MenuItem(_menuPath)]
        public static void SpawnPieces()
        {
            // Ищем первый PieceSpawner на сцене
            var spawner = Object.FindFirstObjectByType<PieceHolder>();
            if (spawner == null)
            {
                Debug.LogError("[PieceSpawnerEditor] Не найден PieceSpawner на сцене");
                return;
            }

            spawner.SpawnPieces();
            Debug.Log("[PieceSpawnerEditor] Шашки сгенерированы через меню");
        }
    }
}