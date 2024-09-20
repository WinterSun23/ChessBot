using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    // Start is called before the first frame update
    const int NUM_SQUARES = 8*8;
    public GameObject Board_;
    public GameObject square;
    private GameObject[] boardSquares = new GameObject[NUM_SQUARES];
    public Sprite[] pieceSprites = new Sprite[12]; //white frist then black


    public GameObject piece;
    GameObject selectedPiece = null;
    private Pieces pieces;

    public static Dictionary<string, int> pieceStr_to_piece = new Dictionary<string, int>{
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


    
    void Start()
    {
        Board_.transform.position = new Vector3(-0.5f, -0.5f, 0);
        GenChessBoard();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
            SelectPiece();
        DisplayChessBoard();
    }

    void GenChessBoard(){
        for(int i=0; i<NUM_SQUARES;i++){
            GameObject square = GameObject.Instantiate(this.square);
            square.GetComponent<Renderer>().material.color = (i/8 + i%8) % 2 == 0 ? Color.white : Color.black;
            square.transform.position = new Vector3(i%8 - 4, i/8 - 4, -0.1f);
            square.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            boardSquares[i] = square;
        }
        pieces = new Pieces(piece);
        LoadFEN("r1bk3r/p2pBpNp/n4n2/1p1NP2P/6P1/3P4/P1P1K3/q5b1");
    }

    private void DisplayChessBoard(){
        if(selectedPiece == null){
            //ulong piecesPlaced = pieces.GetBitBoardAll();
            for(int i=0; i<NUM_SQUARES;i++){
                // if ((piecesPlaced & (1UL << i)) != 0)
                //     boardSquares[i].GetComponent<Renderer>().material.color = Color.blue;
                // else
                    boardSquares[i].GetComponent<Renderer>().material.color = (i/8 + i%8) % 2 == 0 ? Color.white : Color.black;
            }
        }
        else{
            ulong moves = pieces.GetPossibleMovesBitBoard(pieceStr_to_piece[selectedPiece.name[..1]],(int)Mathf.Floor(selectedPiece.transform.position.x + 4), (int)Mathf.Floor(selectedPiece.transform.position.y + 4));
            for(int i=0; i<NUM_SQUARES;i++){
                if ((moves & (1UL << i)) != 0)
                    boardSquares[i].GetComponent<Renderer>().material.color = Color.green;
                else
                    boardSquares[i].GetComponent<Renderer>().material.color = (i/8 + i%8) % 2 == 0 ? Color.white : Color.black;
            }
        }
    }

    public void LoadFEN(string fenStr = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"){

        Dictionary<string, int> pieceIndex = new Dictionary<string, int>{
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
                pieces.AddPiece(new Piece(piece, pieceSprites[(pieceStr_to_piece[cStr]&7) +(((pieceStr_to_piece[cStr] & 8) == 8) ? 6 : 0)], cStr+pieceIndex[cStr].ToString(), c>='a' ? 0 : 1, pieceStr_to_piece[cStr], file + rank*8));
                pieceIndex[cStr]++;
                file++;
            }
        }
    }

    
    public bool MovePiece(string name, int from,int to){
        UInt64 moves = pieces.GetPossibleMovesBitBoard(pieceStr_to_piece[name[..1]],from%8, from/8);
        // Debug.Log(Convert.ToString((long)moves, 2) + " " + Convert.ToString((long)toMove, 2));

        if ((moves & (1UL << to)) != 0){
            pieces.MovePiece(name,to);
            return true;
        }
        else{
            Debug.Log("Invalid Move");
            return false;
        }
    }

    void SelectPiece(){
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100)){
            if(selectedPiece != null){
                int from = (int)(Mathf.Floor(selectedPiece.transform.position.x + 4) + Mathf.Floor(selectedPiece.transform.position.y + 4) * 8);
                int to = (int)(Mathf.Floor(hit.collider.transform.position.x + 4) + Mathf.Floor(hit.collider.transform.position.y + 4) * 8);
                
    
                if(!hit.collider.gameObject.name.Contains("Board")){
                    int pieceIndex = pieces.ReturnPieceIndex(selectedPiece.name);
                    Piece piece = pieces.pieces[pieceIndex];

                    int pieceIndex1 = pieces.ReturnPieceIndex(hit.collider.gameObject.name);
                    Piece piece1 = pieces.pieces[pieceIndex1];

                    if((piece.type & 24) == (piece1.type & 24) || pieceIndex == pieceIndex1){
                        selectedPiece = hit.collider.gameObject;
                        Debug.Log(selectedPiece.name);
                        return;
                    }

                    bool moved = MovePiece(selectedPiece.name, from, to);
                    if(!moved){
                        selectedPiece = hit.collider.gameObject;
                        Debug.Log(selectedPiece.name);
                    }
                    else{
                        pieces.RemovePiece(hit.collider.gameObject.name);
                        selectedPiece = null;
                    }
                }
                else{
                    MovePiece(selectedPiece.name, from, to);
                    selectedPiece = null;
                }
            }
            else{
                if(!hit.collider.gameObject.name.Contains("Board")){
                    selectedPiece = hit.collider.gameObject;
                    Debug.Log(selectedPiece.name);
                }
                else
                    selectedPiece = null;
            }
        }
    }
}

