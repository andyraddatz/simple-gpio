rm -r ./out
dotnet publish -c Release -o ./out -r linux-arm --self-contained
# rsync -avz ./out/ pi2b@pi2b:/var/www/simplegpio/