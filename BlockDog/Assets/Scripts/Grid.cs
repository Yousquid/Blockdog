﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
public class Grid : MonoBehaviour {
    public GridCell[,] grid;
    public int gridHeight;
    public int gridWidth;
    public float cellWidth;
    public GameObject wallCell;
    public float dropRate;
    float dropTimer;
    public GameObject block;
    public static Grid me;
    public bool gameOver;
    public int score;
    int level;
    public TextMesh scoreText;
    public TextMesh levelText;
    public TextMesh gameOverText;
    float baseDropRate;
    public TextMesh highScoreText;
    public SpriteRenderer indicator;
    public GameObject wallCollider;
    public FMODUnity.EventReference preparSound;
    public FMODUnity.EventReference blockdropSound;
    
    public FMODUnity.EventReference music_INS;
    public FMODUnity.EventReference gamestartSound;
    public FMODUnity.EventReference blockeliminateSound;
    public bool matchSoundplay;
    public FMODUnity.EventReference dangerSound;
   



    FMOD.Studio.EventInstance dangerInstance;
    FMOD.Studio.EventInstance dropInstance;
    FMOD.Studio.PARAMETER_ID dropStrengthID;

   

    //newAudio
    bool warningLoopOn = false;
    bool playedGameOverSound = false;

