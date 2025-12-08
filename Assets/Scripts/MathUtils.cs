using UnityEngine;

/// <summary>
/// Utility class for common math operations.
/// Provides static helper methods accessible throughout the project.
/// </summary>
public static class MathUtils
{
  /// <summary>
  /// Compares two floating point values and returns true if they are similar within a specified margin.
  /// </summary>
  /// <param name="a">First value to compare.</param>
  /// <param name="b">Second value to compare.</param>
  /// <param name="epsilon">The maximum difference allowed between the values.</param>
  /// <returns>True if the absolute difference between a and b is less than or equal to epsilon.</returns>
  public static bool Approximately(float a, float b, float epsilon = 0.01f) {
    return Mathf.Abs(a - b) <= epsilon;
  }
}
