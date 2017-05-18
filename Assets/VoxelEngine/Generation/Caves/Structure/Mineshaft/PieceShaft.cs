﻿using System.Collections.Generic;
using fNbt;
using UnityEngine;
using VoxelEngine.Blocks;
using VoxelEngine.Level;
using VoxelEngine.Util;

namespace VoxelEngine.Generation.Caves.Structure.Mineshaft {

    public class PieceShaft : PieceBase {
                
        protected bool addedToList = false;

        /// <summary> -1 = bottom piece of stack, 1 = top of stack. </summary>
        private int specialFlag = 0;
        // 0 = NE, 1 = SE, 2 = SW, 3 = NW
        private int floor1Ladder;
        private int floor2Ladder;
        // -1 if there is not a ladder above/below
        private int floorBelowLadderFlag = -1;

        public PieceShaft(NbtCompound tag) : base(tag) {
            this.specialFlag = tag.Get<NbtInt>("flag").IntValue;
            this.floor1Ladder = tag.Get<NbtInt>("f1l").IntValue;
            this.floor2Ladder = tag.Get<NbtInt>("f2l").IntValue;
            this.floorBelowLadderFlag = tag.Get<NbtInt>("fbl").IntValue;
        }

        public PieceShaft(StructureMineshaft shaft, BlockPos hallwayPoint, Direction hallwayDir, int piecesFromCenter, int flag)
            : base(shaft, hallwayPoint + (hallwayDir.direction * 4)) {

            this.calculateBounds();

            if (this.isIntersecting(this.shaft.pieces)) {
                return;
            }
            this.shaft.pieces.Add(this);
            this.addedToList = true;

            piecesFromCenter++;
            if (piecesFromCenter > StructureMineshaft.SIZE_CAP) {
                return;
            }

            this.specialFlag = flag;
            this.floor1Ladder = this.shaft.rnd.Next(4);
            this.floor2Ladder = this.shaft.rnd.Next(4);

            if(this.specialFlag == 0) {
                //This is the middle/first piece
                PieceShaft up = new PieceShaft(this.shaft, new BlockPos(hallwayPoint.x, hallwayPoint.y + 14, hallwayPoint.z), hallwayDir, piecesFromCenter, this.specialFlag + 1);
                PieceShaft down = new PieceShaft(this.shaft, new BlockPos(hallwayPoint.x, hallwayPoint.y - 14, hallwayPoint.z), hallwayDir, piecesFromCenter, this.specialFlag - 1);
                if (!up.addedToList && !down.addedToList) {
                    //Both pieces failed, remove this piece. The whole stack failed, so nothing should be here
                    this.shaft.pieces.RemoveAt(this.shaft.pieces.Count - 1);
                } else {
                    // We added at least one piece, up or down or both
                    if (up.addedToList) {
                        up.floorBelowLadderFlag = this.floor2Ladder;
                    } else {
                        this.specialFlag = 1;
                    }
                    if (down.addedToList) {
                        this.floorBelowLadderFlag = down.floor2Ladder;
                    } else {
                        this.specialFlag = -1;
                    }
                }
            } else if(this.specialFlag == -1) {
                //TODO chance for water filled bottom piece
            }

            this.generateHallwaysAroundPoint(hallwayDir.getOpposite(), this.orgin, 5, piecesFromCenter);
        }

        public override Color getPieceColor() {
            return Color.cyan;
        }

        public override byte getPieceId() {
            return 4;
        }

