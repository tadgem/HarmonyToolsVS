using EnvDTE;
using Mono.Debugging.Soft;
using Mono.Debugging.VisualStudio;

namespace HazelToolsVS.Debugging
{
    public enum HazelSessionType
    {
        PlayInEditor = 0,
        AttachHazelnutDebugger
    }

    internal class HarmonyStartInfo : StartInfo
    {
        public readonly HazelSessionType SessionType;

        public HarmonyStartInfo(SoftDebuggerStartArgs args, DebuggingOptions options, Project startupProject, HazelSessionType sessionType)
            : base(args, options, startupProject)
        {
            SessionType = sessionType;
        }
    }
}
