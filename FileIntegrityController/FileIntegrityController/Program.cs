namespace FileIntegrityController
{
    class Program
    {
        static void Main(string[] args)
        {
            AppController.ManageApp();
            NLog.LogManager.Shutdown();
        }
    }
}
