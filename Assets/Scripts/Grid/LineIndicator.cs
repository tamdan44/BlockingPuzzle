using System.Collections.Generic;
using UnityEngine;

public class LineIndicator : MonoBehaviour
{
    public List<int[]> line_data = new List<int[]>
    {
        new int[] {  0,  1,  2,  3,  4,  5,  6,  7,  8 },
        new int[] {  9, 10, 11, 12, 13, 14, 15, 16, 17 },
        new int[] { 18, 19, 20, 21, 22, 23, 24, 25, 26 },

        new int[] { 27, 28, 29, 30, 31, 32, 33, 34, 35 },
        new int[] { 36, 37, 38, 39, 40, 41, 42, 43, 44 },
        new int[] { 45, 46, 47, 48, 49, 50, 51, 52, 53 },

        new int[] { 54, 55, 56, 57, 58, 59, 60, 61, 62 },
        new int[] { 63, 64, 65, 66, 67, 68, 69, 70, 71 },
        new int[] { 72, 73, 74, 75, 76, 77, 78, 79, 80 }
    };


    public List<int[]> square_data = new List<int[]>
    {
        new int[] {  0,  1,  2,  9, 10, 11, 18, 19, 20 }, // Top-left square
        new int[] {  3,  4,  5, 12, 13, 14, 21, 22, 23 }, // Top-middle square
        new int[] {  6,  7,  8, 15, 16, 17, 24, 25, 26 }, // Top-right square

        new int[] { 27, 28, 29, 36, 37, 38, 45, 46, 47 }, // Middle-left square
        new int[] { 30, 31, 32, 39, 40, 41, 48, 49, 50 }, // Center square
        new int[] { 33, 34, 35, 42, 43, 44, 51, 52, 53 }, // Middle-right square

        new int[] { 54, 55, 56, 63, 64, 65, 72, 73, 74 }, // Bottom-left square
        new int[] { 57, 58, 59, 66, 67, 68, 75, 76, 77 }, // Bottom-middle square
        new int[] { 60, 61, 62, 69, 70, 71, 78, 79, 80 }  // Bottom-right square
    };

    [HideInInspector] public int[] columnIndexes = new int[9] {  0,  1,  2,     3,  4,  5,      6,  7,  8 };

    private (int, int) GetGridSquarePos(int square_index)
    {
        int pos_row = -1;
        int pos_col = -1;

        if(square_index<=80&&square_index>=0){
            pos_row = square_index/9;
            pos_col = square_index%9;
        }
        
        return (pos_row, pos_col);
    }

    public int[] GetVerticalLine(int square_index)
    {
        int[] line = new int[9];
        int col = GetGridSquarePos(square_index).Item2;

        for(int i=0; i<9; i++) {
            line[i] = col;
            col+=9;
        }

        return line;
    }

    public int GetGridSquareIndex(int square_index)
    {
        for(int row=0; row<9; row++)
        {
            for(int col=0; col<9; col++)
            {
                if(square_data[row][col]==square_index)
                {
                    return row;
                }
            }
        }
        return -1;
    }

}
