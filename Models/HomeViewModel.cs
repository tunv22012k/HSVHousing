using System.Collections.Generic;
using System.Web.Mvc;

namespace HSVHousing.Models
{
    public class HomeViewModel
    {
        // Dùng để đổ dữ liệu vào dropdown Quận/Huyện
        public IEnumerable<SelectListItem> DistrictList { get; set; }

        // Danh sách các loại nhà trọ để hiển thị card
        public IEnumerable<Category> RoomCategories { get; set; }

        // Danh sách tin đăng mới nhất
        public IEnumerable<Listing> NewestListings { get; set; }

        // Danh sách bài viết blog mới nhất
        public IEnumerable<BlogPost> NewestBlogPosts { get; set; }

        // Dùng để lưu số lượng tin đăng cho mỗi loại nhà trọ
        public Dictionary<string, int> CategoryCounts { get; set; }

        public HomeViewModel()
        {
            DistrictList = new List<SelectListItem>();
            RoomCategories = new List<Category>();
            NewestListings = new List<Listing>();
            NewestBlogPosts = new List<BlogPost>();
            CategoryCounts = new Dictionary<string, int>();
        }
    }
}