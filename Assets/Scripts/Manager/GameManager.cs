using System;
using System.Collections.Generic;
using System.Debug;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Dream;
using Events;
using Interface.IUntiy;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Manager
{
    /// <summary>
    /// 游戏管理器，负责管理游戏系统的生命周期和初始化
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // 存储所有游戏系统的字典，键为系统类型，值为系统实例
        private readonly static Dictionary<Type, GameSystem> _gameSystems = new Dictionary<Type, GameSystem>();

        // 存储所有需要 LateUpdate 的组件
        private HashSet<ILateUpdate> _lateUpdatesComponent = new HashSet<ILateUpdate>();
        
        /// <summary>
        /// Unity Awake 方法，在对象创建时调用
        /// </summary>
        public void Awake()
        {
            Debug.Log("初始化GameManager");
            //确保不被销毁
            DontDestroyOnLoad(gameObject);
            //设置帧率
            Application.targetFrameRate = 400;
        }

        /// <summary>
        /// OnEnable 时订阅 LateUpdate 注册事件
        /// </summary>
        private void OnEnable()
        {
            EventManager.Instance.Subscribe<ILateUpdate>(GameEvents.LATE_UPDATE_REGISTER, LateUpdateRegister);
        }
        
        
        /// <summary>
        /// OnDisable 时取消订阅 LateUpdate 注册事件
        /// </summary>
        private async void OnDisable()
        {
            await EventManager.Instance.Unsubscribe<ILateUpdate>(GameEvents.LATE_UPDATE_REGISTER, LateUpdateRegister);
        }
        
        /// <summary>
        /// Unity Start 方法，在 Awake 之后调用，执行游戏初始化逻辑
        /// </summary>
        private async void Start()
        {
            // await LoadResources(); //加载资源
            DebugConsoleSystem.Instance.Init(); //初始化调试系统

            // 【重要】先加载 Main 场景：确保场景级绑定（如 Player）先通过 MainInstaller 完成
            // 场景加载时会自动创建 SceneContext，执行 MainInstaller，绑定场景对象
            var handle = SceneManager.LoadSceneAsync("Main"); //加载主场景
            await handle;
            // 此时 Main 场景已加载，SceneContext 已创建，MainInstaller 已执行
            // PlayerProviderLinker.Initialize() 已执行，PlayerProvider 已设置好 Player
         
            // 场景加载完成后，创建并初始化所有 GameSystem
            var types = GetTypesWithGameSystemAttribute();
            foreach (var type in types)
            {
                // 1. 手动创建系统实例
                var instance = Activator.CreateInstance(type);
                var system = instance as GameSystem;
                
                // 2. 手动注入依赖（查找 [Inject] 字段并赋值）
                UniDi.ProjectContext.Instance.Container.Inject(system);
                
                // 3. 调用 Init()，此时所有依赖已注入完成
                system.Init();
                _gameSystems.TryAdd(type, system);
            }
        }

        /// <summary>
        /// Unity LateUpdate 方法，在每帧的 LateUpdate 阶段调用所有注册的组件
        /// </summary>
        private void LateUpdate()
        {
            if (_lateUpdatesComponent.Count <= 0)
            {
                return;
            }

            foreach (var lateUpdate in _lateUpdatesComponent)
            {
                lateUpdate.LateUpdate();
            }
        }

        /// <summary>
        /// 获取所有带有 GameSystemAttribute 特性的类型
        /// </summary>
        /// <returns>类型集合</returns>
        private static IEnumerable<Type> GetTypesWithGameSystemAttribute()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        return Array.Empty<Type>();
                    }
                })
                .Where(type => type.GetCustomAttribute<GameSystemAttribute>() != null && type.GetCustomAttribute<GameSystemAttribute>().collectType == CollectType.Auto);
    
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <returns>异步任务</returns>
        private async UniTask LoadResources()
        {
            // await ResourcesManager.Initialize();
            // await ResourcesManager.Inst.LoadAllConfig();
        }

        /// <summary>
        /// 获取单个指定 GameSystem
        /// </summary>
        /// <typeparam name="T">继承了 GameSystem 的类</typeparam>
        /// <returns>指定的 GameSystem</returns>
        public static T GetGameSystem<T>() where T : GameSystem
        {
            return _gameSystems[typeof(T)] as T;
        }

        /// <summary>
        /// 注册需要 LateUpdate 的组件，统一进行 LateUpdate 调用
        /// </summary>
        /// <param name="iLateUpdate">实现了 ILateUpdate 接口的组件</param>
        private void LateUpdateRegister(ILateUpdate iLateUpdate)
        {
            _lateUpdatesComponent.Add(iLateUpdate);
        }

        /// <summary>
        /// 手动添加管理器到游戏系统中
        /// </summary>
        /// <typeparam name="T">管理器类型</typeparam>
        /// <param name="manager">管理器实例</param>
        /// <param name="objects">初始化参数</param>
        public static void ManuallyAddManager<T>(T manager, object[] objects) where T : GameSystem
        {
            if (_gameSystems.TryAdd(typeof(T), manager))
            {
                manager.ManualInit(objects);
            }
        }
    }
}