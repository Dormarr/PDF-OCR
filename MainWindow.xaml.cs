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
            }
        }

        private void SplitAndRead(string filePath)
        {
            using(var pdfReader = new PdfReader(filePath))
            {
                var pdfDoc = new PdfDocument(pdfReader);

                for(int i = 1; i < pdfDoc.GetNumberOfPages(); i++)
                {
                    //Convert the page.
                    //string extractedText = ReadImage(filePath);
                    
                    string extractedText = ExtractTextFromPage(pdfDoc, i);
                    extractedText = ExtractNameFromText(extractedText);
                    PdfDocument pageOut = new PdfDocument(pdfReader);
                    //pdfDoc.CopyPagesTo(1, 1, pageOut, 0);
                    pdfDoc.GetPage(i).CopyTo(pageOut);
                    SaveAsNew(pageOut, $"{outputFolder}/{extractedText}", i);

                    
                }

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
            // Look for the "Full Name: " phrase in the text
            int fullNameIndex = text.IndexOf($"{startTextIndex}");
            if (fullNameIndex != -1)
            {
                // Find the substring between "Full Name: " and "Title: "
                int startIndex = fullNameIndex + $"{startTextIndex}".Length;
                int endIndex = text.IndexOf($"{endTextIndex}", startIndex);

                if (endIndex != -1)
                {
                    // Extract the substring containing the name
                    string nameSubstring = text.Substring(startIndex, endIndex - startIndex).Trim();

                    // Additional cleanup or processing if needed
                    // You might want to remove leading/trailing spaces or perform more specific extraction logic
                    renamePreview.Text = nameSubstring;
                    Debug.WriteLine(nameSubstring);
                    return nameSubstring;
                }
            }

            return $"Name not found {DateTime.UtcNow.Ticks}";
        }
    }
}