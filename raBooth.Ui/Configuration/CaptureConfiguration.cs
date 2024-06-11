using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raBooth.Ui.Configuration
{
    public class CaptureConfiguration
    {
        public static string SectionName = nameof(CaptureConfiguration);

        public TimeSpan GetReadyMessageDisplayTime { get; set; }
        public TimeSpan CaptureCountdownLength { get; set; }
        public TimeSpan ExitCountdownLength { get; set; }
    }
}
