# Dependency Injection System Improvements

This document outlines the enhanced Dependency Injection (DI) system implemented for the Hyper Casual game project using VContainer.

## Overview

The DI system has been restructured to provide better separation of concerns, testability, and maintainability. The system is organized into two main lifetime scopes:

- **RootLifetimeScope**: Contains singleton services that persist across scenes
- **GameLifetimeScope**: Contains game-specific services and components

## Core Services

### 1. Configuration System
- **IConfigurationService**: Manages game configuration and settings
- **GameConfig**: ScriptableObject containing all game settings
- Provides centralized configuration management with runtime overrides

### 2. Game State Management
- **IGameStateManager**: Manages game states (Menu, Playing, Paused, GameOver, Loading)
- Provides state transition validation and events
- Ensures proper game flow control

### 3. Event System
- **IEventBus**: Decoupled communication between systems
- Type-safe event publishing and subscription
- Includes common game events (PlayerMoved, GrassCut, LevelProgress, GameOver)

### 4. Input System
- **IInputService**: Unified input handling for touch and mouse
- Cross-platform input abstraction
- Event-driven input notifications

### 5. Object Pooling
- **IObjectPoolService**: Enhanced object pooling system
- Automatic pool management and cleanup
- Pre-warming capabilities for better performance

## Service Registration

### RootLifetimeScope Services
```csharp
// Core Services
builder.RegisterComponentOnNewGameObject<CoroutineRunner>(Lifetime.Singleton).DontDestroyOnLoad();
builder.Register<SceneManager>(Lifetime.Singleton);

// Configuration System
builder.Register<ConfigurationService>(Lifetime.Singleton).AsImplementedInterfaces();

// Game State Management
builder.Register<GameStateManager>(Lifetime.Singleton).AsImplementedInterfaces();

// Event System
builder.Register<EventBus>(Lifetime.Singleton).AsImplementedInterfaces();

// Input System
builder.RegisterComponentOnNewGameObject<InputService>(Lifetime.Singleton)
    .AsImplementedInterfaces()
    .DontDestroyOnLoad();

// Object Pooling
builder.RegisterComponentOnNewGameObject<ObjectPoolService>(Lifetime.Singleton)
    .AsImplementedInterfaces()
    .DontDestroyOnLoad();
```

### GameLifetimeScope Services
```csharp
// Register game-specific components
if (grassRenderer != null)
    builder.RegisterInstance(grassRenderer);

if (grassCutter != null)
    builder.RegisterInstance(grassCutter);
```

## Improved Components

### 1. ImprovedPlayerController
- Uses IInputService for input handling
- Publishes PlayerMovedEvent through IEventBus
- Respects game state changes
- Configurable through GameConfig

### 2. ImprovedLevelManager
- Uses IObjectPoolService for chunk management
- Listens to PlayerMovedEvent for level progression
- Publishes LevelProgressEvent and GameOverEvent
- Enhanced pooling with pre-warming

### 3. ImprovedGrassCutter
- Event-driven grass cutting based on player movement
- Publishes GrassCutEvent with cutting statistics
- Respects game state for cutting enablement
- Configurable cutting parameters

## Usage Examples

### Injecting Services
```csharp
public class MyComponent : MonoBehaviour
{
    [Inject] private IEventBus EventBus { get; set; }
    [Inject] private IGameStateManager GameStateManager { get; set; }
    [Inject] private IConfigurationService ConfigService { get; set; }
}
```

### Publishing Events
```csharp
EventBus.Publish(new PlayerMovedEvent 
{ 
    Position = currentPosition,
    PreviousPosition = previousPosition 
});
```

### Subscribing to Events
```csharp
void Start()
{
    EventBus.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
}

void OnDestroy()
{
    EventBus.Unsubscribe<PlayerMovedEvent>(OnPlayerMoved);
}
```

### Using Object Pool
```csharp
// Get object from pool
var chunk = ObjectPoolService.Get(chunkPrefab.GetComponent<Transform>(), parent);

// Return to pool
ObjectPoolService.Return(chunk);
```

## Benefits

1. **Testability**: Services can be easily mocked for unit testing
2. **Maintainability**: Clear separation of concerns and dependencies
3. **Performance**: Object pooling reduces garbage collection
4. **Flexibility**: Event system allows for easy feature additions
5. **Configuration**: Centralized settings management
6. **Debugging**: Better logging and state management

## Future Enhancements

- Analytics service for gameplay metrics
- Save/Load system for game progress
- Audio service for sound management
- Localization service for multi-language support
- Performance monitoring service
