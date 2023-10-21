cls
rd /s /q output
mkdir output

:: Usage: dotnet run {command period in msec} {command noise} {buffer size}

:: 文章第四段的測試數據
dotnet run 100 100  10 2> output\metrics-100-100-10.csv
dotnet run 100 500  10 2> output\metrics-100-500-10.csv
dotnet run 100 500  05 2> output\metrics-100-500-05.csv
dotnet run 100 500  03 2> output\metrics-100-500-03.csv
dotnet run 100 500  02 2> output\metrics-100-500-02.csv
dotnet run 100 500  01 2> output\metrics-100-500-01.csv


dotnet run 100 100  100 2> output\metrics-100-100-100.csv
dotnet run 100 200  100 2> output\metrics-100-200-100.csv
dotnet run 100 300  100 2> output\metrics-100-300-100.csv
dotnet run 100 400  100 2> output\metrics-100-400-100.csv
dotnet run 100 500  100 2> output\metrics-100-500-100.csv
					 						  
dotnet run  70 100  100 2> output\metrics-070-100-100.csv
dotnet run  70 200  100 2> output\metrics-070-200-100.csv
dotnet run  70 300  100 2> output\metrics-070-300-100.csv
dotnet run  70 400  100 2> output\metrics-070-400-100.csv
dotnet run  70 500  100 2> output\metrics-070-500-100.csv
					 						  
dotnet run  30 100  100 2> output\metrics-030-100-100.csv
dotnet run  30 200  100 2> output\metrics-030-200-100.csv
dotnet run  30 300  100 2> output\metrics-030-300-100.csv
dotnet run  30 400  100 2> output\metrics-030-400-100.csv
dotnet run  30 500  100 2> output\metrics-030-500-100.csv

