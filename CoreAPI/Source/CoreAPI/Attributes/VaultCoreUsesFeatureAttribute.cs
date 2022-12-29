namespace VaultCore.CoreAPI;

/// <summary>
/// Use this attribute to declare Features that your core uses. These will be used
/// to know which features to acquire for the core on initialisation 
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class VaultCoreUsesFeatureAttribute : Attribute
{
    public Type[] CoreFeatureTypes { get; }
    
    public VaultCoreUsesFeatureAttribute(Type coreFeatureType) : this(new [] { coreFeatureType }) { }

    public VaultCoreUsesFeatureAttribute(params Type[] coreFeatureTypes)
    {
        CoreFeatureTypes = coreFeatureTypes;
    }
}