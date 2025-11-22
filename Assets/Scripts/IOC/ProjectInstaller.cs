using Interface;
using IOC;
using UniDi;
using UnityEngine;

namespace IOC
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Todo 这个是样例
            
            // // 使用泛型 Provider：绑定全局 Player Provider（单例，跨场景访问）
            // Container.Bind<IProvider<GameObject>>()
            //     .WithId("Player")
            //     .To<Provider<GameObject>>()
            //     .AsSingle()
            //     .NonLazy();

        }
    }
}