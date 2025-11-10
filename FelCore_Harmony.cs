using HarmonyLib;
using Verse;
using System.Reflection;
using FelPawns;
using RimWorld;

namespace FelCore
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Log.Message("[FelCore] Initializing FelWorld AI Harmony patches...");
            var harmony = new Harmony("felrim.FelWorldAI");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[FelCore] Harmony patches applied successfully.");
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
    public static class Game_InitNewGame_Patch
    {
        public static void Postfix()
        {
            // After a new game is initialized, show the character selector window.
            Find.WindowStack.Add(new FelPawn_CharacterSelectorWindow());
        }
    }

    [HarmonyPatch(typeof(MapInterface), nameof(MapInterface.HandleMapClicks))]
    public static class MapInterface_HandleMapClicks_Patch
    {
        public static void Postfix()
        {
            if (FelPawns.KeyBindingDefOf.FelPawns_OpenChat.JustPressed)
            {
                if (Find.WindowStack.IsOpen<FelPawn_ChatWindow>()) return;
                
                var mapComponent = Find.CurrentMap.GetComponent<FelCore_MapComponent>();
                if (mapComponent?.MainCharacter != null)
                {
                    Find.WindowStack.Add(new FelPawn_ChatWindow());
                }
                else
                {
                    Log.Message("You must select a main character to chat.");
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class Pawn_SpawnSetup_Patch
    {
        public static void Postfix(Pawn __instance, Map map, bool respawningAfterLoad)
        {
            if (!__instance.RaceProps.Humanlike) return;
            
            var comp = __instance.GetComp<FelPawn_Component>();
            var mapComponent = map?.GetComponent<FelCore_MapComponent>();
            
            if (comp == null && !respawningAfterLoad)
            {
                Log.Message($"[FelCore] Injecting FelPawn_Component for {__instance.Name.ToStringShort}");
                
                var compProperties = new CompProperties_FelPawn();
                
                comp = new FelPawn_Component();
                comp.parent = __instance;
                __instance.AllComps.Add(comp);
                comp.Initialize(compProperties);
                
                Log.Message($"[FelCore] Successfully injected and initialized FelPawn_Component for {__instance.Name.ToStringShort}");
            }
            
            if (comp != null)
            {
                mapComponent?.ProfileController?.ApplyDefaults(comp);
            }
            
            if (comp != null && (__instance.Faction?.IsPlayer == true || __instance.IsPrisonerOfColony == true))
            {
                Log.Message($"[FelCore] Registering {__instance.Name.ToStringShort} with MapComponent");
                mapComponent?.RegisterPawn(comp);
            }
        }
    }
    
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DeSpawn))]
    public static class Pawn_DeSpawn_Patch
    {
        public static void Prefix(Pawn __instance)
        {
            if (__instance.RaceProps.Humanlike)
            {
                var comp = __instance.GetComp<FelPawn_Component>();
                if (comp != null)
                {
                    __instance.Map?.GetComponent<FelCore_MapComponent>()?.DeregisterPawn(comp);
                }
            }
        }
    }
}
