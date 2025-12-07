using System;
using VContainer.Unity;
namespace PriceSystem.Base
{
    /// <summary>
    /// 游戏系统基类。
    /// <para>提供标准的生命周期管理接口。</para>
    /// <para>直接重写 Start/Tick/Dispose 即可实现逻辑。</para>
    /// </summary>
    public abstract class GameSystem : IStartable, ITickable, ILateTickable, IDisposable
    {
        /// <summary>
        /// 同步启动入口。
        /// <para>由 VContainer 在构建完成后自动调用。</para>
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// 每帧逻辑更新。
        /// <para>由 VContainer 驱动。</para>
        /// </summary>
        public virtual void Tick() { }

        /// <summary>
        /// 每帧后期逻辑更新。
        /// <para>由 VContainer 驱动。</para>
        /// </summary>
        public virtual void LateTick() { }

        /// <summary>
        /// 资源销毁与清理。
        /// <para>当 Scope 被销毁时自动调用。</para>
        /// </summary>
        public virtual void Dispose() { }
    }
}