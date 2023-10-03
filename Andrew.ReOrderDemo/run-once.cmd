cls
rd /s /q output
mkdir output

:: Usage: dotnet run {command period in msec} {command noise} {buffer duration in msec} {buffer size}

dotnet run 70 500 400 10 2> output\metrics-070-400-10.csv
