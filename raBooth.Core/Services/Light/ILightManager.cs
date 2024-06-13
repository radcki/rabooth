namespace raBooth.Core.Services.Light;

public interface ILightManager
{
    Task PulseLights(TimeSpan length);
    Task SetLightsToLowBrightness();
    Task SetLightsToHighBrightness();
}