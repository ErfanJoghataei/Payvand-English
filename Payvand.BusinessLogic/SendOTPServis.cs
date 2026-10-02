using Microsoft.Extensions.Configuration;
using Kavenegar;
using Payvand.DAL.Entities;
using Payvand.DAL.AppDbContext;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Payvand.BusinessLogic
{
    public class SendOTPServis
    {
        private readonly IConfiguration configuration;
        private readonly PayvandDbContext context;
        private readonly ILogger<SendOTPServis> logger;

        public SendOTPServis(IConfiguration configuration, PayvandDbContext context, ILogger<SendOTPServis> logger)
        {
            this.configuration = configuration;
            this.context = context;
            this.logger = logger;
        }
        private string GenerateOtp(int length = 6)
        {
            var min = (int)Math.Pow(10, length - 1);
            var max = (int)Math.Pow(10, length);
            return RandomNumberGenerator.GetInt32(min, max).ToString();
        }

        public async Task<bool> SendOtpLogin(string reciver)
        {
            logger.LogInformation("Sending login OTP. PhoneNumber: {PhoneNumber}", reciver);
            var ApiKey = configuration["Kavenegar:api"];
            var code = GenerateOtp();
            var api = new KavenegarApi(ApiKey);
            try
            {
                api.VerifyLookup(reciver, code, "logincode");
                var oldCodes = context.Otp.Where(c => c.PhoneNumber == reciver);
                context.Otp.RemoveRange(oldCodes);
                await context.Otp.AddAsync(new Otp
                {
                    Code = code,
                    PhoneNumber = reciver,
                    Expiration = DateTime.UtcNow.AddMinutes(5)
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Login OTP sent and stored. PhoneNumber: {PhoneNumber}", reciver);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Login OTP send failed. PhoneNumber: {PhoneNumber}", reciver);
                return false;

            }




        }
        public bool SendOtpResetPass(string reciver)
        {
            logger.LogInformation("Sending reset password OTP. PhoneNumber: {PhoneNumber}", reciver);
            var ApiKey = configuration["Kavenegar:api"];
            var sender = configuration["Kavenegar:Sender"];
            var code = GenerateOtp();
            var message = $"🚀 وقتشه رمزتون رو تازه کنید!  \r\nکد شما برای تغییر رمز: {code}  \r\n<<پیوند>>\r\n";
            var api = new KavenegarApi(ApiKey);
            try
            {
                api.Send(sender, reciver, message);
                var oldCodes = context.Otp.Where(c => c.PhoneNumber == reciver);
                context.Otp.RemoveRange(oldCodes);
                context.Otp.Add(new Otp
                {
                    Code = code,
                    PhoneNumber = reciver,
                    Expiration = DateTime.UtcNow.AddMinutes(5)
                });
                context.SaveChanges();
                logger.LogInformation("Reset password OTP sent and stored. PhoneNumber: {PhoneNumber}", reciver);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reset password OTP send failed. PhoneNumber: {PhoneNumber}", reciver);
                return false;

            }




        }

    }
}
