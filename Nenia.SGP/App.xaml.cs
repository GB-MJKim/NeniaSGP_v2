using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Windows;
using Nenia.SGP.Services;

namespace Nenia.SGP
{
    public partial class App : Application
    {
        private IHost? _host;

        // ✅ 여기 추가
        public IServiceProvider Services
            => _host?.Services ?? throw new InvalidOperationException("Host not initialized.");

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<DatabaseService>();
                    services.AddSingleton<ProductService>();

                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<ImageProcessingService>();

                })
                .Build();

            await _host.StartAsync();

            // DB 스키마 생성 + PRAGMA 적용(무결성/잠금 안정화)
            await _host.Services.GetRequiredService<DatabaseService>().InitializeAsync();

            var main = _host.Services.GetRequiredService<MainWindow>();
            main.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(3));
                _host.Dispose();
            }

            base.OnExit(e);
        }
    }
}
