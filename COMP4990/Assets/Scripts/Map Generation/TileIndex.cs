using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Utils;

public class TileIndex : MonoBehaviour
{
    // -1 IS EMPTY TILE

    public Tile cursorTile;
    public Tile houseTile;

    Dictionary<int, Tile> tileIndex;
    List<int> grassTiles;
    List<int> grassWithFlowersTiles;
    List<int> allGrassTiles;
    List<int> allWaterTiles;

    Dictionary<int, AnimatedTile> seafoamIndex;

    Dictionary<int, GameObject> objectIndex;
    List<int> trees;
    List<int> cacti;
    List<int> bigRocks;

    Dictionary<int, Grid> structIndex;

    Dictionary<int, Tuple<AnimationClip, AnimatorController>> waterEventIndex;

    public Tile grass;
    public Tile grass2;
    public Tile grass3;
    public Tile grass4;
    public Tile grass5;
    public Tile grass6;
    public Tile grass7;
    public Tile grass8;
    public Tile grass9;
    public Tile grass10;
    public Tile grass11;
    public Tile grass12;
    public Tile grass13;
    public Tile grass14;
    public Tile grass15;
    public Tile grass16;
    public Tile grass17;
    public Tile grassWithFlowers;
    public Tile grassWithFlowers2;
    public Tile grassWithFlowers3;
    public Tile grassWithFlowers4;
    public Tile grassWithFlowers5;
    public Tile grassWithFlowers6;
    public Tile grassWithFlowers7;
    public Tile grassWithFlowers8;
    public Tile grassWithFlowers9;
    public Tile grassWithFlowers10;
    public Tile grassWithFlowers11;
    public Tile grassWithFlowers12;
    public Tile grassWithFlowers13;
    public Tile grassWithFlowers14;
    public Tile grassWithFlowers15;
    public Tile stone;
    public Tile sand;
    public Tile water;
    public Tile terrainTile;
    public Tile deepWater;

    public AnimatedTile sfLeft;
    public AnimatedTile sfRight;
    public AnimatedTile sfTop;
    public AnimatedTile sfBottom;
    public AnimatedTile sfTopLeft;
    public AnimatedTile sfTopRight;
    public AnimatedTile sfBottomLeft;
    public AnimatedTile sfBottomRight;
    public AnimatedTile sfOpenDown;
    public AnimatedTile sfOpenUp;
    public AnimatedTile sfOpenRight;
    public AnimatedTile sfOpenLeft;
    public AnimatedTile sfLeftAndRight;
    public AnimatedTile sfUpAndDown;

    public AnimatedTile sfcDownRight;
    public AnimatedTile sfcDownLeft;
    public AnimatedTile sfcDownLeftDownRight;
    public AnimatedTile sfcUpRight;
    public AnimatedTile sfcUpRightDownRight;
    public AnimatedTile sfcUpRightDownLeft;
    public AnimatedTile sfcUpRightDownLeftDownRight;
    public AnimatedTile sfcUpLeft;
    public AnimatedTile sfcUpLeftDownRight;
    public AnimatedTile sfcUpLeftDownLeft;
    public AnimatedTile sfcUpLeftDownLeftDownRight;
    public AnimatedTile sfcUpLeftUpRight;
    public AnimatedTile sfcUpLeftUpRightDownRight;
    public AnimatedTile sfcUpLeftUpRightDownLeft;
    public AnimatedTile sfcAllCorners;

    public AnimatedTile sfcwUpLeftWaterRightDown;
    public AnimatedTile sfcwUpLeftWaterRight;
    public AnimatedTile sfcwUpLeftWaterDown;
    public AnimatedTile sfcwUpRightWaterLeftDown;
    public AnimatedTile sfcwUpRightWaterLeft;
    public AnimatedTile sfcwUpRightWaterDown;
    public AnimatedTile sfcwDownLeftWaterRightUp;
    public AnimatedTile sfcwDownLeftWaterRight;
    public AnimatedTile sfcwDownLeftWaterUp;
    public AnimatedTile sfcwDownRightWaterLeftUp;
    public AnimatedTile sfcwDownRightWaterLeft;
    public AnimatedTile sfcwDownRightWaterUp;


