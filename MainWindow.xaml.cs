using System;
using System.Windows;
using System.Windows.Forms;
using iText.Kernel.Pdf;
using Tesseract;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;



namespace PDF_OCR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string inputFolder;
        string outputFolder;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            if(folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                inputFolder = folderDialog.SelectedPath;
            }
        }

        private void OutputFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                outputFolder = folderDialog.SelectedPath;
            }
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            string[] filePaths = Directory.GetFiles(inputFolder);
            foreach (string filePath in filePaths)
            {
                SplitAndRead(filePath);
                Debug.WriteLine(filePath);
            }
        }

        private void SplitAndRead(string filePath)
        {
            Debug.WriteLine(Environment.NewLine + "Check 1: Init");
            try
            {
                using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(filePath)))
                {
                    //string extractedText = ExtractTextFromPage(pdfDoc, 1);
                    string pngPath = PDFtoPNG(filePath);
                    string extractedText = ReadImage(pngPath);
                    Debug.WriteLine($"Check 3: PNG Processed.");
                    string sanitisedText = SanitiseFileName(extractedText);
                    Debug.WriteLine($"Extracted: {extractedText}  Sanitised: {sanitisedText}");
                    File.Delete(pngPath);
                    SaveAsNew(pdfDoc, $"{outputFolder}/{sanitisedText}.pdf", 1);
                    //File.Move(filePath, $"{outputFolder}/{sanitisedText}.pdf");
                    Debug.WriteLine("Process Complete. File moved and renamed.");
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine($"Unable to read or extract. {e.Message}");
            }
        }

        private string SanitiseFileName(string input)
        {
            input = input.Replace(".", "");

            foreach(char c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input;
        }

        private void SaveAsNew(PdfDocument page, string outputFile, int pageIndex)
        {
            using(var outputStream = new FileStream(outputFile, FileMode.Create))
            {
                using (var pdfWriter = new PdfWriter(outputStream))
                {
                    using(var outputDoc = new PdfDocument(pdfWriter))
                    {
                        outputDoc.AddPage(page.GetPage(pageIndex).CopyTo(outputDoc));
                        Debug.WriteLine($"Saved as new {outputFile}");
                    }
                }
            }
        }

        private string PDFtoPNG(string filePath)
        {
            //Turn pdf page into png in temp file.
            Debug.WriteLine("Beginning PNG Generation.");
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            Debug.WriteLine($"File Name: {fileName}");

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "magick";
                process.StartInfo.Arguments = $"convert -density 300 \"{filePath}\" -quality 100 \"{outputFolder}\\{fileName}.png";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine($"Failed to make PNG: {error}");
                }

                return $"{outputFolder}\\{fileName}.png";
            }
        }

        private string ReadImage(string filePath)
        {
            Debug.WriteLine("Check 2: Reading Image");
            var ocrEngine = new TesseractEngine("C:\\Program Files\\Tesseract-OCR\\tessdata", "eng", EngineMode.TesseractAndLstm);
            var image = Pix.LoadFromFile(filePath);

            using (var page = ocrEngine.Process(image))
            {
                Debug.WriteLine("Processing through Tesseract");
                return ExtractNameFromText(page.GetText());
            }
        }

        private string ExtractNameFromText(string text)
        {
            string regexPattern = $@"{startTextIndex.Text}(.+?){endTextIndex.Text}";
            var match = Regex.Match(text, regexPattern, RegexOptions.Singleline);

            if (match.Success)
            {
                Debug.WriteLine($"Match Value: {match.Groups[1].Value}");

                return match.Groups[1].Value.Trim();
            }

            return $"Name not found {DateTime.UtcNow.Ticks}";
        }
    }
}