
namespace TelegrafClient.Influx
{
    public class TelegrafCollectorConfiguration
    {
        private TelegrafCollectorConfiguration()
        {
            Tag = new PipelinedCollectorTagConfiguration(this);
            WriteTo = new PipelinedCollectorSocketConfiguration(this);
        }

        public PipelinedCollectorTagConfiguration Tag { get; }

        public PipelinedCollectorSocketConfiguration WriteTo { get; }

        public PipelinedMetricsCollector CreateCollector()
        {
            var emitter = WriteTo.CreateEmitter(null, out var disposeEmitter);

            return new PipelinedMetricsCollector(emitter, Tag.CreateEnricher(), () =>
            {
                disposeEmitter?.Invoke();
            });
        }

        public static TelegrafCollectorConfiguration Create() => 
            new TelegrafCollectorConfiguration();
    }
}
