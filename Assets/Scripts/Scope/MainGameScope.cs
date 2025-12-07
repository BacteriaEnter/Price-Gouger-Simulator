using VContainer;
using VContainer.Unity;
namespace Scope
{
    public class MainGameScope : LifetimeScope
    {
        // 1. 引用场景里的物体
        // [SerializeField]
        // PlayerModel playerInScene; 比如这个
        

        protected override void Configure(IContainerBuilder builder)
        {
            // 2. 注册 View 组件 下面是例子 使用的时候删除
            // builder.RegisterComponent(playerInScene);

            
            // 3. 注册只属于这个场景的 System (EntryPoint) 下面是例子 使用的时候删除
            // builder.RegisterEntryPoint<PlayerMoveSystem>();
        }
    }
}