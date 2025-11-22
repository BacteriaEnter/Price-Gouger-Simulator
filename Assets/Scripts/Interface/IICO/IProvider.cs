using UnityEngine;
namespace Interface
{

    /// <summary>
    /// 通用 Provider 接口：提供跨场景访问任意类型对象的接口
    /// </summary>
    /// <typeparam name="T">要提供的对象类型（如 GameObject、Camera 等）</typeparam>
    public interface IProvider<T> where T : Object
    {
        /// <summary>
        /// 获取当前场景中的对象实例
        /// </summary>
        T Value { get; }

        /// <summary>
        /// 设置对象实例（由链接器在场景加载后调用）
        /// </summary>
        void SetValue(T value);
    }

}