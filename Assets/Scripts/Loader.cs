using UnityEngine.UI;
using UnityEngine;

public class Loader : MonoBehaviour {
    GameManager gm;

    public Shader normalShader;
    public Shader selectedShader;
    public Shader highlightShader;
    public GameObject selectorObj;
    public GameObject board;
    public GameObject whiteText;
    public GameObject blackText;
    public GameObject gameOver;
    public bool isAi = true;

    // Use this for initialization
    void Awake () {
        gm = GameManager.Instance;
        gm.normalShader = normalShader;
        gm.selectedShader = selectedShader;
        gm.highlightShader = highlightShader;
        gm.selectorObj = selectorObj;
        gm.board = board.GetComponent<Board>();
        board.GetComponent<Board>().Init();
        gm.whiteText = whiteText.GetComponent<Text>();
        gm.blackText = blackText.GetComponent<Text>();
        gm.gameOverText = gameOver.GetComponent<Text>();
        gm.isAi = isAi;
        if (isAi)
        {
            AIBehaviour ai = ScriptableObject.CreateInstance<AIBehaviour>();
            gm.ai = ai;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
