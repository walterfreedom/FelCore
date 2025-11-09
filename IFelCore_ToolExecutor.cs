using Newtonsoft.Json.Linq;
using Verse;

namespace FelCore
{
    public interface IFelCore_ToolExecutor
    {
        /// <summary>
        /// Attempts to execute a tool call.
        /// </summary>
        /// <param name="toolName">The name of the function to execute.</param>
        /// <param name="args">The JSON arguments for the function.</param>
        /// <param name="pawn">The pawn that is executing the tool.</param>
        /// <returns>True if the tool was handled, false otherwise.</returns>
        bool TryExecuteTool(string toolName, JObject args, Pawn pawn);
    }
}
