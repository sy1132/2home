using System.ComponentModel.DataAnnotations;

namespace _2home.ViewModels
{
    public class UserRegisterViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải có độ dài từ 3 đến 50 ký tự.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, ErrorMessage = "Họ và tên không quá 100 ký tự.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StrongPassword(ErrorMessage = "Mật khẩu phải dài ít nhất 6 ký tự, chứa chữ hoa, chữ thường, số và ký tự đặc biệt.")]

        public string Password { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống.")]
        public string Gender { get; set; }
    }
}
