namespace System.Debug
{
    public class DebugConsoleSystem
    {
        // 单例实例
        private static readonly DebugConsoleSystem _instance;
        
        // 建议服务实例
        public SuggestionService suggestionService;
        
        /// <summary>
        /// 静态构造函数，初始化单例实例
        /// </summary>
        static DebugConsoleSystem()
        {
            _instance = new DebugConsoleSystem();
        }

        // 获取单例实例
        public static DebugConsoleSystem Instance => _instance;

        /// <summary>
        /// 初始化调试控制台系统
        /// </summary>
        public void Init()
        {
            suggestionService = new SuggestionService();
        }
    }
}