using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Build.Framework;
using System.Text.Json;

namespace VaultCore.CoreAPI;

internal class JsonManifestGenerationBuildTask : Microsoft.Build.Utilities.Task
{
    private const string I_VAULT_CORE_INTERFACE_NAME = "IVaultCore";
    private const string VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME = "VaultCoreDescriptionAttribute";
    
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private class CoreEntry
    {
        public string Name;
        public string Description;
        public string EmulatedSystemName;
        public string Version;
        
        public CoreEntry(string name, string description, string emulatedSystemName, string version)
        {
            Name = name;
            Description = description;
            EmulatedSystemName = emulatedSystemName;
            Version = version;
        }
    }
    
    private string _dllOutputPath = null!;
    
    [Required]
    private string DllOutputPath
    {
        get => _dllOutputPath;
        set => _dllOutputPath = value;
    }
    
    public override bool Execute()
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
        
        Log.LogMessage($"Creating Json Manifest for {Path.GetFileName(DllOutputPath)}");
        
        var jsonManifestOutput = Path.Combine(
            Path.GetDirectoryName(DllOutputPath)!,
            Path.GetFileNameWithoutExtension(DllOutputPath) + "_Manifest.json");
        
        var codeEntryData = new List<CoreEntry>();
        
        //Load the assembly
        var resolver = new PathAssemblyResolver(new[] { DllOutputPath, typeof(object).Assembly.Location });
        
        using (var mlc = new MetadataLoadContext(resolver))
        {
            // Load assembly into MetadataLoadContext.
            Assembly assembly = mlc.LoadFromAssemblyPath(DllOutputPath);

            var coreTypes = assembly.GetTypes()
                .Where(p => p.IsClass && !p.IsAbstract && p.GetInterface(I_VAULT_CORE_INTERFACE_NAME) != null)
                .ToList();
            
            if(coreTypes.Count == 0)
            {
                Log.LogError($"No classes implementing {I_VAULT_CORE_INTERFACE_NAME} found in assembly");
                return false;
            }
            
            foreach(var coreType in coreTypes)
            {
                Log.LogMessage($"Found Core: {coreType.Name}");
                
                var descriptionAttribute = coreType.GetCustomAttributes()
                    .FirstOrDefault(p => p.GetType().Name == VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME);
                
                if (descriptionAttribute == null)
                {
                    Log.LogError($"Unable to find {VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME} Attribute on core type {I_VAULT_CORE_INTERFACE_NAME}");
                    continue;
                }
                
                var attributeType = descriptionAttribute.GetType();

                var coreName = attributeType.GetProperty("Name", BindingFlags.NonPublic)!.GetValue(descriptionAttribute) as string;
                var coreDescription = attributeType.GetProperty("Description", BindingFlags.NonPublic)!.GetValue(descriptionAttribute) as string;
                var coreEmulatedSystemName = attributeType.GetProperty("EmulatedSystemName", BindingFlags.NonPublic)!.GetValue(descriptionAttribute) as string;
                var coreVersion = attributeType.GetProperty("Version", BindingFlags.NonPublic)!.GetValue(descriptionAttribute) as string;
                
                if(coreName == null)
                {
                    Log.LogError($"Unable to get Core Name from {VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME} Attribute");
                    continue;
                }
                
                if(coreDescription == null)
                {
                    Log.LogError($"Unable to get Core Description from {VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME} Attribute");
                    continue;
                }
                
                if(coreEmulatedSystemName == null)
                {
                    Log.LogError($"Unable to get Core Emulated System Name from {VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME} Attribute");
                    continue;
                }
                
                if(coreVersion == null)
                {
                    Log.LogError($"Unable to get Core Version from {VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME} Attribute");
                    continue;
                }
                
                codeEntryData.Add(new CoreEntry(coreName, coreDescription,
                    coreEmulatedSystemName, coreVersion));
            }
            
            if(Log.HasLoggedErrors)
            {
                return false;
            }
        }
        
        string json = JsonSerializer.Serialize(codeEntryData, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(jsonManifestOutput, json);
        
        return true;
    }
}