public class Pieces{
    public List<Piece> pieces = new List<Piece>(); //white first then black
    public List<Piece> removedPieces = new List<Piece>(); //white first then black
    public GameObject piece;

    public Pieces(GameObject piece){
        this.piece = piece;
        // CreatePieces();
    }

    public void AddPiece(Piece piece){
        pieces.Add(piece);
    }
    public void RemovePiece(Piece piece){
        GameObject.Destroy(piece.piece);
        removedPieces.Add(piece);
        pieces.Remove(piece);
    }
    public void RemovePiece(string name){
        int index = ReturnPieceIndex(name);
        GameObject.Destroy(pieces[index].piece);
        removedPieces.Add(pieces[index]);
        pieces.RemoveAt(index);
    }

    public int ReturnPieceIndex(string name){
        int index = 0;
        foreach(Piece p in pieces){
            if(p.piece.name.Equals(name))
                return index;
            index++;
        }
        Debug.Log("Piece not found "+ name);
        return -1;
    }

    public int ReturnPieceIndex(int index){
        int indexReturn = 0;
        foreach(Piece p in pieces){
            if(p.index == index)
                return indexReturn;
            indexReturn++;
        }
        Debug.Log("Piece not found "+ index);
        return -1;
    }


    public void MovePiece(string name, int index = -1){
        int pieceIndex = ReturnPieceIndex(name);
        // Debug.Log(piece.type);
        if(index == -1)
            index = pieces[pieceIndex].index;
        pieces[pieceIndex].piece.transform.position = new Vector3(index%8 - 4, index/8 - 4, pieces[pieceIndex].piece.transform.position.z);
        pieces[pieceIndex].index = index;
    }

    public ulong GetBitBoard(int type = 16|8){
        ulong ans = 0UL;
        foreach(Piece p in pieces){
            if ((p.type & type) !=0)
                ans |= 1UL << p.index;
        }
        return ans;
    }

    public ulong GetPossibleMovesBitBoard(Piece piece){
        return GetPossibleMovesBitBoard(piece.type, piece.index%8, piece.index/8);
    }

