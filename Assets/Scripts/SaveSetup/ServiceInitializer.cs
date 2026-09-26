using UnityEngine;

public class ServiceInitializer
{
    public IService instant;
    public IService deffered;
    public IService flaky;

    public void InitializeServices()
    {
        instant = new PerKeySerializingService(new InstantService());
        deffered = new PerKeySerializingService(new DefferedService());
        flaky = new PerKeySerializingService(new FlakyService());

    }

    public IService GetService(ServiceType type) => type switch
    {
        ServiceType.Immediate => instant,
        ServiceType.Deffered => deffered,
        ServiceType.Flaky => flaky,
        _ => instant
    };
}
