using Shared;

namespace Factory
{
    public class Factory
    {
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly ILogger<Factory> _logger;
        private readonly Guid _id;
        private readonly Random _random = new();
        private double _status;

        internal Factory(Guid id, ILogger<Factory> logger)
        {
            _id = id;
            _status = 100.0;
            _logger = logger;
        }

        internal async Task<FactoryInfo> GetInfoAsync(CancellationToken ct)
        {
            _logger.LogDebug("GetInfoAsync requested");
            await _semaphore.WaitAsync(ct);
            try
            {
                FactoryState state;
                if (_status > 1.0)
                {
                    state = FactoryState.Running;
                }
                else
                {
                    state = FactoryState.Broken;
                }
                _logger.LogDebug("FactoryInfo formed");
                return new FactoryInfo { ID = _id, Status = _status, State = state };
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
                if (_status > 1.0)
                {
                    _logger.LogDebug("Factory do work. Status: {status}", _status);
                    _status -= _random.NextDouble();
                    _logger.LogDebug("Factory work done. Status: {status}", _status);
                }
                else
                {
                    _logger.LogDebug("Factory broken");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
