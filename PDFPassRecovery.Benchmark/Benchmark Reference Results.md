## Date: 15.11.2026


BenchmarkDotNet v0.15.0, Windows 11 (10.0.26300.9974)  
* 11th Gen Intel Core i7-11850H 2.50GHz, 1 CPU, 16 logical and 8 physical cores  
* [Host]     : .NET Framework 4.8 (4.8.10008.0), X64 RyuJIT VectorSize=256  

| Method              | Mean      | Error     | StdDev    |
|-------------------- |----------:|----------:|----------:|
| TestV1R2Performance |  35.94 ms |  0.695 ms |  0.879 ms |
| TestV2R3Perfromance | 722.47 ms |  8.254 ms |  7.721 ms |
| TestV4R4Performance | 738.08 ms | 12.827 ms | 11.371 ms |