    public GameObject objTree1;
    public GameObject objTree2;
    public GameObject objTree3;

    public GameObject objCactus1;
    public GameObject objCactus2;
    public GameObject objCactus3;
    public GameObject objCactus4;
    public GameObject objSmallCactus1;
    public GameObject objSmallCactus2;
    
    public GameObject objStone;
    public GameObject objIronOre;
    public GameObject objGoldOre;
    public GameObject objCopperOre;
    public GameObject bigRock5;
    public GameObject bigRock6;

    public Grid struct1;
    public Grid struct2;
    public Grid struct3;

    // water event
    public AnimationClip bubble1Clip;
    public AnimationClip bubble2Clip;
    public AnimationClip ripple1Clip;

    public AnimatorController bubble1Controller;
    public AnimatorController bubble2Controller;
    public AnimatorController ripple1Controller;

    // building blocks ---------------------------------------------------------------------

    public Tile woodenWall; // 101

    void Awake()
    {   

        tileIndex = new Dictionary<int, Tile>
        {
            { 0, grass },
            { 1, grass2 },
            { 2, grass3 },
            { 3, grass4 },
            { 4, grass5 },
            { 5, grass6 },
            { 6, grass7 },
            { 7, grass8 },
            { 8, grass9 },
            { 9, grass10 },
            { 10, grass11 },
            { 11, grass12 },
            { 12, grass13 },
            { 13, grass14 },
            { 14, grass15 },
            { 15, grass16 },
            { 16, grass17 },
            { 17, grassWithFlowers },
            { 18, grassWithFlowers2 },
            { 19, grassWithFlowers3 },
            { 20, grassWithFlowers4 },
            { 21, grassWithFlowers5 },
            { 22, grassWithFlowers6 },
            { 23, grassWithFlowers7 },
            { 24, grassWithFlowers8 },
            { 25, grassWithFlowers9 },
            { 26, grassWithFlowers10 },
            { 27, grassWithFlowers11 },
            { 28, grassWithFlowers12 },
            { 29, grassWithFlowers13 },
            { 30, grassWithFlowers14 },
            { 31, grassWithFlowers15 },
            { 32, stone },
            { 33, sand },
            { 34, water },
            { 35, terrainTile },
            { 36, deepWater },
            { 101, woodenWall}
        };

        seafoamIndex = new Dictionary<int, AnimatedTile>
        {
            { 0, sfLeft },
            { 1, sfRight },
            { 2, sfTop },
            { 3, sfBottom },
            { 4, sfTopLeft },
            { 5, sfTopRight },
            { 6, sfBottomLeft },
            { 7, sfBottomRight },
            { 8, sfOpenDown },
            { 9, sfOpenUp },
            { 10, sfOpenRight },
            { 11, sfOpenLeft },
            { 12, sfLeftAndRight },
            { 13, sfUpAndDown },

            { 14, sfcDownRight },
            { 15, sfcDownLeft },
            { 16, sfcDownLeftDownRight },
            { 17, sfcUpRight },
            { 18, sfcUpRightDownRight },
            { 19, sfcUpRightDownLeft },
            { 20, sfcUpRightDownLeftDownRight },
            { 21, sfcUpLeft },
            { 22, sfcUpLeftDownRight },
            { 23, sfcUpLeftDownLeft },
            { 24, sfcUpLeftDownLeftDownRight },
            { 25, sfcUpLeftUpRight },
            { 26, sfcUpLeftUpRightDownRight },
            { 27, sfcUpLeftUpRightDownLeft },
            { 28, sfcAllCorners },

            { 29, sfcwUpLeftWaterRightDown },
            { 30, sfcwUpLeftWaterRight },
            { 31, sfcwUpLeftWaterDown },
            { 32, sfcwUpRightWaterLeftDown },
            { 33, sfcwUpRightWaterLeft },
            { 34, sfcwUpRightWaterDown },
            { 35, sfcwDownLeftWaterRightUp },
            { 36, sfcwDownLeftWaterRight },
            { 37, sfcwDownLeftWaterUp },
            { 38, sfcwDownRightWaterLeftUp },
            { 39, sfcwDownRightWaterLeft },
            { 40, sfcwDownRightWaterUp },
        };

        objectIndex = new Dictionary<int, GameObject>
        {
            { 0, objTree1 },
            { 1, objTree2 },
            { 2, objTree3 },
            { 3, objStone },
            { 4, objCopperOre },
            { 5, objIronOre },
            { 6, objGoldOre },
            { 7, bigRock5 },
            { 8, bigRock6 },
            { 9, objCactus1 },
            { 10, objCactus2 },
            { 11, objCactus3 },
            { 12, objCactus4 },
            { 13, objSmallCactus1 },
            { 14, objSmallCactus2 },
        };

        structIndex = new Dictionary<int, Grid>{
            { 0, struct1 },
            { 1, struct2 },
            { 2, struct3 },
        };

        waterEventIndex = new Dictionary<int, Tuple<AnimationClip, AnimatorController>>
        {
            { 0, new Tuple<AnimationClip, AnimatorController>(bubble1Clip, bubble1Controller) },
            { 1, new Tuple<AnimationClip, AnimatorController>(bubble2Clip, bubble2Controller) },
            { 2, new Tuple<AnimationClip, AnimatorController>(ripple1Clip, ripple1Controller) },

        };

        grassTiles = new List<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16
        };

