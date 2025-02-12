using System;

namespace SYSTools.Updater.Services
{
    public interface ILogger
    {
        void Log(string message);
        void LogError(string message);
        void UpdateStatus(string status);
        void UpdateProgress(double value);
    }
} 