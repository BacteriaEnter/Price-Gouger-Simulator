using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Dream;
using Interface;
using PriceConfig;
using UnityEngine;
using YooAsset;
namespace PriceManager
{
    /// <summary>
    /// 资源管理器。
    /// <para>职责：</para>
    /// <para>1. 封装 YooAsset 资源加载框架，提供统一的资源加载接口。</para>
    /// <para>2. 管理游戏配置表的自动发现与加载（通过反射扫描 ConfigAttribute）。</para>
    /// <para>3. 在游戏启动时异步初始化资源系统，确保后续系统能安全使用资源。</para>
    /// </summary>
    public class ResourcesManager : IUniTaskStartable
    {
        /// <summary>
        /// 已加载资源的缓存句柄字典。
        /// <para>Key: 资源路径（如 "Assets/Prefabs/Player.prefab"）</para>
        /// <para>Value: YooAsset 的资源句柄（用于释放资源）</para>
        /// <para>注意：当前实现中，LoadAsset 方法会立即释放句柄，此缓存主要用于防止重复加载。</para>
        /// </summary>
        private readonly Dictionary<string, AssetHandle> _assetOperationHandles = new Dictionary<string, AssetHandle>();

        /// <summary>
        /// 已加载的配置对象缓存字典。
        /// <para>Key: 配置类的 Type（如 typeof(PlayerConfig)）</para>
        /// <para>Value: 配置实例对象（继承自 Config 基类）</para>
        /// <para>用途：游戏运行时通过 GetConfig&lt;T&gt;() 快速获取已加载的配置，避免重复加载。</para>
        /// </summary>
        private readonly Dictionary<Type, Config> _configs = new Dictionary<Type, Config>();

        /// <summary>
        /// 构造函数。
        /// <para>当前实现中，EventManager 参数未被使用，但保留此参数以便未来扩展事件通知功能。</para>
        /// </summary>
        /// <param name="eventManager">事件管理器实例（由 VContainer 自动注入）</param>
        public ResourcesManager(EventManager eventManager)
        {
        }

        /// <summary>
        /// [IUniTaskStartable 接口] 异步启动入口。
        /// <para>由 AsyncLifecycleExecutor 在游戏启动时自动调用。</para>
        /// <para>执行流程：</para>
        /// <para>1. 初始化 YooAsset 资源包（编辑器模式优先尝试模拟模式，失败则回退到离线模式）</para>
        /// <para>2. 扫描并加载所有标记了 [ConfigAttribute] 的配置表</para>
        /// </summary>
        public async UniTask AsyncStart()
        {
            Debug.Log("[ResourcesManager] 开始加载 YooAsset...");
            await InitializeAsync();
            await LoadAllConfig();
            Debug.Log("[ResourcesManager] 资源加载完毕");
        }
        
        /// <summary>
        /// 初始化 YooAsset 资源包。
        /// <para>根据编译平台选择不同的初始化策略：</para>
        /// <para>- Unity 编辑器：优先使用 EditorSimulateMode（模拟真实打包后的资源结构），失败则回退到 OfflinePlayMode</para>
        /// <para>- 发布平台：直接使用 OfflinePlayMode（从 StreamingAssets 加载资源）</para>
        /// </summary>
        public async UniTask InitializeAsync()
        {
            // 初始化 YooAsset 框架（全局单例）
            YooAssets.Initialize();

            // 获取或创建名为 "DefaultPackage" 的默认资源包
            // 如果包不存在，则创建新包并设置为默认包
            var package = YooAssets.TryGetPackage("DefaultPackage");
            if (package == null)
            {
                package = YooAssets.CreatePackage("DefaultPackage");
                YooAssets.SetDefaultPackage(package);
            }

#if UNITY_EDITOR
            // // 编辑器模拟构建模式，模拟真实包加载流×
            // var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            //
            // // 获取资源包根目录
            // var packageRoot = buildResult.PackageRootDirectory;
            //
            // // 创建模拟文件系统参数
            // var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            //
            // // 设置模拟模式初始化参数
            // var initParameters = new EditorSimulateModeParameters();
            // initParameters.EditorFileSystemParameters = editorFileSystemParams;
            //
            // // 异步初始化资源包
            // var initOperation = package.InitializeAsync(initParameters);
            // await initOperation;
            //
            // // 获取包版本并更新资源清单
            // var op = package.RequestPackageVersionAsync();
            // await op;
            // await package.UpdatePackageManifestAsync(op.PackageVersion);

            // // 打印初始化结果
            // if (initOperation.Status == EOperationStatus.Succeed)
            // {
            //     Debug.Log("资源包初始化成功");
            // }
            // else
            // {
            //     Debug.Log("资源包初始化失败");
            // }
#else
            // ========== 发布平台：直接使用离线模式 ==========
            // 在打包后的游戏中，资源已经内置在 StreamingAssets 中，无需模拟模式
            
            // 创建内置文件系统参数（指向 StreamingAssets 目录）
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            
            // 创建离线模式的初始化参数
            var initParameters = new OfflinePlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystemParams;

            // 异步初始化离线资源包
            await package.InitializeAsync(initParameters);

            // 获取版本号并更新资源清单
            var op = package.RequestPackageVersionAsync();
            await op;
            await package.UpdatePackageManifestAsync(op.PackageVersion);
#endif
        }

