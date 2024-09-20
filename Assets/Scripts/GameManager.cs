using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    const int NUM_SQUARES = 8*8;


    [Header("Board Attributes")]
    public GameObject Board_;
    public GameObject square;
    public Color blackCol;
    public Color whiteCol;
    public Color possibleMoveCol;
    public GameObject piece;
    public Sprite[] pieceSprites = new Sprite[12]; //white first then black
    GameObject selectedPiece = null;
    private Dictionary<string,GameObject> pieceObjects = new ();


    private GameObject[] boardSquares = new GameObject[NUM_SQUARES];

    
    public static Dictionary<string, int> pieceStr_to_piece = new (){
        {"p", 0 | 8},
        {"r", 1 | 8},
        {"n", 2 | 8},
        {"b", 3 | 8},
        {"q", 4 | 8},
        {"k", 5 | 8},
        {"P", 0 | 16},
        {"R", 1 | 16},
        {"N", 2 | 16},
        {"B", 3 | 16},
        {"Q", 4 | 16},
        {"K", 5 | 16}
    };
    Board board;

    int curCol = 16;

    public string fenStringInp;
    void Start()
    {
        GenChessBoard();
        board = new Board();
        board.LoadFEN(fenStringInp);
        // pieces = new Pieces(piece);

        foreach (KeyValuePair<string, PieceAbs> pieceDict in board.pieces){
            GameObject pieceObj = Instantiate(piece);
            pieceObj.name = pieceDict.Key;
            pieceObj.transform.position = new Vector3(pieceDict.Value.Position % 8 - 4, pieceDict.Value.Position / 8 - 4, -0.5f);
            bool isWhite = (pieceDict.Value.Type & 16) == 16;

            switch(pieceDict.Value.Type & 7){
                case 0:
                    pieceObj.GetComponent<SpriteRenderer>().sprite = pieceSprites[ isWhite? 0 : 6];
                    break;
                case 1:
                    pieceObj.GetComponent<SpriteRenderer>().sprite = pieceSprites[ isWhite? 1 : 7];
                    break;
                case 2:
                    pieceObj.GetComponent<SpriteRenderer>().sprite = pieceSprites[ isWhite? 2 : 8];
                    break;
                case 3:
                    pieceObj.GetComponent<SpriteRenderer>().sprite = pieceSprites[ isWhite? 3 : 9];
                    break;
                case 4:
                    pieceObj.GetComponent<SpriteRenderer>().sprite = pieceSprites[ isWhite? 4 : 10];
                    break;
                case 5:
                    pieceObj.GetComponent<SpriteRenderer>().sprite = pieceSprites[ isWhite? 5 : 11];
                    break;
            }
            pieceObjects.Add(pieceDict.Key,pieceObj);
        }
    }

    void printDetails(){
        Debug.Log("All"+System.Convert.ToString((long)board.bitBoardsAll, 2).PadLeft(64, '0'));
        Debug.Log("White"+System.Convert.ToString((long)board.bitBoardsWhite, 2).PadLeft(64, '0'));
        Debug.Log("Black"+System.Convert.ToString((long)board.bitBoardsBlack, 2).PadLeft(64, '0'));
        Debug.Log("Pawns"+System.Convert.ToString((long)board.bitBoardsPawns, 2).PadLeft(64, '0'));
        Debug.Log("Knights"+System.Convert.ToString((long)board.bitBoardsKnights, 2).PadLeft(64, '0'));
        Debug.Log("Bishops"+System.Convert.ToString((long)board.bitBoardsBishops, 2).PadLeft(64, '0'));
        Debug.Log("Rooks"+System.Convert.ToString((long)board.bitBoardsRooks, 2).PadLeft(64, '0'));
        Debug.Log("Queens"+System.Convert.ToString((long)board.bitBoardsQueens, 2).PadLeft(64, '0'));
        Debug.Log("Kings"+System.Convert.ToString((long)board.bitBoardsKings, 2).PadLeft(64, '0'));
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            SelectPiece();
        }
        DisplayChessBoard(selectedPiece!=null,false);
    }

    void GenChessBoard(){
        Board_.transform.position = new Vector3(-0.5f, -0.5f, 0);
        for(int i=0; i<NUM_SQUARES;i++){
            GameObject square = GameObject.Instantiate(this.square);
            square.transform.position = new Vector3(i%8 - 4, i/8 - 4, 0);
            square.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            boardSquares[i] = square;
        }
    }

    //flags - for showing prevMove, possible moves
    private void DisplayChessBoard(bool showPossible, bool showPrevMove){
        ulong moves = 0UL;
        ulong moves1 = 0UL;
        if (showPossible){
            moves = board.pieces[selectedPiece.name].GenerateMoves();
            moves1 = board.pieces[selectedPiece.name].GenerateMoves();
            for (int i = 0; i < 64; i++)
            {
                if ((moves & (1UL << i)) != 0)
                {
                    Move move = board.pieces[selectedPiece.name].ReturnMove(i);
                    if(!board.CheckIfLegalMove(move)){
                        moves ^= 1UL << i;
                    }
                }
            }
            //print in binary
            // Debug.Log(System.Convert.ToString((long)moves, 2).PadLeft(64, '0'));
        }
        for(int i=0; i<NUM_SQUARES;i++){
            // Make showing prevMove
            if ((moves & (1UL  << i)) != 0)
                boardSquares[i].GetComponent<Renderer>().material.color = possibleMoveCol;
            else if ((moves1 & (1UL  << i)) != 0)
                boardSquares[i].GetComponent<Renderer>().material.color = Color.blue;
            else
                boardSquares[i].GetComponent<Renderer>().material.color = (i/8 + i%8) % 2 == 0 ? whiteCol : blackCol;
        }
    }
    
    void SelectPiece(){
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100)){
            if(selectedPiece != null){
                
                if(!hit.collider.gameObject.name.Contains("Board")){
                    if(curCol == (board.pieces[hit.collider.gameObject.name].Type&24)){
                        if(selectedPiece.name == hit.collider.gameObject.name){
                            selectedPiece = null;
                            Debug.Log("same so null");
                        }
                        else{
                            selectedPiece = hit.collider.gameObject;
                            Debug.Log("Exchange");
                        }
                        
                    }
                    else{
                        int to = (int)(Mathf.Floor(hit.point.x + 4 + 0.5f) + Mathf.Floor(hit.point.y + 4 + 0.5f) * 8);

                        int moved = board.MakeMove(selectedPiece.name, to);
                        if(moved!=0){
                            pieceObjects[selectedPiece.name].transform.position = new Vector3(to % 8 - 4, to / 8 - 4, -0.5f);
                            curCol^=24;
                            selectedPiece = null;

                            if(moved == 2){
                                pieceObjects.Remove(hit.collider.gameObject.name);
                                Destroy(hit.collider.gameObject);
                            }

                            Debug.Log("Moving, capture");
                        }
                    }
                }
                else{
                    
                    int to = (int)(Mathf.Floor(hit.point.x + 4) + Mathf.Floor(hit.point.y + 4) * 8);
            
                    int moved = board.MakeMove(selectedPiece.name, to);
                    if(moved!=0){
                        pieceObjects[selectedPiece.name].transform.position = new Vector3(to % 8 - 4, to / 8 - 4, -0.5f);
                        curCol^=24;
                        selectedPiece = null;
                        Debug.Log("Moving, no capture");
                    }
                }
                Debug.Log((selectedPiece!=null ? selectedPiece.name : "None")+ "  Next is "+(curCol == 16 ? "White" : "Black"));
            }
            else{
                if(!hit.collider.gameObject.name.Contains("Board") && curCol == (board.pieces[hit.collider.gameObject.name].Type&24)){
                    selectedPiece = hit.collider.gameObject;
                    Debug.Log("Reset and selected " + selectedPiece.name);
                }
                
            }
        }
    }

}

