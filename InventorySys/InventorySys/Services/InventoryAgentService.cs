#pragma warning disable OPENAI001

using InventorySys.Services;
using OpenAI.Responses;
using System.Text.Json;

namespace InventorySys.Services
{
    public class InventoryAgentService
    {
        private readonly InventoryToolService _toolService;
        private readonly ResponsesClient _client;

        private const string Model = "gpt-5.2";

        public InventoryAgentService(
            InventoryToolService toolService,
            IConfiguration configuration)
        {
            _toolService = toolService;

            var apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException(
                    "OpenAI API key 未設定。"
                );
            }

            _client = new ResponsesClient(apiKey);
        }

        // Tool 1：低庫存
        private static readonly FunctionTool GetLowStockTool =
            ResponseTool.CreateFunctionTool(
                functionName: "get_low_stock_items",
                functionDescription:
                    "取得目前需要補貨的低庫存食材。當使用者詢問哪些食材需要補貨、哪些庫存不足時使用。",
                functionParameters: BinaryData.FromString(
                    """
                    {
                        "type": "object",
                        "properties": {},
                        "additionalProperties": false
                    }
                    """
                ),
                strictModeEnabled: true
            );

        // Tool 2：庫存預測
        private static readonly FunctionTool GetInventoryForecastTool =
            ResponseTool.CreateFunctionTool(
                functionName: "get_inventory_forecast",
                functionDescription:
                     "取得食材庫存預測資料，包含目前庫存、最低庫存、平均每日使用量、預測需求量、建議補貨量與預估補貨成本。當使用者詢問未來庫存需求、庫存預測、建議補貨量或預測期間的補貨成本時使用。usageDays 是計算歷史平均每日使用量的天數，forecastDays 是預測未來需求的天數。當使用者未明確指定時，使用 14 天的歷史使用量與 7 天的預測期間。",
                functionParameters: BinaryData.FromString(
                    """
                    {
                        "type": "object",
                        "properties": {
                            "usageDays": {
                                "type": "integer",
                                "description": "計算歷史平均每日使用量的天數，預設為 14 天。"
                            },
                            "forecastDays": {
                                "type": "integer",
                                "description": "預測未來需求的天數，預設為 7 天。"
                            }
                        },
                        "required": [
                            "usageDays",
                            "forecastDays"
                        ],
                        "additionalProperties": false
                    }
                    """
                ),
                strictModeEnabled: true
            );

        // Tool 3：庫存摘要
        private static readonly FunctionTool GetInventorySummaryTool =
            ResponseTool.CreateFunctionTool(
                functionName: "get_inventory_summary",
                functionDescription:
                    "取得目前庫存摘要，包含食材總數、需要補貨的食材數量，以及預估補貨總成本。當使用者詢問目前庫存狀況、需要補貨幾項食材或預計補貨成本時使用。",
                functionParameters: BinaryData.FromString(
                    """
                    {
                        "type": "object",
                        "properties": {},
                        "additionalProperties": false
                    }
                    """
                ),
                strictModeEnabled: true
            );

