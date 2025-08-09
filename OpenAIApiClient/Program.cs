using OpenAIApiClient;
using OpenAIApiClient.Models;

class Program
{
    private static OpenAIApiService? _apiService;
    private static string _selectedModel = "google/gemma-3n-e4b";
    private static List<string> _availableModels = new();

    static async Task Main(string[] args)
    {
        // APIサービスのインスタンスを作成
        using var apiService = new OpenAIApiService("http://172.19.96.1:1234");
        _apiService = apiService;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== OpenAI API クライアント ===");
            Console.WriteLine($"現在のモデル: {_selectedModel}");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("1. 利用可能なモデル一覧を取得");
            Console.WriteLine("2. モデルを選択/変更");
            Console.WriteLine("3. シンプルなチャット（簡単な使い方）");
            Console.WriteLine("4. 詳細なチャットリクエスト（カスタマイズ可能）");
            Console.WriteLine("5. 対話形式のチャット");
            Console.WriteLine("6. 終了");
            Console.WriteLine("================================");
            Console.Write("選択してください (1-6): ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ShowAvailableModelsAsync();
                        break;
                    case "2":
                        await SelectModelAsync();
                        break;
                    case "3":
                        await SimpleChatAsync();
                        break;
                    case "4":
                        await DetailedChatRequestAsync();
                        break;
                    case "5":
                        await InteractiveChatAsync();
                        break;
                    case "6":
                        Console.WriteLine("終了します。");
                        return;
                    default:
                        Console.WriteLine("無効な選択です。もう一度お試しください。");
                        break;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\nAPIリクエストエラー: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nエラーが発生しました: {ex.Message}");
            }

            if (choice != "6")
            {
                Console.WriteLine("\nEnterキーを押してメニューに戻ります...");
                Console.ReadLine();
            }
        }
    }

    private static async Task ShowAvailableModelsAsync()
    {
        Console.Clear();
        Console.WriteLine("=== 利用可能なモデル ===");
        
        var models = await _apiService!.GetModelsAsync();
        
        if (models.Data.Any())
        {
            _availableModels.Clear();
            foreach (var model in models.Data)
            {
                Console.WriteLine($"- {model.Id} (owned by: {model.OwnedBy})");
                _availableModels.Add(model.Id);
            }
        }
        else
        {
            Console.WriteLine("利用可能なモデルが見つかりませんでした。");
        }
    }

    private static async Task SelectModelAsync()
    {
        Console.Clear();
        Console.WriteLine("=== モデル選択 ===");
        
        // 利用可能なモデルを取得（まだ取得していない場合）
        if (!_availableModels.Any())
        {
            Console.WriteLine("モデル一覧を取得中...");
            var models = await _apiService!.GetModelsAsync();
            if (models.Data.Any())
            {
                _availableModels.Clear();
                foreach (var model in models.Data)
                {
                    _availableModels.Add(model.Id);
                }
            }
        }

        if (_availableModels.Any())
        {
            Console.WriteLine("\n利用可能なモデル:");
            for (int i = 0; i < _availableModels.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_availableModels[i]}");
            }
            
            Console.WriteLine("\n0. 手動でモデル名を入力");
            Console.Write("\n選択してください: ");
            
            var selection = Console.ReadLine();
            
            if (selection == "0")
            {
                Console.Write("モデル名を入力してください: ");
                var customModel = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(customModel))
                {
                    _selectedModel = customModel;
                    Console.WriteLine($"\nモデルを '{_selectedModel}' に設定しました。");
                }
            }
            else if (int.TryParse(selection, out int index) && index > 0 && index <= _availableModels.Count)
            {
                _selectedModel = _availableModels[index - 1];
                Console.WriteLine($"\nモデルを '{_selectedModel}' に設定しました。");
            }
            else
            {
                Console.WriteLine("無効な選択です。");
            }
        }
        else
        {
            Console.Write("モデル一覧を取得できませんでした。手動でモデル名を入力してください: ");
            var customModel = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(customModel))
            {
                _selectedModel = customModel;
                Console.WriteLine($"\nモデルを '{_selectedModel}' に設定しました。");
            }
        }
    }

    private static async Task SimpleChatAsync()
    {
        Console.Clear();
        Console.WriteLine("=== シンプルなチャット ===");
        Console.Write("メッセージを入力してください: ");
        
        var message = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine("メッセージが入力されませんでした。");
            return;
        }

        Console.WriteLine("\n応答を待っています...");
        var response = await _apiService!.SimpleChatAsync(message, _selectedModel);
        
        Console.WriteLine("\n--- 応答 ---");
        Console.WriteLine(response);
    }

    private static async Task DetailedChatRequestAsync()
    {
        Console.Clear();
        Console.WriteLine("=== 詳細なチャットリクエスト ===");
        
        Console.Write("システムプロンプト (省略可能): ");
        var systemPrompt = Console.ReadLine();
        
        Console.Write("ユーザーメッセージ: ");
        var userMessage = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            Console.WriteLine("ユーザーメッセージが必要です。");
            return;
        }

        Console.Write("最大トークン数 (デフォルト: 300): ");
        var maxTokensInput = Console.ReadLine();
        int maxTokens = string.IsNullOrWhiteSpace(maxTokensInput) ? 300 : int.Parse(maxTokensInput);

        Console.Write("Temperature (0.0-2.0, デフォルト: 0.7): ");
        var tempInput = Console.ReadLine();
        double temperature = string.IsNullOrWhiteSpace(tempInput) ? 0.7 : double.Parse(tempInput);

        var messages = new List<ChatMessage>();
        
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            messages.Add(new() { Role = "system", Content = systemPrompt });
        }
        
        messages.Add(new() { Role = "user", Content = userMessage });

        var chatRequest = new ChatCompletionRequest
        {
            Model = _selectedModel,
            Messages = messages,
            MaxTokens = maxTokens,
            Temperature = temperature
        };

        Console.WriteLine("\n応答を待っています...");
        var chatResponse = await _apiService!.CreateChatCompletionAsync(chatRequest);
        
        Console.WriteLine("\n--- 詳細情報 ---");
        Console.WriteLine($"Model: {chatResponse.Model}");
        Console.WriteLine($"Response ID: {chatResponse.Id}");
        
        if (chatResponse.Choices.Any())
        {
            var choice = chatResponse.Choices.First();
            Console.WriteLine($"\n--- 応答 ---");
            Console.WriteLine(choice.Message.Content);
            Console.WriteLine($"\nFinish Reason: {choice.FinishReason}");
        }

        if (chatResponse.Usage != null)
        {
            Console.WriteLine($"\n--- トークン使用量 ---");
            Console.WriteLine($"Prompt: {chatResponse.Usage.PromptTokens}");
            Console.WriteLine($"Completion: {chatResponse.Usage.CompletionTokens}");
            Console.WriteLine($"Total: {chatResponse.Usage.TotalTokens}");
        }
    }

    private static async Task InteractiveChatAsync()
    {
        Console.Clear();
        Console.WriteLine("=== 対話形式のチャット ===");
        Console.WriteLine("チャットを開始します。'exit'と入力すると終了します。");
        Console.WriteLine("'clear'と入力すると会話履歴をクリアします。");
        
        var conversationMessages = new List<ChatMessage>();
        
        while (true)
        {
            Console.Write("\nYou: ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrEmpty(input) || input.ToLower() == "exit")
            {
                Console.WriteLine("チャットを終了します。");
                break;
            }
            
            if (input.ToLower() == "clear")
            {
                conversationMessages.Clear();
                Console.WriteLine("会話履歴をクリアしました。");
                continue;
            }
            
            conversationMessages.Add(new() { Role = "user", Content = input });
            
            var conversationRequest = new ChatCompletionRequest
            {
                Model = _selectedModel,
                Messages = conversationMessages,
                MaxTokens = 500
            };
            
            Console.WriteLine("応答を待っています...");
            
            try
            {
                var response = await _apiService!.CreateChatCompletionAsync(conversationRequest);
                var assistantMessage = response.Choices.FirstOrDefault()?.Message;
                
                if (assistantMessage != null)
                {
                    Console.WriteLine($"\nAssistant: {assistantMessage.Content}");
                    conversationMessages.Add(assistantMessage);
                }
                else
                {
                    Console.WriteLine("応答を取得できませんでした。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"エラー: {ex.Message}");
                Console.WriteLine("会話を続けますか？ (y/n)");
                if (Console.ReadLine()?.ToLower() != "y")
                    break;
            }
        }
    }
}