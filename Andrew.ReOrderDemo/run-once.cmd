cls
rd /s /q output
mkdir output

:: Usage: dotnet run {command period in msec} {command noise} {buffer size}

dotnet run 100 500 10 2> output\metrics-100-500-10.csv
