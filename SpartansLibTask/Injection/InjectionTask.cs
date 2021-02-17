using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SpartansLib.Injection
{
    public class InjectionTask : Task
    {
        [Required]
        public string TargetAssemblyName { get; set; }

        [Required]
        public string ProjectDir { get; set; }

        [Required]
        public string Configuration { get; set; }

        public bool EnableChecks { get; set; } = true;
        public bool EnableChecksInRelease { get; set; } = false;

        public override bool Execute()
        {
            var buildType = Configuration.Contains("Release") ? "Release" : "Debug";

            var godotLinkedAssembliesDir = $"{ProjectDir}.mono/temp/bin/{Configuration}/";
            var targetDllPath = $"{godotLinkedAssembliesDir}{TargetAssemblyName}.dll";
            var spartansLibDllPath = $"{godotLinkedAssembliesDir}SpartansLib.dll";
            var godotMainAssemblyDir = $"{ProjectDir}.mono/assemblies/{buildType}/";
            var debugChecksEnabled = EnableChecks && (buildType == "Debug" || EnableChecksInRelease);

            /*using (GodotDllModifier dllModifier = new GodotDllModifier(targetDllPath,
                godotMainAssemblyDir,
                godotLinkedAssembliesDir,
                debugChecksEnabled))
            {
                dllModifier.ModifyDll();
            }*/

            Log.LogMessage(MessageImportance.Low, "Starting Injector");
            using (var godotInjector = new GodotInjector(targetDllPath,
                spartansLibDllPath,
                godotMainAssemblyDir,
                godotLinkedAssembliesDir,
                Configuration,
                debugChecksEnabled))
            {
                Log.LogMessage(MessageImportance.Low,"Beginning Injection");
                godotInjector.Inject();
                Log.LogMessage(MessageImportance.Low,"Injection Complete");
            }

            Log.LogMessage(MessageImportance.High, "Injector Terminated");
            return true;
        }
    }
}