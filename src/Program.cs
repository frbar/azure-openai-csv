using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using System.Text;

// https://devblogs.microsoft.com/dotnet/demystifying-retrieval-augmented-generation-with-dotnet/

string aoaiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
string aoaiApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!;
string aoaiModel = "gpt-4-32k";

// Initialize the kernel
IKernel kernel = Kernel.Builder
    .WithLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
    .WithAzureChatCompletionService(aoaiModel, aoaiEndpoint, aoaiApiKey)
    .Build();

// Create a new chat
IChatCompletion ai = kernel.GetService<IChatCompletion>();
ChatHistory chat = ai.CreateNewChat("You are an AI assistant that helps people find information.");
StringBuilder builder = new();

string content = File.ReadAllText("resources/tips-and-hours-oct-2023.csv");
string jsonExample = File.ReadAllText("resources/example.json");
chat.AddUserMessage("Le contexte est : saisie des éléments collaborateurs");
chat.AddUserMessage("Le nom du fichier est : tips-and-hours-oct-2023.csv");
chat.AddUserMessage("Pour générer du JSON, utilise du lower case, avec _ entre les mots");
chat.AddUserMessage("Pour générer du JSON, utilise le format DateTime (yyyy-MM-dd) pour les jours ou mois ou années");
chat.AddUserMessage("Pour générer du JSON sur des éléments collaborateurs, prend exemple sur ce format : " + Environment.NewLine + jsonExample);
chat.AddUserMessage("Voici le contenu du fichier à analyser, pour mes collaborateurs : " + Environment.NewLine + content);

// Q&A loop
while (true)
{
    Console.Write("Question: ");
    chat.AddUserMessage(Console.ReadLine()!);

    builder.Clear();
    await foreach (string message in ai.GenerateMessageStreamAsync(chat))
    {
        Console.Write(message);
        builder.Append(message);
    }
    Console.WriteLine();
    chat.AddAssistantMessage(builder.ToString());
  
    Console.WriteLine();
}