public class Board{
    public ulong bitBoardsAll = 0UL;
    public ulong bitBoardsWhite = 0UL;
    public ulong bitBoardsBlack = 0UL;

    public ulong bitBoardsPawns = 0UL;
    public ulong bitBoardsKnights = 0UL;
    public ulong bitBoardsBishops = 0UL;
    public ulong bitBoardsRooks = 0UL;
    public ulong bitBoardsQueens = 0UL;
    public ulong bitBoardsKings = 0UL;

    public Dictionary<int, Move> movesDoneUndo = new Dictionary<int, Move>();
    public Dictionary<int, Move> movesDoneRedo = new Dictionary<int, Move>();

    public Dictionary<string,PieceAbs> pieces = new Dictionary<string, PieceAbs>();

    public Board(){
        
    }

    public void LoadFEN(string fenStr = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"){
        bitBoardsAll = 0UL;
        bitBoardsWhite = 0UL;
        bitBoardsBlack = 0UL;
        bitBoardsPawns = 0UL;
        bitBoardsKnights = 0UL;
        bitBoardsBishops = 0UL;
        bitBoardsRooks = 0UL;
        bitBoardsQueens = 0UL;
        bitBoardsKings = 0UL;

        pieces.Clear();
        movesDoneUndo.Clear();
        movesDoneRedo.Clear();


        Dictionary<string, int> pieceIndex = new (){
            {"p", 0},
            {"r", 0},
            {"n", 0},
            {"b", 0},
            {"q", 0},
            {"k", 0},
            {"P", 0},
            {"R", 0},
            {"N", 0},
            {"B", 0},
            {"Q", 0},
            {"K", 0}
        };
        
        int file = 0,rank = 7;
        foreach(char c in fenStr){
            // Debug.Log(c+" "+file+" "+rank);
            if(c == '/'){
                rank--;
                file = 0;
            }
            else if(c >= '1' && c <= '8'){
                    file += c - '0';
                    file%=8;
            }
            else{
                string cStr = c.ToString();
                // name  - cStr+pieceIndex[cStr].ToString()
                // type - pieceStr_to_piece[cStr]

                PieceAbs piece = null;
                switch(GameManager.pieceStr_to_piece[cStr]&7){
                    case 0:
                        piece = new Pawn(this, file + rank * 8, c<'a');
                        bitBoardsPawns |= 1UL << (file + rank*8);
                        break;
                    case 1:
                        piece = new Rook(this, file + rank * 8, c<'a');
                        bitBoardsRooks |= 1UL << (file + rank*8);
                        break;
                    case 2:
                        piece = new Knight(this, file + rank * 8, c<'a');
                        bitBoardsKnights |= 1UL << (file + rank*8);
                        break;
                    case 3:
                        piece = new Bishop(this, file + rank * 8, c<'a');
                        bitBoardsBishops |= 1UL << (file + rank*8);
                        break;
                    case 4:
                        piece = new Queen(this, file + rank * 8, c<'a');
                        bitBoardsQueens |= 1UL << (file + rank*8);
                        break;
                    case 5:
                        piece = new King(this, file + rank * 8, c<'a');
                        bitBoardsKings |= 1UL << (file + rank*8);
                        break;

                }
                bitBoardsAll |= 1UL << (file + rank*8);
                if(c<'a')
                    bitBoardsWhite |= 1UL << (file + rank*8);
                else
                    bitBoardsBlack |= 1UL << (file + rank*8);

                pieces.Add(cStr+pieceIndex[cStr].ToString(), piece);
                pieceIndex[cStr]++;
                file++;
            }
        }
    }


