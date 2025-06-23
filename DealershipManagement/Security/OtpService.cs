using Dealership_Management.Data;
using Dealership_Management.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Dealership_Management.Security
{
    public class OtpService : IOtpService
    {
        private readonly DealershipDbContext _context;
        private static readonly Random _random = new Random();
        private const int OtpLength = 6;
        private const int OtpExpiryMinutes = 5;

        public OtpService(DealershipDbContext context)
        {
            _context = context;
        }

        public async Task<OtpCode> GenerateOtpAsync(int userId, OtpPurpose purpose)
        {
            // Invalidate previous OTPs for this user and purpose
            var oldOtps = await _context.OtpCodes
                .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
            foreach (var otp in oldOtps)
            {
                otp.IsUsed = true;
            }

            var code = GenerateRandomCode(OtpLength);
            var otpCode = new OtpCode
            {
                UserId = userId,
                Code = code,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.OtpCodes.Add(otpCode);
            await _context.SaveChangesAsync();

            // Simulate delivery
            Console.WriteLine($"[OTP] UserId: {userId}, Purpose: {purpose}, Code: {code}");

            return otpCode;
        }

        public async Task<bool> ValidateOtpAsync(int userId, string code, OtpPurpose purpose)
        {
            var otp = await _context.OtpCodes
                .Where(o => o.UserId == userId && o.Purpose == purpose && o.Code == code && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
            if (otp == null)
                return false;
            if (otp.ExpiresAt < DateTime.UtcNow)
                return false;
            otp.IsUsed = true;
            await _context.SaveChangesAsync();
            return true;
        }

        private static string GenerateRandomCode(int length)
        {
            var chars = "0123456789";
            var codeChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                codeChars[i] = chars[_random.Next(chars.Length)];
            }
            return new string(codeChars);
        }
    }
}