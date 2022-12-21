using System.Reflection;

namespace VaultCore.CoreAPI;

/// <summary>
/// Use this attribute to describe the core to a frontend. Must be added to a class deriving from
/// IVaultCore in order for it to generate the json manifest alongside the core.
/// </summary>
[AttributeUsage( AttributeTargets.Class, Inherited = false)]
public class VaultCoreDescriptionAttribute : Attribute
{
    private readonly string _name;
    private readonly string _description;
    private readonly string _emulatedSystemName;
    private readonly string _customVersion;
    private readonly bool _useAssemblyVersionForCoreVersion;
    
    /// <summary>
    /// Name of the Core
    /// </summary>
    public string Name => _name;
    
    /// <summary>
    /// Description of the Core
    /// </summary>
    public string Description => _description;
    
    /// <summary>
    /// Name of the System that is been emulated by this core (if any)
    /// </summary>
    public string EmulatedSystemName => _emulatedSystemName;

    /// <summary>
    /// The Version of the core
    /// </summary>
    public string Version 
    {
        get
        {
            if(_useAssemblyVersionForCoreVersion == false)
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

    /// <summary>
    /// Provides a core description for the frontend. Uses the assembly version for the core version
    /// </summary>
    /// <param name="name">Name of the Core</param>
    /// <param name="description">Description of the Core</param>
    /// <param name="emulatedSystemName">Name of the System that is been emulated by this core (if any)</param>
    public VaultCoreDescriptionAttribute(string name, string description, string emulatedSystemName) 
    {
        _description = description;
        _name = name;
        _emulatedSystemName = emulatedSystemName;
        _customVersion = string.Empty;
        _useAssemblyVersionForCoreVersion = true;
    }
    
    /// <summary>
    /// rovides a core description for the frontend. Uses a custom version string for the core version
    /// </summary>
    /// <param name="name">Name of the Core</param>
    /// <param name="description">Description of the Core</param>
    /// <param name="emulatedSystemName">Name of the System that is been emulated by this core (if any)</param>
    /// <param name="customVersion">Custom Version string for this Core</param>
    //P
    public VaultCoreDescriptionAttribute(string name, string description, string emulatedSystemName, string customVersion) 
        : this(name, description, emulatedSystemName)
    {
        _customVersion = customVersion;
        _useAssemblyVersionForCoreVersion = false;
    }
}