    public int MakeMove(string pieceName, int targetPos, bool checkLegality = true){
        PieceAbs piece = pieces[pieceName];
        Move move = piece.ReturnMove(targetPos,true);
        if((move!=null) && (!checkLegality || CheckIfLegalMove(move))){

            UpdateBoardByMove(move);
            movesDoneUndo.Add(movesDoneUndo.Count, move);

            return move.isCapture ? 2:1;
        }
        else
            return 0;
    }

    void UpdateBoardByMove(Move move, bool opposite = false){
        PieceAbs piece = move.Piece;
        //assumed that the piece.Position = move.targetPos, so we switch start and target

        if(move.isCapture && !opposite)
            ToggleRemovePiece(move.TargetPos);

        if(opposite){
            (move.TargetPos, move.StartPos) = (move.StartPos, move.TargetPos);
        }
        //Asssumed that the piece has already been toggled off before
        
        switch (piece.Type & 7)
        {
            case 0:bitBoardsPawns^= 1UL << move.StartPos; bitBoardsPawns^= 1UL << move.TargetPos;break;
            case 1:bitBoardsRooks^= 1UL << move.StartPos; bitBoardsRooks^= 1UL << move.TargetPos;break;
            case 2:bitBoardsKnights^= 1UL << move.StartPos; bitBoardsKnights^= 1UL << move.TargetPos;break;
            case 3:bitBoardsBishops^= 1UL << move.StartPos; bitBoardsBishops^= 1UL << move.TargetPos;break;
            case 4:bitBoardsQueens^= 1UL << move.StartPos; bitBoardsQueens^= 1UL << move.TargetPos;break;
            case 5:bitBoardsKings^= 1UL << move.StartPos; bitBoardsKings^= 1UL << move.TargetPos;break;
        }

        bitBoardsAll ^= 1UL << move.StartPos;
        bitBoardsAll ^= 1UL << move.TargetPos;
        if(piece.IsWhite) { bitBoardsWhite ^= 1UL << move.StartPos; bitBoardsWhite ^= 1UL << move.TargetPos;}
        else { bitBoardsBlack ^= 1UL << move.StartPos; bitBoardsBlack ^= 1UL << move.TargetPos; }
        piece.Position = move.TargetPos;
        
        if(opposite){
            (move.TargetPos, move.StartPos) = (move.StartPos, move.TargetPos);
        }
        if(move.isCapture && opposite)
            ToggleRemovePiece(move.TargetPos);
    }

    
    public ulong GenerateAllPossibleMoves(int type){
        ulong moves = 0UL;
        bool isWhite = (type & 16) == 16;
        
        foreach(KeyValuePair<string, PieceAbs> piece in pieces){
            if(piece.Value.IsWhite == isWhite){
                moves |= piece.Value.GenerateMoves();
            }
        }

        return moves;
    }

