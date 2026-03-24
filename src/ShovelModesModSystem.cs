using Flow86.ShovelModes.Behaviors;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

[assembly: ModInfo("Shovel Modes", "shovelmodes",
                    Authors = new string[] { "Flow86" },
                    Description = "Adds 1x3 and 3x3 digging modes to shovels.",
                    Version = "1.0.0")]

namespace Flow86.ShovelModes
{
    public class ShovelModesModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterCollectibleBehaviorClass("ShovelModes", typeof(Behaviors.ShovelModesBehavior));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("[ShovelModes] Starting server side");

            foreach (Item item in api.World.SearchItems("shovel-*"))
            {
                Mod.Logger.Notification("[ShovelModes] Adding shovelmodes to " + item.Code.Path);

                if (!item.HasBehavior<ShovelModesBehavior>())
                {
                    var list = new List<CollectibleBehavior>(item.CollectibleBehaviors);
                    list.Add(new Behaviors.ShovelModesBehavior(item));
                    item.CollectibleBehaviors = list.ToArray();
                }
            }
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            Mod.Logger.Notification("[ShovelModes] Starting client side");
        }
    }
}
