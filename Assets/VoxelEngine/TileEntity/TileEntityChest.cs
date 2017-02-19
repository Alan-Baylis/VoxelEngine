﻿using VoxelEngine.Containers.Data;
using VoxelEngine.Level;
using UnityEngine;
using fNbt;

namespace VoxelEngine.TileEntity {

    public class TileEntityChest : TileEntityGameObject {

        public ContainerData chestData;
        public ChestOpen chestOpen;

        public TileEntityChest(World world, int x, int y, int z, byte meta) : base(world, x, y, z, References.list.chestPrefab) {
            this.chestData = new ContainerData(2, 2);
            this.gameObject.transform.position = new Vector3(x, y - 0.05f, z);
            this.gameObject.transform.rotation = Quaternion.Euler(0, meta * 90, 0);
            this.chestOpen = this.gameObject.GetComponent<ChestOpen>();
        }

        public override NbtCompound writeToNbt(NbtCompound tag) {
            base.writeToNbt(tag);
            tag.Add(this.chestData.writeToNbt(new NbtCompound("container")));
            return tag;
        }

        public override void readFromNbt(NbtCompound tag) {
            base.readFromNbt(tag);
            this.chestData.readFromNbt(tag.Get<NbtCompound>("container"));
        }

        public override int getId() {
            return 1;
        }
    }
}
