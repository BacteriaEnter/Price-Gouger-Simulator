using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace PriceManager
{
    public class EventManager
    {
        // 多线程安全的字典，存储事件主题和对应的委托集合
        private readonly ConcurrentDictionary<string, HashSet<Delegate>> _eventConcurrentDictionary = new ConcurrentDictionary<string, HashSet<Delegate>>();

        /// <summary>
        /// 静态构造函数，初始化单例实例
        /// </summary>
        static EventManager()
        {
            
        }

        /// <summary>
        /// 订阅一个不带参数的事件。
        /// </summary>
        /// <param name="topic">事件名称，非空非空字符串</param>
        /// <param name="callback">事件触发时要执行的无参回调</param>
        /// <exception cref="ArgumentException">当 topic 为空或空白时抛出</exception>
        /// <exception cref="ArgumentNullException">当 callback 为 null 时抛出</exception>
        public void Subscribe(string topic, Action callback)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("事件主题不能为空", nameof(topic));
            }
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            _eventConcurrentDictionary.AddOrUpdate(topic, new HashSet<Delegate> {callback},
                (_, callbacks) =>
                {
                    callbacks.Add(callback);
                    return callbacks;
                });
        }

        /// <summary>
        /// 订阅一个带一个参数的事件。
        /// </summary>
        /// <typeparam name="T">回调参数类型</typeparam>
        /// <param name="topic">事件名称</param>
        /// <param name="callback">事件触发时要执行的回调，接收一个 T 类型参数</param>
        public void Subscribe<T>(string topic, Action<T> callback)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("事件主题不能为空", nameof(topic));
            }
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            _eventConcurrentDictionary.AddOrUpdate(topic,
                new HashSet<Delegate> {callback},
                (_, callbacks) =>
                {
                    callbacks.Add(callback);
                    return callbacks;
                });
        }

        /// <summary>
        /// 订阅一个带两个参数的事件。
        /// </summary>
        /// <typeparam name="T1">第一个参数类型</typeparam>
        /// <typeparam name="T2">第二个参数类型</typeparam>
        /// <param name="topic">事件名称</param>
        /// <param name="callback">事件触发时要执行的回调，接收两个参数</param>
        public void Subscribe<T1, T2>(string topic, Action<T1, T2> callback)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("事件主题不能为空", nameof(topic));
            }
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            _eventConcurrentDictionary.AddOrUpdate(topic,
                new HashSet<Delegate> {callback},
                (_, callbacks) =>
                {
                    callbacks.Add(callback);
                    return callbacks;
                });
        }

        /// <summary>
        /// 异步取消订阅一个不带参数的事件。延期一帧执行，避免在回调内部立即修改集合导致遍历时出现错误。
        /// </summary>
        /// <param name="topic">事件名称</param>
        /// <param name="callback">之前注册过的回调</param>
        public async UniTask Unsubscribe(string topic, Action callback)
        {
            // 延迟一帧，保证当前帧的 Publish 循环能安全完成
            await UniTask.DelayFrame(1);

            if (string.IsNullOrWhiteSpace(topic) || callback == null)
            {
                return;
            }

            if (_eventConcurrentDictionary.TryGetValue(topic, out var callbacks))
            {
                callbacks.Remove(callback);
                //如果这个主题没有事件方法了就把这个主题也删除。
                if (callbacks.Count == 0)
                {
                    _eventConcurrentDictionary.TryRemove(topic, out _);
                }
            }
        }

        /// <summary>
        /// 异步取消订阅一个带一个参数的事件。
        /// </summary>
        /// <param name="topic">事件名称</param>
        /// <param name="callback">之前注册过的有一个参数的回调</param>
        public async UniTask Unsubscribe<T>(string topic, Action<T> callback)
        {
            await UniTask.DelayFrame(1);
            if (string.IsNullOrWhiteSpace(topic) || callback == null)
            {
                return;
            }

            if (_eventConcurrentDictionary.TryGetValue(topic, out var callbacks))
            {
                callbacks.Remove(callback);
                if (callbacks.Count == 0)
                {
                    _eventConcurrentDictionary.TryRemove(topic, out _);
                }
            }
        }

        /// <summary>
        /// 异步取消订阅一个带两个参数的事件。
        /// </summary>
        /// <param name="topic">事件名称</param>
        /// <param name="callback">之前注册过的有两个参数的回调</param>
        public async UniTask Unsubscribe<T1, T2>(string topic, Action<T1, T2> callback)
        {
            await UniTask.DelayFrame(1);
            if (string.IsNullOrWhiteSpace(topic) || callback == null)
            {
                return;
            }

            if (_eventConcurrentDictionary.TryGetValue(topic, out var callbacks))
            {
                callbacks.Remove(callback);
                if (callbacks.Count == 0)
                {
                    _eventConcurrentDictionary.TryRemove(topic, out _);
                }
            }
        }

        /// <summary>
        /// 发布一个不带参数的事件，依次调用所有订阅者。
        /// </summary>
        /// <param name="topic">事件名称</param>
        public void Publish(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return;
            }

            if (_eventConcurrentDictionary.TryGetValue(topic, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    try
                    {
                        if (callback is Action typedCallback)
                        {
                            typedCallback();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"事件执行时出错: {ex.Message}\n{ex.StackTrace}");
                        // 保留调用链
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 发布一个带一个参数的事件，依次调用所有匹配 Action T 的订阅者。
        /// </summary>
        /// <param name="topic">事件名称</param>
        /// <param name="message">参数值</param>
        public void Publish<T>(string topic, T message)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return;
            }

            if (_eventConcurrentDictionary.TryGetValue(topic, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    try
                    {
                        if (callback is Action<T> typedCallback)
                        {
                            typedCallback(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"事件执行时出错: {ex.Message}\n{ex.StackTrace}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 发布一个带两个参数的事件，依次调用所有匹配 Action T1,T2 的订阅者。
        /// </summary>
        /// <param name="topic">事件名称</param>
        /// <param name="param1">T1 参数一</param>
        /// <param name="param2"></param>
        public void Publish<T1, T2>(string topic, T1 param1, T2 param2)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return;
            }

            if (_eventConcurrentDictionary.TryGetValue(topic, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    try
                    {
                        if (callback is Action<T1, T2> typedCallback)
                        {
                            typedCallback(param1, param2);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"事件执行时出错: {ex.Message}\n{ex.StackTrace}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 清除所有已注册的主题和回调，一般在切换场景或重置游戏状态时使用
        /// </summary>
        public void Clear()
        {
            _eventConcurrentDictionary.Clear();
        }
    }
}