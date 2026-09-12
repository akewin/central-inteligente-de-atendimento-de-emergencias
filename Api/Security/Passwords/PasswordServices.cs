using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Middlewares;
using Api.Models;
using Api.Helpers;
using Api.Security.Jwt;
using Api.Security.Passwords;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Api.Services.AuthServices
{
    public class PasswordServices
    {
        private readonly ApiDbContext _context;
        private readonly CreateSystemLog _createSystemLog;
        private readonly PasswordResetEmailService _passwordResetEmailService;

        public PasswordServices(ApiDbContext context, CreateSystemLog createSystemLog, PasswordResetEmailService passwordResetEmailService)
        {
            _context = context;
            _createSystemLog = createSystemLog;
            _passwordResetEmailService = passwordResetEmailService;
        }

        public async Task RequestNewPasswordAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new AppException("Email inválido.", (int)HttpStatusCode.BadRequest);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                throw new AppException("Email não cadastrado.", (int)HttpStatusCode.NotFound);

            var claims = new[]
            {
                new System.Security.Claims.Claim("id", user.Id.ToString()),
                new System.Security.Claims.Claim("email", user.Email)
            };

            var token = JwtServices.Create(claims, expireMinutes: 15);

            var webAppUrl = EnvLoader.GetEnv("WEB_APP_URL");
            var resetLink = $"{webAppUrl.TrimEnd('/')}/password-reset?token={token}";

            await _passwordResetEmailService.SendEmailAsync(user.Email, resetLink);

            Console.WriteLine($"[DEBUG] Link de redefinição enviado para {email}: {resetLink}");

            await _createSystemLog.ExecuteAsync(SystemLogActionFactory.NewPasswordRequest(user.Username), user.Id);
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
                throw new AppException("Token ou senha inválidos.", (int)HttpStatusCode.BadRequest);

            System.Security.Claims.ClaimsPrincipal principal;
            try
            {
                principal = JwtServices.Validate(token, validateLifetime: true);
            }
            catch
            {
                throw new AppException("Token inválido ou expirado.", (int)HttpStatusCode.Unauthorized);
            }

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new AppException("Token inválido.", (int)HttpStatusCode.Unauthorized);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new AppException("Usuário não encontrado.", (int)HttpStatusCode.NotFound);

            user.Password = PasswordHash.Generate(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await _createSystemLog.ExecuteAsync(SystemLogActionFactory.PasswordReset(user.Username), user.Id);
        }
    }
}
