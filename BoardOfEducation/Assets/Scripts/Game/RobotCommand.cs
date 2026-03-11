namespace BoardOfEducation.Game
{
    public enum RobotCommand { Forward, TurnLeft, TurnRight, Jump }

    public enum Direction { Up, Right, Down, Left }

    public static class CommandMapping
    {
        /// <summary>
        /// Maps Arcade glyph IDs to robot commands.
        /// Robot Yellow(0)=Forward, Purple(1)=TurnLeft, Orange(2)=TurnRight, Pink(3)=Jump.
        /// </summary>
        public static bool TryGetCommand(int glyphId, out RobotCommand command)
        {
            // Only glyph 0 (Robot Yellow) is used — always maps to Forward
            if (glyphId == 0)
            {
                command = RobotCommand.Forward;
                return true;
            }
            command = default;
            return false;
        }

        public static string GetCommandName(RobotCommand cmd)
        {
            switch (cmd)
            {
                case RobotCommand.Forward: return "Forward";
                case RobotCommand.TurnLeft: return "Turn Left";
                case RobotCommand.TurnRight: return "Turn Right";
                case RobotCommand.Jump: return "Jump";
                default: return "?";
            }
        }

        public static Direction TurnLeft(Direction dir)
        {
            return (Direction)(((int)dir + 3) % 4);
        }

        public static Direction TurnRight(Direction dir)
        {
            return (Direction)(((int)dir + 1) % 4);
        }

        public static UnityEngine.Vector2Int DirectionToOffset(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return new UnityEngine.Vector2Int(0, -1);    // row decreases
                case Direction.Right: return new UnityEngine.Vector2Int(1, 0);  // col increases
                case Direction.Down: return new UnityEngine.Vector2Int(0, 1);   // row increases
                case Direction.Left: return new UnityEngine.Vector2Int(-1, 0);  // col decreases
                default: return UnityEngine.Vector2Int.zero;
            }
        }
    }
}
