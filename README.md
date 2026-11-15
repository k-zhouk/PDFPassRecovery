# PDF Password Recovery Tool

This is a specific tool to recover lost/ forgotten passwords for a PDF file.

# Why does it exist?
I started this project because Hashcat (https://hashcat.net/) (at least at the time I tried it) had a bug in OpenCL implementation of the PDF encryption algorithms, so from time to time Hashcat didn't recover a password for every run.

# Supported PDF Versions:

1/ PDF 1.2  
2/ PDF 1.3  
3/ PDF 1.4  
4/ PDF 1.6  
5/ PDF 1.7  
6/ PDF 1.7 extension level 3/ 5  
7/ PDF 2.0  

# Usage
In a command line run:  
\> PDFPassRecovery <PDF_file_name>  

To get help:  
\> PDFPassRecovery  
or alternatively:  
\> PDFPassRecovery -h  

# Configruation
Configuration is done through the "PDFPassConfig.xml" configuration file.

# Additional Notes
1/ Algorithms for password recovery were created based on the PDF reference speciications that are freely available.  
2/ There is also a version PDF 1.7 extension level 8, that is not available. However, based on some findings, encryption should be the same as in PDF 2.0  
