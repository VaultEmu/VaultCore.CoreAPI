using System.Reflection;
using VaultCore.CoreAPI;

namespace VaultCoreAPI;

//Use this attribute to describe the core to a frontend
[AttributeUsage( AttributeTargets.Class, Inherited = false)]
public class VaultCoreDescriptionAttribute : Attribute
{
    private readonly string _name;
    private readonly string _description;
    private readonly string _emulatedSystemName;
    private readonly string _customVersion;
    private readonly bool _useAssemblyVersionForCoreVersion;
    
    public string Name => _name;
    public string Description => _description;
    public string EmulatedSystemName => _emulatedSystemName;

    public string Version 
    {
        get
        {
            if(_useAssemblyVersionForCoreVersion)
            {
                return _customVersion;
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if(version != null)
            {
                return version.ToString();
            }
            
            return "UNKNOWN VERSION";
        }   
    }

    //Provides a core description for the frontend. Uses the assembly version for the core version
    public VaultCoreDescriptionAttribute(string name, string description, string emulatedSystemName, List<IFeature>? requiredFeatures = null) 
    {
        _description = description;
        _name = name;
        _emulatedSystemName = emulatedSystemName;
        _customVersion = string.Empty;
        _useAssemblyVersionForCoreVersion = true;
    }
    
    //Provides a core description for the frontend. Uses a custom version string for the core version
    public VaultCoreDescriptionAttribute(string name, string description, string emulatedSystemName, string customVersion) 
        : this(name, description, emulatedSystemName)
    {
        _customVersion = customVersion;
        _useAssemblyVersionForCoreVersion = false;
    }
}