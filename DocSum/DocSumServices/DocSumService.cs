/*using DocSumRepository;
using common.model;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Reflection.PortableExecutable;

namespace DocSumServices
{
    public class DocSumService : IDocSumService
    {
        private readonly IDocSumRepo _docSumRepo;

        public DocSumService(IDocSumRepo docSumRepo)
        {
            _docSumRepo = docSumRepo;
        }

        public string ProcessDocument(byte[] documentData, string fileName)
        {
            // Here, you can implement logic to summarize the document
            // For simplicity, we'll just save the document as it is
            var filePath = _docSumRepo.SaveDocument(documentData, fileName);

            // Parse the document
            ParseDocument(filePath);

            return filePath;
        }

        public List<string> ParseDocument(string filePath)
        {
            // Initialize PDF document parser
            PdfReader pdfReader = new PdfReader(filePath);
            PdfDocument pdfDocument = new PdfDocument(pdfReader);
            List<string> pages = new List<string>();
            // Initialize text extraction strategy
            LocationTextExtractionStrategy extractionStrategy = new LocationTextExtractionStrategy();

            // Extract text from each page
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                // Get page content
                PdfPage page = pdfDocument.GetPage(i);
                PdfCanvasProcessor parser = new PdfCanvasProcessor(extractionStrategy);
                parser.ProcessPageContent(page);

                // Get extracted text
                string pageText = extractionStrategy.GetResultantText();
                pages.Add(pageText);
                // Process extracted text as needed
                // For example, you can save it to a database, perform further analysis, etc.
                
            }

            // Close PDF document
            pdfDocument.Close();
            return pages;
        }
    }
}
*/
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;

using DocSumRepository;
using common.model;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Collections.Generic;
using Azure;
using Azure.AI.TextAnalytics;


namespace DocSumServices
{
    public class DocSumService : IDocSumService
    {
        private readonly IDocSumRepo _docSumRepo;
        private readonly TextAnalyticsClient _textAnalyticsClient;

        public DocSumService(IDocSumRepo docSumRepo, string textAnalyticsEndpoint, string textAnalyticsKey)
        {
            _docSumRepo = docSumRepo;

        }

        public async Task<ConversationModel> ProcessDocument(byte[] documentData, string fileName)
        {
            // Here, you can implement logic to summarize the document
            // For simplicity, we'll just save the document as it is
            var filePath = _docSumRepo.SaveDocument(documentData, fileName);

            // Parse and summarize the document
            List<string> summary;
            var conv = await SummarizeDocument(filePath);

            return conv;
        }

        public async Task<ConversationModel> SummarizeDocument(string filePath)
        {
            var credential = new AzureKeyCredential("ef1d66e6592f4cac8079fbcef9c0bd4b");
            var endpoint = new Uri("https://textanalyticssum.cognitiveservices.azure.com/");
            TextAnalyticsClient _textAnalyticsClient = new(endpoint, credential);
            // Parse the document
            List<string> pages =await ParseDocument(filePath);

            // Concatenate text from all pages
            string documentText = string.Join(" ", pages);

            // Extract key phrases using Text Analytics
            // var response = _textAnalyticsClient.ExtractKeyPhrases(documentText);

            // Build summary from key phrases
            //  List<string> keyPhrases = new List<string>(response.Value);
            //  string summary = string.Join(", ", keyPhrases);
            List<string> batchedDocuments = new()
            {
                documentText
            };
            List<string> summaries = new List<string>();
            // Perform the text analysis operation.
            // View the operation results.

            // Perform the text analysis operation.
            AbstractiveSummarizeOperation operation = _textAnalyticsClient.AbstractiveSummarize(WaitUntil.Completed, batchedDocuments);
            await foreach (AbstractiveSummarizeResultCollection documentsInPage in operation.Value)
            {
                Console.WriteLine($"Abstractive Summarize, model version: \"{documentsInPage.ModelVersion}\"");
                Console.WriteLine();

                foreach (AbstractiveSummarizeResult documentResult in documentsInPage)
                {
                    if (documentResult.HasError)
                    {
                        Console.WriteLine($"  Error!");
                        Console.WriteLine($"  Document error code: {documentResult.Error.ErrorCode}");
                        Console.WriteLine($"  Message: {documentResult.Error.Message}");
                        continue;
                    }

                    Console.WriteLine($"  Produced the following abstractive summaries:");
                    Console.WriteLine();

                    foreach (AbstractiveSummary summary in documentResult.Summaries)
                    {
                        Console.WriteLine($"  Text: {summary.Text.Replace("\n", " ")}");
                        Console.WriteLine($"  Contexts:");
                        summaries.Add(summary.Text.Replace("\n", " "));

                        foreach (AbstractiveSummaryContext context in summary.Contexts)
                        {
                            Console.WriteLine($"    Offset: {context.Offset}");
                            Console.WriteLine($"    Length: {context.Length}");
                        }

                        Console.WriteLine();
                    }
                }
            }
            var conv = await _docSumRepo.StoreConversation(pages, summaries,filePath);

            return conv;
        }


        public async Task<List<string>> ParseDocument(string filePath)
        {
            // Initialize PDF document parser
            PdfReader pdfReader = new PdfReader(filePath);
            PdfDocument pdfDocument = new PdfDocument(pdfReader);
            List<string> pages = new List<string>();
            // Initialize text extraction strategy
            LocationTextExtractionStrategy extractionStrategy = new LocationTextExtractionStrategy();

            // Extract text from each page
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                // Get page content
                PdfPage page = pdfDocument.GetPage(i);
                PdfCanvasProcessor parser = new PdfCanvasProcessor(extractionStrategy);
                parser.ProcessPageContent(page);

                // Get extracted text
                string pageText = extractionStrategy.GetResultantText();
                pages.Add(pageText);
            }

            // Close PDF document
            pdfDocument.Close();
            return pages;
        }


        public async Task<ConversationModel> GetConversation(string id)
        {
            return await _docSumRepo.GetConversation(id);
        }
        public async Task<List<ConversationModel>> GetConversationAll()
        {
            return await _docSumRepo.GetConversationAll();
        }

        public async Task<ConversationModel> UpdateConversation(string id, string userprompt)
        {
            ConversationModel conversation = await _docSumRepo.GetConversation(id);
            var botreply = await GenerateBotReply(userprompt, conversation.Pages[0], conversation.Conv);
            // Append the new user prompt and bot reply to the conversation.conv string
            conversation.Conv += $"UserPrompt: {userprompt};BotReply: {botreply};";
            await _docSumRepo.UpdateConversation(id, conversation);
            return conversation;
        }
        private async Task<string> GenerateBotReply(string userPrompt, string content, string previousConversation)
        {
            // Concatenate user prompt, content, and previous conversation for context
            string context = $"Here is a document content : {content}\n\n Here is previous conversation in context of content : \"{previousConversation}\"\n\n  Now continue the conversation, by replying to User Prompt : \"{userPrompt}\" ";
            // Call Azure's generative AI (GPT) service to generate a reply


            // Retrieve generated reply from the analysis result
            // Assuming the generated reply is stored in a property called 'GeneratedReply'
            string generatedReply = "";
            return generatedReply;
        }
    }
}
