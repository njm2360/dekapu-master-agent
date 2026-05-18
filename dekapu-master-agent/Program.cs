namespace dekapu_master_agent
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            const string mutexName = "dekapu_master_agent_single_instance";

            using var mutex = new Mutex(true, mutexName, out bool createdNew);

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

            try
            {
                ApplicationConfiguration.Initialize();

                var config = ConfigLoader.Load("config.json");
                Application.Run(new TrayAppContext(config));
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}
