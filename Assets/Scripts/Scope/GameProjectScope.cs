using Interface;
using PriceManager;
using PriceSystem.Debug;
using VContainer;
using VContainer.Unity;
namespace Scope
{
    public class GameProjectScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<EventManager>(Lifetime.Singleton);
            builder.Register<SuggestionService>(Lifetime.Singleton);
            builder.Register<ResourcesManager>(Lifetime.Singleton)
                .As<IUniTaskStartable>() // 贴上标签：我是要异步启动的
                .AsSelf();
            
            // Debug 系统依赖资源，所以排在 Resource 后面
            builder.RegisterEntryPoint<DebugConsoleSystem>()
                .As<IUniTaskStartable>() 
                .AsSelf();


            builder.Register<GameManger>(Lifetime.Singleton)
                .As<IUniTaskStartable>()
                .AsSelf();
            
            // 运行所有IUniTaskStartable标识的。 VContainer 会运行它，执行里面的的初始化。然后再解开锁 让所有IUniTaskStartable的
            builder.RegisterEntryPoint<AsyncLifecycleExecutor>();
        }
    }
}