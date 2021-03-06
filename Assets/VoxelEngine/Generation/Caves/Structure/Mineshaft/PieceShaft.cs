﻿using fNbt;
using UnityEngine;
using VoxelEngine.Blocks;
using VoxelEngine.Level;
using VoxelEngine.Util;

namespace VoxelEngine.Generation.Caves.Structure.Mineshaft {

    public class PieceShaft : PieceBase {
                
        protected bool addedToList = false;

        /// <summary> -1 = bottom piece of stack, 1 = top of stack. </summary>
        private int specialFlag = 0;
        /// <summary> 0 = NE, 1 = SE, 2 = SW, 3 = NW. </summary>
        private byte floor1Ladder;
        private byte floor2Ladder;
        /// <summary> -1 if there is not a ladder above/below. </summary>
        private int floorBelowLadderFlag = -1;
        private bool closeColumns;
        private byte floorType;

        public PieceShaft(NbtCompound tag) : base(tag) {
            this.specialFlag = tag.Get<NbtByte>("flag").Value;
            this.floor1Ladder = tag.Get<NbtByte>("f1l").Value;
            this.floor2Ladder = tag.Get<NbtByte>("f2l").Value;
            this.floorBelowLadderFlag = tag.Get<NbtByte>("fbl").Value;
            this.closeColumns = tag.Get<NbtByte>("closeColumns").Value == 1;
            this.floorType = tag.Get<NbtByte>("floorType").Value;
        }

