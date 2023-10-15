using System;
using System.ComponentModel.Design;
using System.IO;
using Mono.Debugging.Client;
using Mono.Debugging.Soft;
using Mono.Debugger.Soft;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Collections.Generic;

namespace HazelToolsVS.Debugging
{
	internal class HarmonyDebuggerSession : SoftDebuggerSession
	{
		private bool m_IsAttached;
		private MenuCommand m_AttachToHazelnutMenuItem;

		private BreakpointStore _store => Breakpoints;

		protected void OnOutput(bool isStderr, string text)
		{
			Console.WriteLine($"Output : {text}");
		}

        protected void OnLog(bool isStderr, string text)
        {
            Console.WriteLine($"Log : {text}");
        }

		protected void OnDebug(int level, string category, string message)
		{
            Console.WriteLine($"Debug{level} : {category} : {message}");
        }

		protected void OnBreakpointTrace(BreakEvent be, string trace)
		{
			Console.WriteLine($"Breakpoint Trace : {trace}");
		}

		private string OnTypeResolve(string identifier, SourceLocation location)
		{
			return identifier;
        }

		protected bool OnException(Exception e)
		{
			return false;
		}
        
		protected override void OnStepInstruction()
        {
            base.OnStepInstruction();
        }


        protected override void OnRun(DebuggerStartInfo startInfo)
		{
			OutputWriter += OnOutput;
			LogWriter += OnLog;
			DebugWriter += OnDebug;
            TypeResolverHandler += OnTypeResolve;
			ExceptionHandler += OnException;

			var harmonyStartInfo = startInfo as HarmonyStartInfo;

			Dictionary<string, string> replacements = new Dictionary<string, string>();

			foreach(var kvp in harmonyStartInfo.AssemblyPathMap)
			{
				if(kvp.Value.Contains(".."))
				{
					replacements.Add(kvp.Key, Path.GetFullPath(kvp.Value));
				}
			}

			foreach (var kvp in replacements)
			{
				harmonyStartInfo.AssemblyPathMap[kvp.Key] = kvp.Value;
			}

            switch (harmonyStartInfo.SessionType)
			{
			case HarmonySessionType.Launch:
				break;
			case HarmonySessionType.AttachHarmonyDebugger:
			{
				m_IsAttached = true;
				base.OnRun(harmonyStartInfo);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException(harmonyStartInfo.SessionType.ToString());
			}
		}

        protected override string GetConnectingMessage(DebuggerStartInfo dsi)
        {
            return base.GetConnectingMessage(dsi);
        }

        protected override BreakEventInfo OnInsertBreakEvent(BreakEvent breakEvent)
        {
            BreakEventInfo info = base.OnInsertBreakEvent(breakEvent);
			return info;
        }

        protected override void OnEnableBreakEvent(BreakEventInfo eventInfo, bool enable)
        {
            base.OnEnableBreakEvent(eventInfo, enable);
        }

        protected override bool HandleException(Exception ex)
        {
            return base.HandleException(ex);
        }

        protected override void OnStarted(ThreadInfo t)
        {
            base.OnStarted(t);
        }

        protected override void OnConnectionError(Exception ex)
		{
			// The session was manually terminated
			if (HasExited)
			{
				base.OnConnectionError(ex);
				return;
			}

			if (ex is VMDisconnectedException || ex is IOException)
			{
				HasExited = true;
				base.OnConnectionError(ex);
				return;
			}

			string message = "An error occured when trying to attach to Hazelnut. Please make sure that Hazelnut is running and that it's up-to-date.";
			message += Environment.NewLine;
			message += string.Format("Message: {0}", ex != null ? ex.Message : "No error message provided.");

			if (ex != null)
			{
				message += Environment.NewLine;
				message += string.Format("Source: {0}", ex.Source);
				message += Environment.NewLine;
				message += string.Format("Stack Trace: {0}", ex.StackTrace);

				if (ex.InnerException != null)
				{
					message += Environment.NewLine;
					message += string.Format("Inner Exception: {0}", ex.InnerException.ToString());
				}
			}
			
			_ = HarmonyToolsPackage.Instance.ShowErrorMessageBoxAsync("Connection Error", message);
			base.OnConnectionError(ex);
		}

		protected override void OnExit()
		{
			if (m_IsAttached)
			{
				m_IsAttached = false;
				base.OnDetach();
			}
			else
			{
				base.OnExit();
			}
		}
	}
}
