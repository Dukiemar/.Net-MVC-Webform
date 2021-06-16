using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SagicorForms.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SelectPdf;

using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using iText.Kernel.Geom;
using iText.Layout.Borders;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net.Mime;
using SmtpClient = System.Net.Mail.SmtpClient;
using System.Drawing;
using Microsoft.AspNetCore.Http;

namespace SagicorForms.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

 //ITEXT7 PDF
       [HttpPost]
        public IActionResult GeneratePdf(IFormCollection formdata)
        {            
            MemoryStream ms = new MemoryStream();
            PdfWriter pw = new PdfWriter(ms);
            PdfDocument pdf = new PdfDocument(pw);
            Document doc = new Document(pdf, PageSize.LETTER);
            doc.SetMargins(75, 35, 70, 35);


 //ADD TABLE FOR THE HEADERS ON THE PDF
            Style styles = new Style()
                .SetFontSize(14)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER);

            Table table = new Table(1).UseAllAvailableWidth();
            Cell cell = new Cell().Add(new Paragraph("SECURITIES TRADE ORDER").SetFontSize(20))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(Border.NO_BORDER);
            table.AddCell(cell);
            cell = new Cell().Add(new Paragraph("Summary of Request").SetFontSize(14))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(Border.NO_BORDER);
            table.AddCell(cell);
            doc.Add(table);

//ADD TABLE TO ACCEPT THE DATA COMING FROM THE FORM 
            Table _table = new Table(2).UseAllAvailableWidth();
            Cell _cell = new Cell().Add(new Paragraph("DATA FIELD"));
            _table.AddHeaderCell(_cell.AddStyle(styles));
            _cell = new Cell().Add(new Paragraph("DETAILS"));
            _table.AddHeaderCell(_cell.AddStyle(styles));
                
            foreach (var item in formdata)
            {
                if ((item.Value).ToString() != "" & (item.Key).ToString() != "SignatureData" & (item.Key).ToString() != "SignatureData1" & (item.Key).ToString() != "SignatureData2")
                {
                    _cell = new Cell().Add(new Paragraph((item.Key).ToString()));
                    _table.AddCell(_cell);
                    _cell = new Cell().Add(new Paragraph((item.Value).ToString()));
                    _table.AddCell(_cell);
                }
            }
            doc.Add(_table);
         