    private void Awake() {
        me = this;
    }
    void Start () {

        

        baseDropRate = dropRate;
        grid = new GridCell[gridHeight, gridWidth];
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int j = 0; j < grid.GetLength(1); j++) {
                grid[i, j] = new GridCell(new Vector2(i, j), ToWorld(new Vector2(i, j)), ref grid);
                if (i == 0 || j == 0 || i == grid.GetLength(0) -1 || j == grid.GetLength(1) -1) {
                    Instantiate(wallCell, grid[i, j].worldPos, Quaternion.identity);
                    grid[i, j].ocupied = true;
                    grid[i, j].color = -2;
                }
            }
        }
        int num = 0;
        if (PlayerPrefs.HasKey("HighScore")) {
            num = PlayerPrefs.GetInt("HighScore");
        }
        highScoreText.text = "\n" + num;
        SpawnWallColliders();

        dropInstance = FMODUnity.RuntimeManager.CreateInstance(blockdropSound);

        FMOD.Studio.EventDescription jumpSoundDescription;
        dropInstance.getDescription(out jumpSoundDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION jumpSoundStrengthPramaterDescription;
        jumpSoundDescription.getParameterDescriptionByName("Drop_Volume", out jumpSoundStrengthPramaterDescription);
        dropStrengthID = jumpSoundStrengthPramaterDescription.id;

        warningLoopOn = false;
        playedGameOverSound = false;

        //NewSound
        FMODUnity.RuntimeManager.PlayOneShot(gamestartSound);
        //AudioDirector.Instance.PlaySound(AudioDirector.Instance.gameStartSound, false, 0f, AudioDirector.Instance.gameStartVolume, 0f, true);


        dangerInstance = FMODUnity.RuntimeManager.CreateInstance(dangerSound);


      


    }

    public void SpawnWallColliders() {
        GameObject bloop = Instantiate(wallCollider, (ToWorld(Vector2.zero) + ToWorld(new Vector2(0, gridHeight-1))) / 2, Quaternion.identity);
        bloop.transform.localScale = new Vector2(1, grid.GetLength(1));
        bloop = Instantiate(wallCollider, 
                           (ToWorld(new Vector2(gridWidth-1, 0)) + ToWorld(new Vector2(gridWidth-1, gridHeight-1))) / 2, 
                           Quaternion.identity);
        bloop.transform.localScale = new Vector2(1, grid.GetLength(1));
        bloop = Instantiate(wallCollider,
                   (ToWorld(new Vector2(1, 0)) + ToWorld(new Vector2(gridWidth - 2, 0))) / 2,
                   Quaternion.identity);
        bloop.transform.localScale = new Vector2(gridWidth-2, 1);
        bloop = Instantiate(wallCollider,
           (ToWorld(new Vector2(1, gridHeight-1)) + ToWorld(new Vector2(gridWidth - 2, gridHeight-1))) / 2,
           Quaternion.identity);
        bloop.transform.localScale = new Vector2(gridWidth - 2, 1);
    }
    public Vector2 ToWorld(Vector2 gridPos) {
        return new Vector2(gridPos.x * cellWidth, gridPos.y * cellWidth);
    }

    public Vector2 ToGrid(Vector2 worldPos) {
        return new Vector2(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
    }

    public GridCell GetCell(Vector2 pos) {
        return grid[(int)pos.x, (int)pos.y];
    }

    public GridCell GetCell(float x, float y) {
        return grid[(int)x, (int)y];
    }

    public GridCell NextNearest(Vector2 pos) {
        Vector2 gridPos = ToGrid(pos);
        if (pos.x > gridPos.x) {
            gridPos = new Vector2(gridPos.x + 1, gridPos.y);
        } else {
            gridPos = new Vector2(gridPos.x - 1, gridPos.y);
        }
        return grid[(int)gridPos.x, (int)gridPos.y] ;
    }
	
    /*public GridCell GetNearestGridCell(Vector2 pos) {
        float dist = 9999;
        int searchDist = 1;
        Vector2 gridPos = ToGrid(pos);
        
    }*/

	void Update () {
        level = (score / 20);
        //levelText.text = level.ToString;
        level = Mathf.Min(level, 20);
        dropRate = baseDropRate - (float)level * .05f;
        scoreText.text = "\n" + score;
        levelText.text = "\n" + level;
        if (gameOver) {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "FINAL SCORE\n" + score + "\n\n\n\nZ TO RESTART";
            int num = 0;
            if (PlayerPrefs.HasKey("HighScore")) {
                num = PlayerPrefs.GetInt("HighScore");
            }
            if (score > num) {
                PlayerPrefs.SetInt("HighScore", score);
            }
            return;
        }
        dropTimer += Time.deltaTime;
        if (dropTimer >= dropRate) {
            dropTimer -= dropRate;
            StartCoroutine(DropBlock());
            
        }

        MatchCheck2();
        UpdateGrid();
	}
    IEnumerator DropBlock() {
        Vector2 pos = ToWorld(new Vector2(Random.Range(1, grid.GetLength(0) - 1), grid.GetLength(1) - 3));
        GameObject bloop = Instantiate(indicator.gameObject, pos + new Vector2(0f, -1.5f), Quaternion.identity);
        int num = Random.Range(0, Global.me.blockColors.Length);
        bloop.GetComponent<SpriteRenderer>().color = Global.me.blockColors[num];

        //NewSound
        FMODUnity.RuntimeManager.PlayOneShot(preparSound);
        //AudioDirector.Instance.PlaySound(AudioDirector.Instance.blockPrepareDrop, true, bloop.transform.position.x, AudioDirector.Instance.blockPrepVolume);

        yield return new WaitForSeconds(.5f);
        Instantiate(block, pos, Quaternion.identity).GetComponent<FallingBlock>().colNum = num;

        //NewSound
        //FMODUnity.RuntimeManager.PlayOneShot(blockdropSound);
        //AudioDirector.Instance.PlaySound(AudioDirector.Instance.blockDrop, true, bloop.transform.position.x, AudioDirector.Instance.blockDropVolume);
        dropInstance.setParameterByID(dropStrengthID, Random.Range(0F, 1F));
        if (dropInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackstate;
            dropInstance.getPlaybackState(out playbackstate);
            if (playbackstate == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                dropInstance.start();
            }
        }

       
        yield return new WaitForSeconds(.2f);
        Destroy(bloop);
    }
    void MatchCheck() {
        for (int i = 0; i < grid.GetLength(0); i++) {
            int coolNum = 0;
            int lastCol = -1;
            for (int j = 0; j < grid.GetLength(1); j++) {
                int col = grid[i, j].color;
                if (col >= 0 && col == lastCol) {
                    coolNum++;
                } else {
                    coolNum = 0;
                }
                lastCol = col;
                if (coolNum >= 2) {
                    grid[i, j].BlowUp();
                    grid[i, j-1].BlowUp();
                    grid[i, j-2].BlowUp();
                }
            }
        }
        for (int i = 0; i < grid.GetLength(1); i++) {
            int coolNum = 0;
            int lastCol = -1;
            for (int j = 0; j < grid.GetLength(0); j++) {
                int col = grid[j, i].color;
                if (col >= 0 && col == lastCol) {
                    coolNum++;
                } else {
                    coolNum = 0;
                }
                lastCol = col;
                if (coolNum >= 2) {
                    grid[j, i].BlowUp();
                    grid[j-1, i].BlowUp();
                    grid[j-2, i].BlowUp();

                }
            }
        }
    }

    void MatchCheck2() {
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int j = 0; j < grid.GetLength(1); j++) {
                if (grid[i, j].color >= 0) {
                    grid[i, j].CheckNeighbs(0);//AddUpRightNeighbs();
                }
            }
        }
        /*for (int i = grid.GetLength(0)-1; i >= 0; i--) {
            for (int j = grid.GetLength(1)-1; j >= 0; j--) {
                if (grid[i, j].color >= 0) {
                    grid[i, j].AddBotLeftNeighbs();
                }
            }
        }*/
    }

    void UpdateGrid() {
        int highestBlockGrid = 0;
        for (int i = 0; i < grid.GetLength(0); i++) {

            for (int j = 0; j < grid.GetLength(1); j++) {
                grid[i, j].Update();

                if (grid[i, j].color >= 0 && grid[i,j].ocupied && j > highestBlockGrid)
                {
                    highestBlockGrid = j;
                }

                

                if (grid[i,j].color >= 0 && grid[i, j].ocupied && j >= 11) {

                    //NewSound
                    //Turn warning loop off
                    dangerInstance.stop(0f);
                    AudioDirector.Instance.FadeOutAudio(AudioDirector.Instance.dangerSource, 0.15f);

                    EndGame();
                }
            }
        }
        //NewSound
        if (highestBlockGrid >= 8)
        {
            
            //fade in warning loop, if it's not already
            if (!warningLoopOn)
            {
                if (dangerInstance.isValid())
                {
                    FMOD.Studio.PLAYBACK_STATE playbackstate;
                    dangerInstance.getPlaybackState(out playbackstate);
                    if (playbackstate == FMOD.Studio.PLAYBACK_STATE.STOPPED)
                    {
                        dangerInstance.start();
                    }
                }
                Debug.Log("play warning sound??");
                AudioDirector.Instance.FadeInAudio(
                    AudioDirector.Instance.dangerSource,
                    AudioDirector.Instance.dangerVolume,
                    0.5f);
                warningLoopOn = true;
            }

        }
        else if (warningLoopOn && highestBlockGrid < 8)
        {

            Debug.Log(" fade out warning sound ");
            //turn warning loop off when no longer in danger
            dangerInstance.stop(0f);

            AudioDirector.Instance.FadeOutAudio(
                    AudioDirector.Instance.dangerSource,
                    0.5f);
            warningLoopOn = false;

        }
    }
    void EndGame() {


        Debug.Log("overFilled");
        gameOver = true;

        //NewSound
        if (!playedGameOverSound)
        {
            
            AudioDirector.Instance.PlaySound(AudioDirector.Instance.gameOverSound, false, 0f, AudioDirector.Instance.gameOverVolume, 0f, true);
            AudioDirector.Instance.gameOverSnapshot.TransitionTo(0.5f);
            playedGameOverSound = true;
        }

    }
}


