﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World : MonoBehaviour {
    //Dictionary of all loaded chunks with a WorldPos as the key
    public Dictionary<BlockPos, Chunk> loadedChunks = new Dictionary<BlockPos, Chunk>();
    public GameObject chunkPrefab;

    public string worldName;
    public ChunkGenerator generator;

    void Awake() {
        this.worldName = "world";
        this.generator = new ChunkGenerator(this);
    }

    void LateUpdate() {
        if (Input.GetKeyDown(KeyCode.R)) {
            this.saveWorld();
        }
    }

    //Loads a new chunk, loading it if the save exists, otherwise we generate a new one.
    public Chunk loadChunk(BlockPos pos) {
        GameObject newChunkObject = Instantiate(chunkPrefab, pos.toVector(), Quaternion.Euler(Vector3.zero)) as GameObject;
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();
        newChunk.initChunk(this, pos);

        //Add it to the chunks dictionary with the position as the key
        loadedChunks.Add(pos, newChunk);

        if(!Serialization.LoadChunk(newChunk)) {
            this.generator.generateChunk(newChunk);
            //new TerrainGen().ChunkGen(newChunk);
        }
        return newChunk;
    }

    //Unloads a chunk, removing references and saving it
    public void unloadChunk(BlockPos pos) {
        Chunk chunk = null;
        if (loadedChunks.TryGetValue(pos, out chunk)) {
            //Serialization.SaveChunk(chunk);
            Object.Destroy(chunk.gameObject);
            loadedChunks.Remove(pos);
        }
    }

    //Returns the chunk at x, y, z (World coordinates)
    public Chunk getChunk(int x, int y, int z) {
        BlockPos pos = new BlockPos();
        float multiple = Chunk.SIZE;
        pos.x = Mathf.FloorToInt(x / multiple) * Chunk.SIZE;
        pos.y = Mathf.FloorToInt(y / multiple) * Chunk.SIZE;
        pos.z = Mathf.FloorToInt(z / multiple) * Chunk.SIZE;

        Chunk containerChunk = null;
        loadedChunks.TryGetValue(pos, out containerChunk);
        return containerChunk;
    }

    public Block getBlock(BlockPos pos) {
        return this.getBlock(pos.x, pos.y, pos.z);
    }

    public Block getBlock(int x, int y, int z) {
        Chunk chunk = this.getChunk(x, y, z);
        if (chunk != null) {
            return chunk.getBlock(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z);
        }
        else {
            return Block.air;
        }
    }

    public void setBlock(BlockPos pos, Block block) {
        this.setBlock(pos.x, pos.y, pos.z, block);
    }

    public void setBlock(int x, int y, int z, Block block) {
        Chunk chunk = this.getChunk(x, y, z);
        if (chunk != null) {
            chunk.setBlock(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, block);
            chunk.dirty = true;

            this.UpdateIfEqual(x - chunk.pos.x, 0,              new BlockPos(x - 1, y, z));
            this.UpdateIfEqual(x - chunk.pos.x, Chunk.SIZE - 1, new BlockPos(x + 1, y, z));
            this.UpdateIfEqual(y - chunk.pos.y, 0,              new BlockPos(x, y - 1, z));
            this.UpdateIfEqual(y - chunk.pos.y, Chunk.SIZE - 1, new BlockPos(x, y + 1, z));
            this.UpdateIfEqual(z - chunk.pos.z, 0,              new BlockPos(x, y, z - 1));
            this.UpdateIfEqual(z - chunk.pos.z, Chunk.SIZE - 1, new BlockPos(x, y, z + 1));        
        }
    }

    //What's this do?
    void UpdateIfEqual(int value1, int value2, BlockPos pos) {
        if (value1 == value2) {
            Chunk chunk = getChunk(pos.x, pos.y, pos.z);
            if (chunk != null) {
                chunk.dirty = true;
            }
        }
    }

    //Saves the world and all loaded chunks.
    public void saveWorld() {
        List<Chunk> tempChunkList = new List<Chunk>();
        foreach (Chunk c in this.loadedChunks.Values) {
            tempChunkList.Add(c);
        }
        foreach (Chunk c in tempChunkList) {
            Serialization.SaveChunk(c);
            Object.Destroy(c.gameObject);
            loadedChunks.Remove(c.pos);
        }
    }
}
