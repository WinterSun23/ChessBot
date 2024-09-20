using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitBoardGen
{
    // a blocker represents that rook can move to a square before 
    //the blocker in that direction for each of the four directions
    public static ulong[] RookBlockerBitBoards = new ulong[64];
    public static Hashtable RookMoves = new Hashtable();

    public BitBoardGen()
    {
        GenerateBlockerBitBoard();
        GeneratePossibleRookMoves();
    }

    void GenerateBlockerBitBoard()
    {
        for (int i = 0; i < 64; i++)
        {
            ulong moves = 0UL;
            int row = i / 8;
            int col = i % 8;

            for (int j = 0; j < 8; j++)
            {
                moves |= 1UL << (j * 8 + col);
                moves |= 1UL << (row * 8 + j);
            }
            moves ^= 1UL << i;

            RookBlockerBitBoards[i] = moves;
        }
    }

    void GeneratePossibleRookMoves()
    {
        for (int i = 0; i < 64; i++)
        {
            ulong moves = 0UL;
            int row = i / 8;
            int col = i % 8;

            for (int j = 0; j < 8; j++)
            {
                moves |= 1UL << (j * 8 + col);
                moves |= 1UL << (row * 8 + j);
            }
            moves ^= 1UL << i;

            RookMoves.Add(i, moves);
        }
    }
}
