﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;


public delegate void SelectedBlockChangeHandler(Vector2Int selectPos);


[SelectionBase]
public class CoordGrid : UnitySingleton<CoordGrid> {

    [Header("# Size")]
    public int width;
    public int height;

    /// <summary>
    /// 可用的格子列表(后面场景编辑器就是编辑这个)
    /// 会用作大部分对象字典的key值
    /// </summary>
    public List<Vector2Int> coords;       

    [Header("# ResRef")]
    public Cell cellPrefab;         // 单元格
    public Block blockPrefab;       // Block对象

    public GameObject selectedBlockBorderPrefab;

    public GameObject blocksHolder;

    // 变量
    //private List<Cell> cells;           // 背景块引用
    //private List<Block> blocks;         // Block对象列表
    public Dictionary<Vector2Int, Block> blocks;  // Block 字典,key是坐标
    public Dictionary<Vector2Int, Cell> cells; // Cell字典
    //private int[,] matrix;              // 矩阵信息

    private GameObject selectedBlockBorder; // 边框物体引用

    // Event
    private SelectedBlockChangeHandler SelectedBlockChange;

    

    private Block _currentSelectedBlock; // 当前选择对象的引用
    public Block currentSelectedBlock {
        get {
            return _currentSelectedBlock;
        }
        set {
            _currentSelectedBlock = value;

            if (SelectedBlockChange != null)
                if (_currentSelectedBlock != null)
                    SelectedBlockChange(_currentSelectedBlock.pos);
                else
                    SelectedBlockChange(Vector2Int.one*10000); // 放在外地看不见
        }
    }

    /// <summary>
    /// 父物体中心点
    /// </summary>
    private Vector3 origin{
        get{
            return new Vector3(-(float)width*0.5f+0.5f,-(float)height*0.5f+0.5f);
        }
    }



    // 初始化场景坐标信息
    void CreateCoords() {
        coords = new List<Vector2Int>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                coords.Add(new Vector2Int(x, y));
            }
        }
    }

    /// <summary>
    /// 创建格子
    /// </summary>
    void CreateGrid() {
        bool lord = false;
        cells = new Dictionary<Vector2Int, Cell>();

        foreach (var coord in coords) {
            lord = Convert.ToBoolean((coord.x + coord.y) & 1);
            CreateCell(coord, lord, transform);
        }

    }
    /// <summary>
    /// 创建格子单元格
    /// </summary>
    /// <param name="coord">格子坐标</param>
    /// <param name="lord">light or dark</param>
    /// <param name="parent">父物体</param>
    void CreateCell(Vector2Int coord,bool lord,Transform parent) {
        Cell c = Instantiate(cellPrefab) as Cell;
        c.lord = lord;        
        c.transform.SetParent(parent,false);
        c.pos = coord;
        cells.Add(coord,c);
    }



    /// <summary>
    /// 用Block填充场景
    /// </summary>
    void CreateBlocks() {
        blocks = new Dictionary<Vector2Int, Block>();
        if (blocksHolder == null) {
            blocksHolder = new GameObject() {
                name = "BlockHolder",
            };
        }
        blocksHolder.transform.localPosition = origin;
        foreach (var coord in coords) {
            SpawnBlock(coord, blocksHolder.transform);
        }

    }

    /// <summary>
    /// 填充每个单元格
    /// </summary>
    /// <param name="coord">格子坐标</param>
    /// <param name="p">父物体</param>
    void SpawnBlock(Vector2Int coord,Transform p) {

        Block b = BlockPool.Instance.RandomPop();

        b.transform.Reset(p);
        b.pos = coord;
        b.name = coord.ToString();
        blocks.Add(b.pos, b);
        //matrix[x, y] = (int)b.colorType;
    }

    /// <summary>
    /// 消除
    /// </summary>
    void ClearUp() {

    }

    
    void CheckClear() {

    }

    public bool InBound(Vector2Int pos) {
        return coords.Contains(pos);
    }

    

    void CheckBlocks() {
        foreach (var b in blocks) {
            b.Value.CheckNeightbourColor();
        }
    }

    void CheckAllBlockCount() {
        foreach (var b in blocks) {
            b.Value.CheckNeighbours();
        }
    }


    public void ReSpawnBlocks() {
        //Vector2Int selectPos = Vector2Int.zero;
        //bool hasSelected = false;
        if (currentSelectedBlock) {
            currentSelectedBlock.Deselect();
            //selectPos = currentSelectedBlock.pos;
            //hasSelected = true;
        }


        foreach (var item in blocks) {
            BlockPool.Instance.Push(item.Value,(int)item.Value.colorType);
        }

        CreateBlocks();

        //currentSelectedBlock = blocks[selectPos];
        //if(hasSelected)
        //    blocks[selectPos].Select();
        CheckBlocks();
        CheckAllBlockCount();
        CheckBombs();
    }

    /// <summary>
    /// 选择框改变事件
    /// </summary>
    /// <param name="pos">选择框的格子坐标</param>
    public void OnSelectedBlockChange(Vector2Int pos) {
        Debug.Log("Set Selected");
        if (selectedBlockBorder == null) {
            selectedBlockBorder = Instantiate(selectedBlockBorderPrefab);
        }
        selectedBlockBorder.transform.SetParent(transform);
        selectedBlockBorder.transform.localPosition = new Vector3(pos.x, pos.y);
    }

    //// Unity消息

    private void Awake() {
        SelectedBlockChange += OnSelectedBlockChange;
        transform.localPosition = origin;

        CreateCoords();
        CreateGrid();
        CreateBlocks();

        CheckBlocks();
        CheckAllBlockCount();
        CheckBombs();
    }


    
    void CheckBombs() {
        List<Block>[] bombsBlocks = new List<Block>[5];
        for (int i = 0; i < 5; i++) {
            bombsBlocks[i] = new List<Block>();
        }
        foreach (var b in blocks.Values) {
            if (b.CheckBombType() != BombType.None) {
                bombsBlocks[(int)b.CheckBombType()-1].Add(b);
            }
        }

        foreach (var item in bombsBlocks[(int)BombType.Super]) {

        }
        
    }
}
