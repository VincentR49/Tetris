using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tetris
{
    public class Board
    {
        public int Height { set; get; }
        public int Width { set; get; }
        public List<Block> Blocks { set; get; }
        private readonly int yOffset = 2;
        public int VisibleHeight => Height - yOffset;
        public Board(int height, int width)
        {
            Height = height;
            Width = width;
            Blocks = new List<Block>();
        }

        // Display the game board
        public void Draw(SpriteBatch spriteBatch, Rectangle boardLocation, Texture2D texture1px)
        {
            // Draw the blocks
            int blockWidth = boardLocation.Width / Width;
            int blockHeight = boardLocation.Height / VisibleHeight;
            foreach (Block b in Blocks)
            {
                if (b.Y >= VisibleHeight)
                    continue;
                // draw the block
                int x = boardLocation.X + b.X * blockWidth;
                int y = boardLocation.Y + (VisibleHeight - b.Y - 1) * blockHeight;
                Rectangle blockLocation = new Rectangle(x, y, blockWidth, blockHeight);
                b.Draw(spriteBatch, blockLocation);
            }
            // Draw the grid
            for (int x = 0; x <= Width; x++)
            {
                Rectangle rectangle = new Rectangle(boardLocation.X + x * blockWidth, boardLocation.Y, 1, blockHeight * VisibleHeight);
                spriteBatch.Draw(texture1px, rectangle, Color.Black);
            }
            for (int y = 0; y <= VisibleHeight; y++)
            {
                Rectangle rectangle = new Rectangle(boardLocation.X, boardLocation.Y + y * blockHeight, blockWidth * Width, 1);
                spriteBatch.Draw(texture1px, rectangle, Color.Black);
            }
        }

        // Check if the position is free
        public bool IsFreeAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return false;
            else
            { 
                foreach (Block b in Blocks)
                    if (b.X == x && b.Y == y) return false;
            }
            return true;
        }

        // Clear the given raw
        public void ClearRow (int y)
        {
            Blocks?.RemoveAll(b => b.Y == y);
            // All the block over the row move down by 1
            Blocks.FindAll(b => b.Y > y)?.ForEach(b => b.MoveTo(b.X, b.Y - 1));
        }

        // Reset the board
        // Remove all the blocks
        public void Reset()
        {
            Blocks.Clear();
            Blocks = new List<Block>();
        }


        // Return the number of row cleaned
        public int ClearRow()
        {
            int rowCleared = 0;
            for (int y = 0; y < Height; y++)
            {
                if (GetNumberOfBlockAtRow(y) >= Width)
                {
                    ClearRow(y);
                    y--;
                    rowCleared++;
                }
            }
            return rowCleared;
        }

        public int GetNumberOfBlockAtRow(int y)
        {
            if (Blocks == null)
                return 0;
            else
                return Blocks.FindAll(b => b.Y == y).Count;
        }

    }
}
