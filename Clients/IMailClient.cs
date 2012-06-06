using System;

namespace MailDeluxe {
    public interface IMailClient : IDisposable {
        int GetMessageCount();
        MailMessage GetMessage(int index, bool headersonly = false);
        MailMessage GetMessage(string uid, bool headersonly = false);
        void DeleteMessage(string uid);
        void DeleteMessage(MailDeluxe.MailMessage msg);
    }
}
