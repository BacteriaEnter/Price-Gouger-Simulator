using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace Interface
{
    // 它是 VContainer 唯一的 EntryPoint，负责调度别人
    public class AsyncLifecycleExecutor : IStartable
    {
        readonly IEnumerable<IUniTaskStartable> _asyncSystems;

        // VContainer 会按照注册顺序，把所有 IUniTaskStartable 打包传进来
        public AsyncLifecycleExecutor(IEnumerable<IUniTaskStartable> asyncSystems)
        {
            _asyncSystems = asyncSystems;
        }

        public async void Start()
        { 
            Debug.Log(">>> [Executor] 开始异步初始化流程...");

            foreach (var system in _asyncSystems)
            {
                var name = system.GetType().Name;
                Debug.Log($"-> 正在启动: {name} ...");
                
                // 关键：串行等待！
                await system.AsyncStart();
                
                Debug.Log($"<- {name} 启动完成");
            }

            Debug.Log(">>> [Executor] 所有系统就绪，游戏正式运行 <<<");
        }
    }
}