using Cysharp.Threading.Tasks;
namespace Interface
{
    public interface IUniTaskStartable
    {
        UniTask AsyncStart();
    }
}