﻿using UnityEngine;
using VoxelEngine.Containers;
using VoxelEngine.Items;
using VoxelEngine.Level;
using VoxelEngine.Render;
using VoxelEngine.Render.NewSys;
using VoxelEngine.Util;

namespace VoxelEngine.Blocks {

    public abstract class BlockLogicBase : Block {

        public BlockLogicBase(int id) : base(id) {
            this.setTransparent();
            this.setMineTime(0.5f);
            this.setRenderer(RenderManager.LOGIC_PLATE);
        }

        public override UvPlane getUvPlane(int meta, Direction direction) {
            if(direction.axis == EnumAxis.X || direction.axis == EnumAxis.Z) {
                return new UvPlane(new TexturePos(9, 1), 0, 0, 32, 4);
            } else if(direction == Direction.DOWN) {
                return new UvPlane(new TexturePos(9, 0), 0, 0, 32, 32); // Bottom
            } else {
                return new UvPlane(this.getTopTexture(meta * 90), 0, 0, 32, 32); // Top
            }
        }

        public override int adjustMetaOnPlace(World world, BlockPos pos, int meta, Direction clickedDir, Vector3 angle) {
            if (Mathf.Abs(angle.x) > Mathf.Abs(angle.z)) { // X aixs
                if (angle.x > 0) {
                    return 1; // East
                } else {
                    return 3; // West
                }
            } else { // Z axis
                if (angle.z > 0) {
                    return 2; // North
                } else {
                    return 0; // South
                }
            }
        }

        public override ItemStack[] getDrops(World world, BlockPos pos, int meta, ItemTool brokenWith) {
            return base.getDrops(world, pos, 0, brokenWith);
        }

        public override bool isValidPlaceLocation(World world, BlockPos pos, int meta, Direction clickedDirNormal, BlockState clickedBlock) {
            return world.getBlock(pos.move(Direction.DOWN)).isSolid;
        }

        public override void onNeighborChange(World world, BlockPos pos, int meta, Direction neighborDir) {
            if (neighborDir == Direction.DOWN && !world.getBlock(pos.move(neighborDir)).isSolid) {
                world.breakBlock(pos, null);
            }
        }

        public abstract TexturePos getTopTexture(int rotation);
    }
}
