namespace ScraperWorker
{
    public class Worker : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Collection sources have not been implemented yet.
            return Task.CompletedTask;
        }
    }
}
