using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using raBooth.Core.Services.Light;

namespace raBooth.Infrastructure.Services.Light
{
    public class LightsConfiguration
    {
        public static readonly string SectionName = nameof(LightsConfiguration);
        public List<DeconzLightConfiguration> DeconzLightConfigurations { get; set; }
        public int LowBrightness { get; set; }
        public int HighBrightness { get; set; }
    }

    public class LightManager : ILightManager
    {
        private readonly LightsConfiguration _configuration;
        private readonly List<ILight> _lights = [];

        public LightManager(LightsConfiguration configuration)
        {
            _configuration = configuration;
            InitializeLights();
        }

        private void InitializeLights()
        {
            foreach (var lightConfiguration in _configuration.DeconzLightConfigurations ?? [])
            {
                _lights.Add(new DeconzLight(lightConfiguration));
            }
        }

        public async Task PulseLights(TimeSpan length)
        {
            _ = SetLightsToHighBrightness();
            await Task.Delay(length);
            _ = SetLightsToLowBrightness();
        }

        public Task SetLightsToLowBrightness()
        {
            Parallel.ForEach(_lights, light => light.SetLightBrightness(_configuration.LowBrightness));
            return Task.CompletedTask;
        }
        public Task SetLightsToHighBrightness()
        {
            Parallel.ForEach(_lights, light => light.SetLightBrightness(_configuration.HighBrightness));
            return Task.CompletedTask;
        }
    }
}
