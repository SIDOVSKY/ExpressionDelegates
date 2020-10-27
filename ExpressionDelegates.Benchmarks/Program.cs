using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace ExpressionDelegates.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = default(IConfig);

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