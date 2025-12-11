using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class MapCreation : MonoBehaviour
{
  [Header("Map Data")]
  public TextAsset[] levelMaps; //array pels nivells

  public Transform spawnPoint;

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
  public Vector3 CreateMap(int levelIndex) {
    if (!IsConfigValid()) return new Vector3(0, 10, 0);

    if (levelIndex < 0 || levelIndex >= levelMaps.Length) {
      Debug.LogError($"[MapCreation] Nivel {levelIndex} fuera de rango.");
      return new Vector3(0, 10, 0);
    }

    TextAsset currentMap = levelMaps[levelIndex];
    int[]     mapNumbers = ParseMap(currentMap.text);
    int       sizeX      = mapNumbers[MapSizeXIndex];
    int       sizeZ      = mapNumbers[MapSizeZIndex];

    //devuelve pos inicio 
    return BuildLevel(mapNumbers, sizeX, sizeZ);
  }


  private bool IsConfigValid() {
    if (levelMaps == null || levelMaps.Length == 0) {
      Debug.LogError("MapCreation: No maps assigned in the array.");
      return false;
    }

    var tilePrefabs = new List<GameObject>
      { tileNormal, tileGoal, tileFragile, tileSeparator, tileObutton };
    if (tilePrefabs.All(o => o != null)) return true;

    Debug.LogError("MapCreation: No tile prefab assigned.");
    return false;
  }


  private int[] ParseMap(string mapText) {
    string[] stringNumbers =
      mapText.Split(MapSeparators, StringSplitOptions.RemoveEmptyEntries);
    int   totalNumbers                                = stringNumbers.Length;
    int[] numbers                                     = new int[totalNumbers];
    for (int i = 0; i < totalNumbers; i++) numbers[i] = int.Parse(stringNumbers[i]);
    return numbers;
  }

  private Vector3 BuildLevel(int[] mapNumbers, int sizeX, int sizeZ) {
    // Calculamos el centro
    float offsetX = sizeX / 2f;
    float offsetZ = sizeZ / 2f;
    
    Vector3 spawnPosition = Vector3.zero;
    Vector3 firstNormal   =  Vector3.zero;
    bool    found         = false;

    for (int z = 0; z < sizeZ; z++) {
      int rowOffset = z * sizeX;
      for (int x = 0; x < sizeX; x++) {
        int index = MapDataStartIndex + rowOffset + x;
        var type  = (TileType)mapNumbers[index];

        float finalX = x - offsetX;
        float finalZ = z - offsetZ;
        CreateTileAt(type, finalX, finalZ);
        if (found) continue;
        if (firstNormal == Vector3.zero && type == TileType.Normal) { 
          firstNormal = new Vector3(finalX, 10f, finalZ);
          continue;
        }
        if (type != TileType.Spawn) continue;
        spawnPosition = new Vector3(finalX, 10f, finalZ);
        found         = true;
      }
    }

    return found ? spawnPosition : firstNormal;
  }

  private void CreateTileAt(TileType tileType, float x, float z) {
    var        position = new Vector3(x, TileYOffset, z);
    Quaternion rotation = Quaternion.identity;

    GameObject tilePrefab;
    switch (tileType) {
    case TileType.Empty:     return;
    case TileType.Normal:    tilePrefab = tileNormal; break;
    case TileType.Goal:      tilePrefab = tileGoal; break;
    case TileType.Fragile:   tilePrefab = tileFragile; break;
    case TileType.Separator: tilePrefab = tileSeparator; break;
    case TileType.Obutton:   tilePrefab = tileObutton; break;
    case TileType.Xbutton:   tilePrefab = tileXbutton; break;
    case TileType.Spawn:     tilePrefab = tileNormal; break;
    default:
      throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);
    }
    Debug.Assert(tilePrefab != null);
    GameObject obj = Instantiate(tilePrefab, position, rotation);
    obj.transform.parent = transform;
    TileAnimator animator = obj.AddComponent<TileAnimator>();
    animator.Animate(position);
  }

  public void UnloadMap() {
    foreach (Transform child in transform)
      Destroy(child.gameObject);
  }

  private enum TileType
  {
    Empty = 1,
    Normal = 2,
    Goal = 3,
    Fragile = 4,
    Separator = 5,
    Obutton = 6,
    Xbutton = 7,
    Spawn = 8
  }
}