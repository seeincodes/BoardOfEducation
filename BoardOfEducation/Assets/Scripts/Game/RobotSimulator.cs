using System.Collections.Generic;
using UnityEngine;

namespace BoardOfEducation.Game
{
    public struct StepResult
    {
        public Vector2Int Position;
        public Direction Facing;
        public bool Success;   // true = valid move
        public bool Jumped;    // true = crossed a gap
        public bool ReachedGoal;
    }

    /// <summary>
    /// Pure static logic: runs a sequence of commands on a grid
    /// and returns step-by-step results for animation.
    /// </summary>
    public static class RobotSimulator
    {
        public static List<StepResult> Run(GridData grid, RobotCommand[] commands)
        {
            var steps = new List<StepResult>();
            var pos = grid.StartPos;
            var dir = grid.StartDir;

            // Initial position
            steps.Add(new StepResult
            {
                Position = pos,
                Facing = dir,
                Success = true,
                Jumped = false,
                ReachedGoal = pos == grid.GoalPos
            });

            foreach (var cmd in commands)
            {
                switch (cmd)
                {
                    case RobotCommand.TurnLeft:
                        dir = CommandMapping.TurnLeft(dir);
                        steps.Add(new StepResult
                        {
                            Position = pos,
                            Facing = dir,
                            Success = true,
                            ReachedGoal = pos == grid.GoalPos
                        });
                        break;

                    case RobotCommand.TurnRight:
                        dir = CommandMapping.TurnRight(dir);
                        steps.Add(new StepResult
                        {
                            Position = pos,
                            Facing = dir,
                            Success = true,
                            ReachedGoal = pos == grid.GoalPos
                        });
                        break;

                    case RobotCommand.Forward:
                    {
                        var offset = CommandMapping.DirectionToOffset(dir);
                        var next = pos + offset;
                        var cell = grid.GetCell(next);

                        if (cell == CellType.Blocked || !grid.InBounds(next))
                        {
                            steps.Add(new StepResult
                            {
                                Position = pos,
                                Facing = dir,
                                Success = false,
                                ReachedGoal = false
                            });
                            return steps; // hit wall, stop
                        }

                        if (cell == CellType.Gap)
                        {
                            steps.Add(new StepResult
                            {
                                Position = next,
                                Facing = dir,
                                Success = false,
                                ReachedGoal = false
                            });
                            return steps; // fell in gap, stop
                        }

                        pos = next;
                        steps.Add(new StepResult
                        {
                            Position = pos,
                            Facing = dir,
                            Success = true,
                            ReachedGoal = pos == grid.GoalPos
                        });
                        break;
                    }

                    case RobotCommand.Jump:
                    {
                        var offset = CommandMapping.DirectionToOffset(dir);
                        var over = pos + offset;       // cell being jumped over
                        var landing = pos + offset * 2; // landing cell

                        if (!grid.InBounds(landing))
                        {
                            steps.Add(new StepResult
                            {
                                Position = pos,
                                Facing = dir,
                                Success = false,
                                ReachedGoal = false
                            });
                            return steps;
                        }

                        var landingCell = grid.GetCell(landing);
                        if (landingCell == CellType.Blocked || landingCell == CellType.Gap)
                        {
                            steps.Add(new StepResult
                            {
                                Position = landing,
                                Facing = dir,
                                Success = false,
                                ReachedGoal = false
                            });
                            return steps;
                        }

                        pos = landing;
                        steps.Add(new StepResult
                        {
                            Position = pos,
                            Facing = dir,
                            Success = true,
                            Jumped = true,
                            ReachedGoal = pos == grid.GoalPos
                        });
                        break;
                    }
                }

                // Stop early if we reached the goal
                if (steps[steps.Count - 1].ReachedGoal)
                    return steps;
            }

            return steps;
        }
    }
}
