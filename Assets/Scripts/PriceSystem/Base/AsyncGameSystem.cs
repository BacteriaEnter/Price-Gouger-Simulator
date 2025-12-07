using Cysharp.Threading.Tasks;
using Interface;
namespace PriceSystem.Base
{
    /// <summary>
    /// 异步游戏系统基类。
    /// <para>适用于需要异步加载资源（如配置表、预制体）的系统。</para>
    /// <para>特性：</para>
    /// <para>1. 自动对接 AsyncLifecycleExecutor。</para>
    /// <para>2. 内置 Ready 锁，在 AsyncStart 完成前自动拦截 Tick/LateTick。</para>
    /// </summary>
    public abstract class AsyncGameSystem : GameSystem, IUniTaskStartable
    {
        /// <summary>
        /// 系统就绪状态锁。
        /// <para>true: 初始化已完成，Tick/LateTick 可以运行。</para>
        /// <para>false: 初始化未完成，Tick/LateTick 被拦截。</para>
        /// </summary>
        private bool _isReady = false;

        /// <summary>
        /// 异步启动入口。
        /// <para>由 AsyncLifecycleExecutor 负责调度。</para>
        /// </summary>
        public async UniTask AsyncStart()
        {
            // 执行子类的异步初始化逻辑
            await OnStartAsync();
            
            // 初始化完毕，打开安全锁
            _isReady = true; 
        }

        /// <summary>
        /// 拦截 Tick，添加 Ready 检查
        /// </summary>
        public override void Tick()
        {
            if (!_isReady) return;
            base.Tick();
        }

        /// <summary>
        /// 拦截 LateTick，添加 Ready 检查
        /// </summary>
        public override void LateTick()
        {
            if (!_isReady) return;
            base.LateTick();
        }
        
        /// <summary>
        /// 异步初始化逻辑。
        /// </summary>
        protected abstract UniTask OnStartAsync();
    }
}
