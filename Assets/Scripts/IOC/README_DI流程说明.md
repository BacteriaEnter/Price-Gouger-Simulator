# Unity UniDi 依赖注入流程说明

## 📋 目录

1. [核心概念](#核心概念)
2. [完整执行流程](#完整执行流程)
3. [参与的类及作用](#参与的类及作用)
4. [使用示例](#使用示例)
5. [常见问题](#常见问题)

---

## 核心概念

### 什么是依赖注入（DI）？

依赖注入是一种设计模式，让对象不需要自己创建或查找依赖，而是由外部（容器）提供。

```csharp
// ❌ 没有 DI：自己查找依赖
public class MySystem
{
    private GameObject _player;
    
    void Init()
    {
        _player = GameObject.Find("Player");  // 自己查找
    }
}

// ✅ 使用 DI：依赖由容器注入
public class MySystem
{
    [Inject]
    private GameObject _player;  // 容器自动注入
    
    void Init()
    {
        // _player 已经准备好，直接使用
    }
}
```

### 两种容器

- **ProjectContext（全局容器）**：游戏启动时创建，跨场景存在
- **SceneContext（场景容器）**：每个场景有自己的容器，加载场景时创建

---

## 完整执行流程

```
【游戏启动】
    ↓
1. ProjectContext 初始化
    ↓
   执行 ProjectInstaller.InstallBindings()
    ↓
   绑定全局对象：IPlayerProvider → PlayerProvider（单例）
    ↓
【加载 Scene 场景】
    ↓
2. SceneContext 初始化
    ↓
   执行 SceneInstaller.InstallBindings()
    ↓
   绑定场景对象：
   - Player GameObject (WithId("Player"))
   - SceneCamera GameObject (WithId("SceneCameraGo"))
   - SceneObjectsLinker（自动执行）
    ↓
3. SceneObjectsLinker.Initialize() 自动执行
    ↓
   将场景中的 Player 设置到全局 PlayerProvider
    ↓
【GameManager 初始化 System】
    ↓
4. 创建 GameSystem（如 PlayerAngleViewSystem）
    ↓
   Container.Inject(system)  // 手动注入
    ↓
   所有 [Inject] 标记的字段被赋值
    ↓
5. system.Init() 执行
    ↓
   使用已注入的依赖
```

---

## 参与的类及作用

### 1. ProjectInstaller（全局绑定）

**位置**：`Assets/Scripts/Installers/ProjectInstaller.cs`  
**作用**：在全局容器中绑定跨场景对象

```csharp
public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // 绑定全局的 IPlayerProvider
        Container.Bind<IPlayerProvider>()
            .To<PlayerProvider>()
            .AsSingle()    // 单例：整个游戏只有一个实例
            .NonLazy();    // 立即创建，不延迟
    }
}
```

**关键点**：
- 挂载在 ProjectContext 场景中
- 游戏启动时执行
- 绑定的对象跨场景存在

---

### 2. IPlayerProvider（接口）

**位置**：`Assets/Scripts/Interface/IPlayerProvider.cs`  
**作用**：定义访问 Player 的契约

```csharp
public interface IPlayerProvider
{
    GameObject Player { get; }           // 获取当前 Player
    void SetPlayer(GameObject player);   // 设置 Player
}
```

**关键点**：
- 使用接口而不是具体类，便于解耦和测试
- 全局系统通过这个接口访问 Player

---

### 3. PlayerProvider（实现类）

**位置**：`Assets/Scripts/Manager/PlayerProvider.cs`  
**作用**：存储 Player 引用

```csharp
public class PlayerProvider : IPlayerProvider
{
    public GameObject Player { get; private set; }
    
    public void SetPlayer(GameObject player)
    {
        Player = player;
    }
}
```

**关键点**：
- 纯 C# 类，不继承 MonoBehaviour
- 由容器管理生命周期
- 在全局容器中创建，跨场景存在

---

### 4. SceneInstaller（场景绑定）

**位置**：`Assets/Scripts/Installers/SceneInstaller.cs`  
**作用**：绑定 Scene 场景中的 GameObject

```csharp
public class SceneInstaller : MonoInstaller
{
    [SerializeField] private GameObject playerGameObject;
    [SerializeField] private Camera SceneCamera;
    
    public override void InstallBindings()
    {
        // 绑定场景中的 Player GameObject
        Container.Bind<GameObject>()
            .WithId("Player")
            .FromInstance(playerGameObject)
            .AsSingle();
        
        // 绑定场景中的 SceneCamera
        Container.Bind<GameObject>()
            .WithId("SceneCameraGo")
            .FromInstance(SceneCamera.gameObject)
            .AsSingle();
        
        // 绑定链接器，把场景 Player 设置到全局 Provider
        Container.BindInterfacesAndSelfTo<SceneObjectsLinker>()
            .AsSingle()
            .NonLazy();
    }
}
```

**关键点**：
- 挂载在 Scene 场景的 SceneContext 上
- 在 Inspector 中拖拽场景对象到字段
- `WithId("...")` 用于区分多个同类型绑定

---

### 5. SceneObjectsLinker（链接器）

**位置**：`Assets/Scripts/Installers/SceneInstaller.cs`（内部类）  
**作用**：桥接场景容器和全局容器

```csharp
public class SceneObjectsLinker : IInitializable
{
    private readonly IPlayerProvider _provider;  // 来自全局容器
    private readonly GameObject _player;         // 来自场景容器
    
    public SceneObjectsLinker(
        IPlayerProvider provider,                     // 从 ProjectContext 获取
        [Inject(Id = "Player")] GameObject player)    // 从 SceneContext 获取
    {
        _provider = provider;
        _player = player;
    }
    
    public void Initialize()
    {
        // UniDi 会自动调用这个方法
        _provider.SetPlayer(_player);
    }
}
```

**关键点**：
- 实现 `IInitializable` 接口，`Initialize()` 自动执行
- 构造函数同时注入全局和场景的依赖（跨容器注入）
- 负责把场景对象设置到全局 Provider

---

### 6. PlayerAngleViewSystem（使用方）

**位置**：`Assets/Scripts/System/Player/PlayerAngleViewSystem.cs`  
**作用**：使用 Player 实现游戏逻辑

```csharp
[GameSystem(CollectType.Auto)]
public class PlayerAngleViewSystem : GameSystem, ILateUpdate
{
    // 注入全局的 IPlayerProvider
    [Inject]
    private IPlayerProvider _playerProvider;
    
    // 注入场景中的 SceneCamera
    [Inject(Id = "SceneCameraGo")]
    private GameObject _SceneCamera;
    
    // 直接注入场景中的 Player
    [Inject(Id = "Player")]
    private GameObject _player;
    
    public override void Init()
    {
        // 优先使用直接注入的 Player
        var player = _player ?? _playerProvider?.Player;
        
        if (player != null)
        {
            ApplyPlayer(player);
        }
    }
    
    private void ApplyPlayer(GameObject player)
    {
        // 使用 Player 进行初始化
        // ...
    }
}
```

**关键点**：
- `[Inject]`：告诉容器这个字段需要注入
- `[Inject(Id = "...")]`：注入特定标识的对象
- 不需要手动查找或创建依赖

---

### 7. GameManager（手动注入）

**位置**：`Assets/Scripts/Manager/GameManager.cs`  
**作用**：初始化 GameSystem 并手动注入依赖

```csharp
public class GameManager : MonoBehaviour
{
    async void Start()
    {
        // 加载 Scene 场景
        var handle = SceneManager.LoadSceneAsync("Scene");
        await handle;
        
        // 场景加载完成后，创建并注入所有 GameSystem
        var types = GetTypesWithGameSystemAttribute();
        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            var system = instance as GameSystem;
            
            // 手动调用依赖注入
            ProjectContext.Instance.Container.Inject(system);
            
            // 注入完成后，调用 Init()
            system.Init();
            
            _gameSystems.TryAdd(type, system);
        }
    }
}
```

**关键点**：
- GameSystem 是手动创建的，不是容器创建的
- 必须手动调用 `Container.Inject()` 来注入依赖
- 在场景加载完成后执行，确保场景对象已绑定

---

## 使用示例

### 场景 1：在 System 中使用场景对象

```csharp
public class MySystem : GameSystem
{
    // 直接注入场景对象
    [Inject(Id = "Player")]
    private GameObject _player;
    
    [Inject(Id = "SceneCameraGo")]
    private GameObject _SceneCamera;
    
    public override void Init()
    {
        // 直接使用，已自动注入
        Debug.Log($"Player: {_player.name}");
        Debug.Log($"Camera: {_SceneCamera.name}");
    }
}
```

### 场景 2：在 System 中使用全局 Provider

```csharp
public class MySystem : GameSystem
{
    // 注入全局 Provider（适用于跨场景）
    [Inject]
    private IPlayerProvider _playerProvider;
    
    public override void Init()
    {
        // 通过 Provider 获取 Player
        var player = _playerProvider.Player;
        
        if (player != null)
        {
            Debug.Log($"Player from Provider: {player.name}");
        }
    }
}
```

### 场景 3：添加新的场景对象绑定

**步骤 1：在 SceneInstaller 中添加字段和绑定**

```csharp
public class SceneInstaller : MonoInstaller
{
    [SerializeField] private GameObject playerGameObject;
    [SerializeField] private GameObject enemyManager;  // 新增
    
    public override void InstallBindings()
    {
        // 绑定 Player
        Container.Bind<GameObject>()
            .WithId("Player")
            .FromInstance(playerGameObject)
            .AsSingle();
        
        // 绑定 EnemyManager
        Container.Bind<GameObject>()
            .WithId("EnemyManager")
            .FromInstance(enemyManager)
            .AsSingle();
    }
}
```

**步骤 2：在 Inspector 中拖拽对象**

1. 打开 Scene 场景
2. 选中 SceneContext GameObject
3. 找到 SceneInstaller 组件
4. 将 EnemyManager GameObject 拖拽到对应字段

**步骤 3：在 System 中使用**

```csharp
public class EnemySystem : GameSystem
{
    [Inject(Id = "EnemyManager")]
    private GameObject _enemyManager;
    
    public override void Init()
    {
        Debug.Log($"EnemyManager: {_enemyManager.name}");
    }
}
```

---

## 常见问题

### Q1: 什么时候用 Provider，什么时候直接注入？

**使用 Provider（通过 IPlayerProvider）：**
- 对象需要跨场景访问
- 对象需要在运行时动态切换
- 多个场景都有同一类型的对象

**直接注入（通过 [Inject(Id = "...")])：**
- 对象只在当前场景使用
- 不需要跨场景访问
- 绑定关系简单明确

### Q2: [Inject(Id = "...")] 中的 Id 怎么匹配？

```csharp
// SceneInstaller.cs - 绑定时指定 Id
Container.Bind<GameObject>()
    .WithId("Player")  // ← 这个 Id
    .FromInstance(playerGameObject)
    .AsSingle();

// System.cs - 注入时使用相同 Id
[Inject(Id = "Player")]  // ← 必须完全匹配（大小写敏感）
private GameObject _player;
```

### Q3: 为什么字段是 null？

**检查清单：**
1. SceneInstaller 是否挂载在 SceneContext 上
2. Inspector 中的字段是否已拖拽赋值
3. Id 是否匹配（大小写敏感）
4. 场景是否已加载（System 创建前场景必须加载）
5. 是否调用了 `Container.Inject(system)`

### Q4: AsSingle、AsCached、AsTransient 的区别？

- **AsSingle**：单例，整个容器只创建一个实例（推荐）
- **AsCached**：缓存，同一次解析过程中返回同一实例
- **AsTransient**：临时，每次请求都创建新实例（慎用，性能差）

### Q5: 如何调试 DI 注入问题？

```csharp
public override void Init()
{
    // 添加日志检查注入是否成功
    if (_player == null)
    {
        Debug.LogError("Player is null! Check SceneInstaller bindings.");
        return;
    }
    
    Debug.Log($"Injected Player: {_player.name}");
}
```

---

## 总结

### 核心流程

```
ProjectInstaller（全局）
    → 绑定 IPlayerProvider
    
SceneInstaller（场景）
    → 绑定场景对象（Player, Camera 等）
    → 绑定 SceneObjectsLinker
    
SceneObjectsLinker
    → 把场景 Player 设置到全局 Provider
    
GameManager
    → 创建 GameSystem
    → 调用 Container.Inject(system)
    
GameSystem
    → [Inject] 字段自动赋值
    → Init() 中使用依赖
```

### 关键原则

1. **接口优先**：使用接口而不是具体类
2. **明确 Id**：多个同类型绑定必须用 Id 区分
3. **场景分离**：全局对象在 ProjectInstaller，场景对象在 SceneInstaller
4. **手动注入**：手动创建的对象必须调用 `Container.Inject()`

---

**文档版本：** 1.0  
**最后更新：** 2024年11月  
**项目：** Dream (Unity + UniDi)

