using Dream;
namespace System
{
    /// <summary>
    /// 游戏系统基类：所有 System 继承此类
    /// 支持通过 UniDi 自动注入依赖（使用 [Inject] 标记字段）
    /// </summary>
    public abstract class GameSystem
    {
        /// <summary>
        /// 初始化系统（依赖注入完成后调用）
        /// </summary>
        [AutoCall]
        public abstract void Init();

        /// <summary>
        /// 手动初始化（可选，支持自定义参数）
        /// </summary>
        public virtual void ManualInit(object[] objs) { }

        /// <summary>
        /// 释放资源（取消订阅事件等）
        /// </summary>
        public abstract void ManualDispose();
    }
}