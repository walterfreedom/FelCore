using Verse;

namespace FelCore
{
    public interface IFelAgent
    {
        Pawn Pawn { get; }
        bool IsActive { get; }
        bool IsPlayerControlled { get; }
        string BuildContext();
        string BuildGameState();
        string systemPrompt { get; }
        void Speak(string message);
        void ProcessAgent();
    }
}
