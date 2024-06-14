using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colourful;

namespace raBooth.Core.Services.Light
{
    public interface ILight
    {
        public Task TurnLightOn();
        public Task TurnLightOff();
        public Task SetLightColor(xyYColor color);
        public Task SetLightTemperature(int temperature);
        public Task SetLightBrightness(int brightness);
        public Task<xyYColor> GetLightColor();
        public Task<int> GetLightTemperature();
        public Task<int> GetLightBrightness();
    }
    
}
