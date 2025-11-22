using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
namespace Dream
{
   public class ResourcesManager
    {
        // 单例实例
        private readonly static ResourcesManager resourceManager = new ResourcesManager();
        
        // 获取单例实例
        public static ResourcesManager Inst => resourceManager;

        // 已加载资源的缓存句柄
        private static Dictionary<string, AssetHandle> assetOperationHandles = new Dictionary<string, AssetHandle>();

        // 已加载的配置对象缓存，按类型区分
        private readonly Dictionary<Type, Config.Config> configs = new Dictionary<Type, Config.Config>();

        /// <summary>
        /// 初始化资源系统（使用 YooAsset）
        /// 根据平台区分编辑器模拟模式与离线模式初始化
        /// </summary>
        public async static UniTask Initialize()
        {
            // 初始化 YooAsset 框架
            YooAssets.Initialize();

            // 获取或创建默认资源包
            var package = YooAssets.TryGetPackage("DefaultPackage");
            if (package == null)
            {
                package = YooAssets.CreatePackage("DefaultPackage");
                YooAssets.SetDefaultPackage(package);
            }

#if UNITY_EDITOR
            try
            {
                // 编辑器模拟构建模式，模拟真实包加载流程
                var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");

                // 获取资源包根目录
                var packageRoot = buildResult.PackageRootDirectory;

                // 创建模拟文件系统参数
                var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

                // 设置模拟模式初始化参数
                var initParameters = new EditorSimulateModeParameters();
                initParameters.EditorFileSystemParameters = editorFileSystemParams;

                // 异步初始化资源包
                var initOperation = package.InitializeAsync(initParameters);
                await initOperation;

                // 获取包版本并更新资源清单
                var op = package.RequestPackageVersionAsync();
                await op;
                await package.UpdatePackageManifestAsync(op.PackageVersion);

                if (initOperation.Status == EOperationStatus.Succeed)
                {
                    Debug.Log("资源包初始化成功（EditorSimulateMode）");
                    return;
                }
                else
                {
                    Debug.LogWarning("资源包初始化失败（EditorSimulateMode），将回退到 Offline 模式");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"EditorSimulate 初始化失败：{e.Message}\n回退到 Offline 模式");
            }

            // 回退：使用离线模式参数
            {
                var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                var initParameters = new OfflinePlayModeParameters();
                initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                await package.InitializeAsync(initParameters);
                var op = package.RequestPackageVersionAsync();
                await op;
                await package.UpdatePackageManifestAsync(op.PackageVersion);
                Debug.Log("资源包初始化成功（OfflinePlayMode 回退）");
            }
#else
            // 构建发布平台使用的离线模式
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            var initParameters = new OfflinePlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystemParams;

            // 异步初始化离线资源包
            await package.InitializeAsync(initParameters);

            // 获取版本号并更新清单
            var op = package.RequestPackageVersionAsync();
            await op;
            await package.UpdatePackageManifestAsync(op.PackageVersion);
#endif
        }

        /// <summary>
        /// 同步加载指定路径的资源，类型为 T（UnityEngine.Object 子类）
        /// 自动缓存并释放句柄
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetPath">资源路径</param>
        /// <returns>加载完成的资源对象</returns>
        public static T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            // 若缓存中不存在，则同步加载
            if (!assetOperationHandles.TryGetValue(assetPath, out AssetHandle handle))
            {
                handle = YooAssets.LoadAssetSync<T>(assetPath);
                assetOperationHandles[assetPath] = handle;
            }

            // 获取资源对象
            var resource = handle.GetAssetObject<T>();

            // 立即释放句柄，移除缓存（YooAsset 设计允许立即释放）
            handle.Release();
            assetOperationHandles.Remove(assetPath);

            return resource;
        }

        /// <summary>
        /// 获取所有带有 ConfigAttribute 特性的类型（用于配置表加载）
        /// </summary>
        /// <returns>类型集合</returns>
        private static IEnumerable<Type> GetTypesWithConfigAttribute()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    // 有些程序集可能无法反射出类型，避免抛出异常
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        return Array.Empty<Type>();
                    }
                })
                .Where(type => type.GetCustomAttribute<ConfigAttribute>() != null);
        }

        /// <summary>
        /// 加载所有配置类型，并调用其 LoadConfig 方法初始化
        /// 只加载带有 ConfigAttribute 特性的类型
        /// </summary>
        public async UniTask LoadAllConfig()
        {
            // 获取所有标记了 ConfigAttribute 的配置类类型
            var configTypes = GetTypesWithConfigAttribute();

            // 遍历所有配置类型进行初始化
            foreach (var type in configTypes)
            {
                try
                {
                    // 动态创建实例
                    var instance = Activator.CreateInstance(type);

                    // 强制转为 Config 类型
                    var config = instance as Config.Config;
                    
                   await config.LoadConfig();
                   
                    configs.TryAdd(type, config);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to initialize config {type.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 获取指定类型的配置对象（已加载的）
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <returns>配置实例对象</returns>
        public T GetConfig<T>() where T : Config.Config
        {
            // 从缓存中获取指定类型的配置
            if (configs.TryGetValue(typeof(T), out Config.Config config))
            {
                return config as T;
            }

            return null;
        }
    }
}