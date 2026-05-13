using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HSVHousing.Models
{
    public class CommentViewModel
    {
        [Required]
        public int ListingID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung bình luận.")]
        [StringLength(1000, ErrorMessage = "Bình luận không được quá 1000 ký tự.")]
        [AllowHtml]
        public string Comment { get; set; }
    }
}