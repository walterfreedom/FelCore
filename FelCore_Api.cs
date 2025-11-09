using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Verse;

namespace FelCore
{
    // This file contains the data structures for communicating with the LLM API.
    // It is a direct port of the classes from LLMApiStructures.cs,
    // with Unity-specific types like GameObject and attributes removed.
    
    #region API Data Structures

    public class ChatCompletionRequest
    {
        public ChatMessage[] messages;
        public float temperature = 0.7f;
        public int max_tokens = 1000;
        public bool stream = false;
        public ToolDefinition[] tools;
    }

    public class ChatMessage
    {
        public string role; // "system", "user", "assistant"
        public string content;
        public ToolCall[] tool_calls;
    }

    public class ChatCompletionResponse
    {
        public Choice[] choices;
        public Usage usage;
    }

    public class Choice
    {
        public ChatMessage message;
        public string finish_reason;
    }

    public class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    public class ToolCall
    {
        public string id;
        public string type; // "function"
        public FunctionCallData function;
    }

    public class FunctionCallData
    {
        public string name;
        public string arguments; // JSON string
    }

    public class ToolDefinition
    {
        public string type; // "function"
        public FunctionDefinition function;
    }

    public class FunctionDefinition
    {
        public string name;
        public string description;
        public object parameters; // JSON schema for function parameters
    }

    // This class is used internally by the mod to pass tool call data
    public class FunctionCall
    {
        public string name;
        public JObject arguments;
        public Pawn pawn;
    }

    #endregion

    #region Tool Definition Structures

    // Inspector-friendly tool definition system
    public class AgentToolDefinition
    {
        public string name;
        public string description;
        public List<ToolParameter> parameters = new List<ToolParameter>();
        
        // In the RimWorld version, the ToolAgent concept will be handled differently.
        // For now, we are commenting this out.
        // public ToolAgent ToolAgent = null;

        public object GetParametersObject()
        {
            if (parameters == null || parameters.Count == 0)
            {
                var emptySchema = new
                {
                    type = "object",
                    properties = new { },
                    required = new string[0]
                };
                return JObject.FromObject(emptySchema);
            }

            var properties = new Dictionary<string, object>();
            var required = new List<string>();

            foreach (var param in parameters)
            {
                var paramDef = new
                {
                    type = param.type,
                    description = param.description
                };

                if (param.enumValues?.Length > 0)
                {
                    properties[param.name] = new
                    {
                        type = param.type,
                        description = param.description,
                        @enum = param.enumValues
                    };
                }
                else
                {
                    properties[param.name] = paramDef;
                }

                if (param.required)
                    required.Add(param.name);
            }

            var schema = new
            {
                type = "object",
                properties = properties,
                required = required.ToArray()
            };
            
            return JObject.FromObject(schema);
        }
    }

    public class ToolParameter
    {
        public string name;
        public string type = "string";
        public string description;
        public bool required = false;
        public string[] enumValues;
    }

    #endregion
}
