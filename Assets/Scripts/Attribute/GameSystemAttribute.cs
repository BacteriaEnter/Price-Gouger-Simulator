using System;

namespace Dream
{
    /// <summary>
    /// 游戏系统特性，用于标记游戏系统类，指定其收集和管理方式
    /// </summary>
    public class GameSystemAttribute : Attribute
    {
        // 收集类型
        public CollectType collectType;

        /// <summary>
        /// 构造函数，初始化游戏系统特性
        /// </summary>
        /// <param name="collectType">收集类型</param>
        public GameSystemAttribute(CollectType collectType)
        {
            this.collectType = collectType;
        }
    }

    /// <summary>
    /// 收集类型枚举，定义游戏系统的管理方式
    /// </summary>
    public enum CollectType
    {
        /// <summary>
        /// 自动收集和管理
        /// </summary>
        Auto,

        /// <summary>
        /// 手动管理
        /// </summary>
        Manual
    }
}