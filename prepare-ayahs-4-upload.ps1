$srcfile = "./quran-ayahs.csv"
$destfile = "./quran-ayahs-2-upload.csv"
# Force double-quotation in CSV file 
Import-CSV $srcfile | Export-CSV $destfile
# Remove the first line of the file introduced by the above step :-) 
(Get-Content $destfile | Select-Object -Skip 1) | Set-Content $destfile