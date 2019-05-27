using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    public GameObject stonePrefab;
    public GameObject cellPrefab;
    public GameObject canvas;

    private Camera cam;
    int turnCount = 0;
    int turnPlayer = 1; // 0:無し 1:黒　2:白
    int notTurnPlayer = 2; // 0:無し 1:黒　2:白

    int blackPoint = 0;
    int whitePoint = 0;

    private int[,,] gameField = new int[,,]{ // 色(0:無し 1:黒　2:白)　と　点数
        { { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0} },
        { { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0} },
        { { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0} },
        { { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0} },
        { { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0} },
        { { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0} },
        { { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0} },
        { { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0}, { 0, 0} }
    };


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        // 横位置、縦位置、色(0:無し 1:黒　2:白)
        genarateStone(4, 4, 2);
        genarateStone(5, 4, 1);
        genarateStone(4, 5, 1);
        genarateStone(5, 5, 2);

        isGameOver();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Fire1"))
        {
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = 15;
            Vector3 v = cam.ScreenToWorldPoint(screenPoint);
            int key_x = (int)(Mathf.Floor(v.x) + 5.0f);
            int key_y = (int)(9-(Mathf.Floor(v.z) + 5.0f));

            print("v: " + v);
            print("x:z = " + key_x + ":" + key_y);

            // クリックしたのが盤面上で空いていれば石置き処理
            if ((key_x <= 8) && (key_x >= 1) && (key_y <= 8) && (key_y >= 1))
            {
                if (gameField[key_x - 1, key_y - 1, 0] == 0)
                {
                    putStone(key_x, key_y);
                }
            }
        }
    }

    // 石生成処理
    void genarateStone(int x, int y, int stoneColler)
    {
        float xPos = x - 4.5f;
        float yPos = (9 - y) - 4.5f;

        GameObject stone = Instantiate(stonePrefab, new Vector3(xPos, 0.01f, yPos), Quaternion.identity);
        stone.name = "stone" + "_" + x + "_" + y;
        stone.transform.parent = this.transform;

        if (stoneColler == 1 )
        {
            stone.GetComponent<Renderer>().material.color = Color.black;
            stone.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().color = Color.white;
            blackPoint += 1;
        }
        else if (stoneColler == 2)
        {
            whitePoint += 1;
        }

        gameField[x-1, y-1, 0] = stoneColler;
        gameField[x-1, y-1, 1] = 1;
    }

    // ターン経過処理
    void changeTurn()
    {
        turnCount += 1;

        if (turnPlayer == 1)
        {
            turnPlayer = 2;
            notTurnPlayer = 1;
            canvas.transform.GetChild(0).GetComponent<Text>().color = new Color(1, 1, 1, 1);
            canvas.transform.GetChild(0).GetComponent<Text>().text = "白の手番";
        }
        else if (turnPlayer == 2)
        {
            turnPlayer = 1;
            notTurnPlayer = 2;
            canvas.transform.GetChild(0).GetComponent<Text>().color = new Color(0, 0, 0, 1);
            canvas.transform.GetChild(0).GetComponent<Text>().text = "黒の手番";
        }

        GameObject[] cells = GameObject.FindGameObjectsWithTag("cell");
        foreach (GameObject cell in cells)
        {
            Destroy(cell);
        }

        isGameOver();
    }

    // 石が置けるかチェック（ゲームオーバーチェック）
    void isGameOver()
    {
        bool isPossiblePut = false;

        for (int col = 0; col < 8; col++)
        {
            for (int row = 0; row < 8; row++)
            {
                if (gameField[col, row, 0] == 0)
                {
                    bool[] isPossiblePutDir = isPutStone(col + 1, row + 1);

                    foreach (var item in isPossiblePutDir)
                    {
                        if (item)
                        {
                            isPossiblePut = true;
                            
                            float xPos = col - 3.5f;
                            float yPos = (8 - row) - 4.5f;
                            GameObject cell = Instantiate(cellPrefab, new Vector3(xPos, 0.01f, yPos), Quaternion.identity);
                        }
                    }
                }
            }
        }

        if (!isPossiblePut)
        {
            string resultText = "";

            if (blackPoint == whitePoint)
            {
                resultText = "引き分け";
            }
            else if (blackPoint > whitePoint)
            {
                resultText = "黒の勝ち";
            }
            else if (blackPoint < whitePoint)
            {
                resultText = "白の勝ち";
            }

            canvas.transform.GetChild(0).GetComponent<Text>().text = "GAME OVER\n" + resultText;
        }
    }


    // 石を置く処理
    void putStone(int key_x, int key_y)
    {
        bool isPossiblePut = false;
        bool[] isPossiblePutDir = isPutStone(key_x, key_y);

        foreach (var item in isPossiblePutDir)
        {
            if (item)
            {
                isPossiblePut = true;
                break;
            }
        }

        // 石を置いてひっくり返す処理
        if (isPossiblePut)
        {
            genarateStone(key_x, key_y, turnPlayer);

            for (int i = 0; i < 8; i++)
            {
                int checkPosX = key_x;
                int checkPosY = key_y;

                if (isPossiblePutDir[i])
                {
                    for (int ii = 0; ii < 8; ii++)
                    {
                        int[] checkPos = checkPosDir(i, checkPosX, checkPosY);
                        checkPosX = checkPos[0];
                        checkPosY = checkPos[1];
                        
                        // プレイヤーの石があれば終了
                        if (gameField[checkPosX - 1, checkPosY - 1, 0] == turnPlayer)
                        {
                            break;
                        }

                        // 対戦相手の石があればひっくり返す
                        if (gameField[checkPosX - 1, checkPosY - 1, 0] == notTurnPlayer)
                        {
                            rebersiStone(checkPosX, checkPosY);
                        }
                    }
                }
            }

            changeTurn();
        }
    }

    // 石が置けるかチェック
    bool[] isPutStone(int key_x, int key_y)
    {
        bool[] isPossiblePutDir = new bool[] { false, false, false, false, false, false, false, false };

        // 置けるかチェック
        for (int i = 0; i < 8; i++)
        {
            int checkPosX = key_x;
            int checkPosY = key_y;
            for (int ii = 0; ii < 8; ii++)
            {
                int[] checkPos = checkPosDir(i, checkPosX, checkPosY);
                checkPosX = checkPos[0];
                checkPosY = checkPos[1];

                // 盤からはみ出たら終了
                if ((checkPosX > 8) || (checkPosX < 1) || (checkPosY > 8) || (checkPosY < 1))
                {
                    break;
                }

                // 1マス目でプレイヤーの石があれば終了
                if (gameField[checkPosX - 1, checkPosY - 1, 0] == turnPlayer && ii == 0)
                {
                    break;
                }

                // 空白マスであれば終了
                if (gameField[checkPosX - 1, checkPosY - 1, 0] == 0)
                {
                    break;
                }

                // 2マス目以降でプレイヤーの石があれば終了
                if (gameField[checkPosX - 1, checkPosY - 1, 0] == turnPlayer)
                {
                    isPossiblePutDir[i] = true;
                }
            }
        }

        return isPossiblePutDir;
    }

    // 石をひっくり返す処理
    void rebersiStone(int checkPosX, int checkPosY)
    {
        string stoneName = "stone" + "_" + checkPosX + "_" + checkPosY;
        GameObject stone = this.transform.Find(stoneName).gameObject;

        int stonePoint = gameField[checkPosX - 1, checkPosY - 1, 1] + 1;
        stone.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = stonePoint.ToString();
        
        if (turnPlayer == 1)
        {
            stone.GetComponent<Renderer>().material.color = Color.black;
            stone.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().color = Color.white;
            blackPoint += stonePoint;
            whitePoint -= (stonePoint - 1);
            gameField[checkPosX - 1, checkPosY - 1, 0] = 1;
        }
        else if (turnPlayer == 2)
        {
            stone.GetComponent<Renderer>().material.color = Color.white;
            stone.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().color = Color.black;
            blackPoint -= (stonePoint - 1);
            whitePoint += stonePoint;
            gameField[checkPosX - 1, checkPosY - 1, 0] = 2;
        }

        gameField[checkPosX - 1, checkPosY - 1, 1] = stonePoint;
        canvas.transform.GetChild(1).GetComponent<Text>().text = "黒 " + blackPoint + "点";
        canvas.transform.GetChild(2).GetComponent<Text>().text = "白 " + whitePoint + "点";

        print("whitePoint:blackPoint => " + whitePoint + ":" + blackPoint);
    }

    // チェックする方角に応じて座標を返す
    int[] checkPosDir(int dir, int checkPosX, int checkPosY)
    {
        switch (dir)
        {
            case 0: // 上
                checkPosY -= 1;
                break;
            case 1: // 右上
                checkPosX += 1;
                checkPosY -= 1;
                break;
            case 2: // 右
                checkPosX += 1;
                break;
            case 3: // 右下
                checkPosX += 1;
                checkPosY += 1;
                break;
            case 4: // 下
                checkPosY += 1;
                break;
            case 5: // 左下
                checkPosX -= 1;
                checkPosY += 1;
                break;
            case 6: // 左
                checkPosX -= 1;
                break;
            case 7: // 左上
                checkPosX -= 1;
                checkPosY -= 1;
                break;
            default:
                break;
        }

        int[] checkPos = new int[]{ checkPosX, checkPosY };
        return checkPos;
    }

}
