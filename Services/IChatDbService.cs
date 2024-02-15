using LAST.Models.Api;

namespace LAST.Services
{
    public interface IChatDbService
    {
        /// <summary>
        /// Makes the user a member of the conversation
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Conversation AddConversationMember(int conversationId, int userId);

        /// <summary>
        /// Creates a new public conversation with a given name
        /// </summary>
        /// <param name="creatorUserId"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        Conversation CreateGroupChat(int creatorUserId, string groupName);

        /// <summary>
        /// Creates a new private conversation for given users.
        /// </summary>
        /// <param name="user1Id"></param>
        /// <param name="user2Id"></param>
        /// <returns></returns>
        Conversation CreatePrivateChat(int user1Id, int user2Id);

        /// <summary>
        /// Find users and conversations with names matching the search query.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        List<ConversationSearchItem> FindUsersAndConversations(string searchQuery);

        /// <summary>
        /// Find users and conversations with names matching the search query, exluding the current user
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        List<ConversationSearchItem> FindUsersAndConversations(string searchQuery, int currentUserId);

        /// <summary>
        /// Query conversations for the specified user and return a list of db entries without any processing. 
        /// </summary>
        /// <remarks>
        /// Used to broadcast messages in ChatHub.
        /// </remarks>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<Conversation> GetConversationEntriesForUser(int userId);

        /// <summary>
        /// Query conversations for the specified user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<ConversationListItem> GetConversationsForUser(int userId);

        /// <summary>
        /// Retrieves the latest N messages in a given conversation
        /// </summary>
        /// <param name="conversationId">Id of the conversation to get messages from</param>
        /// <param name="n">A number of messages to retrieve</param>
        /// <returns></returns>
        List<Message> GetLastNMessages(int conversationId, int n);

        /// <summary>
        /// Query a number of new messages in DB for specific user
        /// </summary>
        /// <param name="userId">User ID in database</param>
        /// <returns>A dictionary with pairs conversation ID —> number of new messages</returns>
        Dictionary<int, int> GetNewMessageCountForUser(int userId);

        /// <summary>
        /// Retrieves the messages sent before a certain time for a given conversation.
        /// </summary>
        /// <param name="conversationId">Id of the conversation to get messages from</param>
        /// <param name="n">A number of messages to retrieve</param>
        /// <param name="dateTimeOfLastMessage">Search messages before this time</param>
        /// <returns></returns>
        List<Message> GetPrevNMessages(int conversationId, int n, DateTime dateTimeOfLastMessage);

        /// <summary>
        /// Query members for the specified conversation
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        List<ConversationMember> GetUsersInConversation(int conversationId);

        /// <summary>
        /// Check if the user is a member of the conversation
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        bool IsUserConversationMember(int userId, int conversationId);

        /// <summary>
        /// Remove a user from the collection of members in a given conversation.
        /// </summary>
        /// <remarks>
        /// On the database side it deletes a corresponding many-to-many connection entry.
        /// </remarks>
        /// <param name="conversationId"></param>
        /// <param name="userId"></param>
        void LeaveConversation(int conversationId, int userId);

        /// <summary>
        /// Write information about a user being active into the database.
        /// </summary>
        /// <param name="userId"></param>
        void RegisterUserActivity(int userId);

        /// <summary>
        /// Write information about a user connecting to conversation into the database
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        void RegisterUserConnectingToConversation(int userId, int conversationId);

        /// <summary>
        /// Write information about a user disconnecting from conversation into the database
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        void RegisterUserDisconnectingFromConversation(int userId, int conversationId);

        /// <summary>
        /// Write information about a user being inactive into the database
        /// </summary>
        /// <param name="userId"></param>
        void RegisterUserInactivity(int userId);

        /// <summary>
        /// Save message in the DB
        /// </summary>
        /// <param name="message"></param>
        void SaveMessage(Message message);
    }
}
