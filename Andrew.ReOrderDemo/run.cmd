cls
rd /s /q output
mkdir output

dotnet run 100 10 2> output\metrics-100-10.csv
dotnet run 200 10 2> output\metrics-200-10.csv
dotnet run 300 10 2> output\metrics-300-10.csv
dotnet run 400 10 2> output\metrics-400-10.csv
dotnet run 500 10 2> output\metrics-500-10.csv

dotnet run 500 1 2> output\metrics-500-1.csv
dotnet run 500 2 2> output\metrics-500-2.csv
dotnet run 500 3 2> output\metrics-500-3.csv
dotnet run 500 4 2> output\metrics-500-4.csv
dotnet run 500 5 2> output\metrics-500-5.csv
