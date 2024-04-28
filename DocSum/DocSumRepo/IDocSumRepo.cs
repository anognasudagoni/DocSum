using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using common.model;

namespace DocSumRepository
{
    public interface IDocSumRepo
    {
        public string SaveDocument(byte[] documentData, string fileName);
        public void ParseDocument(string filePath);
        public Task<ConversationModel> StoreConversation(List<string> pages, List<string> summaries,string filePath);
        public Task<ConversationModel> GetConversation(string id);
        public Task<List<ConversationModel>> GetConversationAll();
        public Task<ConversationModel> UpdateConversation(string id, ConversationModel conversation);
    }

}
