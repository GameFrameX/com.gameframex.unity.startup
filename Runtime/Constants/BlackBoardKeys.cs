namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// FSM BlackBoard key 常量。StartupRunner 通过单个 key 把 <see cref="StartupContext"/> 注入 procedure FSM，
    /// 所有 Procedure 状态通过 StartupProcedureUtility.GetContext 读取。
    /// </summary>
    public static class BlackBoardKeys
    {
        /// <summary>
        /// StartupContext 实例的 BlackBoard key。
        /// </summary>
        public const string StartupContext = "__startup_context__";
    }
}
