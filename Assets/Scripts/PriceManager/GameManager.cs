using Cysharp.Threading.Tasks;
using Interface;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace PriceManager
{
    public class GameManger : IUniTaskStartable
    {
        public async UniTask AsyncStart()
        {
            Debug.Log("[GameFlow] 所有前置系统已就绪，请求进入 Main 场景...");

            // 加载场景
            var handle = SceneManager.LoadSceneAsync("Main");
            await handle;

            Debug.Log("[GameFlow] Main 场景加载完毕！");
        }
    }
}