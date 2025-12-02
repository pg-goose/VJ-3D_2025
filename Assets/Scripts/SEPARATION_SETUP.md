# Player Separation System - Setup Guide

This guide explains how to set up the player separation functionality for your Bloxorz clone.

## Overview

The separation system splits the cuboid player into two individual cubes when stepping on a TileSeparator. Players can then control each cube independently by switching between them.

## Scripts Created

1. **PlayerSeparator.cs** - Main separation logic (attach to player prefab)
2. **MoveCube.cs** - Individual cube movement controller
3. **SeparatorTile.cs** - Separator tile trigger component
4. **CubeVisualFeedback.cs** - Optional visual feedback for active/inactive cubes

## Setup Instructions

### 1. Input Actions Setup

Add a new action to your `InputSystem_Actions.inputactions`:

- **Action Name**: "Switch"
- **Action Type**: Button
- **Binding**: Space (or any key you prefer)

This allows players to switch between the two cubes when separated.

### 2. Player Prefab Setup

Add the `PlayerSeparator` component to your existing player prefab:

1. Select your player prefab
2. Add Component → PlayerSeparator
3. Configure the following:
   - **Cube Prefab**: Assign the cube prefab (see step 3)
   - **Center A**: Drag your CenterA transform
   - **Center B**: Drag your CenterB transform
   - **Separation Sound**: Optional audio clip

### 3. Cube Prefab Creation

Create a cube prefab for the separated state:

1. Create a new GameObject with:
   - Cube mesh (0.5 x 0.5 x 0.5 scale)
   - Box Collider
   - Rigidbody (same settings as player)
   - MoveCube component
   - CubeVisualFeedback component (optional)

2. Configure MoveCube:
   - **Rot Speed**: Same as your player's rotation speed
   - **Sounds**: Optional movement sounds
   - **Fall Sound**: Optional falling sound

3. Save as a prefab (e.g., "PlayerCube")

### 4. TileSeparator Prefab Setup

Add the separator trigger to your TileSeparator prefab:

1. Select your TileSeparator prefab
2. Add Component → SeparatorTile
3. Ensure it has a **Collider** with **Is Trigger** enabled
4. Add the "Separator" tag:
   - Go to Tags & Layers
   - Add a new tag called "Separator"
   - Assign it to your TileSeparator prefab

Alternative: Simply ensure "Separator" is in the GameObject's name.

### 5. Player Tag

Ensure your player prefab has the "Player" tag assigned (required for goal detection and other systems).

### 6. Ground Layer

Verify that your tiles are on the "Ground" layer (required for grounding detection).

## How It Works

### Separation Flow:

1. Player (cuboid) steps on TileSeparator
2. PlayerSeparator detects the trigger
3. Creates two cube instances at CenterA and CenterB positions
4. Disables the cuboid's MoveCuboid component and rendering
5. Player can now control one cube at a time

### Cube Control:

- **Movement**: WASD/Arrow keys (same as cuboid)
- **Switch**: Space (or configured key)
- Only the active cube responds to movement input
- Inactive cube is visually dimmed (if using CubeVisualFeedback)

### Merging (Future Enhancement):

The `MergeCubes()` method is prepared for rejoining cubes:
- Call when cubes are adjacent (1 unit apart)
- Restores the original cuboid state
- Can be triggered by a special tile or button

## Optional Enhancements

### Visual Feedback:

The `CubeVisualFeedback` component provides:
- Color change between active/inactive cubes
- Emission glow on active cube
- Smooth color transitions

### Audio Feedback:

Configure optional sounds:
- Separation sound when splitting
- Activation sound on separator tile
- Individual cube movement sounds

### Merge Tile:

Create a merge tile similar to the separator:
- Detects when both cubes are on it
- Calls `PlayerSeparator.MergeCubes()`
- Rejoins the player into cuboid form

## Troubleshooting

**Cubes not spawning:**
- Check that the cube prefab is assigned
- Verify CenterA and CenterB are set

**Can't switch between cubes:**
- Ensure "Switch" action exists in InputSystem_Actions
- Check that both cubes have MoveCube component

**Separation not triggering:**
- Verify TileSeparator has a trigger collider
- Check for "Separator" tag or name contains "Separator"
- Ensure PlayerSeparator component is on the player

**Cubes falling through the floor:**
- Verify Ground layer mask is set correctly
- Check that tiles are on the "Ground" layer
