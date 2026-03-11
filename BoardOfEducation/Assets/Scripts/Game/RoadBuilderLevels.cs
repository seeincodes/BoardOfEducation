using UnityEngine;

namespace BoardOfEducation.Game
{
    /// <summary>
    /// Static level definitions for Robot Road Builder.
    /// 5 levels from 1 piece to 5 pieces.
    /// </summary>
    public static class RoadBuilderLevels
    {
        public static int Count => 5;

        public static GridData GetLevel(int index)
        {
            switch (index)
            {
                case 0: return Level1();
                case 1: return Level2();
                case 2: return Level3();
                case 3: return Level4();
                case 4: return Level5();
                default: return Level1();
            }
        }

        // Level 1: Straight line — 2 Forward
        // [S] [ ] [G]
        private static GridData Level1()
        {
            var cells = new CellType[1, 3];
            cells[0, 0] = CellType.Start;
            cells[0, 1] = CellType.Empty;
            cells[0, 2] = CellType.Goal;
            return new GridData("Go Forward!", 1, 3, cells,
                new Vector2Int(0, 0), Direction.Right, new Vector2Int(2, 0), 2,
                "Place 2 Forward pieces to reach the goal.",
                new[] { 0 }); // Yellow=Forward only
        }

        // Level 2: Right turn — Forward + TurnRight + Forward
        // [S] [ ]
        //     [G]
        private static GridData Level2()
        {
            var cells = new CellType[2, 2];
            cells[0, 0] = CellType.Start;
            cells[0, 1] = CellType.Empty;
            cells[1, 0] = CellType.Blocked;
            cells[1, 1] = CellType.Goal;
            return new GridData("Turn Right!", 2, 2, cells,
                new Vector2Int(0, 0), Direction.Right, new Vector2Int(1, 1), 3,
                "Use Forward and Turn Right to navigate the L-shape!",
                new[] { 0, 2 }); // Yellow=Forward, Orange=TurnRight
        }

        // Level 3: L-shape path — 3 commands
        // [S] [ ] [ ]
        //         [G]
        private static GridData Level3()
        {
            var cells = new CellType[2, 3];
            cells[0, 0] = CellType.Start;
            cells[0, 1] = CellType.Empty;
            cells[0, 2] = CellType.Empty;
            cells[1, 0] = CellType.Blocked;
            cells[1, 1] = CellType.Blocked;
            cells[1, 2] = CellType.Goal;
            return new GridData("Around the Corner!", 2, 3, cells,
                new Vector2Int(0, 0), Direction.Right, new Vector2Int(2, 1), 4,
                "Go forward, turn the corner, and keep going!",
                new[] { 0, 2 }); // Yellow=Forward, Orange=TurnRight
        }

        // Level 4: Gap crossing — includes Jump
        // [S] [ ] [gap] [G]
        private static GridData Level4()
        {
            var cells = new CellType[1, 4];
            cells[0, 0] = CellType.Start;
            cells[0, 1] = CellType.Empty;
            cells[0, 2] = CellType.Gap;
            cells[0, 3] = CellType.Goal;
            return new GridData("Jump the Gap!", 1, 4, cells,
                new Vector2Int(0, 0), Direction.Right, new Vector2Int(3, 0), 3,
                "Use Jump to leap over the gap in the road!",
                new[] { 0, 3 }); // Yellow=Forward, Pink=Jump
        }

        // Level 5: Complex path with turns and jump
        // [S] [ ] [  ] [ ]
        // [X] [X] [gap][ ]
        // [ ] [ ] [  ] [G]
        private static GridData Level5()
        {
            var cells = new CellType[3, 4];
            // Row 0
            cells[0, 0] = CellType.Start;
            cells[0, 1] = CellType.Empty;
            cells[0, 2] = CellType.Empty;
            cells[0, 3] = CellType.Empty;
            // Row 1
            cells[1, 0] = CellType.Blocked;
            cells[1, 1] = CellType.Blocked;
            cells[1, 2] = CellType.Gap;
            cells[1, 3] = CellType.Empty;
            // Row 2
            cells[2, 0] = CellType.Empty;
            cells[2, 1] = CellType.Empty;
            cells[2, 2] = CellType.Empty;
            cells[2, 3] = CellType.Goal;
            return new GridData("Big Adventure!", 3, 4, cells,
                new Vector2Int(0, 0), Direction.Right, new Vector2Int(3, 2), 5,
                "Combine Forward, Turn, and Jump to reach the goal!",
                new[] { 0, 2, 3 }); // Yellow=Forward, Orange=TurnRight, Pink=Jump
        }
    }
}
