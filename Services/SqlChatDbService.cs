using LAST.Data.Context;
using LAST.Models.Api;
using LAST.Models.IdentityModels;
using Microsoft.EntityFrameworkCore;
using System;

namespace LAST.Services
{
    public class SqlChatDbService: IChatDbService
    {
        private AppDbContext dbContext;

        public SqlChatDbService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public List<ConversationSearchItem> FindUsersAndConversations(string searchQuery)
        {
            var conversations = FindConversations(searchQuery);
            var users = FindUsers(searchQuery);

            var searchResults = conversations
                .Select(c => new ConversationSearchItem(c))
                .Concat(users.Select(u => new ConversationSearchItem(u)))
                .ToList();
            return searchResults;
        }


        public List<ConversationSearchItem> FindUsersAndConversations(string searchQuery, int currentUserId)
        {
            var searchResults = FindUsersAndConversations(searchQuery)
                .Where(c => !c.IsUser || (c.IsUser && c.Id != currentUserId))
                .ToList();
            return searchResults;
        }


        public void SaveMessage(Message message)
        {
            dbContext.Messages.Add(message);
            dbContext.SaveChanges();
            dbContext.Entry(message)
                .Reference(msg => msg.User)
                .Load();
        }


        public List<ConversationListItem> GetConversationsForUser(int userId)
        {
            var queryResults = dbContext.Conversations
                .Where(x => x.Participants.Any(u => u.Id == userId))
                .Include(c => c.Participants)
                .ToList();

            var newMessages = GetNewMessageCountForUser(userId);

            // transform query results into API compliant format
            var conversations = queryResults
                .Select(c => {
                    var lastMsg = GetLastNMessages(c.Id, 1).FirstOrDefault();
                    var newMsgCount = newMessages.GetValueOrDefault(c.Id, 0);
                    string name = c.Name;
                    int? otherUserId = null;
                    if (!c.IsGroupChat)
                    {
                        var otherUser = c.Participants.Where(p => p.Id != userId).FirstOrDefault();
                        // process an edge case when 1 (of the 2) user has left a private chat
                        if (otherUser == null)
                        {
                            var memberEmails = Helpers.ParseConversationName(c.Name);
                            var currentUser = c.Participants.First(u => u.Id == userId);
                            var otherUserEmail = memberEmails.First(email => !email.Equals(currentUser.Email));
                            otherUser = dbContext.Users.Where(u => u.Email.Equals(otherUserEmail)).FirstOrDefault();
                        }
                        name = otherUser?.UserName ?? "name missing";
                        otherUserId = otherUser?.Id ?? null;
                    }

                    return new ConversationListItem(c.Id, name, lastMsg, newMsgCount, otherUserId);
                })
                .ToList();

            return conversations;
        }


        public Dictionary<int, int> GetNewMessageCountForUser(int userId)
        {
            var newMessagesGroupedQuery =
                from msg in dbContext.Messages
                join userAct in dbContext.UserConversationActivities
                on msg.ConversationId equals userAct.ConversationId into tempJoin
                from userAct in tempJoin.DefaultIfEmpty()
                where userAct.UserId == userId
                    && ((userAct.LastDisconnect >= userAct.LastConnect && msg.SentAt > userAct.LastDisconnect)
                    || userAct.LastConnect == null)
                group msg by msg.ConversationId into msgGroup
                select new { ConversationId = msgGroup.Key, Count = msgGroup.Count() };

            var newMessageCounts = newMessagesGroupedQuery.ToDictionary(k => k.ConversationId, v => v.Count);
            return newMessageCounts;
        }


        public List<Conversation> GetConversationEntriesForUser(int userId)
        {
            var queryResults = dbContext.Conversations
                .Where(x => x.Participants.Any(u => u.Id == userId))
                .ToList();

            return queryResults;
        }


        public List<Message> GetLastNMessages(int conversationId, int n)
        {
            var messages = dbContext.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)
                .Take(n)
                .Reverse()
                .Include(m => m.User)
                .ToList();
            return messages;
        }


