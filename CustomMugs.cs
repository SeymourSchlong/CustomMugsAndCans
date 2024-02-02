using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CustomMugs
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class CustomMugsMod : BaseUnityPlugin
    {
        private const string modGUID = "ironbean.CustomMugs";
        private const string modName = "Custom Mugs";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static CustomMugsMod Instance;

        internal static ManualLogSource mls;

        public static List<string> MugFolders = new List<string>();
        public static readonly List<string> MugFiles = new List<string>();
        public static readonly List<Texture> MugTextures = new List<Texture>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("Loading custom mugs...");

            MugFolders = Directory.GetDirectories(Paths.PluginPath, "ScrapTextures", SearchOption.AllDirectories).ToList();
            foreach (string mugFolder in MugFolders)
            {
                string[] files = Directory.GetFiles(Path.Combine(mugFolder, "mugs"));
                foreach (string text in files)
                {
                    if (Path.GetExtension(text) != ".old")
                    {
                        MugFiles.Add(text);
                    }
                }
            }

            foreach (string mugFile in MugFiles)
            {
                Texture2D texture = new Texture2D(2, 2);
                ImageConversion.LoadImage(texture, File.ReadAllBytes(mugFile));
                MugTextures.Add(texture);
            }

            mls.LogInfo("Loaded " + MugFiles.Count + " mugs");

            harmony.PatchAll(typeof(CustomMugsMod));
            harmony.PatchAll(typeof(MugPatch));
        }

        [HarmonyPatch(typeof(GrabbableObject))]
        internal class MugPatch
        {
            [HarmonyPatch("Start")]
            [HarmonyPostfix]
            private static void CreatePatch(GrabbableObject __instance)
            {
                if (__instance.itemProperties.name != "Mug") return;
                if (MugFiles.Count == 0) return;

                MeshRenderer renderer = __instance.gameObject.GetComponentInChildren<MeshRenderer>();
                if (renderer == null) return;

                renderer.material.mainTexture = MugTextures[__instance.scrapValue % MugTextures.Count];
            }
        }
    }
}
