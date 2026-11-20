using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace PDFPassRecovery.Benchmark
{
    public class Program
    {
        static void Main(string[] args)
        {
            var config = DefaultConfig.Instance
                .AddJob(Job.Default.WithId("AVX512_Native"))
                .AddJob(Job.Default
                    .WithId("AVX2_Restricted")
                    .WithEnvironmentVariable("DOTNET_EnableAVX512", "0"));

            //var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
            var summary = BenchmarkRunner.Run<BruteForceFunctionsBenchmarks>(config);
        }
    }
}
