using BookStore_API.Contracts;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Services
{
    public class LoggerService : ILoggerService
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        public void logDebug(string message)
        {
            logger.Debug(message);
        }

        public void logError(string message)
        {
            logger.Error(message);
        }

        public void logInfo(string message)
        {
            logger.Info(message);
        }

        public void logWarning(string message)
        {
            logger.Warn(message);
        }
    }
}
