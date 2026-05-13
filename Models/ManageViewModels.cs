using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HSVHousing.Models
{
    // ViewModel cho form Chỉnh sửa Hồ sơ cá nhân
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
    }

    // ViewModel cho form Đăng tin và Sửa tin
    public class ListingFormViewModel
    {
        public int ListingID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [Display(Name = "Tiêu đề tin đăng")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả.")]
        [Display(Name = "Mô tả chi tiết")]
        [AllowHtml]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá thuê.")]
        [Display(Name = "Giá thuê (VND/tháng)")]
        [Range(100000, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 100,000.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập diện tích.")]
        [Display(Name = "Diện tích (m²)")]
        [Range(1, double.MaxValue, ErrorMessage = "Diện tích phải lớn hơn 0.")]
        public double Area { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể.")]
        [Display(Name = "Địa chỉ cụ thể (Số nhà, tên đường)")]
        public string AddressStreet { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Phường/Xã.")]
        [Display(Name = "Phường/Xã")]
        public int WardID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Loại nhà trọ.")]
        [Display(Name = "Loại nhà trọ")]
        public int CategoryID { get; set; }

        // Dùng để đổ dữ liệu vào các dropdown
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> WardList { get; set; }
    }
}