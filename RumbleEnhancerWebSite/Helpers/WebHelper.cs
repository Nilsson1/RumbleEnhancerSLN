using HtmlAgilityPack;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace RumbleEnhancerWebSite.Helpers
{
    public static class WebHelper
    {
        public static void SendEmail(string email, string verificationString)
        {
            string to = email; //To address    
            string from = "noreplyrumbleenhancer@gmail.com"; //From address    
            MailMessage message = new MailMessage(from, to);

            string mailbody = "Verify your RumbleEnhancer account by typing the following identifier into the description of a video of yours. <br/> Identifier: " + verificationString;
            message.Subject = "Verify your RumbleEnhancer";
            message.Body = mailbody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587); //Gmail smtp    
            System.Net.NetworkCredential basicCredential1 = new
            System.Net.NetworkCredential("noreplyrumbleenhancer@gmail.com", "zggtniiqvsnxwxkn");
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;
            try
            {
                client.Send(message);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string[] GetVideoData(string url)
        {
            string modifiedUrl = url;
            if (url.StartsWith("rumble.com/") || url.StartsWith("www.rumble.com/"))
                modifiedUrl = url.Insert(0, "https://");

            if (!(modifiedUrl.StartsWith("https://rumble.com/") || modifiedUrl.StartsWith("http://rumble.com/") || modifiedUrl.StartsWith("https://www.rumble.com/") || modifiedUrl.StartsWith("http://www.rumble.com/")))
                return new string[] { "", ""};

            string HTML;
            string[] data = { "", "" };

            using (var wc = new WebClient())
            {
                try
                {
                    HTML = wc.DownloadString(modifiedUrl);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(HTML);
                    HtmlNode accountName = doc.DocumentNode.SelectSingleNode("//div[@class='media-heading-name']");
                    HtmlNode description = doc.DocumentNode.SelectSingleNode("//p[@class='media-description']");
                    if (description != null)
                    {
                        data[0] = accountName.InnerHtml.ToString();
                        data[1] = description.InnerHtml.ToString();
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Implement exception return
                }
            }
            return data;
        }
    }
}
