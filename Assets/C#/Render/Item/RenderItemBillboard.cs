﻿using System;
using UnityEngine;

public class RenderItemBillboard : IRenderItem {

    public MeshData renderItem(ItemStack stack) {
        MeshData meshData = new MeshData();

        // Faces front, +z
        meshData.addVertex(new Vector3(0.5f, -0.5f, 0));  //bottom right
        meshData.addVertex(new Vector3(0.5f, 0.5f, 0));   //top right
        meshData.addVertex(new Vector3(-0.5f, 0.5f, 0));  //top left
        meshData.addVertex(new Vector3(-0.5f, -0.5f, 0)); //bottom left
        meshData.addQuadTriangles();
        meshData.uv.AddRange(this.setUVs(stack.item.texturePos));

        // Faces back, -z
        meshData.addVertex(new Vector3(-0.5f, -0.5f, 0));
        meshData.addVertex(new Vector3(-0.5f, 0.5f, 0));
        meshData.addVertex(new Vector3(0.5f, 0.5f, 0));
        meshData.addVertex(new Vector3(0.5f, -0.5f, 0));
        meshData.addQuadTriangles();
        meshData.uv.AddRange(this.setUVs(stack.item.texturePos));

        return meshData;
    }

    public Matrix4x4 getMatrix(Transform t) {
        return Matrix4x4.TRS(t.position + -t.forward, Quaternion.identity, new Vector3(0.1f, 0.1f, 0.1f));
    }

    private Vector2[] setUVs(TexturePos pos) {
        float x = TexturePos.ITEM_SIZE * pos.x;
        float y = TexturePos.ITEM_SIZE * pos.y;
        Vector2[] UVs = new Vector2[4] {
            new Vector2(x, y),
            new Vector2(x, y + TexturePos.ITEM_SIZE),
            new Vector2(x + TexturePos.ITEM_SIZE, y + TexturePos.ITEM_SIZE),
            new Vector2(x + TexturePos.ITEM_SIZE, y) };
        return UVs;
    }
}