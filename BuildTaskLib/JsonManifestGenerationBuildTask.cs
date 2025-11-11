using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Build.Framework;

namespace VaultCore.BuildTasks;

public class JsonManifestGenerationBuildTask : Microsoft.Build.Utilities.Task
{
    private const string VAULT_CORE_BASE_CLASS_NAME = "VaultCoreBase";
    private const string VAULT_CORE_FEATURE_INTERFACE_NAME = "IVaultCoreFeature";
    private const string VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME = "VaultCoreDescriptionAttribute";
    private const string VAULT_CORE_USED_FEATURES_ATTRIBUTE_NAME = "VaultCoreUsesFeatureAttribute";
    
    [Serializable]
    private class CoreEntry
    {
        public string Name;
        public string Description;
        public string EmulatedSystemName;
        public string Version;
        public string CoreClassName;
        public string[] CoreFeaturesUsed;

        public CoreEntry(string name, string description, string emulatedSystemName, string version, string coreClassName, string[] coreFeaturesUsed)
        {
            Name = name;
            Description = description;
            EmulatedSystemName = emulatedSystemName;
            Version = version;
            CoreClassName = coreClassName;
            CoreFeaturesUsed = coreFeaturesUsed;
        }
    }

    [Required]
    public string DllPath { get; set; } = null!;

    [Required]
    public string ManifestOutputPath { get; set; } = null!;

    public override bool Execute()
    {
        try
        {
            if(string.IsNullOrEmpty(DllPath))
            {
                Log.LogError("DllPath not set");
                return false;
            }
            
            if(string.IsNullOrEmpty(ManifestOutputPath))
            {
                Log.LogError("ManifestOutputPath not set");
                return false;
            }
            
            if(File.Exists(DllPath) == false)
            {
                Log.LogError($"Unable to find File at {DllPath}");
                return false;
            }

            Log.LogMessage(MessageImportance.High, $"Creating Json Manifest for Cores in {Path.GetFileName(DllPath)}...");

            var codeEntryData = new List<CoreEntry>();

            //Load the assembly
            Assembly assembly = Assembly.LoadFrom(DllPath);

            var coreTypes = assembly.GetTypes()
                .Where(p => p.IsClass && !p.IsAbstract && TypeIsChildOfVaultCoreBaseType(p))
                .ToList();

            if(coreTypes.Count == 0)
            {
                Log.LogMessage(MessageImportance.High, $"No classes implementing {VAULT_CORE_BASE_CLASS_NAME} found.");
                return true;
            }

            Log.LogMessage(MessageImportance.High, $"{coreTypes.Count} Core(s) found. Validating cores and generating manifest...");
            
            foreach (var coreType in coreTypes)
            {
                Log.LogMessage(MessageImportance.High, $"Processing Core: {coreType.Name}");
                
                var coreFeatureAttributes = coreType.GetCustomAttributes()
                    .Where(p => p.GetType().Name == VAULT_CORE_USED_FEATURES_ATTRIBUTE_NAME)
                    .ToList();
                
                var coreFeaturesUsed = Array.Empty<string>();

                if(coreFeatureAttributes.Count == 0)
                {
                    Log.LogMessage($"Unable to find {VAULT_CORE_USED_FEATURES_ATTRIBUTE_NAME} Attribute on core type {coreType}. This may be intended " +
                                   $"if the core uses no features");
                }
                else
                {
                    var coreFeaturesUsedAttributeType = coreFeatureAttributes[0].GetType();
                    
                    List<string> coreFeatureTypeNames = new List<string>();
                    
                    foreach(var coreFeatureAttribute in coreFeatureAttributes)
                    {
                        var coreFeatureTypes = coreFeaturesUsedAttributeType.GetProperty("CoreFeatureTypes")!.GetValue(coreFeatureAttribute) as Type[];
                        
                        if(coreFeatureTypes == null)
                        {
                            Log.LogError($"Vault Core Used Features Attribute on {coreType} returned null feature types. Is it set correctly?");
                            continue;
                        }
                        
                        foreach(var coreFeature in coreFeatureTypes)
                        {
                            if(coreFeature.GetInterface(VAULT_CORE_FEATURE_INTERFACE_NAME) == null)
                            {
                                Log.LogError($"Vault Core Used Features Attribute on {coreType} returned feature type {coreFeature} that does not inherit " +
                                             $"from {VAULT_CORE_FEATURE_INTERFACE_NAME}.");
                                continue;
                            }
                            
                            coreFeatureTypeNames.Add(coreFeature.Name);
                        }
                    }
                    
                    coreFeaturesUsed = coreFeatureTypeNames.Distinct().ToArray();
                }

                var descriptionAttribute = coreType.GetCustomAttributes()
                    .FirstOrDefault(p => p.GetType().Name == VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME);

                if(descriptionAttribute == null)
                {
                    Log.LogError($"Unable to find {VAULT_CORE_DESCRIPTION_ATTRIBUTE_NAME} Attribute on core type {coreType}");
                    continue;
                }

                var descriptionAttributeType = descriptionAttribute.GetType();
                var coreName = descriptionAttributeType.GetProperty("Name")!.GetValue(descriptionAttribute) as string;
                var coreDescription = descriptionAttributeType.GetProperty("Description")!.GetValue(descriptionAttribute) as string;
                var coreEmulatedSystemName =
                    descriptionAttributeType.GetProperty("EmulatedSystemName")!.GetValue(descriptionAttribute) as string;
                var coreVersion = descriptionAttributeType.GetProperty("Version")!.GetValue(descriptionAttribute) as string;

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
                    coreEmulatedSystemName, coreVersion, coreType.Name, coreFeaturesUsed));

                if(Log.HasLoggedErrors)
                {
                    return false;
                }
            }

            string json = JsonSerializer.Serialize(codeEntryData, new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true });
            File.WriteAllText(ManifestOutputPath, json);

            return true;
        }
        catch (Exception e)
        {
            Log.LogError("Exception thrown during task");
            Log.LogErrorFromException(e, true);
            return false;
        }
    }
    
    private static bool TypeIsChildOfVaultCoreBaseType(Type type)
    {
        // return all inherited types
        var currentBaseType = type.BaseType;
        while (currentBaseType != null)
        {
            if(string.Equals(currentBaseType.Name, VAULT_CORE_BASE_CLASS_NAME))
            {
                return true;
            }
            currentBaseType = currentBaseType.BaseType;
        }
        
        return false;
    }
}