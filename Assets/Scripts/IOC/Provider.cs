using Interface;
using UnityEngine;
namespace IOC
{
    /// <summary>
    /// 通用 Provider 实现类：存储任意类型对象的引用
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public class Provider<T> : IProvider<T> where T : Object
    {
        public T Value { get; private set; }

        public void SetValue(T value)
        {
            Value = value;
        }
    }
}