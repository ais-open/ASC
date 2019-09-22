using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using AzureServiceCatalog.Models;
using System.Net.Mail;
using System.Text;

namespace AzureServiceCatalog.Helpers
{
    public class NotificationHelper
    {
        private const string htmlLineBreak = "<br />";
        public async Task SendFeedbackNotificationAsync(FeedbackViewModel model, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "NotificationHelper:SendFeedbackNotification");
            try
            {
                if (model != null)
                {
                    var subject = string.Format(Config.FeedbackEmailSubjectFormat, model.Subject);
                    var message = $"Name: {model.Name}{htmlLineBreak}Email: {model.Email}{htmlLineBreak}Subject: {model.Subject}{htmlLineBreak}Comments: {model.Comments}";

                    await SendEmailNotification(Config.AdminEmailAddress, subject, message, parentOperationContext);
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task SendSupportNotificationAsync(EnrollmentSupportViewModel model, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "NotificationHelper:SendSupportNotification");
            try
            {
                if (model != null)
                {
                    var subject = Config.SupportEmailSubjectFormat;
                    var message = $"First Name: {model.FirstName}{htmlLineBreak}Last Name: {model.LastName}{htmlLineBreak}Company: {model.Company}{htmlLineBreak}Title: {model.Title}{htmlLineBreak}" +
                                  $"Email: {model.Email}{htmlLineBreak}Phone: {model.Phone}{htmlLineBreak}Comments: {model.Comments}";

                    await SendEmailNotification(Config.AdminEmailAddress, subject, message, parentOperationContext);
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        public async Task SendActivationNotificationAsync(Organization org, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "NotificationHelper:SendActivationNotification");
            try
            {
                if (org != null)
                {
                    var subject = $"New Activation: {org.VerifiedDomain}";
                    var message = $"New Activation{htmlLineBreak}First Name: {ClaimsPrincipal.Current.FirstName()}{htmlLineBreak}Last Name: {ClaimsPrincipal.Current.LastName()}{htmlLineBreak}Email: {ClaimsPrincipal.Current.EmailAddress()}{htmlLineBreak}Org Domain: {org.VerifiedDomain}{htmlLineBreak}Date Enrolled: {org.EnrolledDate}{htmlLineBreak}";

                    await SendEmailNotification(Config.AdminEmailAddress, subject, message, parentOperationContext);
                }
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }

        private async Task SendEmailNotification(string sendToEmailAddress, string subject, string message, BaseOperationContext parentOperationContext)
        {
            var thisOperationContext = new BaseOperationContext(parentOperationContext, "NotificationHelper:SendEmailNotification");
            try
            {
                if (string.IsNullOrEmpty(sendToEmailAddress))
                {
                    string invalidEmailMessage = "Notification not sent. Email address not available";
                    Trace.TraceError(string.Format(message));
                    Trace.TraceError(invalidEmailMessage);
                    throw new FormatException(invalidEmailMessage);
                }

                MailAddress from = new MailAddress(Config.NotificationFromEmailAddress, Config.NotificationFromName);
                MailAddress to = new MailAddress(sendToEmailAddress);
                MailMessage mailMessage = new MailMessage(from, to);
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.IsBodyHtml = true;

                SmtpClient smtpClient = new SmtpClient(Config.SmtpHost, int.Parse(Config.SmtpPort))
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential()
                    {
                        UserName = Config.SmtpUserName,
                        Password = Config.SmtpPassword
                    }
                };

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(thisOperationContext.OperationId, thisOperationContext.OperationName, ex);
                throw new HttpException("Email could not be sent!");
            }
            finally
            {
                thisOperationContext.CalculateTimeTaken();
                TraceHelper.TraceOperation(thisOperationContext);
            }
        }
    }
}