public class GridCell
{
    public int color;
    public Vector2 gridPos;
    public Vector2 worldPos;
    public bool ocupied;
    public FallingBlock myBlock;
    public GridCell[,] grid;
    public bool blowUpFlag;
    public int goodNeighbs;
    public GridCell[] neighbs;
    bool startFlag;
    public bool lookedAt;
    public FMODUnity.EventReference blockeliminateSound;
    public GridCell (Vector2 _gridPos, Vector2 _worldPos, ref GridCell[,] grd) {
        gridPos = _gridPos;
        worldPos = _worldPos;
        color = -1;
        grid = grd;
        startFlag = true;
        neighbs = new GridCell[4];
        blowUpFlag = false;

    }

    public void BlowUp() {
        blowUpFlag = true;
        for (int j = 0; j < 4; j++) {
            if (color == neighbs[j].color && !neighbs[j].blowUpFlag) {
                neighbs[j].BlowUp();
            }
        }
    }
    public void GetBlock(FallingBlock block) {
        block.myCell = this;
        ocupied = true;
        myBlock = block;
        color = block.colNum;
    }
    public int CheckNeighbs(int depth) {
        lookedAt = true;
        bool imADad = false;
        if (depth == 0) {
            imADad = true;
        }
        for (int i = 0; i < 4; i++) {
            if (!neighbs[i].lookedAt && color == neighbs[i].color) {
                depth++;
                depth = neighbs[i].CheckNeighbs(depth);
                //neighbs[i].blowUpFlag = true;
            }
        }
        if (depth >= 3 && imADad) {

            //NewSound
            FMODUnity.RuntimeManager.PlayOneShot(blockeliminateSound);
            //AudioDirector.Instance.PlaySound(AudioDirector.Instance.matchSounds, false, 0, AudioDirector.Instance.matchVolume, 0, true);
            
            BlowUp();
        }
       
        return depth;
    }

    

    public void Update() {
        
        goodNeighbs = 0;
        lookedAt = false;
        if (startFlag && color != -2) {
            neighbs[0] = grid[(int)gridPos.x + 1, (int)gridPos.y];
            neighbs[1] = grid[(int)gridPos.x, (int)gridPos.y + 1];
            neighbs[2] = grid[(int)gridPos.x - 1, (int)gridPos.y];
            neighbs[3] = grid[(int)gridPos.x, (int)gridPos.y - 1];
            startFlag = false;
        }
        if (blowUpFlag) {
            Grid.me.score++;
            CameraControl.me.Flash(Global.me.blockColors[color]);
            myBlock.BlowUp();
            LoseBlock();
            blowUpFlag = false;
            CameraControl.me.Shake(1.2f);
            
        }
        if (ocupied && color >= 0 && !grid[(int)gridPos.x,(int)gridPos.y - 1].ocupied) {
            grid[(int)gridPos.x, (int)gridPos.y-1].GetBlock(myBlock);
            LoseBlock();
        }
        
    }
    public void LoseBlock() {
        ocupied = false;
        myBlock = null;
        color = -1;
    }

}
