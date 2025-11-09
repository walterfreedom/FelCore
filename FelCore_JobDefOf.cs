using RimWorld;
using Verse;

namespace FelCore
{
    [DefOf]
    public static class FelCore_JobDefOf
    {
        // This allows us to define custom jobs later if needed
        static FelCore_JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FelCore_JobDefOf));
        }
    }
}