    public ulong GetPossibleMovesBitBoard(int type,int file, int rank)
    {
        ulong ans = 0UL;
        ulong bitBoard = GetBitBoard();
        ulong move = 0UL;
        switch (type & 7)
        {
            case 0: // Pawn
                int singleStepRank = ((type & 8) == 8) ? Mathf.Max(rank - 1, 0) : Mathf.Min(rank + 1, 7);

                if(file - 1 >=0){
                    move = 1UL << (file - 1 + singleStepRank * 8);
                    if ((GetBitBoard((type&24)^24) & move) != 0)
                        ans |= 1UL << (file - 1 + singleStepRank * 8);
                }
                if(file + 1 <=7){
                    move = 1UL << (file + 1 + singleStepRank * 8);
                    if ((GetBitBoard((type&24)^24) & move) != 0)
                        ans |= 1UL << (file + 1 + singleStepRank * 8);
                }

                int doubleStepRank = ((type & 8) == 8) ? Mathf.Max(rank - 2, 0) : Mathf.Min(rank + 2, 7);
                move = 1UL << (file + singleStepRank * 8);
                if ((GetBitBoard() & move) == 0){
                    ans |= 1UL << (file + singleStepRank * 8);
                    if ((rank == 1 && (type & 16) == 16) || (rank == 6 && (type & 8) == 8)){
                        move = 1UL << (file + doubleStepRank * 8);
                        if ((bitBoard & move) == 0)
                            ans |= 1UL << (file + doubleStepRank * 8);
                    }
                }
                break;
            case 1: // Rook
                for (int i = file+1; i < 8; i++)
                {
                    move = 1UL << (i + rank * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for (int i = file-1; i >= 0; i--)
                {
                    move = 1UL << (i + rank * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for (int i = rank+1; i < 8; i++)
                {
                    move = 1UL << (file + i * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for (int i = rank-1; i >= 0; i--)
                {
                    move = 1UL << (file + i * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                break;
            case 2: // Knight
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if ((Mathf.Abs(i - file) == 2 && Mathf.Abs(j - rank) == 1) || (Mathf.Abs(i - file) == 1 && Mathf.Abs(j - rank) == 2))
                        {
                            move = 1UL << (i + j * 8);
                            if ((GetBitBoard(type&24) & move) == 0)
                                ans |= 1UL << (i + j * 8);
                        }
                    }
                }
                break;
            case 3: // Bishop
                for(int i=file+1,j=rank+1;i<8 && j<8;i++,j++){
                    move = 1UL << (i + j * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for(int i=file-1,j=rank-1;i>=0 && j>=0;i--,j--){
                    move = 1UL << (i + j * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for(int i=file+1,j=rank-1;i<8 && j>=0;i++,j--){
                    move = 1UL << (i + j * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for(int i=file-1,j=rank+1;i>=0 && j<8;i--,j++){
                    move = 1UL << (i + j * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                break;
            case 4: // Queen
                for (int i = file+1; i < 8; i++)
                {
                    move = 1UL << (i + rank * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for (int i = file-1; i >= 0; i--)
                {
                    move = 1UL << (i + rank * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for (int i = rank+1; i < 8; i++)
                {
                    move = 1UL << (file + i * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for (int i = rank-1; i >= 0; i--)
                {
                    move = 1UL << (file + i * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for(int i=file+1,j=rank+1;i<8 && j<8;i++,j++){
                    move = 1UL << (i + j * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for(int i=file-1,j=rank-1;i>=0 && j>=0;i--,j--){
                    move = 1UL << (i + j * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for(int i=file+1,j=rank-1;i<8 && j>=0;i++,j--){
                    move = 1UL << (i + j * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                for(int i=file-1,j=rank+1;i>=0 && j<8;i--,j++){
                    move = 1UL << (i + j * 8);
                    if ((bitBoard & move) != 0){
                        if((GetBitBoard((type&24)^24) & move) != 0)
                            ans |= move;
                        break;
                    }
                    ans |= move;
                }
                break;
            case 5: // King
                int startFile = file == 0 ? 0 : file - 1;
                int startRank = rank == 0 ? 0 : rank - 1;
                int endFile = file == 7 ? 7 : file + 1;
                int endRank = rank == 7 ? 7 : rank + 1;

                for (int i = startFile; i <= endFile; i++)
                    for (int j = startRank; j <= endRank; j++){
                        move = 1UL << (i + j * 8);
                        if ((GetBitBoard(type&24) & move) == 0)
                            ans |= 1UL << (i + j * 8);
                    }
                break;
        }
        //check for potential checks



        //remove current square from possible moves
        ans &= ~(1UL << (file + rank * 8));
        return ans;
    }
    
}

public class Piece{
    public int type; //0 for black and 1 for white | 0 for pawn, 1 for rook, 2 for knight, 3 for bishop, 4 for queen, 5 for king
    public GameObject piece;
    public string pieceName = "";
    public int index;

    public Piece(GameObject piece, Sprite pieceSprite, string name, int col, int type, int index){
        this.piece = GameObject.Instantiate(piece);
        this.piece.GetComponent<SpriteRenderer>().sprite = pieceSprite;
        this.piece.name = name;
        this.pieceName = (type & 7).ToString();
        this.type = type | ((col+1)*8);
        this.index = index;
        this.piece.transform.position = new Vector3(index%8 - 4, index/8 - 4, piece.transform.position.z);
        // this.piece.transform.position = GetWorldPos();
    }
}
