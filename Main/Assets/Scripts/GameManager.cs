using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AStarSharp;
using System;
using UnityEngine.SocialPlatforms;

public class GameManager : MonoBehaviour
{
    public List<List<Node>> Grid;

    public int level = 0;
    public float levelStartDelay;
    public float turnDelay;
    public static GameManager instance = null;

    private Text objectiveText;
    private GameObject levelImage;
    private BoardManager boardScript;
    public List<Player> characters;
    private bool doingSetup;
    private bool zooming;

    public bool gettingNextCharacter;
    public Player activeCharacter;
    public bool dialogueInProgress;
    public int round;
    private bool playerTurn;
    public bool playerStart;
    public List<String> objectives;
    public int objective;
    public bool auto;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        // DontDestroyOnLoad(gameObject);
        characters = new List<Player>();
        boardScript = GetComponent<BoardManager>();
        dialogueInProgress = false;
        playerTurn = playerStart;
        zooming = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!doingSetup && gettingNextCharacter)
        {
            if (auto)
                StartCoroutine(PlayerSelectCharacter());
            else {
                gettingNextCharacter = GameObject.Find("MouseManager").GetComponent<MouseManager>().GetNextCharacter(GetPlayableCharacters());
                if (!gettingNextCharacter)
                    StartCoroutine(StartTurn());
            }
        }

        if (!doingSetup && activeCharacter == null && !gettingNextCharacter)
        {
            StartCoroutine(NextTurn());
        }
    }

    private void SetAlliances() {
        //temporary while there are no "neutral" CPUs
        foreach (Character character in GetPlayableCharacters(true))
        {
            foreach (Character other in GetPlayableCharacters(true))
            {
                if (character != other)
                    character.Ally(other);
            }
        }
        foreach (Character character in GetNonplayableCharacters())
        {
            foreach (Character other in GetNonplayableCharacters())
            {
                if (character != other)
                    character.Ally(other);
            }
        }
        GetPlayableCharacters(true)[0].Enemy(GetNonplayableCharacters()[0]);
        doingSetup = false;
        SoundManager.instance.PlayMusic(1);
    }

    public void CheckRoofs() {
        foreach (BoardManager.Roof roof in boardScript.GetRoofs()) {
            bool overPlayableCharacter = false;
            foreach (Player player in GetPlayableCharacters(true)) {
                if (roof.positions.Contains(new Vector2Int((int) player.gameObject.transform.position.x, (int) player.gameObject.transform.position.y))) {
                    overPlayableCharacter = true;
                }
            }
            roof.tileRenderer.enabled = !overPlayableCharacter;
        }
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        level++;
        InitGame();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void InitGame()
    {
        doingSetup = true;

        //levelImage = GameObject.Find("LevelImage"); 
        objectiveText = GameObject.Find("ObjectiveText").GetComponent<Text>();
        objectiveText.text = objectives[objective];
        objectiveText.alignment = TextAnchor.MiddleCenter;
        objectiveText.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        objectiveText.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        objectiveText.gameObject.SetActive(true);
        //Invoke("UpdateObjectiveText", levelStartDelay);
        Invoke("SetAlliances", levelStartDelay);

        characters.Clear();
        Grid = boardScript.SetupScene(level);
        round = 0;
        SoundManager.instance.Startup();
    }

    private void UpdateObjectiveText()
    {
        objectiveText.text = "Enemies Remaining: " + GetNonplayableCharacters(true).Count;
        objectiveText.alignment = TextAnchor.MiddleRight;
        objectiveText.GetComponent<RectTransform>().localScale = new Vector3(0.5f, 0.5f, 1);
        objectiveText.GetComponent<RectTransform>().localPosition = new Vector3(430, 260, 0);
    }

    public void GameOver()
    {
        objectiveText.text = "Game Over";
        objectiveText.gameObject.SetActive(true);
        enabled = false;
    }

    public IEnumerator PlayerSelectCharacter()
    {
        if (auto) {
            gettingNextCharacter = false;
            MenuManager.instance.HideIndicators();
            List<Player> playableCharacters = GetPlayableCharacters();

            int i = new System.Random().Next(playableCharacters.Count);
            activeCharacter = playableCharacters[i];

            StartCoroutine(StartTurn(true));
            yield break;
        }

        Dictionary<Vector2, Vector2[]> paths = new Dictionary<Vector2, Vector2[]>();
        foreach (Player character in GetPlayableCharacters())
        {
            paths.Add((Vector2)character.transform.position, new Vector2[] { (Vector2)character.transform.position });
        }
        GameObject.Find("MenuManager").GetComponent<MenuManager>().ShowPaths(paths);            

        gettingNextCharacter = true;
        CameraTarget(GetPlayableCharacters()[0].gameObject); //temp until camera follows mouse
        yield return new WaitForSeconds(turnDelay);
        if (gettingNextCharacter)
            GameObject.Find("Main Camera").GetComponent<CameraManager>().FollowMouse();
    }

    public void AISelectCharacter()
    {
        List<Player> nonplayableCharacters = GetNonplayableCharacters();

        int i = new System.Random().Next(nonplayableCharacters.Count);
        activeCharacter = nonplayableCharacters[i];

        StartCoroutine(StartTurn(true));
    }

    public IEnumerator NextTurn()
    {
        UpdateObjectiveText();
        if (GetNonplayableCharacters(true).Count == 0)
        {
            LevelOver(true);
            yield break;
        } else if (GetPlayableCharacters(true).Count == 0) {
            LevelOver(false);
            yield break;
        }

        if (GetPlayableCharacters().Count == 0)
        {
            if (playerStart)
                playerTurn = false;
            else
            {
                NextRound();
                yield break;
            }
        }
        
        if (GetNonplayableCharacters().Count == 0)
        {
            if (!playerStart)
                playerTurn = true;
            else
            {
                NextRound();
                yield break;
            }
        }

        if (playerTurn)
        {
            StartCoroutine(PlayerSelectCharacter());
        } else {
            //yield return new WaitForSeconds(turnDelay);
            AISelectCharacter();
        }
    }

    private IEnumerator StartTurn(bool delay = false)
    {
        Debug.Log("Active Character: " + activeCharacter);
        CameraTarget(activeCharacter.gameObject);
        GameObject.Find("Main Camera").GetComponent<CameraManager>().FollowTarget();
        if (delay)
            yield return new WaitForSeconds(turnDelay);
        activeCharacter.StartTurn();
    }

    public void NextRound()
    {
        round++;
        playerTurn = playerStart;

        foreach (Player character in characters)
        {
            character.SetHasGone(false);
        }

        StartCoroutine(NextTurn());
    }

    public void AddCharacterToList(Player character)
    {
        characters.Add(character);
    }

    public bool IsPlayableCharacter(Player character) {
        return GetPlayableCharacters().Contains(character);
    }

    private List<Player> GetPlayableCharacters(bool ignoreGone = false)
    {
        List<Player> playableCharacters = new List<Player>();
        foreach (Player character in characters)
        {
            if (character.playable && (ignoreGone || !character.HasGone()))
            {
                playableCharacters.Add(character);
            }
        }

        return playableCharacters;
    }

    private List<Player> GetNonplayableCharacters(bool ignoreGone = false)
    {
        List<Player> nonplayableCharacters = new List<Player>();
        foreach (Player character in characters)
        {
            if (!character.playable && (ignoreGone || !character.HasGone()))
            //if (!character.playable && !character.hasGone && character.currentObjective != null && character.objectives.Count > 0)
            {
                nonplayableCharacters.Add(character);
            }
        }

        return nonplayableCharacters;
    }

    private void LevelOver(bool won)
    {
        objectiveText.text = won ? "Objective Complete" : "Objective Failed";
        objectiveText.alignment = TextAnchor.MiddleCenter;
        objectiveText.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        objectiveText.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        SoundManager.instance.Finish(won);
    }

    public void RemoveDeadCharacters()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].GetHealth() <= 0)
                characters.RemoveAt(i);
        }
    }

    public void UpdateNode(Vector2 position, bool damageable, float health)
    {
        Grid[(int)position.x][(int)position.y] = new Node(position, damageable, health + 1);
    }

    public void CameraTarget(GameObject toTarget, float zoom = 1)
    {
        Camera.main.GetComponent<CameraManager>().Target(toTarget);
        StartCoroutine(CameraZoom(zoom));
    }

    private IEnumerator CameraZoom(float zoom) {
        while (zooming == true) {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Zooming to: " + zoom);
        zooming = true;
        while (Math.Abs(Camera.main.orthographicSize - (Camera.main.orthographicSize*0.99f + zoom*5*0.01f)) > 0.0001) {
            Camera.main.orthographicSize = Camera.main.orthographicSize*0.99f + zoom*5*0.01f;
            yield return new WaitForEndOfFrame();
        }
        Camera.main.orthographicSize = zoom*5;
        zooming = false;
    }
}
