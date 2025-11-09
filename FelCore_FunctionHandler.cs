using Verse;
using System;
using System.Linq;

namespace FelCore
{
    public static class FelCore_FunctionHandler
    {
        public static void HandleFunctionCall(FunctionCall functionCall)
        {
            foreach (var comp in functionCall.pawn.AllComps)
            {
                if (comp is IFelCore_ToolExecutor executor)
                {
                    try
                    {
                        if (executor.TryExecuteTool(functionCall.name, functionCall.arguments, functionCall.pawn))
                        {
                            Log.Message($"[FelCore] Executed tool '{functionCall.name}' via {comp.GetType().Name}");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[FelCore] Error executing tool '{functionCall.name}' in {comp.GetType().Name}: {ex.Message}");
                    }
                }
            }

            Log.Warning($"[FelCore] No executor found for tool: {functionCall.name}");
        }
    }
}
