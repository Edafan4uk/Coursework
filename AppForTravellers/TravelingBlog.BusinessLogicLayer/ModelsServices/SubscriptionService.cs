﻿using System.Collections.Generic;
using System.Linq;
using TravelingBlog.BusinessLogicLayer.ModelsServices.Contracts;
using TravelingBlog.DataAcceesLayer.Data;
using TravelingBlog.DataAcceesLayer.Models.Entities;
using TravelingBlog.Models.ViewModels.DTO;

namespace TravelingBlog.BusinessLogicLayer.ModelsServices
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext Context;

        public SubscriptionService(ApplicationDbContext context)
        {
            this.Context = context;
        }

        public IEnumerable<SubscriptionDTO> GetUserSubscription(string id)
        {
            var user = Context.UserInfos.FirstOrDefault(x => x.IdentityId == id);
            
            List<SubscriptionDTO> subs = new List<SubscriptionDTO>();

            if (user is null)
                return subs;

            var entity = Context.Subscriptions.Where(x => x.UserInfoId == user.Id).ToList();

            if (entity == null)
                return subs;
            foreach (var i in entity)
            {

                var currUser = Context.UserInfos.FirstOrDefault(x => x.Id == i.SubcriberId);
                if (currUser == null)

                {
                    continue;
                }
                subs.Add(new SubscriptionDTO
                {
                    FirstName = currUser.FirstName,
                    LastName = currUser.LastName,
                    SubcriberId = i.SubcriberId,
                    UserInfoId = i.UserInfoId
                });
            }


            return subs;
        }

        public bool SubscribeTo(string id, int Subscriberid)
        {

            var user = Context.UserInfos.FirstOrDefault(x => x.IdentityId == id);
            var Subscriber = Context.UserInfos.FirstOrDefault(x => x.Id == Subscriberid);
            if (Subscriberid == user.Id)

            {
                return false;
            }

            {
                var subCheck = Context.Subscriptions.Where(x => x.UserInfoId == user.Id);
                foreach (var i in subCheck)
                {
                    if (i.SubcriberId == Subscriberid)
                    {
                        return true;
                    }
                }
                Subscription sub = new Subscription { SubscriberIdNavidgation = Subscriber, UserInfoIdNavigation = user };
                Context.Subscriptions.Add(sub);
                Context.SaveChanges();
                return true;
            }
        }

        public bool UnSubscribeFrom(string id, int Subscriberid)
        {

            var user = Context.UserInfos.FirstOrDefault(x => x.IdentityId == id);
            var Subscriber = Context.UserInfos.FirstOrDefault(x => x.Id == Subscriberid);
            if (user == null || Subscriber == null)

            {
                return false;
            }
            var subEntity = Context.Subscriptions.FirstOrDefault(x => x.SubcriberId == Subscriber.Id && x.UserInfoId == user.Id);
            if (subEntity == null)
            {
                return false;
            }
            Context.Subscriptions.Remove(subEntity);
            Context.SaveChanges();

            return true;
        }
    }
}