    public bool CheckIfLegalMove(Move move){
        if(move == null)
            return false;
        UpdateBoardByMove(move);

        ulong moves = GenerateAllPossibleMoves(move.Piece.Type^24);
        bool isCheck = (moves & bitBoardsKings) != 0;

        UpdateBoardByMove(move,true);

        return !isCheck;
    }

    public void ToggleRemovePiece(int pos){
        foreach(KeyValuePair<string, PieceAbs> piece in pieces){
            if(piece.Value.Position == pos){
                bitBoardsAll ^= 1UL << pos;
                if(piece.Value.IsWhite)
                    bitBoardsWhite ^= 1UL << pos;
                else
                    bitBoardsBlack ^= 1UL << pos;
                switch(piece.Value.Type & 7){
                    case 0:
                        bitBoardsPawns ^= 1UL << pos;
                        break;
                    case 1:
                        bitBoardsRooks ^= 1UL << pos;
                        break;
                    case 2:
                        bitBoardsKnights ^= 1UL << pos;
                        break;
                    case 3:
                        bitBoardsBishops ^= 1UL << pos;
                        break;
                    case 4:
                        bitBoardsQueens ^= 1UL << pos;
                        break;
                    case 5:
                        bitBoardsKings ^= 1UL << pos;
                        break;
                }
                // pieces.Remove(piece.Key);
                break;
            }
        }
    }
}

public class Move{
    public PieceAbs Piece;

    public string pieceName;

    public int StartPos;
    public int TargetPos;

    public bool isCapture = false;
    public bool isCastle = false;

    public PieceAbs capturedPiece = null;

    public Move(PieceAbs piece, string pieceName, int startPos, int targetPos){
        Piece = piece;
        this.pieceName = pieceName;
        StartPos = startPos;
        TargetPos = targetPos;
    }
}

public abstract class PieceAbs {
    public int Position { get; set; }
    public bool IsWhite { get; set; }
    public int Type { get; set; }
    public Board Board { get; set; }

    public PieceAbs(Board board, int position, int type) {
        Position = position;
        Type = type;
        IsWhite = (Type & 16) == 16;
        Board = board;
    }

    public abstract ulong GenerateMoves();
    public virtual Move ReturnMove(int targetPos, bool check = false){
        if((check && (((1UL << targetPos) & GenerateMoves()) != 0)) || !check){
            Move move = new Move(this, Board.pieces.FirstOrDefault(p=> p.Value == this).Key ,Position, targetPos);
            if((Board.bitBoardsAll & (1UL << targetPos)) != 0){
                move.isCapture = true;
                move.capturedPiece = Board.pieces.Values.FirstOrDefault(x => x.Position == targetPos);
            }
            return move;
        }
        return null;
    }
}

public class Pawn : PieceAbs {
    public Pawn(Board board,int position, bool isWhite) : base(board, position, 0|(isWhite ? 16 : 8)) {}

