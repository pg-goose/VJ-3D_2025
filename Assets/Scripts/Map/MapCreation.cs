using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;


enum TileType {
  Empty = 2,
  Normal,
  Goal,
  Spawn,
  Fragile,
  Separator,
  Obutton,
  Xbutton,
  Obridge,
  Xbridge
}

class BridgeHandler
{
  // bridge tile 0, bridge tile 1
  private GameObject xbr0, xbr1;
  private GameObject obr0, obr1;

  private void WireBridge(GameObject br0, GameObject br1, BridgeController.BridgeType type) {
    BridgeController ctl = br0.GetComponent<BridgeController>();
    Assert.IsNotNull(ctl);
    ctl.SetType(type);
    ctl.SetOther(br1);
    
    ctl = br1.GetComponent<BridgeController>();
    Assert.IsNotNull(ctl);
    ctl.SetType(type);
    ctl.SetOther(br0);
  }

  public bool Incomplete() {
    return ((!obr0) != (!obr1)) || ((!xbr0) != (!xbr1));
  }

  public void AddTile(TileType type, GameObject tile) {
    switch (type) {
    case TileType.Obridge:
      if (obr0) obr1 = tile;
      else obr0      = tile;
    break;
    case TileType.Xbridge:
      if (xbr0) xbr1 = tile;
      else xbr0      = tile;
    break;
    }

    if (obr0 && obr1) WireBridge(obr0, obr1, BridgeController.BridgeType.O);
    if (xbr0 && xbr1) WireBridge(xbr0, xbr1, BridgeController.BridgeType.X);
  }
}

public class MapCreation : MonoBehaviour
{
  [Header("Map Data")] public TextAsset[] levelMaps; //array pels nivells

  public Transform spawnPoint;

  [Header("Prefabs")] public GameObject tileNormal;
  public GameObject tileGoal;
  public GameObject tileFragile;
  public GameObject tileSeparator;
  public GameObject tileObutton;
  public GameObject tileXbutton;
  public GameObject tileBridge;

  private const int MapSizeXIndex = 0;
  private const int MapSizeZIndex = 1;
  private const int MapDataStartIndex = 2;
  private const float TileYOffset = -0.05f;
  private static readonly char[] MapSeparators = { ' ', '\n', '\r' };

  /// <summary>
  /// Result of map creation containing spawn position and tile animators
  /// </summary>
  public struct MapData {
    public Vector3 SpawnPosition;
    public List<TileAnimator> TileAnimators;
  }

  /// <summary>
  /// Crea el mapa basado en el índice del nivel (0 para nivel 1, 1 para nivel 2...)
  /// Returns MapData with spawn position and list of tile animators for orchestration
  /// </summary>
  public MapData CreateMap(int levelIndex) {
    var result = new MapData {
      SpawnPosition = new Vector3(0, 10, 0),
      TileAnimators = new List<TileAnimator>()
    };
    
    if (!IsConfigValid()) return result;

    if (levelIndex < 0 || levelIndex >= levelMaps.Length) {
      Debug.LogError($"[MapCreation] Nivel {levelIndex} fuera de rango.");
      return result;
    }

    TextAsset currentMap = levelMaps[levelIndex];
    int[]     mapNumbers = ParseMap(currentMap.text);
    int       sizeX      = mapNumbers[MapSizeXIndex];
    int       sizeZ      = mapNumbers[MapSizeZIndex];

    return BuildLevel(mapNumbers, sizeX, sizeZ);
  }


  private bool IsConfigValid() {
    if (levelMaps == null || levelMaps.Length == 0) {
      Debug.LogError("MapCreation: No maps assigned in the array.");
      return false;
    }

    var tilePrefabs = new List<GameObject>
      { tileNormal, tileGoal, tileFragile, tileSeparator, tileObutton, tileBridge };
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

  private MapData BuildLevel(int[] mapNumbers, int sizeX, int sizeZ) {
    var result = new MapData {
      SpawnPosition = Vector3.zero,
      TileAnimators = new List<TileAnimator>()
    };
    
    var bridgeHandler = new BridgeHandler();
    // Calculamos el centro
    float offsetX = sizeX / 2f;
    float offsetZ = sizeZ / 2f;
    
    Vector3 firstNormal = Vector3.zero;
    bool    found       = false;

    for (int z = 0; z < sizeZ; z++) {
      int rowOffset = z * sizeX;
      for (int x = 0; x < sizeX; x++) {
        int index = MapDataStartIndex + rowOffset + x;
        var type  = (TileType)mapNumbers[index];

        float      finalX = x - offsetX;
        float      finalZ = z - offsetZ;
        GameObject tile   = CreateTileAt(type, finalX, finalZ, result.TileAnimators);
        if (type is TileType.Obridge or TileType.Xbridge) {
          bridgeHandler.AddTile(type, tile);
          continue;
        }

        if (found) continue;
        if (firstNormal == Vector3.zero && type == TileType.Normal) {
          firstNormal = new Vector3(finalX, 10f, finalZ);
          continue;
        }

        if (type != TileType.Spawn) continue;
        result.SpawnPosition = new Vector3(finalX, 10f, finalZ);
        found = true;
      }
    }

    Assert.IsFalse(bridgeHandler.Incomplete(), "Parsed map had an incomplete bridge!");
    if (!found) result.SpawnPosition = firstNormal;
    return result;
  }

  private GameObject CreateTileAt(TileType tileType, float x, float z, List<TileAnimator> animators) {
    var        position = new Vector3(x, TileYOffset, z);
    Quaternion rotation = Quaternion.identity;

    GameObject tilePrefab;
    switch (tileType) {
    case TileType.Empty:     return null;
    case TileType.Normal:    tilePrefab = tileNormal; break;
    case TileType.Goal:      tilePrefab = tileGoal; break;
    case TileType.Spawn:     tilePrefab = tileNormal; break;
    case TileType.Fragile:   tilePrefab = tileFragile; break;
    case TileType.Separator: tilePrefab = tileSeparator; break;
    case TileType.Obutton:   tilePrefab = tileObutton; break;
    case TileType.Xbutton:   tilePrefab = tileXbutton; break;
    case TileType.Obridge:   tilePrefab = tileBridge; break;
    case TileType.Xbridge:   tilePrefab = tileBridge; break;
    default:
      throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);
    }

    Debug.Assert(tilePrefab != null);
    GameObject tile = Instantiate(tilePrefab, position, rotation);
    tile.transform.parent = transform;
    
    // Add animator and prepare it, but don't start yet
    TileAnimator animator = tile.AddComponent<TileAnimator>();
    animator.Prepare(position);
    animators.Add(animator);
    
    return tile;
  }

  public void UnloadMap() {
    foreach (Transform child in transform)
      Destroy(child.gameObject);
  }
}