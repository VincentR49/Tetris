using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tetris
{
    public class Block
    {
        public Texture2D Texture { set; get; }
        private Board Board { set; get; }
        public int X { set; get; }
        public int Y { set; get; }
        
        public Block(Board board, int x, int y, Texture2D texture)
        {
            if (!board.IsFreeAt(x, y))
                Console.Error.WriteLine("Placement impossible: {0}, {1}", x, y);
            Board = board;
            X = x;
            Y = y;
            Texture = texture;
            board.Blocks.Add(this); // add the block in the board
        }

        public void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle location)
        {
            spriteBatch.Draw(Texture, location, Color.White);
        }

    }
}