        grassWithFlowersTiles = new List<int>
        {
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
        };

        allGrassTiles = new List<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
        };

        allWaterTiles = new List<int>
        {
            34, 36
        };

        trees = new List<int>
        {
            0, 1, 2
        };

        cacti = new List<int>
        {
            9, 10, 11, 12, 13, 14
        };

        bigRocks = new List<int>
        {
            3, 4, 5, 6, 7, 8
        };
    }

    public List<int> GetGrassTiles()
    {
        return grassTiles;
    }

    public List<int> GetGrassWithFlowersTiles()
    {
        return grassWithFlowersTiles;
    }

    public List<int> GetAllGrassTiles()
    {
        return allGrassTiles;
    }

    public List<int> GetAllWaterTiles()
    {
        return allWaterTiles;
    }

    public Tile GetTile(int index)
    {
        return tileIndex[index];
    }

    public AnimatedTile GetSeafoam(int index)
    {
        return seafoamIndex[index];
    }

    public List<int> GetAllTrees()
    {
        return trees;
    }
    
    public List<int> GetAllCactus()
    {
        return cacti;
    }

    public List<int> GetAllBigRocks()
    {
        return bigRocks;
    }

    public GameObject GetObject(int index)
    {
        return objectIndex[index];
    }

    public Grid GetStructure(int index)
    {
        return structIndex[index];
    }

    public Tuple<AnimationClip, AnimatorController> GetWaterEvent(int index)
    {
        return waterEventIndex[index];
    }

    public int GetWaterEventCount()
    {
        return waterEventIndex.Keys.Count;
    }

    public Tile GetCursorTile()
    {
        return cursorTile;
    }

    public Tile GetHouseTile()
    {
        return houseTile;
    }

    public GameObject GetStoneObj()
    {
        return objStone;
    }
    public int GetStoneObjIndex()
    {
        return 3;
    }

    public GameObject GetCopperOreObj()
    {
        return objCopperOre;
    }
    public int GetCopperOreObjIndex()
    {
        return 4;
    }

    public GameObject GetIronOreObj()
    {
        return objIronOre;
    }
    public int GetIronOreObjIndex()
    {
        return 5;
    }

    public GameObject GetGoldOreObj()
    {
        return objGoldOre;
    }
    public int GetGoldOreObjIndex()
    {
        return 6;
    }
}