    public override ulong GenerateMoves() {
        ulong occupied = Board.bitBoardsAll;
        ulong enemies = IsWhite ? Board.bitBoardsBlack : Board.bitBoardsWhite;
        ulong friends = occupied ^ enemies;
        ulong pawns = Board.bitBoardsPawns & friends & (1UL << Position);

        ulong promotionMask = IsWhite ? (ulong)0x00FF000000000000 : 0x000000000000FF00;


        ulong singleMove = (IsWhite ? (pawns << 8) : (pawns >> 8)) & ~occupied;
        ulong doubleMovePoss = pawns & (IsWhite ? (ulong)0x000000000000FF00 : 0x00FF000000000000);
        ulong doubleMove = (singleMove!=0) ? ((IsWhite ? (doubleMovePoss << 16) : (doubleMovePoss >> 16)) & ~occupied) : 0UL;
        ulong captureLeft = (IsWhite ? (pawns << 7) : (pawns >> 9)) & enemies & 0x7F7F7F7F7F7F7F7F; // Mask to avoid wraparound
        ulong captureRight = (IsWhite ? (pawns << 9) : (pawns >> 7)) & enemies & 0xFEFEFEFEFEFEFEFE; // Mask to avoid wraparound

        ulong promotion = singleMove & promotionMask;
        ulong promotionCaptureLeft = captureLeft & promotionMask;
        ulong promotionCaptureRight = captureRight & promotionMask;

        ulong allMoves = singleMove | doubleMove | captureLeft | captureRight | promotion | promotionCaptureLeft | promotionCaptureRight;
        
        return allMoves;
    }
}

public class Knight : PieceAbs {
    public Knight(Board board,int position, bool isWhite) : base(board, position, 2|(isWhite ? 16 : 8)) {}

    const ulong notAFile = 0xfefefefefefefefe;
    const ulong notABFile = 0xfcfcfcfcfcfcfcfc;
    const ulong notHFile = 0x7f7f7f7f7f7f7f7f;
    const ulong notGHFile = 0x3f3f3f3f3f3f3f3f;


    public override ulong GenerateMoves() {
        ulong friends = IsWhite ? Board.bitBoardsWhite : Board.bitBoardsBlack;
        ulong knights = Board.bitBoardsKnights & friends & (1UL << Position);

        ulong allMoves = (~friends) & (noEaEa(knights) | noNoEa(knights) | soEaEa(knights) | soSoEa(knights) | noNoWe(knights) | noWeWe(knights) | soWeWe(knights) | soSoWe(knights));

        return allMoves;
    }

    ulong noNoEa(ulong b) {return (b << 17) & notAFile;}
    ulong noEaEa(ulong b) {return (b << 10) & notABFile;}
    ulong soEaEa(ulong b) {return (b >>  6) & notABFile;}
    ulong soSoEa(ulong b) {return (b >> 15) & notAFile;}
    ulong noNoWe(ulong b) {return (b << 15) & notHFile;}
    ulong noWeWe(ulong b) {return (b <<  6) & notGHFile;}
    ulong soWeWe(ulong b) {return (b >> 10) & notGHFile;}
    ulong soSoWe(ulong b) {return (b >> 17) & notHFile;}
}

public abstract class SlidingPieces : PieceAbs{
    protected SlidingPieces(Board board, int position, int type) : base(board, position, type){}

    public static ulong GenerateSlideMovesRook(ulong position, ulong occupied, ulong friends, int direction)
    {
        if(position == 0){
            return 0UL;
        }
        ulong moves = 0UL;
        int i = 1;
        int initRank = (int)((ulong)Math.Log(position, 2) / 8);
        int initFile = (int)((ulong)Math.Log(position, 2) % 8);
        
        while (true)
        {
            position = direction > 0 ? position << direction : position >> -direction;
            if(position == 0)
                break;

            // Check if the piece has wrapped around to a different rank
            int newRank = (int)((ulong)Math.Log(position, 2) / 8);
            int newFile = (int)((ulong)Math.Log(position, 2) % 8);
            if ((newRank != initRank) && (newFile != initFile)) break;
            moves |= position;
            if (position == 0) break;
            if ((position & occupied) != 0){ 
                if((position & friends)==0)
                    moves |= position;
                break;
            }
            i += 1;
        }

        return moves;
    }

    public static ulong GenerateSlideMovesBishop(ulong position, ulong occupied, ulong friends, int direction)
    {
        if (position == 0) return 0UL;

        ulong fileMask = direction > 0 ? 0x7F7F7F7F7F7F7F7FUL : 0xFEFEFEFEFEFEFEFEUL;

        ulong moves = 0UL;
        while (true)
        {
            position = direction > 0 ? position << direction : position >> -direction;

            position &= fileMask;
            if (position == 0) break;
            moves |= position;
            if ((position & occupied) != 0){ 
                if((position & friends)!=0)
                    moves ^= position;
                break;
            }
        }

        return moves;
    }
}

