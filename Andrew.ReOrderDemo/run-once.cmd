cls
rd /s /q output
mkdir output

:: Usage: dotnet run {command period in msec} {command noise} {buffer size}

dotnet run 70 500 10 2> output\metrics-070-500-10.csv
