using System;
using System.Collections.Generic;
using System.Text;

namespace TravelingBlog.Models.ViewModels.DTO
{
    public class AvatarDto
    {
        public int Id { get; set; }
        public byte[] Content { get; set; }
        public int UserId { get; set; }
    }
}
