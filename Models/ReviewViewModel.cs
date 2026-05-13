using System.ComponentModel.DataAnnotations;

namespace HSVHousing.Models
{
    public class ReviewViewModel
    {
        [Required]
        public int ListingID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá.")]
        [Range(1, 5, ErrorMessage = "Vui lòng chọn từ 1 đến 5 sao.")]
        [Display(Name = "Đánh giá của bạn")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Bình luận không được quá 1000 ký tự.")]
        [Display(Name = "Viết bình luận (tùy chọn)")]
        public string Comment { get; set; }
    }
}
