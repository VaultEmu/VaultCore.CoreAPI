using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Build.Framework;

namespace VaultCore.BuildTasks;

public class JsonManifestGenerationBuildTask : Microsoft.Build.Utilities.Task
{
    private const string I_VAULT_CORE_INTERFACE_NAME = "IVaultCore";
    private const string VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME = "VaultCoreDescriptionAttribute";
    
    [Serializable]
    private class CoreEntry
    {
        public string Name;
        public string Description;
        public string EmulatedSystemName;
        public string Version;
        public string CoreClassName;

        public CoreEntry(string name, string description, string emulatedSystemName, string version, string coreClassName)
        {
            Name = name;
            Description = description;
            EmulatedSystemName = emulatedSystemName;
            Version = version;
            CoreClassName = coreClassName;
        }
    }

    private string _dllOutputPath = null!;

    [Required]
    public string DllOutputPath
    {
        get => _dllOutputPath;
        set => _dllOutputPath = value;
    }
    public override bool Execute()
    {
        try
        {
            if(string.IsNullOrEmpty(DllOutputPath))
            {
                Log.LogError("DllOutputPath not set");
                return false;
            }

            if(File.Exists(DllOutputPath) == false)
            {
                Log.LogError($"Unable to find File at {DllOutputPath}");
                return false;
            }

            Log.LogMessage(MessageImportance.High, $"Creating Json Manifest for Cores in {Path.GetFileName(DllOutputPath)}...");

            var jsonManifestOutput = Path.Combine(
                Path.GetDirectoryName(DllOutputPath)!,
                Path.GetFileNameWithoutExtension(DllOutputPath) + "_Manifest.json");

            var codeEntryData = new List<CoreEntry>();

            //Load the assembly
            Assembly assembly = Assembly.LoadFrom(DllOutputPath);

            var coreTypes = assembly.GetTypes()
                .Where(p => p.IsClass && !p.IsAbstract && p.GetInterface(I_VAULT_CORE_INTERFACE_NAME) != null)
                .ToList();

            if(coreTypes.Count == 0)
            {
                Log.LogMessage(MessageImportance.High, $"No classes implementing {I_VAULT_CORE_INTERFACE_NAME} found.");
                return true;
            }

            Log.LogMessage(MessageImportance.High, $"{coreTypes.Count} Core(s) found. Validating cores and generating manifest...");
            
            foreach (var coreType in coreTypes)
            {
                Log.LogMessage(MessageImportance.High, $"Processing Core: {coreType.Name}");

                var descriptionAttribute = coreType.GetCustomAttributes()
                    .FirstOrDefault(p => p.GetType().Name == VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME);

                if(descriptionAttribute == null)
                {
                    Log.LogError($"Unable to find {VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME} Attribute on core type {I_VAULT_CORE_INTERFACE_NAME}");
                    continue;
                }

                var attributeType = descriptionAttribute.GetType();
                
                var coreName = attributeType.GetProperty("Name")!.GetValue(descriptionAttribute) as string;
                var coreDescription = attributeType.GetProperty("Description")!.GetValue(descriptionAttribute) as string;
                var coreEmulatedSystemName =
                    attributeType.GetProperty("EmulatedSystemName")!.GetValue(descriptionAttribute) as string;
                var coreVersion = attributeType.GetProperty("Version")!.GetValue(descriptionAttribute) as string;

                if(coreName == null)
                {
                    Log.LogError($"Unable to get Core Name from {VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME} Attribute for core {coreType.Name}");
                    continue;
                }

                if(coreDescription == null)
                {
                    Log.LogError($"Unable to get Core Description from {VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME} Attribute for core {coreType.Name}");
                    continue;
                }

                if(coreEmulatedSystemName == null)
                {
                    Log.LogError($"Unable to get Core Emulated System Name from {VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME} Attribute for core {coreType.Name}");
                    continue;
                }

                if(coreVersion == null)
                {
                    Log.LogError($"Unable to get Core Version from {VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME} Attribute for core {coreType.Name}");
                    continue;
                }

                codeEntryData.Add(new CoreEntry(coreName, coreDescription,
                    coreEmulatedSystemName, coreVersion, coreType.Name));

                if(Log.HasLoggedErrors)
                {
                    return false;
                }
            }

            string json = JsonSerializer.Serialize(codeEntryData, new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true });
            File.WriteAllText(jsonManifestOutput, json);

            return true;
        }
        catch (Exception e)
        {
            Log.LogError("Exception thrown during task");
            Log.LogErrorFromException(e, true);
            return false;
        }
    }
}