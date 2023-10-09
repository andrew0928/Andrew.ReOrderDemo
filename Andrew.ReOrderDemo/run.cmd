cls
rd /s /q output
mkdir output

:: Usage: dotnet run {command period in msec} {command noise} {buffer size}


dotnet run 100 500  100 2> output\metrics-100-500-100.csv
dotnet run 100 500  100 2> output\metrics-100-500-100.csv
dotnet run 100 500  100 2> output\metrics-100-500-100.csv
dotnet run 100 500  100 2> output\metrics-100-500-100.csv
dotnet run 100 500  100 2> output\metrics-100-500-100.csv
					 						  
dotnet run  70 500  100 2> output\metrics-070-500-100.csv
dotnet run  70 500  100 2> output\metrics-070-500-100.csv
dotnet run  70 500  100 2> output\metrics-070-500-100.csv
dotnet run  70 500  100 2> output\metrics-070-500-100.csv
dotnet run  70 500  100 2> output\metrics-070-500-100.csv
					 						  
dotnet run  30 500  100 2> output\metrics-030-500-100.csv
dotnet run  30 500  100 2> output\metrics-030-500-100.csv
dotnet run  30 500  100 2> output\metrics-030-500-100.csv
dotnet run  30 500  100 2> output\metrics-030-500-100.csv
dotnet run  30 500  100 2> output\metrics-030-500-100.csv

