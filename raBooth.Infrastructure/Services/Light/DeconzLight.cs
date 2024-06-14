using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Colourful;
using Newtonsoft.Json;
using raBooth.Core.Services.Light;
using RestSharp;

namespace raBooth.Infrastructure.Services.Light
{
    public class DeconzLightConfiguration
    {
        public string StateUrl { get; init; }
        public string AccessKey { get; init; }
        public int TransitionTime { get; init; } = 10;
    }

    public class DeconzLight : ILight
    {
        private readonly DeconzLightConfiguration _configuration;
        private readonly IRestClient _client;


        public DeconzLight(DeconzLightConfiguration configuration)
        {
            _configuration = configuration;
            _client = new RestClient();
        }

        public async Task TurnLightOn()
        {
            throw new NotImplementedException();
        }

        public async Task TurnLightOff()
        {
            throw new NotImplementedException();
        }

        public async Task SetLightColor(xyYColor color)
        {
            var request = new RestRequest(_configuration.StateUrl);
            request.AddJsonBody(new
            {
                xy = new[] { color.x, color.y },
                transitiontime = _configuration.TransitionTime
            });

            _ = await _client.PutAsync(request);
        }

        public async Task SetLightTemperature(int temperature)
        {
            var request = new RestRequest(_configuration.StateUrl);
            request.AddJsonBody(new
            {
                ct = 1000000 / Math.Clamp(temperature, 2000, 6500),
                transitiontime = _configuration.TransitionTime
            });

            _ = await _client.PutAsync(request);
        }

        public async Task SetLightBrightness(int brightness)
        {

            Debug.WriteLine("SetLightBrightness " + brightness);
            try
            {
                var request = new RestRequest(_configuration.StateUrl);
                request.AddJsonBody(new
                                    {
                                        bri = Math.Clamp(brightness, 0, 255),
                                        transitiontime = _configuration.TransitionTime
                                    });

                var response = await _client.PutAsync(request);
            }
            catch (Exception ex) 
            {
                
            }
        }
        public async Task<xyYColor> GetLightColor()
        {
            throw new NotImplementedException();
        }
        public async Task<int> GetLightTemperature()
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetLightBrightness()
        {
            throw new NotImplementedException();
        }

    }
}