        public PieceShaft(StructureMineshaft shaft, BlockPos hallwayPoint, Direction hallwayDir, int piecesFromCenter, int flag)
            : base(shaft, hallwayPoint + (hallwayDir.blockPos * 4)) {

            this.calculateBounds();

            if (this.isIntersecting()) {
                return;
            }
            this.shaft.pieces.Add(this);
            this.addedToList = true;

            piecesFromCenter++;
            if (piecesFromCenter > StructureMineshaft.SIZE_CAP) {
                return;
            }

            this.specialFlag = flag;
            this.floor1Ladder = (byte)this.shaft.rnd.Next(4);
            this.floor2Ladder = (byte)this.shaft.rnd.Next(4);

            // Only pick a random floor block and column mode if this is the root piece (middle).
            if (this.specialFlag != 0) {
                this.closeColumns = this.shaft.rnd.Next(4) == 0;
                this.floorType = (byte)this.shaft.rnd.Next(2);
            }

            if(this.specialFlag == 0) {
                //This is the middle/first piece
                PieceShaft up = new PieceShaft(this.shaft, new BlockPos(hallwayPoint.x, hallwayPoint.y + 14, hallwayPoint.z), hallwayDir, piecesFromCenter, this.specialFlag + 1);
                PieceShaft down = new PieceShaft(this.shaft, new BlockPos(hallwayPoint.x, hallwayPoint.y - 14, hallwayPoint.z), hallwayDir, piecesFromCenter, this.specialFlag - 1);
                if (!up.addedToList && !down.addedToList) {
                    //Both pieces failed, remove this piece. The whole stack failed, so nothing should be here.
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
                up.floorType = this.floorType;
                down.floorType = this.floorType;
                up.closeColumns = this.closeColumns;
                down.closeColumns = this.closeColumns;
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
            Direction torchDir = Direction.horizontal[rnd.Next(0, 4)];
            BlockPos torchPos = this.orgin + (torchDir.blockPos * 4);
            int offsetX, offsetY, offsetZ;
            Block block;
            int meta = 0;
            int i = closeColumns ? 2 : 3;
            for (int x = p1.x; x <= p2.x; x++) {
                for (int y = p1.y; y <= p2.y; y++) {
                    for (int z = p1.z; z <= p2.z; z++) {
                        if(chunk.isInChunk(x, y, z)) {
                            block = Block.air;
                            meta = 0;
                            offsetX = x - this.orgin.x;
                            offsetY = y - this.orgin.y;
                            offsetZ = z - this.orgin.z;

                            // Random gravel on ground
                            if (this.specialFlag == -1 && offsetY == -1) {
                                if(rnd.Next(4) == 0) {
                                    block = Block.gravel;
                                } else {
                                    block = null;
                                }
                            }
                            // Top room code block
                            else if (this.specialFlag == 1 && offsetY > 6) {
                                if(offsetY == 13) {
                                    if ((Mathf.Abs(offsetX) < 3 || Mathf.Abs(offsetZ) < 3) && rnd.Next(3) > 0) {
                                        block = null;
                                    }
                                } else if(offsetY == 12) {
                                    if(Mathf.Abs(offsetZ) == 4 || Mathf.Abs(offsetX) == 4) {
                                        if(rnd.Next(2) == 0) {
                                            block = null;
                                        }
                                    }
                                } else if(offsetY == 11) {
                                    if(Mathf.Abs(offsetX) < 4 && offsetZ == 0) {
                                        block = Block.wood;
                                        meta = 0;
                                    }
                                } else if(offsetY == 10) {
                                    if(Mathf.Abs(offsetX) == 2 && (Mathf.Abs(offsetZ) <= 4)) {
                                        block = Block.wood;
                                        meta = 2;
                                    }
                                } else { // 9, 8
                                    int xAbs = Mathf.Abs(offsetX);
                                    int zAbs = Mathf.Abs(offsetZ);
                                    if (xAbs == 2 && zAbs == 4) {
                                        block = Block.wood;
                                        meta = 1;
                                    } else if(offsetY == 7) {
                                        if(xAbs == 3 && zAbs <= 2 && rnd.Next(10) != 0) {
                                            block = Block.fence;
                                        } else if(xAbs == 3 && zAbs == 4 && rnd.Next(15) == 0) {
                                            RandomChest.MINESHAFT_SHAFT.makeChest(chunk.world, x, y, z, z > this.orgin.z ? Direction.SOUTH : Direction.NORTH, rnd);
                                            block = null;
                                        }
                                    }
                                }
                            }
                            // Torch
                            else if (x == torchPos.x && z == torchPos.z && offsetY == 9) {
                                this.addTorch(chunk, x, y, z, torchDir);
                                block = null;
                            }
                            // Columns
                            else if (Mathf.Abs(offsetX) == i && Mathf.Abs(offsetZ) == i) {
                                block = Block.wood;
                                meta = 1;
                            }
                            // Railing
                            else if (offsetY == 7 || (offsetY == 0 && this.specialFlag != -1)) {
                                int xAbs = Mathf.Abs(offsetX);
                                int zAbs = Mathf.Abs(offsetZ);
                                if (((xAbs == 3 && zAbs < 3) || (zAbs == 3 && xAbs < 3)) && rnd.Next(20) != 0) {
                                    block = Block.fence;
                                }
                            }
                            // Floor
                            else if(offsetY == -1 || offsetY == 6) {
                                if(Mathf.Abs(offsetX) > 2 || Mathf.Abs(offsetZ) > 2) {
                                    block = Block.wood;
                                    meta = 1;
                                }
                            }
                            // Side logs
                            else if (offsetY == -1 || offsetY == 6) {
                                int absX = Mathf.Abs(offsetX);
                                int absZ = Mathf.Abs(offsetZ);
                                if (this.floorType < 2) {
                                    if (absX > 2 || absZ > 2) {
                                        if(floorType == 0) {
                                            block = Block.wood;
                                            meta = 1;
                                        } else {
                                            block = Block.cobblestone;
                                        }
                                    }
                                } else if(floorType == 2) {
                                    if (absX == 3 || absZ == 3) {
                                        block = Block.plank;
                                    } else if (absX == 2 && absZ < 2) {
                                        block = Block.wood;
                                        meta = 2;
                                    } else if (absZ == 2 && absX < 2) {
                                        block = Block.wood;
                                        meta = 0;
                                    }
                                }
                            }

                            this.setState(chunk, x, y, z, block, meta);
                        }
                    }
                }
            }

            // Ladders.
            this.placeLadder(this.floor1Ladder, chunk, true);
            if(this.specialFlag != 1) {
                // Don't place ladders on the top floor.
                this.placeLadder(this.floor2Ladder, chunk, false);
            }
        }

        /// <summary>
        /// Places the ladder blocks for a floor.
        /// </summary>
        private void placeLadder(int ladderFlag, Chunk chunk, bool isBottomFloor) {
            BlockPos ladderShift = this.getLadderOffset(ladderFlag);
            int ladderBottom = (isBottomFloor ? this.orgin.y : this.orgin.y + 7);
            int ladderTop = (isBottomFloor ? this.orgin.y + 9 : (this.specialFlag == 1 ? this.orgin.y + 12 : this.orgin.y + 14));
            for (int y = ladderBottom; y < ladderTop; y++) {
                this.setStateIfInChunk(chunk, ladderShift.x, y, ladderShift.z, Block.ladder, ladderFlag);
            }

            // Place the ladder stub
            if (isBottomFloor && this.specialFlag != -1) {
                ladderShift = this.getLadderOffset(this.floorBelowLadderFlag);
                for (int y = this.orgin.y - 1; y < this.orgin.y + 2; y++) {
                    this.setStateIfInChunk(chunk, ladderShift.x, y, ladderShift.z, Block.ladder, this.floorBelowLadderFlag);
                }
            }
        }

        /// <summary>
        /// Shifts the passed BlockPos based on the passed ladder flag.
        /// </summary>
        private BlockPos getLadderOffset(int ladderFlag) {
            if (ladderFlag == 0) {
                return new BlockPos(this.orgin.x + 4, 0, this.orgin.z + 4);
            } else if (ladderFlag == 1) {
                return new BlockPos(this.orgin.x + 4, 0, this.orgin.z - 4);
            } else if (ladderFlag == 2) {
                return new BlockPos(this.orgin.x - 4, 0, this.orgin.z - 4);
            } else { // 3
                return new BlockPos(this.orgin.x - 4, 0, this.orgin.z + 4);
            }
        }

        public override NbtCompound writeToNbt(NbtCompound tag) {
            base.writeToNbt(tag);
            tag.Add(new NbtByte("flag", (byte)this.specialFlag));
            tag.Add(new NbtByte("f1l", this.floor1Ladder));
            tag.Add(new NbtByte("f2l", this.floor2Ladder));
            tag.Add(new NbtByte("fbl", (byte)this.floorBelowLadderFlag));
            tag.Add(new NbtByte("closeColumns", (byte)(this.closeColumns ? 1 : 0)));
            tag.Add(new NbtByte("floorType", this.floorType));
            return tag;
        }

        public override void calculateBounds() {
            this.setPieceSize(1, 12, 4);
        }
    }
}