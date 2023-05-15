using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace FinalProgramm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            bool runAsAdmin = false;
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            // Проверяем, является ли текущий пользователь администратором
            if (!wp.IsInRole(WindowsBuiltInRole.Administrator))
            {
                // Если текущий пользователь не является администратором, запрашиваем права администратора
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas"; // Запуск с правами администратора
                processInfo.Arguments = "/runas";

                try
                {
                    // Запускаем приложение с правами администратора
                    Process.Start(processInfo);
                    runAsAdmin = true;
                }
                catch (Exception ex)
                {
                    // Обработка ошибки при запросе прав администратора
                    MessageBox.Show("Ошибка при запросе прав администратора:\n\n" + ex.Message);
                }
            }

            // Если приложение уже запущено с правами администратора, продолжаем стандартный запуск
            if (!runAsAdmin)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}
