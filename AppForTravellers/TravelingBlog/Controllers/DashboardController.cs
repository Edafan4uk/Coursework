using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelingBlog.BusinessLogicLayer.ModelsServices.Contracts;
using TravelingBlog.DataAcceesLayer.Models.Entities;


namespace TravelingBlog.Controllers
{
    [Authorize(Policy = "ApiUser")]
    [Route("api/[controller]/[action]")]
    public class DashboardController : Controller
    {
        private readonly ClaimsPrincipal caller;

        private readonly IUserService _userService;

        public DashboardController(UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            caller = httpContextAccessor.HttpContext.User;
            _userService = userService;
        }

        // GET api/dashboard/home
        [HttpGet]
        public async Task<IActionResult> Home()
        {
            var userId = caller.Claims.Single(c => c.Type == "id");

            var user = await _userService.GetUserWithAvatar(userId.Value);

            var url = !(user.Avatar?.Content is null) ? Convert.ToBase64String(user.Avatar.Content) : null;
                  

            var model = new
            {
                Message = "This is secure API and user data!",
                user.FirstName,
                user.LastName,
                PictureUrl = url,
                user.Identity.FacebookId
            };

            return new OkObjectResult(model);
        }
    }
}