cls
rd /s /q output
mkdir output

:: Usage: dotnet run {command period in msec} {command noise} {buffer duration in msec} {buffer size}


dotnet run 100 500 100 10 2> output\metrics-100-100-10.csv
dotnet run 100 500 200 10 2> output\metrics-100-200-10.csv
dotnet run 100 500 300 10 2> output\metrics-100-300-10.csv
dotnet run 100 500 400 10 2> output\metrics-100-400-10.csv
dotnet run 100 500 500 10 2> output\metrics-100-500-10.csv

dotnet run 70 500 100 10 2> output\metrics-070-100-10.csv
dotnet run 70 500 200 10 2> output\metrics-070-200-10.csv
dotnet run 70 500 300 10 2> output\metrics-070-300-10.csv
dotnet run 70 500 400 10 2> output\metrics-070-400-10.csv
dotnet run 70 500 500 10 2> output\metrics-070-500-10.csv

dotnet run 30 500 100 10 2> output\metrics-030-100-10.csv
dotnet run 30 500 200 10 2> output\metrics-030-200-10.csv
dotnet run 30 500 300 10 2> output\metrics-030-300-10.csv
dotnet run 30 500 400 10 2> output\metrics-030-400-10.csv
dotnet run 30 500 500 10 2> output\metrics-030-500-10.csv

