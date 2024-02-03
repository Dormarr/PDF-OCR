<h1>PDF OCR</h1>

<h2>Overview</h2>

Honestly this is the laziest project ever. Not only is it written sloppily, the idea stemmed from a desire to automate a tedious task.
In my job, I often have to scan in and name an absurd amount fo files. I am soon leaving this job and have realised I have about 3000 sheets to scan and name individually. Instead, I developed this application that will go through and name them all for me.

It uses 2 flags, a start text index and a stop text index. Both are entered into textboxes in the window and, while scanning, any text found within the two flags will become the final name of the pdf document.

You also choose the input and output folder, so anything in the input will be scanned and read. It's not versatile enough to do anything but pdfs so ensure the input folder consists of exclusively pdfs, the output folder will then duplicate the file with a new name to accomodate for non-locally stored documents.

It's ugly, temporamental and pretty slow but I'll tell you what, it was quicker to make this from scratch than it would have been to rename those files by hand.


<h3>Technologies</h3>
Entirely written in C#.
Uses Tesseract (tessnet) for OCR capabilities.
A WPF project.
I don't know what else...
