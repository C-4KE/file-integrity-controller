namespace FileIntegrityController
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            logger.Debug("Program has started");
            AppController.ManageApp();
            NLog.LogManager.Shutdown();
        }
    }
}
