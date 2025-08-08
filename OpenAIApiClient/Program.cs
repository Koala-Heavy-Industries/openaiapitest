using OpenAIApiClient;
using OpenAIApiClient.Models;

class Program
{
    static async Task Main(string[] args)
    {
        // APIサービスのインスタンスを作成
        using var apiService = new OpenAIApiService("http://172.19.96.1:1234");

        try
        {
            // 1. 利用可能なモデル一覧を取得
            Console.WriteLine("=== 利用可能なモデル ===");
            var models = await apiService.GetModelsAsync();
            foreach (var model in models.Data)
            {
                Console.WriteLine($"- {model.Id} (owned by: {model.OwnedBy})");
            }
            Console.WriteLine();

            // 2. シンプルなチャット（簡単な使い方）
            Console.WriteLine("=== シンプルなチャット例 ===");
            var simpleResponse = await apiService.SimpleChatAsync("こんにちは！今日はいい天気ですね。");
            Console.WriteLine($"Response: {simpleResponse}");
            Console.WriteLine();

            // 3. 詳細なチャットリクエスト（カスタマイズ可能）
            Console.WriteLine("=== 詳細なチャットリクエスト ===");
            var chatRequest = new ChatCompletionRequest
            {
                Model = "google/gemma-3n-e4b",
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = "あなたは親切なアシスタントです。" },
                    new() { Role = "user", Content = "C#の非同期プログラミングについて簡単に説明してください。" }
                },
                MaxTokens = 300,
                Temperature = 0.5
            };

            var chatResponse = await apiService.CreateChatCompletionAsync(chatRequest);
            
            Console.WriteLine($"Model: {chatResponse.Model}");
            Console.WriteLine($"Response ID: {chatResponse.Id}");
            
            if (chatResponse.Choices.Any())
            {
                var choice = chatResponse.Choices.First();
                Console.WriteLine($"Assistant: {choice.Message.Content}");
                Console.WriteLine($"Finish Reason: {choice.FinishReason}");
            }

            if (chatResponse.Usage != null)
            {
                Console.WriteLine($"\nToken Usage:");
                Console.WriteLine($"  Prompt: {chatResponse.Usage.PromptTokens}");
                Console.WriteLine($"  Completion: {chatResponse.Usage.CompletionTokens}");
                Console.WriteLine($"  Total: {chatResponse.Usage.TotalTokens}");
            }

            // 4. 対話形式のチャット例
            Console.WriteLine("\n=== 対話形式のチャット ===");
            Console.WriteLine("チャットを開始します。'exit'と入力すると終了します。");
            
            var conversationMessages = new List<ChatMessage>();
            
            while (true)
            {
                Console.Write("\nYou: ");
                var input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input) || input.ToLower() == "exit")
                    break;
                
                conversationMessages.Add(new() { Role = "user", Content = input });
                
                var conversationRequest = new ChatCompletionRequest
                {
                    Model = "google/gemma-3n-e4b",
                    Messages = conversationMessages,
                    MaxTokens = 500
                };
                
                var response = await apiService.CreateChatCompletionAsync(conversationRequest);
                var assistantMessage = response.Choices.FirstOrDefault()?.Message;
                
                if (assistantMessage != null)
                {
                    Console.WriteLine($"Assistant: {assistantMessage.Content}");
                    conversationMessages.Add(assistantMessage);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"APIリクエストエラー: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"エラーが発生しました: {ex.Message}");
        }
    }
}
