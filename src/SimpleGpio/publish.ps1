Remove-Item -r ./out
dotnet publish -c Release -o ./out -r linux-arm --self-contained