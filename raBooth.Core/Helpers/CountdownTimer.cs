using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raBooth.Core.Helpers
{
    public class CountdownTimer
    {

        private readonly TimeSpan _timespan;
        private readonly TimeSpan _tickRate;
        private bool _isRunning;
        private bool _isCancellationRequested;
        private Task _countdownTask;

        public CountdownTimer(TimeSpan timespan, TimeSpan tickRate)
        {
            _timespan = timespan;
            _tickRate = tickRate;
        }

        public event EventHandler OnElapsed;
        public event EventHandler<CountdownTimerTickEventArgs> OnCountdownTick;
        public bool IsInProgress => _isRunning;

        public Task Start(CancellationToken cancellationToken)
        {
            cancellationToken.Register(Cancel);
            if (_isRunning)
            {
                return _countdownTask;
            }
            var endTime = DateTime.UtcNow + _timespan;
            _isRunning = true;
            _countdownTask = Task.Run(async () =>
                                      {
                                          try
                                          {
                                              var lastTickTime = DateTime.UtcNow;
                                              while (!_isCancellationRequested && DateTime.UtcNow < endTime)
                                              {
                                                  await Task.Delay(_tickRate - (DateTime.UtcNow - lastTickTime));
                                                  lastTickTime = DateTime.UtcNow;
                                                  if (!_isCancellationRequested)
                                                  {
                                                      OnCountdownTick?.Invoke(this, new CountdownTimerTickEventArgs(endTime - DateTime.UtcNow));
                                                  }
                                              }

                                              if (!_isCancellationRequested)
                                              {
                                                  OnElapsed?.Invoke(this, EventArgs.Empty);
                                              }

                                          }
                                          finally
                                          {
                                              _isRunning = false;
                                              _isCancellationRequested = false;
                                          }
                                      });
            return _countdownTask;
        }
        public void Cancel()
        {
            if (_isRunning)
            {
                _isCancellationRequested = true;
            }
        }

        public void FinishEarly()
        {
            if (_isRunning)
            {
                _isCancellationRequested = true;
                OnElapsed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public record CountdownTimerTickEventArgs(TimeSpan RemainingTime);
}
