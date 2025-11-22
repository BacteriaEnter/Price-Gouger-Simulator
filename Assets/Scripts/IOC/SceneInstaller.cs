using System.Collections;
using System.Collections.Generic;
using Interface;
using UniDi;
using UnityEngine;

namespace IOC
{
    public class SceneInstaller : MonoInstaller
    {
        //Todo 这个是样例
        
        // [Header("场景对象引用")]
        // [SerializeField] private GameObject playerGameObject;
        // [SerializeField] private Camera mainCamera;
        //
        // public override void InstallBindings()
        // {
        //     // 绑定 Player GameObject
        //     if (playerGameObject != null)
        //     {
        //         Container.Bind<GameObject>()
        //             .WithId("Player")
        //             .FromInstance(playerGameObject)
        //             .AsSingle();
        //     }
        //     
        //     // 绑定 MainCamera
        //     if (mainCamera != null)
        //     {
        //         Container.Bind<Camera>()
        //             .WithId("MainCamera")
        //             .FromInstance(mainCamera)
        //             .AsSingle();
        //         
        //         Container.Bind<GameObject>()
        //             .WithId("MainCameraGo")
        //             .FromInstance(mainCamera.gameObject)
        //             .AsSingle();
        //     }
        //     
        //     // 绑定通用链接器：将场景对象设置到全局 Provider
        //     Container.BindInterfacesAndSelfTo<SceneObjectsLinker>().AsSingle().NonLazy();
        // }
    }
}

