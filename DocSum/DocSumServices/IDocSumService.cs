using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using common.model;

namespace DocSumServices
{
    public interface IDocSumService
    {
        /*public Task<List<conversations>> GetConversations();*/

        public Task<ConversationModel> ProcessDocument(byte[] documentData, string fileName) ;
        public Task<List<string>> ParseDocument(string filePath);
        public Task<ConversationModel> SummarizeDocument(string filePath);
        public Task<ConversationModel> GetConversation(string id);
        public Task<List<ConversationModel>> GetConversationAll();
        public Task<ConversationModel> UpdateConversation(string id, string userprompt);

    }
}
