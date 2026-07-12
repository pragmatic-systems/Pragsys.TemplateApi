using BenchmarkDotNet.Running;

namespace Pragmatic.TemplateApi.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(Program).Assembly, args: args);
        }
    }
}
