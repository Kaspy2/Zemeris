break>Tesseract/runTessTSV/list.txt
for %%i in (Tesseract/runTessTSV/images/*) do @echo runTessTSV/images/%%i>>Tesseract/runTessTSV/list.txt
cd ./Tesseract
tesseract ./runTessTSV/list.txt -psm 3 ./runTessTSV/output/myFileName tsv