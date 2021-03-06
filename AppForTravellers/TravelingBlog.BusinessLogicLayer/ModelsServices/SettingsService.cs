﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TravelingBlog.BusinessLogicLayer.ModelsServices.Contracts;
using TravelingBlog.BusinessLogicLayer.SecondaryServices.AzureStorage;
using TravelingBlog.BusinessLogicLayer.SecondaryServices.LoggerService;
using TravelingBlog.DataAcceesLayer.Models;
using TravelingBlog.DataAcceesLayer.Models.Entities;
using TravelingBlog.DataAcceesLayer.Repositories.Contracts;
using TravelingBlog.Models.ViewModels.DTO;
using System;
using Microsoft.AspNetCore.Http;

namespace TravelingBlog.BusinessLogicLayer.ModelsServices
{
    public class SettingsService: ISettingsService
    {
        public IAzureBlob azureBlob;
        readonly IUnitOfWork unitOfWork;
        private readonly IHttpContextAccessor http;
        public SettingsService(IUnitOfWork unitOfWork, ILoggerManager logger, IMapper mapper,
            IHttpContextAccessor contextAccessor)
        {
            this.unitOfWork = unitOfWork;
            http = contextAccessor;
        }

        private IRepository<UserInfo> UserRepository => unitOfWork.GetRepository<UserInfo>();
        private IRepository<Avatar> AvatarRepository => unitOfWork.GetRepository<Avatar>();
        public void EditPhoto(SettingDTO settingDTO)
        {
            var image = unitOfWork.GetRepository<UserInfo>().GetAll().Where(u => u.IdentityId == settingDTO.Id)
                .Include(u => u.UserImage).SingleOrDefault();

            var userHsImg = image.UserImage == null ? false : true;

            if (!userHsImg)
            {
                var img = new UserImage
                {
                    Path = settingDTO.PhotoUser
                };
                //image.Include(t => t.UserImage);

                img.UserInfo = image;
                img.UserInfoId = image.Id;
                unitOfWork.GetRepository<UserImage>().Add(img);
            }
            else
            {
                image.UserImage.Path = settingDTO.PhotoUser;
                unitOfWork.GetRepository<UserImage>().Update(image.UserImage); 
            }

            unitOfWork.Complete();
        }

        public void EditUserName(SettingDTO settingDTO)
        {

            var user = unitOfWork.GetRepository<UserInfo>().GetAll();

            foreach (var i in user)
            {
                if (i.IdentityId == settingDTO.Id)
                {
                    i.FirstName = settingDTO.FirstName;
                    i.LastName = settingDTO.LastName;
                    unitOfWork.GetRepository<UserInfo>().Update(i);
                }
            }
            unitOfWork.Complete();
        }

        public void AddPhotoToDb(AvatarDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException();

            

            var userName = http.HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToString() == "id")?.Value;

            var avat = AvatarRepository
                .Find(a => a.User.IdentityId == userName);

            var user = UserRepository
                .Find(u => u.IdentityId == userName);
            
            if(avat == null)
            {
                avat = new Avatar
                {
                    User = user,
                    Content = dto.Content
                };
                AvatarRepository
                    .Add(avat);
            }
            else
            {
                avat.Content = dto.Content;
                AvatarRepository.Update(avat);
            }

            unitOfWork.Complete();        
        }
    }
}
