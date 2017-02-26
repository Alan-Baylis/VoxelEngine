﻿using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VoxelEngine.Containers;
using VoxelEngine.Entities;
using VoxelEngine.Generation;
using VoxelEngine.GUI;
using VoxelEngine.Items;
using VoxelEngine.Level;
using VoxelEngine.Util;

namespace VoxelEngine {

    public class Main : MonoBehaviour {
        public static Main singleton;

        [HideInInspector]
        public bool isDeveloperMode;
        [HideInInspector]
        public bool showDebugText = true;
        [HideInInspector]
        public bool isPaused;

        public World worldObj;
        public EntityPlayer player;

        public Text textDebug;

        public GuiScreen pauseScreen;
        public GuiScreen respawnScreen;
        public GuiScreen currentGui;

        public ContainerManager containerManager;
        public FpsCounter fpsCounter;

        public void Awake() {
            //Make sure the singleton reference is set
            this.GetComponent<References>().initReferences();

            Main.singleton = this;

            Item.initBlockItems();
            Item.preRenderItems();

            this.fpsCounter = new FpsCounter();
        }

        public void Start() {
            this.containerManager = new ContainerManager();

            //Debug instant world generation
            string name = "world" + UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            this.generateWorld(new WorldData(name, UnityEngine.Random.Range(int.MinValue, int.MaxValue), WorldType.CAVE_1.id, true));
        }

        public void Update() {
            if (this.worldObj != null && this.player != null) {
                if (Input.GetKeyDown(KeyCode.F1)) {
                    this.isDeveloperMode = !this.isDeveloperMode;
                }
                if (Input.GetKeyDown(KeyCode.F2)) {
                    ScreenshotHelper.captureScreenshot();
                }
                if (Input.GetKeyDown(KeyCode.F3)) {
                    this.showDebugText = !this.showDebugText;
                    if(!this.showDebugText) {
                        this.textDebug.text = string.Empty;
                    }
                }
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    if (this.player.contManager.isContainerOpen()) {
                        this.player.contManager.closeContainer(this.player);
                    } else {
                        if(this.currentGui != null) {
                            this.currentGui = this.currentGui.onEscape(this);
                        } else {
                            if(!this.isPaused) {
                                this.pauseGame();
                            }
                        }
                    }
                }

                if (!this.isPaused && player.health > 0 && !this.containerManager.isContainerOpen()) {
                    this.player.handleInput();
                }

                this.containerManager.drawContainerContents();

                this.player.containerHotbar.renderContents();

                this.fpsCounter.updateCounter();

                //Update debug text
                if (this.showDebugText) {
                    this.textDebug.text = this.getDebugText();
                }
            } else {
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    this.currentGui = this.currentGui.onEscape(this);
                }
            }
        }

        private void pauseGame() {
            this.isPaused = true;
            this.player.fpc.enabled = false;
            Main.hideMouse(false);
            Time.timeScale = 0;
            this.openGuiScreen(this.pauseScreen);
        }

        public void resumeGame() {
            this.isPaused = false;
            this.player.fpc.enabled = true;
            Main.hideMouse(true);
            Time.timeScale = 1;
        }

        [Obsolete("Use GuiScreen.openGuiScreen instead")]
        public void openGuiScreen(GuiScreen screen) {
            if (this.currentGui != null) {
                //Only the pause screen will not trigger this, as there is no screen before it
                this.currentGui.setActive(false);
            }

            this.currentGui = screen;
            this.currentGui.setActive(true);
        }

        public string getDebugText() {
            StringBuilder s = new StringBuilder();
            s.Append("FPS: " + this.fpsCounter.currentFps);
            s.Append("\nPlayer Position: " + this.transform.position.ToString());
            s.Append("\nPlayer Rotation: " + this.transform.eulerAngles.ToString());
            BlockPos p = this.player.posLookingAt;
            byte meta = this.worldObj.getMeta(p);
            s.Append("\nLooking At: " + this.worldObj.getBlock(p).getName(meta) + ":" + meta + " " + p.ToString());
            s.Append("\n" + this.worldObj.worldData.worldName + " Seed: " + this.worldObj.worldData.seed);
            s.Append("\nPress F3 to toggle");
            return s.ToString();
        }

        public void generateWorld(WorldData data) {
            //this.openGuiScreen(this.waitingScreen);
            //this.data = data;
            this.worldObj = GameObject.Instantiate(References.list.worldPrefab).GetComponent<World>();
            this.worldObj.initWorld(data);

            this.currentGui.setActive(false);
            this.currentGui = null;
            this.player = this.worldObj.spawnPlayer(EntityList.singleton.playerPrefab);
            Main.hideMouse(true);
        }

        //public void onWorldLoadFinish() {
        //    this.currentGui.setActive(false);
        //    this.currentGui = null;
        //    this.player = this.worldObj.spawnPlayer(EntityList.singleton.playerPrefab);
        //    Main.setMouseLock(true);
        //}

        public static void hideMouse(bool flag) {
            Cursor.visible = !flag;
            Cursor.lockState = flag ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
