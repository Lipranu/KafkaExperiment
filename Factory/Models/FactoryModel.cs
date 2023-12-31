﻿using Shared;

namespace Factory.Models
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

        public Guid ID { get { return _id; } }

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

        internal async Task<FactoryData?> DoWorkAsync(CancellationToken ct)
        {
            _logger.LogDebug($"Factory do work requested");
            await _semaphore.WaitAsync(ct);
            try
            {
                if (_state != FactoryState.Running)
                {
                    return null;
                }
                else
                {
                    _logger.LogDebug("Factory do work. Status: {status}", _status);
                    _status -= (_random.NextDouble() / 100);
                    _logger.LogDebug("Factory work done. Status: {status}", _status);
                    if (_status < 1.0) 
                    {
                        _state = FactoryState.Broken;
                    }
                    return new FactoryData { ID = _id, Time = DateTime.Now, Value = _random.Next() };
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
