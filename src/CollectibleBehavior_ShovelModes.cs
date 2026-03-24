using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Flow86.ShovelModes.Behaviors
{
    public class ShovelModesBehavior : CollectibleBehavior
    {
        private SkillItem[] modes;
        private int mode = 0; // 0 = normal, 1 = 1x3, 2 = 3x3

        public ShovelModesBehavior(CollectibleObject collObj) : base(collObj)
        {
            modes = new[]
            {
                new SkillItem { Code = new AssetLocation("normal"),  Name = "Normal" },
                new SkillItem { Code = new AssetLocation("line1x3"), Name = "1×3 Linie" },
                new SkillItem { Code = new AssetLocation("area3x3"), Name = "3×3 Fläche" }
            };
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);


            if (api is ICoreClientAPI capi)
            {
                this.modes[0].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("shovelmodes", "textures/icons/normal.png"), 48, 48, 5, new int?(-1)));
                this.modes[0].TexturePremultipliedAlpha = false;
                this.modes[1].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("shovelmodes", "textures/icons/packyourshovel-pack.svg"), 48, 48, 5, new int?(-1)));
                this.modes[1].TexturePremultipliedAlpha = false;
                this.modes[2].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("shovelmodes", "textures/icons/packyourshovel-path.svg"), 48, 48, 5, new int?(-1)));
                this.modes[2].TexturePremultipliedAlpha = false;
            }
        }

        public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
        {
            return modes;
        }

        public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection)
        {
            return mode;
        }

        public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection, int toolMode)
        {
            mode = toolMode;
            slot.MarkDirty();
        }

        private bool CanShovelBreakBlock(Block block, BlockSelection sel, ItemSlot slot, IPlayer player)
        {
            if (block == null || slot?.Itemstack == null || player == null)
                return false;

            float speed = block.GetMiningSpeed(slot.Itemstack, sel, block, player);

            return speed >= 0.1f;
        }

        public override bool OnBlockBrokenWith(
            IWorldAccessor world, Entity byEntity, ItemSlot itemslot,
            BlockSelection blockSel, float dropQuantityMultiplier, ref EnumHandling bhHandling)
        {
            if (mode == 0 || blockSel == null)
                return base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel, dropQuantityMultiplier, ref bhHandling);

            var player = world.PlayerByUid((byEntity as EntityPlayer)?.PlayerUID);
            if (player == null)
                return base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel, dropQuantityMultiplier, ref bhHandling);

            bool result = base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel, dropQuantityMultiplier, ref bhHandling);

            List<BlockPos> extra = mode switch
            {
                1 => Get1x3(blockSel.Position, blockSel.Face),
                2 => Get3x3(blockSel.Position, blockSel.Face),
                _ => new List<BlockPos>()
            };

            int durabilityCost = 1;

            foreach (var pos in extra)
            {
                Block block = world.BlockAccessor.GetBlock(pos);

                var sel = new BlockSelection()
                {
                    Position = pos,
                    Face = blockSel.Face,
                    HitPosition = blockSel.HitPosition
                };

                if (!CanShovelBreakBlock(block, sel, itemslot, player))
                    continue;

                durabilityCost++;
                world.BlockAccessor.BreakBlock(pos, player);
            }

            collObj.DamageItem(world, byEntity, itemslot, durabilityCost);

            return result;
        }

        private List<BlockPos> Get1x3(BlockPos center, BlockFacing face)
        {
            List<BlockPos> positions = new();

            Vec3i axis;

            switch (face.Axis)
            {
                case EnumAxis.X:
                    // Wand Ost/West → horizontale Linie entlang Z
                    axis = new Vec3i(0, 0, 1);
                    break;

                case EnumAxis.Z:
                    // Wand Nord/Süd → horizontale Linie entlang X
                    axis = new Vec3i(1, 0, 0);
                    break;

                case EnumAxis.Y:
                default:
                    // Boden/Decke → horizontale Linie entlang X
                    axis = new Vec3i(1, 0, 0);
                    break;
            }

            for (int i = -1; i <= 1; i++)
            {
                if (i == 0) continue;

                positions.Add(center.AddCopy(
                    axis.X * i,
                    axis.Y * i,
                    axis.Z * i
                ));
            }

            return positions;
        }

        private List<BlockPos> Get3x3(BlockPos center, BlockFacing face)
        {
            List<BlockPos> positions = new();

            // Zwei Achsen bestimmen, die die Fläche bilden
            Vec3i axisA, axisB;

            switch (face.Axis)
            {
                case EnumAxis.X:
                    axisA = new Vec3i(0, 1, 0);  // Y
                    axisB = new Vec3i(0, 0, 1);  // Z
                    break;

                case EnumAxis.Y:
                    axisA = new Vec3i(1, 0, 0);  // X
                    axisB = new Vec3i(0, 0, 1);  // Z
                    break;

                case EnumAxis.Z:
                default:
                    axisA = new Vec3i(1, 0, 0);  // X
                    axisB = new Vec3i(0, 1, 0);  // Y
                    break;
            }

            for (int ax = -1; ax <= 1; ax++)
            {
                for (int bx = -1; bx <= 1; bx++)
                {
                    if (ax == 0 && bx == 0) continue;

                    positions.Add(center.AddCopy(
                        axisA.X * ax + axisB.X * bx,
                        axisA.Y * ax + axisB.Y * bx,
                        axisA.Z * ax + axisB.Z * bx
                    ));
                }
            }

            return positions;
        }
    }
}
