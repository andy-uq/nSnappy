``` ini

BenchmarkDotNet=v0.11.0, OS=Windows 10.0.17134.165 (1803/April2018Update/Redstone4)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=2.1.301
  [Host]     : .NET Core 2.1.1 (CoreCLR 4.6.26606.02, CoreFX 4.6.26606.05), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.1 (CoreCLR 4.6.26606.02, CoreFX 4.6.26606.05), 64bit RyuJIT


```
|  Method |     Mean |     Error |    StdDev |     Gen 0 |    Gen 1 |   Gen 2 | Allocated |
|-------- |---------:|----------:|----------:|----------:|---------:|--------:|----------:|
| nSnappy | 4.785 ms | 0.0174 ms | 0.0136 ms | 2718.7500 | 195.3125 | 46.8750 |  16.48 MB |
| nSpanny | 5.697 ms | 0.0317 ms | 0.0296 ms |  367.1875 |  93.7500 | 46.8750 |   2.36 MB |
