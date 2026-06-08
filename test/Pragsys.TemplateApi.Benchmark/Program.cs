using BenchmarkDotNet.Running;

namespace Pragsys.TemplateApi.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(Program).Assembly, args: args);
        }
    }
}
