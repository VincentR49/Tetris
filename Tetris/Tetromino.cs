using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tetris
{
    public class Tetromino
    {
        Block[] Blocks = new Block[4];
        Board Board;
        public char Tag;
        public int X; // position X of the tetromino
        public int Y; // position Y of the tetromino
        public int RotStatus = 0;
        public bool IsFalling = true;
        public double TimeSinceStopFalling = 0;

        public int Xghost => X;
        public int Yghost
        {
            get
            {
                int yghost = Y;
                while (CanMoveTo(X, yghost))
                {
                    yghost--;
                }
                return yghost+1;
            }
        }
           
        // Rotation center of the tetromino
        public Vector2 RotCenter
        {
            get
            {
                Vector2 rotCenter = new Vector2(X, Y);
                if (Tag == 'I')
                {
                    switch (RotStatus)
                    {
                        case 0:
                            rotCenter.X += -0.5F;
                            rotCenter.Y += -0.5F;
                            break;
                        case 1:
                            rotCenter.X += -0.5F;
                            rotCenter.Y += 0.5F;
                            break;
                        case 2:
                            rotCenter.X += 0.5F;
                            rotCenter.Y += 0.5F;
                            break;
                        case 3:
                            rotCenter.X += 0.5F;
                            rotCenter.Y += -0.5F;
                            break;
                    }
                }
                return rotCenter;
            }
        }

        // Constructor
        public Tetromino(Board board, int x, int y, char tag, Texture2D blockTexture)
        {
            Tag = tag;
            X = x;
            Y = y;
            Board = board;
            // Block positions
            int[,] blockPos = new int[,] { { 0, -1 }, { 0, 0 }, { -1, 0 }, { 0, 1 } };
            switch (tag)
            {
                case 'I':
                    blockPos = new int[,] { { -2, 0 }, { -1, 0 }, { 0, 0 }, { 1, 0 } };
                    break;
                case 'O':
                    blockPos = new int[,] { { 0, 0 }, { 1, 0 }, { 0, 1 }, { 1, 1 } };
                    break;
                case 'T':
                    blockPos = new int[,] { { -1, 0 }, { 0, 0 }, { 0, 1 }, { 1, 0 } };
                    break;
                case 'S':
                    blockPos = new int[,] { { -1, 0 }, { 0, 0 }, { 0, 1 }, { 1, 1 } };
                    break;
                case 'Z':
                    blockPos = new int[,] { { -1, 1 }, { 0, 1 }, { 0, 0 }, { 1, 0 } };
                    break;
                case 'J':
                    blockPos = new int[,] { { -1, 1 }, { -1, 0 }, { 0, 0 }, { 1, 0 } };
                    break;
                case 'L':
                    blockPos = new int[,] { { -1, 0 }, { 0, 0 }, { 1, 0 }, { 1, 1 } };
                    break;
                default:
                    throw new ArgumentException("Tag doit être inclu dans OIJLSZT");
            }
            // Blocks Initialisation
            for (int i = 0; i < Blocks.Length; i++)
                Blocks[i] = new Block(board, X + blockPos[i, 0], Y + blockPos[i, 1], blockTexture);
        }

        // Rotation (nTurn: nombre de tour - clockwise)
        public bool Rotate(int nTurn)
        {
            float rotation = -(float)Math.PI * nTurn / 2;
            int newRotStatus = (RotStatus + nTurn) % 4;
            Vector2 wallKick = new Vector2();
            //Vector2 origin = RotCenter;
            if ("TSZJLI".Contains(Tag) && CanRotate(rotation, RotCenter, newRotStatus, out wallKick)) // For all tetromino except O (no rotation)
            {
                // Wall Kick
                MoveTo (X + (int) Math.Round(wallKick.X), Y + (int) Math.Round(wallKick.Y), true);
                // Rotation of each Blocks
                foreach (Block b in Blocks)
                {
                    Vector2 newPos = RotateAboutOrigin (new Vector2(b.X, b.Y), RotCenter, rotation);
                    b.MoveTo ((int) Math.Round(newPos.X), (int) Math.Round(newPos.Y));
                }
                if (Tag == 'I') // we update also the position of the reference position of Y (wich is not the rotation center) (confusing...)
                {
                    Vector2 newPos = RotateAboutOrigin(new Vector2(X, Y), RotCenter, rotation);
                    X = (int) Math.Round(newPos.X);
                    Y = (int) Math.Round(newPos.Y);
                }
                RotStatus = newRotStatus; // Update rotation State
            }
            return true;
        }

        // Check if the tetromino can rotate
        // If the tetromino cannot rotate (wall, or other block at the arrival position), 5 configurations are tested (after wall kick)
        // If all the rotations failed, the rotation fail.
        // Otherwise, return true and the value of the wallkick
        public bool CanRotate(float rotation, Vector2 origin, int newRotStatus, out Vector2 wallKick)
        {

            // Parameters initialization
            wallKick = new Vector2(0, 0);
            int[] occupedX = Blocks.Select(n => n.X).ToArray();
            int[] occupedY = Blocks.Select(n => n.Y).ToArray();
            int rotationAttempt = 1; // number of rotations tried
            bool canRotate = false;

            if (Tag == '0') return false;

            while (!canRotate && rotationAttempt <= 5)
            {
                // ----------------------------------------
                // Wall Kick values (tableau à prendre en compte !)
                // ----------------------------------------
                wallKick = GetWallKick(RotStatus, newRotStatus, rotationAttempt, rotation, Tag);
                int xPush = (int)Math.Round(wallKick.X);
                int yPush = (int)Math.Round(wallKick.Y);
                // --------------------------------------------------------
                // Check if all the blocks can rotate (free arrival space)
                // --------------------------------------------------------
                int blockCanMoveCount = 0;
                foreach (Block b in Blocks)
                {
                    Vector2 newPos = RotateAboutOrigin (new Vector2(b.X + xPush, b.Y + yPush), new Vector2 (origin.X + xPush, origin.Y + yPush), rotation);
                    int newX = (int)Math.Round(newPos.X);
                    int newY = (int)Math.Round(newPos.Y);
                    if (Board.IsFreeAt(newX, newY))
                        blockCanMoveCount++; // the block can rotate freely
                    else // A SIMPLIFIER !!!
                    {
                        bool belongToTetromino = false;
                        for (int i = 0; i < occupedX.Length; i++)
                            if (newX == occupedX[i] && newY == occupedY[i])
                                belongToTetromino = true;
                        if (belongToTetromino)
                            blockCanMoveCount++; // the block can move
                        else break;
                    }
                } // end loop on each blocks
                //Console.WriteLine(blockCanMoveCount);
                if (blockCanMoveCount == Blocks.Length) canRotate = true;
                rotationAttempt++;
            } // end while loop on different attemps
            return canRotate;
        }

        public void Update(GameTime gameTime)
        {
            // Check tetromino state
            // If the tetromino cannot move or rotate
            if (!CanMoveTo(X, Y - 1))
            {
                if (TimeSinceStopFalling == 0)
                    TimeSinceStopFalling = gameTime.TotalGameTime.TotalMilliseconds;
                else if (gameTime.TotalGameTime.TotalMilliseconds - TimeSinceStopFalling > 500)
                    IsFalling = false;
            }
            else
            {
                IsFalling = true;
                TimeSinceStopFalling = 0;
            }
        }

        // Move
        public void MoveTo(int x, int y, bool forceMove = false)
        {
            if ( !CanMoveTo(x,y) && !forceMove)
                return;
            // Check here if the movement is possible
            int dX = x - X;
            int dY = y - Y;
            // Apply the movement
            foreach (Block b in Blocks)
                b.MoveTo(b.X + dX, b.Y + dY);
            // Update movement
            X = x;
            Y = y;
        }

        public bool CanMoveTo(int x, int y)
        {
            int[] blocksX = Blocks.Select(n => n.X).ToArray();
            int[] blocksY = Blocks.Select(n => n.Y).ToArray();
            foreach (Block b in Blocks)
            {
                int newX = b.X + x - X;
                int newY = b.Y + y - Y;
                if (!Board.IsFreeAt(newX, newY)) // The board is occuped by another block
                {
                    // We check if the block belong to the tetromino
                    bool belongToTetromino = false;
                    for (int i = 0; i < blocksX.Length; i++)
                        if (newX == blocksX[i] && newY == blocksY[i]) belongToTetromino = true;
                    if (!belongToTetromino) return false;
                } 
            }
            return true;
        }

        public void DrawGhost(SpriteBatch spriteBatch, Rectangle boardLocation)
        {
            int blockWidth = boardLocation.Width / Board.Width;
            int blockHeight = boardLocation.Height / Board.VisibleHeight;
            foreach (Block b in Blocks)
            {
                Rectangle blockLocation = new Rectangle(
                    boardLocation.X + b.X * blockWidth,
                    boardLocation.Y + (Board.VisibleHeight - (b.Y + Yghost - Y) - 1) * blockHeight,
                    blockWidth,
                    blockHeight);
                Color transparentWhite = Color.White;
                transparentWhite.A = 0;
                spriteBatch.Draw(b.Texture, blockLocation, transparentWhite);
            }
        }


        // To move in other class ?
        static public Vector2 RotateAboutOrigin(Vector2 point, Vector2 origin, float rotation)
        {
            return Vector2.Transform(point - origin, Matrix.CreateRotationZ(rotation)) + origin;
        }

        // To move in other class ?
        static public Vector2 GetWallKick(int oldRotStatus, int newRotStatus, int rotationAttempt, float rotation, char tetrominoName)
        {
            Vector2 wallKick = new Vector2(0, 0); // Init
            if (tetrominoName == 'I')
            {
                switch (rotationAttempt)
                {
                    case 1:
                        break;
                    case 2:
                        if      ((oldRotStatus == 0 && newRotStatus == 1) || (oldRotStatus == 3 && newRotStatus == 2)) wallKick = new Vector2(-2, 0);
                        else if ((oldRotStatus == 1 && newRotStatus == 0) || (oldRotStatus == 2 && newRotStatus == 3)) wallKick = new Vector2(2, 0);
                        else if ((oldRotStatus == 1 && newRotStatus == 2) || (oldRotStatus == 0 && newRotStatus == 3)) wallKick = new Vector2(-1, 0);
                        else if ((oldRotStatus == 2 && newRotStatus == 1) || (oldRotStatus == 3 && newRotStatus == 0)) wallKick = new Vector2(1, 0);
                        break;
                    case 3:
                        if      ((oldRotStatus == 0 && newRotStatus == 1) || (oldRotStatus == 3 && newRotStatus == 2)) wallKick = new Vector2(1, 0);
                        else if ((oldRotStatus == 1 && newRotStatus == 0) || (oldRotStatus == 2 && newRotStatus == 3)) wallKick = new Vector2(-1, 0);
                        else if ((oldRotStatus == 1 && newRotStatus == 2) || (oldRotStatus == 0 && newRotStatus == 3)) wallKick = new Vector2(2, 0);
                        else if ((oldRotStatus == 2 && newRotStatus == 1) || (oldRotStatus == 3 && newRotStatus == 0)) wallKick = new Vector2(-2, 0);
                        break;
                    case 4:
                        if      ((oldRotStatus == 0 && newRotStatus == 1) || (oldRotStatus == 3 && newRotStatus == 2)) wallKick = new Vector2(-2, -1);
                        else if ((oldRotStatus == 1 && newRotStatus == 0) || (oldRotStatus == 2 && newRotStatus == 3)) wallKick = new Vector2(2, -1);
                        else if ((oldRotStatus == 1 && newRotStatus == 2) || (oldRotStatus == 0 && newRotStatus == 3)) wallKick = new Vector2(-1, 2);
                        else if ((oldRotStatus == 2 && newRotStatus == 1) || (oldRotStatus == 3 && newRotStatus == 0)) wallKick = new Vector2(1, -2);
                        break;
                    case 5:
                        if      ((oldRotStatus == 0 && newRotStatus == 1) || (oldRotStatus == 3 && newRotStatus == 2)) wallKick = new Vector2(1, 2);
                        else if ((oldRotStatus == 1 && newRotStatus == 0) || (oldRotStatus == 2 && newRotStatus == 3)) wallKick = new Vector2(-1, -2);
                        else if ((oldRotStatus == 1 && newRotStatus == 2) || (oldRotStatus == 0 && newRotStatus == 3)) wallKick = new Vector2(2, -1);
                        else if ((oldRotStatus == 2 && newRotStatus == 1) || (oldRotStatus == 3 && newRotStatus == 0)) wallKick = new Vector2(-2, 1);
                        break;
                }
            }
            else // for the other tetromino
            {
                switch (rotationAttempt)
                {
                    case 1:
                        break;
                    case 2: // ok
                        if (rotation < 0) wallKick = new Vector2(-1, 0);
                        else wallKick = new Vector2(1, 0);
                        break;
                    case 3:
                        if ((oldRotStatus == 0 && newRotStatus == 1) || (oldRotStatus == 2 && newRotStatus == 1)) wallKick = new Vector2(-1, 1);
                        else if ((oldRotStatus == 1 && newRotStatus == 0) || (oldRotStatus == 1 && newRotStatus == 2)) wallKick = new Vector2(1, -1);
                        else if ((oldRotStatus == 2 && newRotStatus == 3) || (oldRotStatus == 3 && newRotStatus == 0)) wallKick = new Vector2(1, 1);
                        else if ((oldRotStatus == 3 && newRotStatus == 2) || (oldRotStatus == 0 && newRotStatus == 3)) wallKick = new Vector2(-1, -1);
                        break;
                    case 4: // ok
                        if (rotation < 0) wallKick = new Vector2(0, -2);
                        else wallKick = new Vector2(0, 2);
                        break;
                    case 5:
                        if      ((oldRotStatus == 0 && newRotStatus == 1) || (oldRotStatus == 2 && newRotStatus == 1)) wallKick = new Vector2(-1, -2);
                        else if ((oldRotStatus == 1 && newRotStatus == 0) || (oldRotStatus == 1 && newRotStatus == 2)) wallKick = new Vector2(1, 2);
                        else if ((oldRotStatus == 2 && newRotStatus == 3) || (oldRotStatus == 3 && newRotStatus == 0)) wallKick = new Vector2(1, -2);
                        else if ((oldRotStatus == 3 && newRotStatus == 2) || (oldRotStatus == 0 && newRotStatus == 3)) wallKick = new Vector2(-1, 2);
                        break;
                }
            }
            return wallKick;
        }
    }
}
