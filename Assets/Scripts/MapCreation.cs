using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapCreation : MonoBehaviour
{
  [Header("Map Data")] 
  public TextAsset[] levelMaps; //array pels nivells

  [Header("Prefabs")] 
  public GameObject tileNormal;
  public GameObject tileGoal;
  public GameObject tileFragile;
  public GameObject tileSeparator;
  public GameObject tileObutton;
  public GameObject tileXbutton;

  
  private const int MapSizeXIndex = 0;
  private const int MapSizeZIndex = 1;
  private const int MapDataStartIndex = 2;
  private const float TileYOffset = -0.05f;
  private static readonly char[] MapSeparators = { ' ', '\n', '\r' };

  

  /// <summary>
  /// Crea el mapa basado en el índice del nivel (0 para nivel 1, 1 para nivel 2...)
  /// </summary>
  public void CreateMap(int levelIndex) {
    if (!IsConfigValid()) return;

    
    if (levelIndex < 0 || levelIndex >= levelMaps.Length) {
        Debug.LogError($"[MapCreation] Nivel {levelIndex} fuera de rango. Tienes {levelMaps.Length} mapas asignados.");
        return;
    }

    TextAsset currentMap = levelMaps[levelIndex];
    Debug.Log($"[MapCreation] Generando mapa: {currentMap.name}");

    int[] mapNumbers = ParseMap(currentMap.text);
    int sizeX = mapNumbers[MapSizeXIndex];
    int sizeZ = mapNumbers[MapSizeZIndex];

    BuildLevel(mapNumbers, sizeX, sizeZ);
  }

  
  private bool IsConfigValid() {
    if (levelMaps == null || levelMaps.Length == 0) {
      Debug.LogError("MapCreation: No maps assigned in the array.");
      return false;
    }
    var tilePrefabs = new List<GameObject> { tileNormal, tileGoal, tileFragile, tileSeparator, tileObutton };
    if (tilePrefabs.All(o => o != null)) return true;
    
    Debug.LogError("MapCreation: No tile prefab assigned.");
    return false;
  }
  
  
  private int[] ParseMap(string mapText) {
    string[] stringNumbers = mapText.Split(MapSeparators, StringSplitOptions.RemoveEmptyEntries);
    int totalNumbers = stringNumbers.Length;
    int[] numbers = new int[totalNumbers];
    for (int i = 0; i < totalNumbers; i++) numbers[i] = int.Parse(stringNumbers[i]);
    return numbers;
  }

  private void BuildLevel(int[] mapNumbers, int sizeX, int sizeZ) {
    for (int z = 0; z < sizeZ; z++) {
      int rowOffset = z * sizeX;
      for (int x = 0; x < sizeX; x++) {
        int index = MapDataStartIndex + rowOffset + x;
        int tileValue = mapNumbers[index];
        var tileType = (TileType)tileValue;
        CreateTileAt(tileType, x, z);
      }
    }
  }

  private void CreateTileAt(TileType tileType, int x, int z) {
    var position = new Vector3(x, TileYOffset, z);
    Quaternion rotation = Quaternion.identity; 

    GameObject tilePrefab = null;
    
    switch (tileType) {
        case TileType.Empty:     return;
        case TileType.Normal:    tilePrefab = tileNormal; break;
        case TileType.Goal:      tilePrefab = tileGoal; break;
        case TileType.Fragile:   tilePrefab = tileFragile; break;
        case TileType.Separator: tilePrefab = tileSeparator; break;
        case TileType.Obutton:   tilePrefab = tileObutton; break;
        case TileType.Xbutton:   tilePrefab = tileXbutton; break;
    }

    if (tilePrefab) {
        GameObject obj = Instantiate(tilePrefab, position, rotation);
        obj.transform.parent = transform;
        TileAnimator animator = obj.AddComponent<TileAnimator>();
        animator.Animate(position);
    }
  }

  public void UnloadMap() {
    foreach (Transform child in transform)
      Destroy(child.gameObject);
  }
  
  private enum TileType { Empty=1, Normal=2, Goal=3, Fragile=4, Separator=5, Obutton=6, Xbutton=7 }
}