using EnvDTE;
using Mono.Debugging.Soft;
using Mono.Debugging.VisualStudio;

namespace HazelToolsVS.Debugging
{
    public enum HarmonySessionType
    {
        Launch = 0,
        AttachHarmonyDebugger
    }

    internal class HarmonyStartInfo : StartInfo
    {
        public readonly HarmonySessionType SessionType;

        public HarmonyStartInfo(SoftDebuggerStartArgs args, DebuggingOptions options, Project startupProject, HarmonySessionType sessionType)
            : base(args, options, startupProject)
        {
            SessionType = sessionType;
        }
    }
}