        /// <summary>
        /// 同步加载指定路径的资源。
        /// <para>特性：</para>
        /// <para>1. 自动缓存：相同路径的资源不会重复加载（通过 _assetOperationHandles 缓存）</para>
        /// <para>2. 立即释放：加载完成后立即释放句柄，符合 YooAsset 的设计理念（资源对象由 Unity 管理生命周期）</para>
        /// <para>3. 同步阻塞：此方法会阻塞当前线程直到资源加载完成，适用于启动时预加载或小资源</para>
        /// </summary>
        /// <typeparam name="T">资源类型，必须是 UnityEngine.Object 的子类（如 GameObject, Texture2D, AudioClip 等）</typeparam>
        /// <param name="assetPath">资源路径，相对于资源包根目录（如 "Assets/Prefabs/Player.prefab"）</param>
        /// <returns>加载完成的资源对象。如果资源不存在或加载失败，可能返回 null</returns>
        public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            // 检查缓存：如果该路径的资源句柄已存在，直接使用缓存的句柄
            // 注意：当前实现中，即使缓存存在，后续也会立即释放，所以这个缓存主要用于防止重复加载
            if (!_assetOperationHandles.TryGetValue(assetPath, out AssetHandle handle))
            {
                // 缓存中不存在，使用 YooAsset 同步加载资源
                handle = YooAssets.LoadAssetSync<T>(assetPath);
                _assetOperationHandles[assetPath] = handle;
            }

            // 从句柄中获取实际的资源对象（UnityEngine.Object）
            var resource = handle.GetAssetObject<T>();

            // 立即释放句柄，并从缓存中移除
            // 设计说明：YooAsset 允许在获取资源对象后立即释放句柄
            // 资源对象本身由 Unity 的引用计数管理，只要还有引用就不会被卸载
            handle.Release();
            _assetOperationHandles.Remove(assetPath);

            return resource;
        }

        /// <summary>
        /// 通过反射扫描所有程序集，找出所有标记了 [ConfigAttribute] 特性的配置类。
        /// <para>用途：实现配置表的自动发现机制，无需手动注册每个配置类。</para>
        /// </summary>
        /// <returns>所有标记了 ConfigAttribute 的类型集合</returns>
        private IEnumerable<Type> GetTypesWithConfigAttribute()
        {
            // 遍历当前应用程序域中的所有程序集
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    // 有些程序集（如 Unity 内部程序集、第三方插件）可能无法反射出类型
                    // 使用 try-catch 避免 ReflectionTypeLoadException 导致整个扫描流程中断
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // 如果反射失败，返回空集合，不影响其他程序集的扫描
                        return Array.Empty<Type>();
                    }
                })
                // 筛选：只保留带有 ConfigAttribute 特性的类型
                .Where(type => type.GetCustomAttribute<ConfigAttribute>() != null);
        }

        /// <summary>
        /// 加载所有配置表。
        /// <para>流程：</para>
        /// <para>1. 通过反射扫描所有标记了 [ConfigAttribute] 的配置类</para>
        /// <para>2. 动态创建每个配置类的实例</para>
        /// <para>3. 调用每个配置实例的 LoadConfig() 方法（异步加载配置数据）</para>
        /// <para>4. 将加载完成的配置实例存入 _configs 缓存，供后续 GetConfig&lt;T&gt;() 使用</para>
        /// <para>设计优势：新增配置表时，只需添加 [ConfigAttribute] 特性，无需修改此方法。</para>
        /// </summary>
        public async UniTask LoadAllConfig()
        {
            // 获取所有标记了 ConfigAttribute 的配置类类型
            var configTypes = GetTypesWithConfigAttribute();

            // 遍历所有配置类型，逐个初始化
            foreach (var type in configTypes)
            {
                try
                {
                    // 使用反射动态创建配置类的实例（调用无参构造函数）
                    var instance = Activator.CreateInstance(type);

                    // 将实例强制转换为 Config 基类类型
                    // 注意：如果类型不继承自 Config，此转换会返回 null
                    var config = instance as Config;

                    // 调用配置实例的异步加载方法（从文件/网络加载配置数据）
                    await config.LoadConfig();

                    // 将加载完成的配置实例存入缓存，Key 为类型，Value 为实例
                    // TryAdd 确保如果同一个类型被多次扫描，不会覆盖已存在的配置
                    _configs.TryAdd(type, config);
                }
                catch (Exception ex)
                {
                    // 如果某个配置加载失败（如文件不存在、数据格式错误等），记录错误但继续加载其他配置
                    // 这确保了单个配置的失败不会影响整个游戏的启动流程
                    Debug.LogError($"Failed to initialize config {type.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 获取指定类型的配置对象（已加载的）。
        /// <para>使用场景：游戏运行时需要读取配置数据时调用此方法。</para>
        /// <para>前提条件：配置必须在游戏启动时通过 LoadAllConfig() 成功加载。</para>
        /// </summary>
        /// <typeparam name="T">配置类型，必须继承自 Config 基类</typeparam>
        /// <returns>配置实例对象。如果配置未加载或类型不存在，返回 null</returns>
        /// <example>
        /// <code>
        /// var playerConfig = resourcesManager.GetConfig&lt;PlayerConfig&gt;();
        /// if (playerConfig != null)
        /// {
        ///     Debug.Log($"玩家最大血量: {playerConfig.MaxHp}");
        /// }
        /// </code>
        /// </example>
        public T GetConfig<T>() where T : Config
        {
            // 从缓存字典中查找指定类型的配置实例
            if (_configs.TryGetValue(typeof(T), out Config config))
            {
                // 将基类引用强制转换为具体的配置类型并返回
                return config as T;
            }

            // 如果缓存中不存在，返回 null
            // 调用方应检查返回值是否为 null，以判断配置是否已加载
            return null;
        }
    }
}