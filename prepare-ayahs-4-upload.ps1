$srcfile = "./quran-ayahs.csv"
$destfile = "./uploads/quran-ayahs-2-upload.csv"
# Force double-quotation in CSV file 
Import-CSV $srcfile | Export-CSV $destfile
# Remove the first line of the file introduced by the above step :-) 
# WARNING: Powershell on the server (where Github action runs) behaves differently....it seems like it does not add a new line!!! 
#(Get-Content $destfile | Select-Object -Skip 1) | Set-Content $destfile