//GET IMAGE OF CLIENTS SIGNATURE AND ADD TO THE PDF TABLE
//The Signature images on the HTML form are returned as base64. The data is stored in a hidden text field on the form. 
//The 'hidden' property was removed to view full code (SEE ROW 439,454 & 469 of INDEX FILE)

            Table table2 = new Table(3).UseAllAvailableWidth();
            Cell cell2 = new Cell().Add(new Paragraph("Client's Signature"));
            table2.AddHeaderCell(cell2.AddStyle(styles));

            if (formdata["SignatureData"].ToString() != "")
            {
                var signature = formdata["SignatureData"].ToString();
                byte[] bytes = Convert.FromBase64String(signature.Substring(22).Replace(" ", "+"));
                ImageData imageData = ImageDataFactory.Create(bytes);
                iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData).ScaleAbsolute(70, 75);
                cell2 = new Cell().Add(image);
                table2.AddCell(cell2);   
            }

            if (formdata["SignatureData1"].ToString() != "")
            {
                var signature = formdata["SignatureData1"].ToString();
                byte[] bytes = Convert.FromBase64String(signature.Substring(22).Replace(" ", "+"));
                ImageData imageData = ImageDataFactory.Create(bytes);
                iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData).ScaleAbsolute(70, 75);
                cell2 = new Cell().Add(new Paragraph("Joint Signature1"));
                table2.AddHeaderCell(cell2.AddStyle(styles));
                cell2 = new Cell().Add(image);
                table2.AddCell(cell2);
            }

            if (formdata["SignatureData2"].ToString() != "")
            {
                var signature = formdata["SignatureData2"].ToString();
                byte[] bytes = Convert.FromBase64String(signature.Substring(22).Replace(" ", "+"));
                ImageData imageData = ImageDataFactory.Create(bytes);
                iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData).ScaleAbsolute(70, 75);
                cell2 = new Cell().Add(new Paragraph("Joint Signature2"));
                table2.AddHeaderCell(cell2.AddStyle(styles));
                cell2 = new Cell().Add(image);
                table2.AddCell(cell2);
            }
            doc.Add(table2);
 
 //ADD TABLE TO HOUSE THE SECTION THAT WOULD BE FILLED OUT BY SIJ BACK OFFICE
            Table table3 = new Table(5).UseAllAvailableWidth();
            Cell cell3 = new Cell(4,1).Add(new Paragraph("FOR OFFICIAL USE"));
            table3.AddCell(cell3.AddStyle(styles));
            cell3 = new Cell().Add(new Paragraph("Time request Received:"))
                .SetBorderRight(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("________").SetFontColor(ColorConstants.WHITE))
                .SetBorderLeft(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("SIJL Representative:"))
                .SetBorderRight(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("________").SetFontColor(ColorConstants.WHITE))
                .SetBorderLeft(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("Date Order Received:"))
                .SetBorderRight(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("________").SetFontColor(ColorConstants.WHITE))
                .SetBorderLeft(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("Treasury:"))
                .SetBorderRight(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("________").SetFontColor(ColorConstants.WHITE))
                .SetBorderLeft(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("Time request Executed:"))
                .SetBorderRight(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("________").SetFontColor(ColorConstants.WHITE))
                .SetBorderLeft(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("Authorized Signatory BSU:"))
                .SetBorderRight(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("________").SetFontColor(ColorConstants.WHITE))
                .SetBorderLeft(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("Request Successful:"))
                .SetBorderRight(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("________").SetFontColor(ColorConstants.WHITE))
                .SetBorderLeft(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("Authorized Signatory BSU:"))
                .SetBorderRight(Border.NO_BORDER);
            table3.AddCell(cell3);
            cell3 = new Cell().Add(new Paragraph("________").SetFontColor(ColorConstants.WHITE))
                .SetBorderLeft(Border.NO_BORDER);
            table3.AddCell(cell3);
            doc.Add(table3);
            //doc.Add(new Paragraph(html).AddStyle(styles));
            doc.Close();

            byte[] pdfBytes = ms.ToArray();
            ms = new MemoryStream();
            ms.Write(pdfBytes, 0, pdfBytes.Length);
            ms.Position = 0;

 //SEND VIA WEB MAIL(MAIL CREDENTIALS REQUIRED IN ORDER FOR THIS TO WORK//
            //MailMessage message = new MailMessage();
            //message.From = new System.Net.Mail.MailAddress("xxxxxxx@gmail.com", "SIJ ADMIN");
            ////recipient address
            //var cliemail = output[23].value;//get the email of the client to be emailed
            //message.To.Add(new MailAddress("" + cliemail));
            //message.To.Add(new MailAddress("xxxxxxx@gmail.com"));
            //// Configuring the SMTP client
            //SmtpClient smtp = new SmtpClient();
            //smtp.Port = 587;
            //smtp.EnableSsl = true;
            //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            //smtp.UseDefaultCredentials = true;
            //smtp.Credentials = new System.Net.NetworkCredential("xxxxxxxx@gmail.com", "gmail_passsword_goes_here");
            //smtp.Host = "smtp.gmail.com";
            ////Formatted mail body
            //message.IsBodyHtml = true;
            //var cliname = output[2].value;//get the name of the client to be emailed
            //message.Subject = "Trade Order: " + cliname;
            //message.Body = "Good Day, Please see the attached trade request to be processed for "+ cliname;
            ////create attachment from memory stream
            //Attachment attachment = new Attachment(ms, new System.Net.Mime.ContentType("Application/Pdf"));
            //attachment.ContentDisposition.FileName = "TradeRequest-"+cliname+" "+DateTime.Now+".Pdf";
            //message.Attachments.Add(attachment);

            //smtp.Send(message);
            //smtp.Dispose();
            //ms.Flush();
 ////FINISH SEND VIA WEBMAIL
 
            return File(pdfBytes, "Application/Pdf", "TradeRequest.pdf");
            
        }
        
        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}
