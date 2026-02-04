# Hyper Casual Runner

Grass-cutting runner game for mobile. Built with Unity 6.

## What it does

- Grass rendering on GPU using compute shaders and instancing
- Cut grass as you run through it
- Lane-based movement system
- Chunks spawn procedurally with object pooling
- Event bus for communication between systems
- VContainer for dependency injection

## Tech

- Unity 6000.3.2f1
- URP 17.3.0
- VContainer 1.16.9
- Unity Input System 1.17.0
- iOS/Android

## Architecture

Services:
- Configuration - game settings
- Game State Manager - state machine
- Event Bus - type-safe events
- Input Service - touch/mouse
- Object Pool - chunk pooling

## Performance

- GPU instancing for grass
- Compute shaders for cutting
- Object pooling for chunks
- Targets 60 FPS on mobile
