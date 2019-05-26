using System;
using System.Collections.Generic;
using System.Text;
using TravelingBlog.DataAcceesLayer.Contracts;
using TravelingBlog.DataAcceesLayer.Models.Entities;

namespace TravelingBlog.DataAcceesLayer.Models
{
    public class Avatar : IEntity
    {
        public int Id { get; set; }
        public byte[] Content { get; set; }
        public UserInfo User { get; set; }
    }
}