        public override void carvePiece(Chunk chunk, System.Random rnd) {
            BlockPos p1 = this.getPosMin();
            BlockPos p2 = this.getPosMax();
            Direction torchDir = Direction.yPlane[rnd.Next(0, 4)];
            BlockPos torchPos = this.orgin + (torchDir.direction * 4);
            int chunkCoordX, chunkCoordY, chunkCoordZ, offsetX, offsetY, offsetZ;
            Block b;
            int meta = 0;
            for (int x = p1.x; x <= p2.x; x++) {
                for (int y = p1.y; y <= p2.y; y++) {
                    for (int z = p1.z; z <= p2.z; z++) {
                        if(chunk.isInChunk(x, y, z)) {
                            b = Block.air;
                            meta = 0;
                            chunkCoordX = x - chunk.pos.x;
                            chunkCoordY = y - chunk.pos.y;
                            chunkCoordZ = z - chunk.pos.z;
                            offsetX = x - this.orgin.x;
                            offsetY = y - this.orgin.y;
                            offsetZ = z - this.orgin.z;

                            // Random gravel on ground
                            if (this.specialFlag == -1 && offsetY == -1) {
                                if(rnd.Next(4) == 0) {
                                    b = Block.gravel;
                                } else {
                                    b = null;
                                }
                            }
                            // Top room code block
                            else if (this.specialFlag == 1 && offsetY > 8) {
                                if(offsetY == 14) {
                                    if ((Mathf.Abs(offsetX) < 3 || Mathf.Abs(offsetZ) < 3) && rnd.Next(3) > 0) {
                                        b = null;
                                    }
                                } else if(offsetY == 13) {
                                    if(Mathf.Abs(offsetZ) == 4 || Mathf.Abs(offsetX) == 4) {
                                        if(rnd.Next(2) == 0) {
                                            b = null;
                                        }
                                    }
                                } else if(offsetY == 12) {
                                    if(Mathf.Abs(offsetX) < 4 && offsetZ == 0) {
                                        b = Block.wood;
                                        meta = 0;
                                    }
                                } else if(offsetY == 11) {
                                    if(Mathf.Abs(offsetX) == 2 && (Mathf.Abs(offsetZ) <= 4)) {
                                        b = Block.wood;
                                        meta = 2;
                                    }
                                } else { // 10, 9, 8
                                    int xAbs = Mathf.Abs(offsetX);
                                    int zAbs = Mathf.Abs(offsetZ);
                                    if (xAbs == 2 && zAbs == 4) {
                                        b = Block.wood;
                                        meta = 1;
                                    } else if(offsetY == 8) {
                                        if(xAbs == 3 && zAbs <= 2 && rnd.Next(10) != 0) {
                                            b = Block.fence;
                                        } else if(xAbs == 3 && zAbs == 4 && rnd.Next(25) == 0) {
                                            RandomChest.SPAWN_CHEST.makeChest(chunk.world, x, y, z, z > this.orgin.z ? Direction.NORTH : Direction.SOUTH, rnd);
                                            continue;
                                        }
                                    }
                                }
                            }
                            // Torch
                            else if (x == torchPos.x && z == torchPos.z && chunkCoordY == 9) {
                                b = Block.torch;
                                meta = BlockTorch.getMetaFromDirection(torchDir);
                                //chunk.world.setBlock(i, j, k, Block.torch, BlockTorch.getMetaFromDirection(torchDir), false);
                                continue;
                            }
                            // Railing
                            else if (offsetY == 7 || (offsetY == 0 && this.specialFlag != -1)) {
                                int xAbs = Mathf.Abs(offsetX);
                                int zAbs = Mathf.Abs(offsetZ);
                                if (((xAbs == 3 && zAbs < 3) || (zAbs == 3 && xAbs < 3)) && rnd.Next(20) != 0) {
                                    b = Block.fence;
                                }
                            }
                            // Floor
                            else if(offsetY == -1 || offsetY == 6) {
                                if(Mathf.Abs(offsetX) > 2 || Mathf.Abs(offsetZ) > 2) {
                                    b = Block.wood;
                                    meta = 1;
                                }
                            }

                            if (b != null) {
                                chunk.setBlock(chunkCoordX, chunkCoordY, chunkCoordZ, b);
                                if (meta != 0) {
                                    chunk.setMeta(chunkCoordX, chunkCoordY, chunkCoordZ, meta);
                                }
                            }
                        }
                    }
                }
            }
            this.placeLadder(this.floor1Ladder, chunk, this.orgin.x, this.orgin.z, true);
            if(this.specialFlag != 1) {
                this.placeLadder(this.floor2Ladder, chunk, this.orgin.x, this.orgin.z, false);
            }
        }

        /// <summary>
        /// Places the ladder blocks for a floor.
        /// </summary>
        private void placeLadder(int ladderFlag, Chunk chunk, int orginX, int orginZ, bool isBottomFloor) {
            BlockPos ladderShift = this.getLadderOffset(ladderFlag, orginX, orginZ);
            if (chunk.isInChunkIgnoreY(ladderShift.x, ladderShift.z)) {
                int x = ladderShift.x - chunk.pos.x;
                int z = ladderShift.z - chunk.pos.z;
                //int ladderBottom = (isBottomFloor ? (this.specialFlag == -1 ? 1 : 1) : 9);
                int ladderBottom = (isBottomFloor ? this.orgin.y : this.orgin.y + 9);
                int ladderTop = (isBottomFloor ? this.orgin.y + 10 : (this.specialFlag == 1 ? this.orgin.y + 12 : this.orgin.y + 14));
                for (int y = ladderBottom; y < ladderTop; y++) {
                    this.setStateIfInChunk(chunk, x, y, z, Block.ladder, ladderFlag);
                }
            }

            // Place the ladder stub
            ladderShift = this.getLadderOffset(this.floorBelowLadderFlag, orginX, orginZ);
            if (chunk.isInChunkIgnoreY(ladderShift.x, ladderShift.z)) {
                if (isBottomFloor && this.floorBelowLadderFlag != -1) {
                    int x = ladderShift.x - chunk.pos.x;
                    int z = ladderShift.z - chunk.pos.z;
                    for (int y = 0; y < 3; y++) {
                        this.setStateIfInChunk(chunk, x, y, z, Block.ladder, ladderFlag);
                    }
                }
            }
        }

        /// <summary>
        /// Shifts the passed BlockPos based on the passed ladder flag.
        /// </summary>
        private BlockPos getLadderOffset(int ladderFlag, int orginX, int orginZ) {
            if (ladderFlag == 0) {
                return new BlockPos(orginX + 4, 0, orginZ + 4);
            } else if (ladderFlag == 1) {
                return new BlockPos(orginX + 4, 0, orginZ + -4);
            } else if (ladderFlag == 2) {
                return new BlockPos(orginX + -4, 0, orginZ + -4);
            } else { // 3
                return new BlockPos(orginX + -4, 0, orginZ + 4);
            }
        }

        public override NbtCompound writeToNbt(NbtCompound tag) {
            base.writeToNbt(tag);
            tag.Add(new NbtInt("flag", this.specialFlag));
            tag.Add(new NbtInt("f1l", this.floor1Ladder));
            tag.Add(new NbtInt("f2l", this.floor2Ladder));
            tag.Add(new NbtInt("fbl", this.floorBelowLadderFlag));
            return tag;
        }

        public override void calculateBounds() {
            this.setPieceSize(1, 12, 4);
        }
    }
}