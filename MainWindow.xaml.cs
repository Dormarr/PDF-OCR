using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Tesseract;
using System.IO;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Windows.Media.Animation;
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

            var pdfReader = new PdfReader(filePath);
            var pdfDoc = new PdfDocument(pdfReader);

            Debug.WriteLine("Check 1");
            for(int i = 1; i < pdfDoc.GetNumberOfPages(); i++)
            {
                //Convert the page.
                    
                //PdfDocument pageOut = new PdfDocument(pdfReader);
                //pdfDoc.GetPage(i).CopyTo(pageOut);
                //string extractedText = ExtractTextFromPage(pdfDoc, i);

                string extractedText = ReadImage(filePath);
                extractedText = ExtractNameFromText(extractedText);
                Debug.WriteLine(extractedText);
                SaveAsNew(pdfDoc, $"{outputFolder}/{extractedText}", i);
            }
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
                    }
                }
            }
        }

        private string ReadImage(string filePath)
        {
            var ocrEngine = new TesseractEngine("C:\\Program Files\\Tesseract-OCR\\tessdata", "eng", EngineMode.Default);
            var image = Pix.LoadFromFile(filePath);

            Debug.WriteLine("Check 2");

            using (var page = ocrEngine.Process(image))
            {

                return ExtractNameFromText(page.GetText());
            }
        }

        private string ExtractTextFromPage(PdfDocument pdfDoc, int pageIndex)
        {
            using (var stream = new MemoryStream())
            {
                using(var textWriter  = new StringWriter())
                {
                    var page = pdfDoc.GetPage(pageIndex);

                    var textExtractor = new LocationTextExtractionStrategy();
                    var parser = new PdfCanvasProcessor(textExtractor);

                    parser.ProcessPageContent(page);
                    textWriter.Write(textExtractor.GetResultantText());
                }
                return stream.ToString();
            }

        }

        private string ExtractNameFromText(string text)
        {
            var match = Regex.Match(text, @"Full Name: (.+?)Title:", RegexOptions.Singleline);

            if (match.Success)
            {
                Debug.WriteLine(match.Groups[1].Value);
                return match.Groups[1].Value.Trim();
            }

            return $"Name not found {DateTime.UtcNow.Ticks}";
        }
    }
}