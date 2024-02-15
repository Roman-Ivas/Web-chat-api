using LAST.Models.Api;
using LAST.Services;
using Microsoft.AspNetCore.SignalR;

namespace LAST.Hubs
{
    public class ChatHub:Hub
    {
        private readonly IChatDbService chatDbService;

        public ChatHub(IChatDbService chatDbService)
        {
            this.chatDbService = chatDbService;
        }
        public async Task<Object> SendMessage(MessageDataFromClient messageData)
        {
            var userId = Helpers.GetUserIdFromClaims(Context.User);
            var userName = Helpers.GetUserNameFromClaims(Context.User);

            bool isGroupChat = messageData.ReceiverConversationId != null && messageData.ReceiverUserId == null;
            bool isPrivateChat = messageData.ReceiverUserId != null;

            string hubGroupName = null;
            if (!isGroupChat && !isPrivateChat)
                return new { error = "Corrupted message data" };

            dynamic responseObj = new System.Dynamic.ExpandoObject();
            int conversationId = -1;
            var message = new Message(
                messageData.MessageText,
                userId.Value,
                conversationId
                );

            if (isGroupChat)
            {
                conversationId = messageData.ReceiverConversationId.Value;
                hubGroupName = GetChatGroupName(messageData.ReceiverConversationId.Value);
                bool isMember = chatDbService.IsUserConversationMember(userId.Value, messageData.ReceiverConversationId.Value);
                // caller is a member — no additional actions required

                // caller is not a member
                if (!isMember)
                {
                    chatDbService.AddConversationMember(messageData.ReceiverConversationId.Value, userId.Value);
                    responseObj.newConversationId = messageData.ReceiverConversationId.Value;
                    await Groups.AddToGroupAsync(Context.ConnectionId, hubGroupName);
                }

                var messageToClients = PrepareMessageToClients(message, conversationId);
                await Clients.Group(hubGroupName).SendAsync("ReceiveMessage", messageToClients);
            }
            else if (isPrivateChat)
            {
                int senderId = userId.Value;
                int receiverId = messageData.ReceiverUserId.Value;

                // conversation exists (and sender knows about it)
                if (messageData.ReceiverConversationId != null)
                {
                    conversationId = messageData.ReceiverConversationId.Value;
                    hubGroupName = GetChatGroupName(messageData.ReceiverConversationId.Value);
                    var messageToClients = PrepareMessageToClients(message, conversationId);
                    await Clients.Group(hubGroupName).SendAsync("ReceiveMessage", messageToClients);

                    // check if the other user is an active participant
                    bool isOtherUserMember = chatDbService.IsUserConversationMember(receiverId, messageData.ReceiverConversationId.Value);
                    if (!isOtherUserMember)
                    {
                        var conversation = chatDbService.AddConversationMember(messageData.ReceiverConversationId.Value, receiverId);
                        var receiver = conversation.Participants.Where(u => u.Id == receiverId).FirstOrDefault();
                        await Clients.User(receiver.Email).SendAsync("AddToGroup", messageToClients);
                    }
                }
                // conversation doesn't exist (or sender doesn't know about it)
                else
                {
                    var privateConversation = chatDbService.CreatePrivateChat(senderId, receiverId);
                    conversationId = privateConversation.Id;
                    responseObj.conversationId = privateConversation.Id;

                    hubGroupName = GetChatGroupName(privateConversation.Id);
                    await Groups.AddToGroupAsync(Context.ConnectionId, hubGroupName);

                    var receiver = privateConversation.Participants.First(u => u.Id == receiverId);
                    var messageToClients = PrepareMessageToClients(message, conversationId);
                    await Clients.Group(hubGroupName).SendAsync("ReceiveMessage", messageToClients);
                    await Clients.User(receiver.Email).SendAsync("AddToGroup", messageToClients);
                }
            }

            responseObj.messageId = message.Id;
            return responseObj;
        }
        public async Task JoinGroup(int conversationId)
        {
            var groupName = GetChatGroupName(conversationId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Remove the caller from the specified broadcast group
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        public async Task LeaveGroup(int conversationId)
        {
            var groupName = GetChatGroupName(conversationId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Helpers.GetUserIdFromClaims(Context.User);
            var groups = chatDbService.GetConversationEntriesForUser(userId.Value);
            var groupNames = new List<string>();
            var addGroupTasks = new List<Task>();
            foreach (var group in groups)
            {
                var groupName = GetChatGroupName(group.Id);
                groupNames.Add(groupName);
                var task = Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                addGroupTasks.Add(task);
            }

            Task.WaitAll(addGroupTasks.ToArray());
            chatDbService.RegisterUserActivity(userId.Value);
            await Clients.Groups(groupNames).SendAsync("UserActivityChanged", userId, true);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Helpers.GetUserIdFromClaims(Context.User);
            var groupNames = chatDbService
                .GetConversationEntriesForUser(userId.Value)
                .Select(x => GetChatGroupName(x.Id))
                .ToList();
            chatDbService.RegisterUserInactivity(userId.Value);
            await Clients.Groups(groupNames).SendAsync("UserActivityChanged", userId, false);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Get a string to use as a hub group name
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        private string GetChatGroupName(int conversationId)
        {
            return $"con.{conversationId}";
        }

        /// <summary>
        /// Set missing properties, save message to the DB and make a normalized version to send to clients.
        /// </summary>
        /// <param name="message">Message entity</param>
        /// <param name="conversationId">Conversation ID in the DB</param>
        private MessageDataToClient PrepareMessageToClients(Message message, int conversationId)
        {
            message.ConversationId = conversationId;
            chatDbService.SaveMessage(message);
            var messageToClients = new MessageDataToClient(message);
            return messageToClients;
        }
    }
}
