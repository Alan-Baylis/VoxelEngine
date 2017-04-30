﻿using VoxelEngine.Containers;
using VoxelEngine.Items;
using VoxelEngine.Level;
using VoxelEngine.Render;
using VoxelEngine.Util;

namespace VoxelEngine.Blocks {

    public class BlockMushroom : Block {
        private int textureY;

        public BlockMushroom(int id, int textureY) : base(id) {
            this.textureY = textureY;
            this.setTransparent();
            this.setMineTime(0.1f);
            this.setRenderer(RenderManager.MUSHROOM);
            this.setStatesUsed(4);
        }

        public override ItemStack[] getDrops(World world, BlockPos pos, int meta, ItemTool brokenWith) {
            return new ItemStack[] { new ItemStack(Item.mushroom, 0, 1) };
        }

        public override void onNeighborChange(World world, BlockPos pos, int meta, Direction neighborDir) {
            if (neighborDir == Direction.DOWN && !world.getBlock(pos.move(neighborDir)).isSolid) {
                world.breakBlock(pos, null);
            }
        }

        public override void onRandomTick(World world, int x, int y, int z, int meta, int tickSeed) {
            base.onRandomTick(world, x, y, z, meta, tickSeed);
            //TODO
        }

        public override TexturePos getTexturePos(Direction direction, int meta) {
            return new TexturePos(5 + meta, textureY);
        }

        public override bool isValidPlaceLocation(World world, BlockPos pos, int meta, Direction intendedDir) {
            return world.getBlock(pos.move(Direction.DOWN)).isSolid;
        }
    }
}
