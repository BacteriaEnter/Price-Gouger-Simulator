using Interface;
using UniDi;
using UnityEngine;

namespace IOC
{
    /// <summary>
    /// 通用链接器：将场景对象设置到全局 Provider
    /// </summary>
    public class SceneObjectsLinker : IInitializable
    {
        //Todo 这个是样例
        
        // private readonly IProvider<GameObject> _playerProvider; // 来自全局容器
        // private readonly GameObject _player; // 来自场景容器
        //
        // public SceneObjectsLinker([Inject(Id = "Player")] IProvider<GameObject> playerProvider, [Inject(Id = "Player")] GameObject player)
        // {
        //     _playerProvider = playerProvider;
        //     _player = player;
        // }

        // UniDi 自动调用，将场景对象设置到全局 Provider
        public void Initialize()
        {
            // _playerProvider.SetValue(_player);
        }
    }
}