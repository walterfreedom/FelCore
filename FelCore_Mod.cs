using System.Linq;
using Verse;
using FelPawns;
using UnityEngine;

namespace FelCore
{
    public class FelCore_Mod : Mod
    {
        public FelCore_Mod(ModContentPack content) : base(content)
        {
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            var listing = new Listing_Standard();
            listing.Begin(inRect);

            if (Widgets.ButtonText(listing.GetRect(30f), "Choose Main Character"))
            {
                var pawns = Find.CurrentMap.mapPawns.FreeColonists.Where(p => p.GetComp<FelPawn_Component>() != null).ToList();
                if (pawns.Any())
                {
                    Find.WindowStack.Add(new FelPawn_CharacterSelectorWindow(pawns));
                }
                else
                {
                    Log.Message("No pawns with the FelPawn component found on the current map.");
                }
            }
            
            listing.End();
        }

        public override string SettingsCategory()
        {
            return "FelCore";
        }
    }
}
