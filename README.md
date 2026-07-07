# Ink Jam

## Engine Version
Unity 6 LTS

## Target Platform
Android (Minimum API Level 35)

## Folder Structure
- `Assets/Scripts/Core`: Pure C# core game logic, zero UnityEngine references.
- `Assets/Scripts/Gameplay`: MonoBehaviours driving Core from the scene.
- `Assets/Scripts/Obstacles`: One file per obstacle type, shared IObstacle.
- `Assets/Scripts/Generator`: Procedural level generator, pure C#.
- `Assets/Scripts/UI`: MonoBehaviours for menus/HUD.
- `Assets/Scripts/Data`: ScriptableObjects + JSON level DTOs.
- `Assets/Resources/Levels`: Hand-built level JSON files.
