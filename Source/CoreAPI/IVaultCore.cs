using VaultCore.CoreAPI;

namespace VaultCoreAPI;

//Main Interface to implement to create an emulation core that vault can use
public interface IVaultCore
{
    //Sets the fixed update in Ms for the core to be updated. If set to 0 then the core will be updated as fast as possible
    //If set low, the frontend may call update multiple times to catch up.
    float UpdateRateMs => 0;
    
    //If UpdateRateMs is set, then this controls the number of Update calls the frontend can call to catch up if needed.
    //This can stop the classic "Spiral of death" if your updates take longer then the time needed
    //If set to 0, then no limit is placed on the updates
    int maxNumUpdates => 0;
    
    //Called when the Core is created to initialise it. You can use ISubsystemResolver to 
    //retrieve subsystems from the frontend that are needed by this core.
    //If Initialization fails, or a needed subsystem cannot be resolved, throw an exception 
    void Initialise(ISubsystemResolver subsystemResolver);
    
    //Called every frame to update the Core. Update rate can be controlled with
    void Update();
    
    //Called before the core is destroyed to allow the core to clean up
    void ShutDown();
}