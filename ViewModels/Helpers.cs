using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace _2home.ViewModels
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        // Regex kiểm tra mật khẩu mạnh (ít nhất 8 ký tự, chữ hoa, chữ thường, số và ký tự đặc biệt)
        private const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{6,}$";

        public StrongPasswordAttribute() : base("Mật khẩu phải dài ít nhất 8 ký tự, chứa chữ hoa, chữ thường, số và ký tự đặc biệt.")
        {
        }

        // Kiểm tra tính hợp lệ của mật khẩu
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            // Nếu mật khẩu trống hoặc không khớp với pattern
            if (string.IsNullOrEmpty(password) || !Regex.IsMatch(password, PasswordPattern))
            {
                return new ValidationResult(ErrorMessage); // Trả về thông báo lỗi nếu không hợp lệ
            }

            return ValidationResult.Success; // Trả về kết quả thành công nếu mật khẩu hợp lệ
        }
    }
}