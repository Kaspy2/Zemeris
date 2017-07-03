break>list.txt
for %%i in (images/*) do @echo testing/images/%%i>>list.txt
cd ../
tesseract ./testing/list.txt -psm 3 ./testing/output/myFileName pdf