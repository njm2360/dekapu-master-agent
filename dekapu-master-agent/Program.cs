namespace dekapu_master_agent
{
    internal static class Program
    {
        private static Mutex? _mutex;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            const string mutexName = "dekapu_master_agent_single_instance";

            _mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                MessageBox.Show(
                    "多重起動を検知しました",
                    "起動エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            ApplicationConfiguration.Initialize();

            var config = AppConfig.Load("config.json");
            Application.Run(new TrayAppContext(config));

            _mutex.ReleaseMutex();
        }
    }
}
