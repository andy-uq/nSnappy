``` ini

BenchmarkDotNet=v0.11.0, OS=Windows 10.0.17134.191 (1803/April2018Update/Redstone4)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
Frequency=2531251 Hz, Resolution=395.0616 ns, Timer=TSC
.NET Core SDK=2.1.302
  [Host]     : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.2 (CoreCLR 4.6.26628.05, CoreFX 4.6.26629.01), 64bit RyuJIT


```
|  Method |     Mean |     Error |    StdDev |     Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|-------- |---------:|----------:|----------:|----------:|--------:|--------:|----------:|
| nSnappy | 6.630 ms | 0.0636 ms | 0.0564 ms | 5429.6875 | 46.8750 | 46.8750 |  16.48 MB |
| nSpanny | 6.758 ms | 0.1019 ms | 0.0851 ms | 5414.0625 | 46.8750 | 46.8750 |  16.41 MB |
