using System.Net.Mail;

namespace Cardápio.Infra.Helpers
{
    public class CEmail
    {
        public static bool SendEmail(string subject, string body, string emailFrom, string emailTo, List<string> emailBccList)
        {
            bool returnValue = true;

            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(emailFrom, emailFrom);
                mail.Sender = new MailAddress(emailFrom, emailFrom);
                mail.To.Add(emailTo);
                if (emailBccList != null && emailBccList.Count > 0)
                {
                    foreach (string emailBcc in emailBccList)
                        if (!string.IsNullOrEmpty(emailBcc))
                            mail.Bcc.Add(emailBcc);
                }

                mail.IsBodyHtml = true;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.Body = body;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.Subject = subject;

                SmtpClient smtp = new SmtpClient("smtp.tvplayer.com.br", 587);
                smtp.Credentials = new System.Net.NetworkCredential("site@tvplayer.com.br", "[SENHA_SMTP_REMOVIDA]");

                try
                {
                    smtp.Send(mail);
                }
                catch
                {
                    returnValue = false;
                }
            }
            catch { returnValue = false; }

            return returnValue;
        }
    }
}
