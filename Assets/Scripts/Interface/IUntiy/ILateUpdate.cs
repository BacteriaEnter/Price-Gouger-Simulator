namespace Interface.IUntiy
{
    public interface ILateUpdate
    {
        /// <summary>
        /// 延迟更新方法，在每帧的 LateUpdate 阶段调用
        /// </summary>
        public void LateUpdate();
    }
}