using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using SupportLink.Models;
using SupportLink.Services;
using System.IO;
using System.Threading.Tasks;

public class ReportController : Controller
{
    private readonly ReportService _reportService;

    public ReportController(ReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IActionResult> Index([FromQuery] ReportFilter filter)
    {
        var reports = await _reportService.GetReportsAsync(filter);
        return View(reports);
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf([FromQuery] ReportFilter filter)
    {
        var reports = await _reportService.GetReportsAsync(filter);

        using var ms = new MemoryStream();
        using (var document = new Document(PageSize.A4.Rotate(), 20f, 20f, 20f, 20f))
        {
            PdfWriter.GetInstance(document, ms);
            document.Open();

            // Header
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var header = new Paragraph("Ticket Reports", headerFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15f
            };
            document.Add(header);

            // Table
            var table = new PdfPTable(16) { WidthPercentage = 100 }; // 16 columns including # 
            table.SetWidths(new float[] { 3, 5, 10, 10, 10, 10, 8, 8, 8, 10, 10, 10, 10, 10, 10, 12 });

            // Column headers
            var columnHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            string[] headers = {
                "No", "Description", "Organization", "Creator Email", "Assigned Agent",
                "Category", "File Type", "Status", "Created At", "Resolved At",
                "Feedback Comments", "Last Updated At", "Last Updated By", "Last Update Status"
            };
            foreach (var headerText in headers)
            {
                var cell = new PdfPCell(new Phrase(headerText, columnHeaderFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    //BackgroundColor = BaseColor.LIGHT_GRAY
                };
                table.AddCell(cell);
            }

            // Data rows
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            int rowNumber = 1;
            foreach (var r in reports)
            {
                table.AddCell(new Phrase(rowNumber.ToString(), cellFont));
                table.AddCell(new Phrase(r.TicketId.ToString(), cellFont));
                table.AddCell(new Phrase(r.TicketName, cellFont));
                table.AddCell(new Phrase(r.Description, cellFont));
                table.AddCell(new Phrase(r.OrganizationName, cellFont));
                table.AddCell(new Phrase(r.CreatedByEmail, cellFont));
                table.AddCell(new Phrase(r.AssignedAgentEmail ?? "N/A", cellFont));
                table.AddCell(new Phrase(r.CategoryName, cellFont));
                table.AddCell(new Phrase(r.FileTypeName ?? "N/A", cellFont));
                table.AddCell(new Phrase(r.StatusName, cellFont));
                table.AddCell(new Phrase(r.CreatedAt.ToString("yyyy-MM-dd HH:mm"), cellFont));
                table.AddCell(new Phrase(r.ResolvedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A", cellFont));
                table.AddCell(new Phrase(r.FeedbackComments ?? "N/A", cellFont));
                table.AddCell(new Phrase(r.LastUpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A", cellFont));
                table.AddCell(new Phrase(r.LastUpdatedBy ?? "N/A", cellFont));
                table.AddCell(new Phrase(r.LastUpdateStatus ?? "N/A", cellFont));

                rowNumber++;
            }

            document.Add(table);
            document.Close();
        }

        string pdfName = $"TicketReports_{DateTime.Now:yyyyMMddHHmm}.pdf";
        return File(ms.ToArray(), "application/pdf", pdfName);
    }
}
