using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

public class MailSender
{
    private MimeMessage _email;

    public MailSender()
    {
        _email = new MimeMessage();
    }

    public void SendMail()
    {
        _email.From.Add(new MailboxAddress("Berco Andrei", "andrei2003berco@gmail.com"));
        _email.To.Add(new MailboxAddress("Andrei Berco", "andrei.berco@isa.utm.md"));

        _email.Subject = "Test email";
        _email.Body = new TextPart("plain")
        {
            Text = "This is a test email for laoratory 3!!!!!!!!"
        };

        using (var smtp = new SmtpClient())
        {
            smtp.Connect("smtp.gmail.com", 587, false);
            smtp.Authenticate("andrei2003berco@gmail.com", "");
            smtp.Send(_email);
            smtp.Disconnect(true);
        }
    }

}