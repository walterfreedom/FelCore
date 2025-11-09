using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using FelPawns;

namespace FelCore
{
    [StaticConstructorOnStartup]
    public class FelCore_MapComponent : MapComponent
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly List<IFelAgent> activePawns = new List<IFelAgent>();
        private Pawn mainCharacter;

        public Pawn MainCharacter => mainCharacter;
        
        public static string BaseUrl = "http://127.0.0.1:4315/v1";
        public static float Temperature = 0.7f;
        public static int MaxTokens = 1000;

        public FelCore_MapComponent(Map map) : base(map) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref mainCharacter, "mainCharacter");
        }
        
        public void SetMainCharacter(Pawn pawn)
        {
            if (mainCharacter != null)
            {
                var oldComp = mainCharacter.GetComp<FelPawn_Component>();
                if (oldComp != null) oldComp.IsPlayerControlled = false;
            }

            mainCharacter = pawn;
            if (mainCharacter != null)
            {
                var newComp = mainCharacter.GetComp<FelPawn_Component>();
                if (newComp != null) newComp.IsPlayerControlled = true;
                Log.Message($"[FelPawns] {pawn.Name.ToStringShort} has been selected as the main character.");
            }
        }
        
        public void RegisterPawn(IFelAgent pawnComp)
        {
            if (!activePawns.Contains(pawnComp))
            {
                activePawns.Add(pawnComp);
            }
        }

        public void DeregisterPawn(IFelAgent pawnComp)
        {
            activePawns.Remove(pawnComp);
        }

        public void ProcessAgentImmediately(IFelAgent agent)
        {
            if (agent != null && !agent.Pawn.Dead && agent.IsActive)
            {
                _ = SendAgentMessageAsync(agent);
            }
        }
        
        public static async Task SendAgentMessageAsync(IFelAgent agent)
        {
            string context = agent.BuildContext();
            if (string.IsNullOrEmpty(context))
            {
                context = "<system>You are currently idle. Interact with nearby colonists or continue with your routine.</system>";
            }

            string gameState = agent.BuildGameState();
            
            var request = new ChatCompletionRequest
            {
                messages = new ChatMessage[]
                {
                    new ChatMessage { role = "system", content = $"{agent.systemPrompt}\n\nCurrent Game State:\n{gameState}" },
                    new ChatMessage { role = "user", content = context }
                },
                temperature = Temperature,
                max_tokens = MaxTokens,
                // tools = agent.GetToolDefinitions() // Add once tools are implemented
            };

            var response = await SendRequestAsync(request);

            if (response?.choices?.Length > 0)
            {
                var choice = response.choices[0];
                string aiResponse = choice.message.content;

                if (!string.IsNullOrEmpty(aiResponse) && !aiResponse.Trim().Equals("[SILENT]", System.StringComparison.OrdinalIgnoreCase))
                {
                    agent.Speak(aiResponse);
                }

                if (choice.message.tool_calls != null)
                {
                    foreach (var toolCall in choice.message.tool_calls)
                    {
                        var functionCall = new FunctionCall
                        {
                            name = toolCall.function.name,
                            arguments = Newtonsoft.Json.Linq.JObject.Parse(toolCall.function.arguments),
                            pawn = agent.Pawn
                        };
                        FelCore_FunctionHandler.HandleFunctionCall(functionCall);
                    }
                }
            }
        }

        private static async Task<ChatCompletionResponse> SendRequestAsync(ChatCompletionRequest request)
        {
            try
            {
                string url = $"{BaseUrl}/chat/completions";
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<ChatCompletionResponse>(responseBody);
                }
                else
                {
                    Log.Error($"[FelCore] API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[FelCore] HTTP Request Failed: {ex.Message}");
            }
            return null;
        }
    }
}