        public async Task<string> Chat(string message)
        {
            try
            {
                List<ResponseItem> inputItems =
                [
                    ResponseItem.CreateUserMessageItem(message)
                ];

                bool requiresAction;

                do
                {
                    requiresAction = false;

                    CreateResponseOptions options =
                        new(Model, inputItems)
                        {
                            Instructions = """
                            你是一個餐廳庫存管理助理。

                            請根據使用者的問題選擇適合的庫存工具：

                            1. get_low_stock_items
                            當使用者詢問哪些食材需要補貨、哪些食材庫存不足時使用。

                            2. get_inventory_summary
                            當使用者詢問整體庫存狀況、食材總數、
                            需要補貨的食材數量或預估補貨總成本時使用。

                            3. get_inventory_forecast
                            當使用者詢問未來庫存需求、庫存預測、
                            預測需求量或建議補貨量時使用。

                            使用 get_inventory_forecast 時：
                            - usageDays 代表計算歷史平均每日使用量的天數。
                            - forecastDays 代表預測未來需求的天數。
                            - 如果使用者沒有指定天數，請使用 usageDays = 14、forecastDays = 7。

                            所有庫存數字都必須透過庫存工具取得，
                            不可以自行猜測或編造。

                            請根據工具回傳的資料回答使用者。

                            回答格式規則：

                            - 使用 Markdown 格式回答。
                            - 清單項目請使用「-」開頭，項目之間不要插入空白行。
                            - 重要的數字或結論可以使用 **粗體**。
                            - 回答保持簡潔，不需要重複說明使用了哪個 Tool。
                            - 如果資料為 0 或沒有需要補貨的項目，直接清楚說明即可。
                            """,
                            Tools =
                            {
                            GetLowStockTool,
                            GetInventoryForecastTool,
                            GetInventorySummaryTool
                            }
                        };

                    ResponseResult response =
                        await _client.CreateResponseAsync(options);

                    inputItems.AddRange(response.OutputItems);

                    foreach (ResponseItem outputItem in response.OutputItems)
                    {
                        if (outputItem is FunctionCallResponseItem functionCall)
                        {
                            switch (functionCall.FunctionName)
                            {
                                // Tool 1
                                case "get_low_stock_items":
                                    {
                                        var lowStockItems =
                                            await _toolService
                                                .GetLowStockItems();

                                        string lowStockJson =
                                            JsonSerializer.Serialize(
                                                lowStockItems
                                            );

                                        inputItems.Add(
                                            new FunctionCallOutputResponseItem(
                                                functionCall.CallId,
                                                lowStockJson
                                            )
                                        );

                                        requiresAction = true;

                                        break;
                                    }

                                // Tool 2
                                case "get_inventory_forecast":
                                    {
                                        using JsonDocument argumentsJson =
                                            JsonDocument.Parse(
                                                functionCall.FunctionArguments
                                            );

                                        int usageDays =
                                            argumentsJson.RootElement
                                                .GetProperty("usageDays")
                                                .GetInt32();

                                        int forecastDays =
                                            argumentsJson.RootElement
                                                .GetProperty("forecastDays")
                                                .GetInt32();

                                        if (usageDays <= 0 || usageDays > 365)
                                        {
                                            throw new ArgumentException("usageDays 必須介於 1 到 365 天。");
                                        }

                                        if (forecastDays <= 0 || forecastDays > 365)
                                        {
                                            throw new ArgumentException("forecastDays 必須介於 1 到 365 天。");
                                        }
                                        var forecast =
                                            await _toolService
                                                .GetInventoryForecast(
                                                    usageDays,
                                                    forecastDays
                                                );

                                        string forecastJson =
                                            JsonSerializer.Serialize(
                                                forecast
                                            );

                                        inputItems.Add(
                                            new FunctionCallOutputResponseItem(
                                                functionCall.CallId,
                                                forecastJson
                                            )
                                        );

                                        requiresAction = true;

                                        break;
                                    }

                                // Tool 3
                                case "get_inventory_summary":
                                    {
                                        var summary =
                                            await _toolService
                                                .GetInventorySummary();

                                        string summaryJson =
                                            JsonSerializer.Serialize(
                                                summary
                                            );

                                        inputItems.Add(
                                            new FunctionCallOutputResponseItem(
                                                functionCall.CallId,
                                                summaryJson
                                            )
                                        );

                                        requiresAction = true;

                                        break;
                                    }

                                default:
                                    {
                                        throw new NotImplementedException(
                                            $"未知的 Tool: {functionCall.FunctionName}"
                                        );
                                    }
                            }
                        }
                    }

                } while (requiresAction);

                var finalResponse =
                    inputItems
                        .OfType<MessageResponseItem>()
                        .LastOrDefault();

                if (finalResponse == null)
                {
                    return "抱歉，目前沒有取得 AI 回覆，請稍後再試。";
                }

                return finalResponse.Content[0].Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Agent Error: {ex}");

                return "抱歉，目前無法取得庫存資訊，請稍後再試。";
            }
        }
    }
}

#pragma warning restore OPENAI001