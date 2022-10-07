namespace VaultCore.CoreAPI;

public interface ISubsystemResolver
{
    //Gets the concrete feature that implements an interface that derives from IFeature.
    //Will return null if the feature is not provided by the frontend
    public T GetFeature<T>() where T : IFeature;
}