        public List<Message> GetPrevNMessages(int conversationId, int n, DateTime dateTimeOfLastMessage)
        {
            var messages = dbContext.Messages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SentAt.CompareTo(dateTimeOfLastMessage) < 0)
                .OrderByDescending(m => m.SentAt)
                .Take(n)
                .Reverse()
                .Include(m => m.User)
                .ToList();
            return messages;
        }


        public Conversation CreateGroupChat(int creatorUserId, string groupName)
        {
            var isDuplicateGroupName = dbContext.Conversations
                .Any(conv => conv.Name.Equals(groupName));

            if (isDuplicateGroupName)
                return null;

            var creator = dbContext.Users.Find(creatorUserId);
            var newGroup = Conversation.CreateGroupConversation(creator, groupName);
            dbContext.Conversations.Add(newGroup);
            dbContext.SaveChanges();
            return newGroup;
        }


        public Conversation CreatePrivateChat(int user1Id, int user2Id)
        {
            var members = dbContext.Users
                .Where(u => u.Id == user1Id || u.Id == user2Id)
                .ToList();
            var user1 = members.First(u => u.Id == user1Id);
            var user2 = members.First(u => u.Id == user2Id);

            // check if conversation already exists (should fire when only the calling user has left)
            var potentiallyExistingConversationName = Conversation.BuildPrivateConversationName(user1, user2);
            var existingConversation = dbContext.Conversations
                .FirstOrDefault(c => !c.IsGroupChat && c.Name.Equals(potentiallyExistingConversationName));
            Conversation conversation = null;
            if (existingConversation != null)
            {
                conversation = existingConversation;
                dbContext.Entry(conversation)
                    .Collection(c => c.Participants)
                    .Load();
                members.ForEach(m => {
                    if (!conversation.Participants.Any(p => p.Id == m.Id))
                    {
                        conversation.Participants.Add(m);
                    }
                });

                dbContext.SaveChanges();
            }
            // conversation doesn't exist
            else
            {
                conversation = Conversation.CreatePrivateConversation(user1, user2);
                dbContext.Conversations.Add(conversation);
                dbContext.SaveChanges();
            }

            return conversation;
        }


        public void LeaveConversation(int conversationId, int userId)
        {
            var conversation = dbContext.Conversations
                .Where(conv => conv.Id == conversationId)
                .Include(conv =>
                    conv.Participants.Where(user => user.Id == userId))
                .SingleOrDefault();

            if (conversation == null)
                return;

            conversation.Participants.RemoveAll(u => u.Id == userId);

            var membersLeft = dbContext.Entry(conversation)
                .Collection(x => x.Participants)
                .Query()
                .Where(user => user.Id != userId)
                .Count();
            if (membersLeft == 0)
            {
                dbContext.Conversations.Remove(conversation);
            }

            dbContext.SaveChanges();
        }


        public Conversation AddConversationMember(int conversationId, int userId)
        {
            var conversation = dbContext.Conversations
                .Where(conv => conv.Id == conversationId)
                .Include(conv => conv.Participants)
                .Single();
            if (!conversation.Participants.Any(p => p.Id == userId))
            {
                var user = dbContext.Users.Find(userId);
                conversation.Participants.Add(user);
                dbContext.SaveChanges();
            }
            return conversation;
        }


        public List<ConversationMember> GetUsersInConversation(int conversationId)
        {
            var conv = dbContext.Conversations.Find(conversationId);
            dbContext.Entry(conv)
                .Collection(c => c.Participants)
                .Load();

            // get user states (active or not) for users in this conversation
            var userStates = conv.Participants
                .Select(x => new {
                    UserId = x.Id,
                    IsActive = (x.LastConnect > x.LastDisconnect) || (x.LastDisconnect == null && x.LastConnect != null)
                })
                .ToDictionary(k => k.UserId, v => v.IsActive);

            var users = conv.Participants
                .Select(x => new ConversationMember(x, userStates.GetValueOrDefault(x.Id, false)))
                .OrderBy(x => x.Username)
                .ToList();
            return users;
        }


        public void RegisterUserConnectingToConversation(int userId, int conversationId)
        {
            var userActivity = dbContext.UserConversationActivities.Find(userId, conversationId);
            if (userActivity == null)
            {
                userActivity = new UserConversationActivity(userId, conversationId);
                dbContext.UserConversationActivities.Add(userActivity);
            }
            userActivity.LastConnect = DateTime.UtcNow;
            dbContext.SaveChanges();
        }


        public void RegisterUserDisconnectingFromConversation(int userId, int conversationId)
        {
            var userActivity = dbContext.UserConversationActivities.Find(userId, conversationId);
            if (userActivity == null)
            {
                userActivity = new UserConversationActivity(userId, conversationId);
                dbContext.UserConversationActivities.Add(userActivity);
            }
            userActivity.LastDisconnect = DateTime.UtcNow;
            dbContext.SaveChanges();
        }


        public void RegisterUserActivity(int userId)
        {
            var user = dbContext.Users.Find(userId);

            if (user == null)
                return;

            user.LastConnect = DateTime.UtcNow;
            dbContext.SaveChanges();
        }


        public void RegisterUserInactivity(int userId)
        {
            var user = dbContext.Users.Find(userId);

            if (user == null)
                return;

            user.LastDisconnect = DateTime.UtcNow;
            dbContext.SaveChanges();
        }


        public bool IsUserConversationMember(int userId, int conversationId)
        {
            var conversation = dbContext.Conversations
                .Where(conv => conv.Id == conversationId)
                .Include(conv => conv.Participants.Where(user => user.Id == userId))
                .FirstOrDefault();

            if (conversation == null)
                return false;

            var res = conversation.Participants.Any(user => user.Id == userId);
            return res;
        }


        /// <summary>
        /// Get conversations with a name matching the search query
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        private List<Conversation> FindConversations(string searchQuery)
        {
            string normalizedSearchQuery = searchQuery.ToLower();
            var foundConversations = dbContext.Conversations
                .Where(con => con.IsGroupChat && con.Name.ToLower().Contains(normalizedSearchQuery))
                .ToList();
            return foundConversations;
        }


        /// <summary>
        /// Get users with a name matching the search query
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        private List<AppUser> FindUsers(string searchQuery)
        {
            string normalizedSearchQuery = searchQuery.ToLower();
            var foundUsers = dbContext.Users
                .Where(u => u.Email.ToLower().Contains(normalizedSearchQuery) ||
                    u.UserName.ToLower().Contains(normalizedSearchQuery))
                .ToList();
            return foundUsers;
        }
    }
}
