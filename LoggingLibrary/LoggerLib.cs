using log4net;
using log4net.Config;

namespace LoggingLibrary
{
    public static class LoggerLib
    {
        static LoggerLib()
        {
            var log4netConfig = new FileInfo("log4net.config");
            XmlConfigurator.Configure(log4netConfig);
        }

        public static ILog GetLogger<T>()
        {
            return LogManager.GetLogger(typeof(T));
        }
    }
}