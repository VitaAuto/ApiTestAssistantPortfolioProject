using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
using ApiTestAssistant.Core.Models;
using ApiTestAssistant.Core.Utils;

namespace ApiTestAssistant.Core.Services
{
    public class TestCaseExportService
    {
        public void ExportToCsv(List<TestCase> testCases, string outPath)
        {
            using (var writer = new StreamWriter(outPath, false, System.Text.Encoding.UTF8))
            {
                writer.WriteLine("Title,Body,Endpoint,Method,Type,Steps,ExpectedResult");
                foreach (var testCase in testCases)
                {
                    var stepsBlock = GherkinFormatter.FormatGherkinBlock(string.Join(";", testCase.Steps));
                    var expectedBlock = GherkinFormatter.FormatGherkinBlock(testCase.ExpectedResult);

                    writer.WriteLine($"{FileExportUtils.EscapeCsv(testCase.Title)},{FileExportUtils.EscapeCsv(FileExportUtils.CleanBody(testCase.Body))},{FileExportUtils.EscapeCsv(testCase.Endpoint)},{FileExportUtils.EscapeCsv(testCase.Method)},{FileExportUtils.EscapeCsv(testCase.Type)},{FileExportUtils.EscapeCsv(stepsBlock)},{FileExportUtils.EscapeCsv(expectedBlock)}");
                }
            }
        }

        public void ExportToExcel(List<TestCase> testCases, string excelPath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("TestCases");
                worksheet.Cell(1, 1).Value = "Title";
                worksheet.Cell(1, 2).Value = "Body";
                worksheet.Cell(1, 3).Value = "Endpoint";
                worksheet.Cell(1, 4).Value = "Method";
                worksheet.Cell(1, 5).Value = "Type";
                worksheet.Cell(1, 6).Value = "Steps";
                worksheet.Cell(1, 7).Value = "ExpectedResult";

                int row = 2;
                foreach (var testCase in testCases)
                {
                    worksheet.Cell(row, 1).Value = testCase.Title;
                    worksheet.Cell(row, 2).Value = FileExportUtils.CleanBody(testCase.Body);
                    worksheet.Cell(row, 3).Value = testCase.Endpoint;
                    worksheet.Cell(row, 4).Value = testCase.Method;
                    worksheet.Cell(row, 5).Value = testCase.Type;
                    worksheet.Cell(row, 6).Value = GherkinFormatter.FormatGherkinBlock(string.Join(";", testCase.Steps));
                    worksheet.Cell(row, 7).Value = GherkinFormatter.FormatGherkinBlock(testCase.ExpectedResult);

                    bool isError =
                        (testCase.Title?.Contains("No test cases generated", StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (testCase.Title?.Contains("Error generating test cases", StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (testCase.Type?.Contains("Error", StringComparison.OrdinalIgnoreCase) ?? false);

                    if (isError)
                    {
                        for (int col = 1; col <= 7; col++)
                        {
                            worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Red;
                        }
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(excelPath);
            }
        }
    }
}