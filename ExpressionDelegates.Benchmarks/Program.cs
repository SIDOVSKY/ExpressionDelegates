using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace ExpressionDelegates.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IConfig config = DefaultConfig.Instance
                .WithOrderer(new JoinedSummaryOrdererByType(SummaryOrderPolicy.FastestToSlowest));

#if DEBUG
            config = new BenchmarkDotNet.Configs.DebugInProcessConfig();
#endif

            BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(args, config);
                //.RunAllJoined(config);
        }
    }
}