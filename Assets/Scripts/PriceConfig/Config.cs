using Cysharp.Threading.Tasks;
namespace PriceConfig
{
    public abstract class Config
    {
        /// <summary>
        /// 异步加载配置数据
        /// </summary>
        /// <returns>异步任务</returns>
        public abstract UniTask LoadConfig();
    }
}