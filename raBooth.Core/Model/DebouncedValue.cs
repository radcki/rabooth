namespace raBooth.Core.Model
{
    public class DebouncedValue<T> : IEquatable<DebouncedValue<T>>
    {
        private T _currentValue;
        private T _candidateSignalState;
        private readonly System.Timers.Timer _debounceTimer;


        public DebouncedValue(T currentValue = default(T), double debounceMiliseconds = 300)
        {
            _currentValue = currentValue;
            _debounceTimer = new System.Timers.Timer(TimeSpan.FromMilliseconds(debounceMiliseconds)) { AutoReset = false };
            _debounceTimer.Elapsed += OnDebounceTimerElapsed;
        }

        private void OnDebounceTimerElapsed(object? sender, EventArgs eventArgs)
        {
            if (!Equals(_candidateSignalState, _currentValue))
            {
                var oldSignalState = _currentValue;
                Value = _candidateSignalState;
            }
        }

        public void Update(T signalState)
        {
            if (_debounceTimer.Enabled && signalState.Equals(Value))
            {
                _debounceTimer.Stop();
            }

            if (!_debounceTimer.Enabled && !Equals(signalState, Value))
            {
                _candidateSignalState = signalState;
                _debounceTimer.Start();
            }
        }

        public T Value
        {
            get => _currentValue;
            private set
            {
                var oldValue = _currentValue;
                if (!Equals(oldValue, value))
                {
                    _currentValue = value;
                    ValueChanged?.Invoke(this, value);
                }
            }
        }

        public event EventHandler<T> ValueChanged;


        public bool Equals(DebouncedValue<T>? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Value.Equals(other.Value);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DebouncedValue<T>)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}