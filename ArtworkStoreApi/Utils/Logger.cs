namespace ArtworkStoreApi.Utils
{
    public interface IAppLogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? ex = null);
        void LogDebug(string message);
    }

    public class AppLogger(ILogger<AppLogger> logger) : IAppLogger
    {
        public void LogInfo(string message)
        {
            logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            logger.LogWarning(message);
        }

        public void LogError(string message, Exception? ex = null)
        {
            if (ex != null)
                logger.LogError(ex, message);
            else
                logger.LogError(message);
        }

        public void LogDebug(string message)
        {
            logger.LogDebug(message);
        }
    }
}