public class Rook : SlidingPieces {
    public Rook(Board board,int position, bool isWhite) : base(board, position, 1|(isWhite ? 16 : 8)) {}

    public override ulong GenerateMoves() {
        ulong friends = IsWhite ? Board.bitBoardsWhite : Board.bitBoardsBlack;
        ulong occupied = Board.bitBoardsAll;
        ulong rooks = 1UL << Position;

        ulong allMoves = 0UL;
        
        allMoves |= GenerateSlideMovesRook(rooks, occupied,friends, 8);
        allMoves |= GenerateSlideMovesRook(rooks, occupied,friends, -8);
        allMoves |= GenerateSlideMovesRook(rooks, occupied,friends, -1);
        allMoves |= GenerateSlideMovesRook(rooks, occupied,friends, 1);

        allMoves &= ~friends;
        return allMoves;
    }

    
}

public class Bishop : SlidingPieces {
    public Bishop(Board board,int position, bool isWhite) : base(board, position, 3|(isWhite ? 16 : 8)) {}

    public override ulong GenerateMoves() {
        ulong friends = IsWhite ? Board.bitBoardsWhite : Board.bitBoardsBlack;
        ulong occupied = Board.bitBoardsAll;
        ulong bishops = 1UL << Position;

        ulong allMoves = 0UL;
        
        allMoves |= GenerateSlideMovesBishop(bishops, occupied,friends, 9);
        allMoves |= GenerateSlideMovesBishop(bishops, occupied,friends, -7);
        allMoves |= GenerateSlideMovesBishop(bishops, occupied,friends, -9);
        allMoves |= GenerateSlideMovesBishop(bishops, occupied,friends, 7);

        allMoves &= ~friends;
        return allMoves;
    }
}

public class Queen : SlidingPieces {
    public Queen(Board board,int position, bool isWhite) : base(board, position, 4|(isWhite ? 16 : 8)) {}

    public override ulong GenerateMoves() {
        ulong friends = IsWhite ? Board.bitBoardsWhite : Board.bitBoardsBlack;
        ulong occupied = Board.bitBoardsAll;
        ulong queens = 1UL << Position;

        ulong allMoves = 0UL;
        
        allMoves |= GenerateSlideMovesBishop(queens, occupied,friends, 9);
        allMoves |= GenerateSlideMovesBishop(queens, occupied,friends, -7);
        allMoves |= GenerateSlideMovesBishop(queens, occupied,friends, -9);
        allMoves |= GenerateSlideMovesBishop(queens, occupied,friends, 7);
        allMoves |= GenerateSlideMovesRook(queens, occupied,friends, 8);
        allMoves |= GenerateSlideMovesRook(queens, occupied,friends, -8);
        allMoves |= GenerateSlideMovesRook(queens, occupied,friends, -1);
        allMoves |= GenerateSlideMovesRook(queens, occupied,friends, 1);

        allMoves &= ~friends;
        return allMoves;
    }
}

public class King : PieceAbs {
    public King(Board board,int position, bool isWhite) : base(board, position, 5|(isWhite ? 16 : 8)) {}

    public override ulong GenerateMoves() {
        ulong occupied = Board.bitBoardsAll;
        ulong enemies = IsWhite ? Board.bitBoardsBlack : Board.bitBoardsWhite;
        ulong friends = IsWhite ? Board.bitBoardsWhite : Board.bitBoardsBlack;
        ulong kings = Board.bitBoardsKings & friends & (1UL << Position);


        ulong allMoves = (~friends) & (noEa(kings) | noWe(kings) | soEa(kings) | soWe(kings) | noNo(kings) | soSo(kings) | noSo(kings) | soNo(kings));

        return allMoves;
    }

    ulong noEa(ulong b) {return (b << 9) & 0xfefefefefefefefe;}
    ulong noWe(ulong b) {return (b << 7) & 0x7f7f7f7f7f7f7f7f;}
    ulong soEa(ulong b) {return (b >> 7) & 0xfefefefefefefefe;}
    ulong soWe(ulong b) {return (b >> 9) & 0x7f7f7f7f7f7f7f7f;}
    ulong noNo(ulong b) {return b << 8 ;}
    ulong soSo(ulong b) {return b >> 8 ;}
    ulong noSo(ulong b) {return (b << 1) & 0xfefefefefefefefe;}
    ulong soNo(ulong b) {return (b >> 1) & 0x7f7f7f7f7f7f7f7f;}

}