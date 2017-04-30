﻿using UnityEngine;

namespace VoxelEngine {

    public class References : MonoBehaviour {

        public static References list; //Singleton

        // Materials.
        public Material blockMaterial;
        public Material itemMaterial;

        // Textures.
        public Texture2D itemAtlas;
        public Texture2D lightColorSheet;

        // Other prefabs.
        public GameObject blockBreakEffect;
        public GameObject worldPrefab;
        public GameObject chunkPrefab;
        public GameObject shadowPrefab;

        // Containers gui prefabs.
        public GameObject containerHotbar;

        public GameObject containerHeldText;

        // Container Builder Parts.
        public GameObject conatinerPartCanvas;
        public GameObject containerPartSlot;

        public Transform containerLeftOrgin;
        public Transform containerRightOrgin;

        // TileEntity prefabs.
        public GameObject chestPrefab;
        public GameObject glorbPrefab;
        public GameObject lanternPrefab;
        public GameObject torchPrefab;
        public GameObject mushroomPrefab;

        public void loadResources() {
            this.blockMaterial = Resources.Load<Material>("Materials/BlockMaterial");
            this.itemMaterial = Resources.Load<Material>("Materials/ItemMaterial");

            this.itemAtlas = Resources.Load<Texture2D>("Images/itemAtlas");
            this.lightColorSheet = Resources.Load<Texture2D>("Images/light_colors");

            this.blockBreakEffect = Resources.Load<GameObject>("Prefabs/BreakBlockEffect");
            this.worldPrefab = Resources.Load<GameObject>("Prefabs/World");
            this.chunkPrefab = Resources.Load<GameObject>("Prefabs/Chunk");
            this.shadowPrefab = Resources.Load<GameObject>("Prefabs/EntityShadow");

            this.chestPrefab = Resources.Load<GameObject>("Prefabs/Blocks/ChestPrefab");
            this.lanternPrefab = Resources.Load<GameObject>("Prefabs/Blocks/LanternPrefab");
            this.torchPrefab = Resources.Load<GameObject>("Prefabs/Blocks/TorchPrefab");
            this.mushroomPrefab = Resources.Load<GameObject>("Prefabs/Blocks/MushroomPrefab");

            References.list = this;
        }
    }
}
