using Shared;

namespace Factory.Model
{
    public class FactoryModel
    {
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly ILogger<FactoryModel> _logger;
        private readonly Random _random = new();
        private readonly Guid _id;
        private FactoryState _state;
        private double _status;

        internal FactoryModel(Guid id, ILogger<FactoryModel> logger)
        {
            _id = id;
            _status = 100.0;
            _logger = logger;
            _state = FactoryState.Stopped;
        }

        internal async Task<FactoryInfo> GetInfoAsync(CancellationToken ct)
        {
            _logger.LogDebug("GetInfoAsync requested");
            await _semaphore.WaitAsync(ct);
            try
            {
                _logger.LogDebug("FactoryInfo formed");
                return new FactoryInfo { ID = _id, Status = _status, State = _state };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal async Task DoWorkAsync(CancellationToken ct)
        {
            _logger.LogDebug($"Factory do work requested");
            await _semaphore.WaitAsync(ct);
            try
            {
                if (_state != FactoryState.Running)
                {
                    return;
                }
                else
                {
                    _logger.LogDebug("Factory do work. Status: {status}", _status);
                    _status -= _random.NextDouble();
                    _logger.LogDebug("Factory work done. Status: {status}", _status);
                    if (_status < 1.0) 
                    {
                        _state = FactoryState.Broken;
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal async Task SwitchState(CancellationToken ct)
        {
            _logger.LogDebug("SwitchState requested");
            await _semaphore.WaitAsync(ct);
            try
            {
                switch(_state)
                {
                    case FactoryState.Running: 
                        _state = FactoryState.Stopped; 
                        break;
                    case FactoryState.Stopped: 
                        _state = FactoryState.Running; 
                        break;
                    case FactoryState.Broken: 
                        break;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
