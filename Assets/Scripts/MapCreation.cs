using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
///   MapCreation instantiates multiple copies of a tile prefab to build a level
///   from the contents of a map text file.
/// </summary>
public class MapCreation : MonoBehaviour
{
  #region Unity

  private void Start() {
    if (!IsConfigValid()) return;

    int[] mapNumbers = ParseMap(map.text);
    int   sizeX      = mapNumbers[MapSizeXIndex];
    int   sizeZ      = mapNumbers[MapSizeZIndex];

    BuildLevel(mapNumbers, sizeX, sizeZ);
  }

  #endregion

  private enum TileType
  {
    Empty = 1,
    Normal = 2,
    Goal = 3,
    Fragile = 4,
    Separator = 5,
    Obutton = 6,
    Xbutton = 7
  }

  #region Constants

  private const int MapSizeXIndex = 0;
  private const int MapSizeZIndex = 1;
  private const int MapDataStartIndex = 2;

  private const float TileYOffset = -0.05f;

  private static readonly char[] MapSeparators = { ' ', '\n', '\r' };

  #endregion

  #region Editor Fields

  [Header("Map Data")] public TextAsset map; // Text file containing the map

  [Header("Prefabs")] public GameObject tileNormal;
  public GameObject tileGoal;
  public GameObject tileFragile;
  public GameObject tileSeparator;
  public GameObject tileObutton;
  public GameObject tileXbutton;

  #endregion

  #region Map Building

  private bool IsConfigValid() {
    if (map == null) {
      Debug.LogError("MapCreation: No map TextAsset assigned.");
      return false;
    }
    // list of prefabs, check if any is null
    var tilePrefabs = new List<GameObject> {
      tileNormal, tileGoal, tileFragile, tileSeparator, tileObutton
    };
    if (tilePrefabs.All(o => o != null))
      return true;
    Debug.LogError("MapCreation: No tile prefab assigned.");
    return false;
  }

  private int[] ParseMap(string mapText) {
    string[] stringNumbers =
      mapText.Split(MapSeparators, StringSplitOptions.RemoveEmptyEntries);

    int   totalNumbers = stringNumbers.Length;
    int[] numbers      = new int[totalNumbers];

    for (int i = 0; i < totalNumbers; i++) numbers[i] = int.Parse(stringNumbers[i]);

    return numbers;
  }

  private void BuildLevel(int[] mapNumbers, int sizeX, int sizeZ) {
    // for each row in the file...
    for (int z = 0; z < sizeZ; z++) {
      int rowOffset = z * sizeX;
      // for each number in the row...
      for (int x = 0; x < sizeX; x++) {
        int index     = MapDataStartIndex + rowOffset + x;
        int tileValue = mapNumbers[index];
        var tileType  = (TileType)tileValue;

        CreateTileAt(tileType, x, z);
      }
    }
  }

  private void CreateTileAt(TileType tileType, int x, int z) {
    var        position = new Vector3(x, TileYOffset, z);
    Quaternion rotation = transform.rotation;

    GameObject tile;
    switch (tileType) {
    case TileType.Empty:     return;
    case TileType.Normal:    tile = tileNormal; break;
    case TileType.Goal:      tile = tileGoal; break;
    case TileType.Fragile:   tile = tileFragile; break;
    case TileType.Separator: tile = tileSeparator; break;
    case TileType.Obutton:   tile = tileObutton; break;
    case TileType.Xbutton:   tile = tileXbutton; break;
    default:
      Debug.LogError("MapCreation: Unknown TileType: " + tileType);
      return;
    }

    GameObject obj = Instantiate(tile, position, rotation);
    obj.transform.parent = transform;
  }

  #endregion
}