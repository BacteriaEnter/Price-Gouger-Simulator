using Cysharp.Threading.Tasks;
using PriceManager;
using PriceSystem.Base;

namespace PriceSystem.Debug
{
    public class DebugConsoleSystem : AsyncGameSystem
    {
        readonly ResourcesManager _resources;
        readonly SuggestionService _suggestionService;

        // 注入资源管理器和服务
        public DebugConsoleSystem(ResourcesManager resources, SuggestionService suggestionService)
        {
            _resources = resources;
            _suggestionService = suggestionService;
        }

        protected override async UniTask OnStartAsync()
        {
            UnityEngine.Debug.Log("[DebugSystem] 开始准备数据...");
            // var cmdConfig = _resources.GetConfig<CommandConfig>();
            UnityEngine.Debug.Log("[DebugSystem] 准备成功...");
            // 把配置塞给 Service，构建搜索树（Trie Tree）等
            // _suggestionService.BuildCache(cmdConfig);
            
            await UniTask.CompletedTask;
        }
    }
}