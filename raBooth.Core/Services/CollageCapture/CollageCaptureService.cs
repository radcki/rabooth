using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using raBooth.Core.Helpers;
using raBooth.Core.Model;

namespace raBooth.Core.Services.CollageCapture
{
    public class CollageCaptureServiceConfiguration
    {
        public TimeSpan CaptureCountdownTickRate { get; init; }= TimeSpan.FromMilliseconds(100);
    }
    public class CollageCaptureService
    {
        private readonly CollageCaptureServiceConfiguration _configuration;

        public CollageCaptureService(CollageCaptureServiceConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task CaptureCollageItems(CollageLayout collage, TimeSpan captureDelay, Action<CaptureCollageItemsTick> onTick, CancellationToken cancellationToken)
        {
            while (collage.HasUncaptredItems() && !cancellationToken.IsCancellationRequested)
            {
                var timer = new CountdownTimer(captureDelay, _configuration.CaptureCountdownTickRate);
                timer.OnCountdownTick += (_, args) => onTick?.Invoke(new CaptureCollageItemsTick(args.RemainingTime));
                timer.OnElapsed += (_, _) => collage.CaptureNextItem();
                await timer.Start(cancellationToken);

            }
        }
    }

    public record CaptureCollageItemsTick(TimeSpan TimeBeforeCapture);
}
