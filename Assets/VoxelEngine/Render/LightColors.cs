﻿using System;
using UnityEngine;

namespace VoxelEngine.Render {

    public class LightColors {

        /// <summary> 36 is the serialized form of _LightColor in the Block shader. </summary>
        public const int SERIALIZED_LightColor = 36;

        private Color[] normalLightColors;
        private Color[] debugLightColors;
        private bool useDebugColor;

        public LightColors() {
            this.normalLightColors = References.list.lightColorSheet.GetPixels();
            this.debugLightColors = References.list.debugLightColorSheet.GetPixels();
        }

        public void toggleUseDebugColors() {
            this.useDebugColor = !this.useDebugColor;
        }

        /// <summary>
        /// Returns the color corresponding to a brightness
        /// </summary>
        public Color getColorFromBrightness(int lightLevel) {
            if(lightLevel < 0 || lightLevel > 15) {
                throw new Exception("Light level is invalid, " + lightLevel);
            }
            return this.useDebugColor ? this.debugLightColors[lightLevel] : this.normalLightColors[lightLevel];
        }

        public Color getSmoothColorFromBrightness(float lightLevel) {
            int i = (int)lightLevel;
            int j = i + 1;
            Color light1 = this.getColorFromBrightness(j > 15 ? 15 : j);
            Color light2 = this.getColorFromBrightness(i);
            return Color.Lerp(light1, light2, lightLevel - i);
        }
    }
}
