using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace ProjectStudyTool;

public class OpenAiService
{
  private readonly string endpoint;
  private readonly string apiKey;
  private readonly string model;

  public OpenAiService()
  {
    endpoint = Environment.GetEnvironmentVariable("endpoint");
    apiKey = Environment.GetEnvironmentVariable("apiKey");
    model = Environment.GetEnvironmentVariable("model");

    if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(model))
    {
        throw new ApplicationException("OpenAI configuration is incomplete.");
    }
  }

  public async Task<IReadOnlyList<ChatMessageContent>> UseOpenAiService(string userContent)
  {
    // Create a new OpenAI chat completion service
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion(model, apiKey);
    var kernel = builder.Build();

    // Set any prompt execution settings
    // There are more than this, but only selected the relevant ones
    // 1000 characters is roughly 750 tokens according to OpenAI
    var executionSettings = new OpenAIPromptExecutionSettings
    {
      MaxTokens = 1500,
      Temperature = 1
    };

    /*
        Make the AI generate flashcard questions and answers from user input.
        Can add more messages to make the responses more specific:
            - AddSystemMessage = instructions or other guidance about how the AI assistant should behave
            - AddUserMessage = current or historical user input
            - AddAssistantMessage = the AI assistant's response to the user input
        Reference: https://techcommunity.microsoft.com/t5/educator-developer-blog/how-to-use-semantickernel-with-openai-and-azure-openai-in-c/ba-p/4081648
    */
    var chat = kernel.GetRequiredService<IChatCompletionService>();
    var chatHistory = new ChatHistory();
    var instructions = "Generate flashcards content from the provided text. "
        + " Generate at least 20 to 100 questions and answers. "
        + " Questions should be numbered QUESTION #. Answers should start with ANSWER #: "
        + " The flashcards are in a question-and-answer or keyword-and-explanation format. "
        + " The questions should be based on the text provided. The answers should be concise and based on the text.";
    chatHistory.AddSystemMessage(instructions);
    chatHistory.AddUserMessage(userContent);
    var response = await chat.GetChatMessageContentsAsync(chatHistory, executionSettings);
    var responseContent = response[^1].Content;
    // Console.WriteLine(responseContent);

    return response;
  }

}
