﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _3D2048.Util;
using System.Diagnostics;
using _3D2048.Properties;

namespace _3D2048.Logic
{
    class GameLogic
    {
        public GameState gameModel { get; private set; }
        public Random random;
        public GameLogic()
        {
            random = new Random();
            reset();
        }

        public Direction getMoveDependentDirection(Direction direction, Camera gameCamera)
        {
            Direction outputDirection = direction;

            switch (gameCamera.getFrontFace())
            {
                case CubeFace.FRONT:
                    // Directions don't need to be changed
                    outputDirection = direction;
                    break;
                case CubeFace.LEFT:
                    if (direction == Direction.Forward)
                    {
                        outputDirection = Direction.Left;
                    }
                    else if (direction == Direction.Back)
                    {
                        outputDirection = Direction.Right;
                    }
                    else if (direction == Direction.Right)
                    {
                        outputDirection = Direction.Forward;
                    }
                    else if (direction == Direction.Left)
                    {
                        outputDirection = Direction.Back;
                    }
                    else
                    {
                        outputDirection = direction; //Up/Down: No change
                    }
                    break;
                case CubeFace.BACK:
                    if (direction == Direction.Forward)
                    {
                        outputDirection = Direction.Back;
                    }
                    else if (direction == Direction.Back)
                    {
                        outputDirection = Direction.Forward;
                    }
                    else if (direction == Direction.Right)
                    {
                        outputDirection = Direction.Left;
                    }
                    else if (direction == Direction.Left)
                    {
                        outputDirection = Direction.Right;
                    }
                    else
                    {
                        outputDirection = direction; //Up/Down: No change
                    }
                    break;
                case CubeFace.RIGHT:
                    if (direction == Direction.Forward)
                    {
                        outputDirection = Direction.Right;
                    }
                    else if (direction == Direction.Back)
                    {
                        outputDirection = Direction.Left;
                    }
                    else if (direction == Direction.Right)
                    {
                        outputDirection = Direction.Back;
                    }
                    else if (direction == Direction.Left)
                    {
                        outputDirection = Direction.Forward;
                    }
                    else
                    {
                        outputDirection = direction; //Up/Down: No change
                    }
                    break;
                default:
                    outputDirection = direction;
                    break;
            }

            return outputDirection;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="axis">The axis along to collapse. Must be 0, 1 or 2 (for x, y, and z)</param>
        /// <param name="stepDir">Either +1 or -1 for the direction in which to collapse</param>
        /// <param name="simulate"> If false, the field isn't modified.</param>
        /// <returns>Returns true if a modification has been made.</returns>
        private bool collapse(int axis, int stepDir, bool simulate)
        {
            bool modified = false;
            int[] pos = new int[3];
            int[] nextPos = new int[3];

            // enumerate all stacks to collapse
            for (int i = 0; i < gameModel.gameSize; i++)
            {
                for (int j = 0; j < gameModel.gameSize; j++)
                {
                    bool stackModified;
                    do
                    {
                        stackModified = false;

                        pos[axis] = stepDir > 0 ? gameModel.gameSize - 1 : 0;
                        pos[(axis + 1) % 3] = i;
                        pos[(axis + 2) % 3] = j;

                        // for each stack element
                        for (int l = 0; l < gameModel.gameSize - 1; l++)
                        {
                            Array.Copy(pos, nextPos, 3);
                            nextPos[axis] -= stepDir;

                            if (gameModel.field[pos[0], pos[1], pos[2]] == 0)
                            {
                                if (gameModel.field[nextPos[0], nextPos[1], nextPos[2]] != 0)
                                {
                                    // move number from next to pos

                                    if (!simulate)
                                    {
                                        gameModel.field[pos[0], pos[1], pos[2]] = gameModel.field[nextPos[0], nextPos[1], nextPos[2]];
                                        gameModel.field[nextPos[0], nextPos[1], nextPos[2]] = 0;
                                        stackModified = true;
                                    }
                                    else
                                        return true;
                                    
                                }
                            }
                            else if (gameModel.field[pos[0], pos[1], pos[2]] == gameModel.field[nextPos[0], nextPos[1], nextPos[2]]) // previous condition ensures that pos and nextPos are not 0
                            {
                                Debug.Assert(gameModel.field[pos[0], pos[1], pos[2]] != 0);

                                // combine numbers into pos
                                if (!simulate)
                                {
                                    gameModel.field[pos[0], pos[1], pos[2]] *= 2;
                                    gameModel.score += gameModel.field[pos[0], pos[1], pos[2]];
                                    gameModel.field[nextPos[0], nextPos[1], nextPos[2]] = 0;
                                    stackModified = true;
                                }
                                else
                                    return true;
                            }

                            Array.Copy(nextPos, pos, 3);
                        }
                    }
                    while (stackModified);

                    if (stackModified == true)
                    {
                        modified = true;
                    }

                }
            }
            return modified;
        }

        public bool collapse(Direction direction, bool simulate = false)
        {
            switch (direction)
            {
                case Logic.Direction.Right:   return collapse(0, +1, simulate);
                case Logic.Direction.Left:    return collapse(0, -1, simulate);
                case Logic.Direction.Up:      return collapse(1, +1, simulate);
                case Logic.Direction.Down:    return collapse(1, -1, simulate);
                case Logic.Direction.Back:    return collapse(2, -1, simulate);
                case Logic.Direction.Forward: return collapse(2, +1, simulate);
                default: return false;
            }

        }
        public void Move(Direction direction)
        {
            collapse(direction);
       
            // loss/win and count zeros
            int nullCounter = 0;

            for (int l = 0; l < gameModel.gameSize; l++)
            {
                for (int j = 0; j < gameModel.gameSize; j++)
                {
                    for (int i = 0; i < gameModel.gameSize; i++)
                    {
                        if (gameModel.field[i, j, l] == 0)
                        {
                            nullCounter++;
                        }
                        else if (gameModel.field[i, j, l] == 2048)
                        {
                            gameModel.won = true;
                        }
                    }
                }
            }
           
            // randomly insert a new 2
            int randomNull = random.Next(0, nullCounter);
            nullCounter = 0;
            bool nullSet = false;
            for (int l = 0; l < gameModel.gameSize && nullSet == false; l++)
            {
                for (int j = 0; j < gameModel.gameSize && nullSet == false; j++)
                {
                    for (int i = 0; i < gameModel.gameSize && nullSet == false; i++)
                    {
                        if (gameModel.field[i, j, l] == 0)
                        {

                            if (randomNull == nullCounter)
                            {
                                nullSet = true;
                                Debug.Assert(gameModel.field[i, j, l] == 0);
                                gameModel.field[i, j, l] = 2;
                            }
                            nullCounter++;
                        }
                    }
                }
            }

            //simulate all possible next moves
            gameModel.lost = true;
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                if (collapse(dir, true) == true)
                {
                    gameModel.lost = false;
                }
            }
        }

        public void reset()
        {
            gameModel = new GameState(Settings.Default.gameSize);
        }

        public void pause()
        {
            gameModel.pause = true;
        }

        public void resume()
        {
            gameModel.pause = false;
        }
    }
}
