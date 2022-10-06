namespace VaultCore.CoreAPI;

public interface ISubsystemResolver
{
    //Gets the concrete subsystem that implements an interface that derives from ISubsystem.
    //Will return null if the subsystem is not provided
    public T GetSubsystem<T>() where T : ISubsystem;
}