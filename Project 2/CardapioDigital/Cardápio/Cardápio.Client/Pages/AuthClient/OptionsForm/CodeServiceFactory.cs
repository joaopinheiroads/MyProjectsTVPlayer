namespace Cardápio.Client.Pages.AuthClient.OptionsForm
{
    public class CodeServiceFactory
    {
        private readonly EmailCodeSenderService _emailService;
        private readonly SmsCodeSenderService _smsService;

        public CodeServiceFactory(EmailCodeSenderService emailService, SmsCodeSenderService smsService)
        {
            _emailService = emailService;
            _smsService = smsService;
        }

        public ICodeSenderService GetService(LoginStep step)
        {
            return step == LoginStep.Email ? _emailService : _smsService;
        }
    }
}
