using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.SocialPlatforms;

public class Player : Character
{
    protected bool gettingMove;
    protected bool gettingAction;
    protected bool gettingTarget;
    protected bool gettingItem;
    protected bool looting;
    protected bool attacking;
    protected bool levelingUp;
    protected int expCarry;
    protected bool backButton;

    // Start is called before the first frame update
    protected override void Start()
    {
        GameManager.instance.AddCharacterToList(this);

        gettingMove = false;
        gettingTarget = false;
        looting = false;
        attacking = false;
        levelingUp = false;
        backButton = false;
        expCarry = 0;

        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.dialogueInProgress)
            return;

        if (gettingMove)
        {
            gettingMove = MouseManager.instance.GetMoveInput(this, paths);
            if (!gettingMove)
            {
                StartCoroutine(SelectedPath());
            }
        }
        if (gettingTarget)
        {
            gettingTarget = MouseManager.instance.GetTargetInput(this, GetObjects());
            if (!gettingTarget)
            {
                Act();
            }
        }
    }

    protected override IEnumerator NextStep()
    {
        if (!playable) {
            StartCoroutine(base.NextStep());
            yield break;
        }

        // Prevents the rest of the players turn from happening until dialogue is resolved (hopefully)
        if (GameManager.instance.dialogueInProgress)
            yield return new WaitUntil(() => GameManager.instance.dialogueInProgress == false);

        GameManager.instance.CameraTarget(this.gameObject);
        if ((movesLeft != totalMoves || actionsLeft != totalActions) && lastState.moves == movesLeft && lastState.actions == actionsLeft && lastState.position == (Vector2)transform.position)
            MenuManager.instance.HideBackButton();
        else
            MenuManager.instance.ShowBackButton();
        UpdateObjectives();
        GetPaths();
        yield return new WaitForSeconds(moveTime);
        FindPath();
    }

    protected override void UpdateObjectives()
    {
        if (!playable) {
            base.UpdateObjectives();
            return;
        }

        currentObjective = new Objective(null, null);
    }

    protected void GetPaths()
    {
        paths.Clear();
        GetPaths(transform.position, new Vector2[0], movesLeft);
    }

    protected void GetPaths(Vector2 next, Vector2[] path, int remainingMoves) //update with better alg/queue?
    {
        //Debug.Log(next.x + ", " + next.y + "; " + GameManager.instance.Grid[(int) next.x][(int) next.y].Walkable);
        if (Array.Exists(path, element => element == next) || next.x < 0 || next.x >= GameManager.instance.Grid.Count || next.y < 0 || next.y >= GameManager.instance.Grid[0].Count 
            || (next != (Vector2) gameObject.transform.position && !GameManager.instance.Grid[(int) next.x][(int) next.y].Walkable)) { return; }
        Vector2 previous = ((path.Length == 0) ? (Vector2)transform.position : path[path.Length - 1]);
        boxCollider.enabled = false;
        RaycastHit2D[] hits = Physics2D.LinecastAll(previous, next, Collisions);
        boxCollider.enabled = true;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform != null && hit.transform.gameObject.GetComponent<InteractableObject>() != null && !hit.transform.gameObject.GetComponent<InteractableObject>().walkOver)
                return;
        }

        Vector2[] newPath = new Vector2[path.Length + 1];
        Array.Copy(path, newPath, path.Length);
        newPath[newPath.Length - 1] = next;
        if (!paths.ContainsKey(next)) { paths.Add(next, newPath); }
        else
        {
            paths.TryGetValue(next, out path);
            if (newPath.Length < path.Length)
            {
                paths.Remove(next);
                paths.Add(next, newPath);
            }
            else
            {
                return;
            }
        }

        remainingMoves--;
        if (remainingMoves >= 0)
        {
            GetPaths(next + new Vector2(1, 0), newPath, remainingMoves);
            GetPaths(next + new Vector2(-1, 0), newPath, remainingMoves);
            GetPaths(next + new Vector2(0, 1), newPath, remainingMoves);
            GetPaths(next + new Vector2(0, -1), newPath, remainingMoves);
        }
    }

    protected override void FindPath()
    {
        if (!playable) {
            base.FindPath();
            return;
        }

        if (paths.Count == 1 && !backButton)
        {
            paths.TryGetValue(transform.position, out pathToObjective);
            StartCoroutine(SelectedPath());
        }
        else
        {
            MenuManager.instance.ShowPaths(paths);
            gettingMove = true;
            backButton = false;
            Debug.Log("Player waiting for move input");
        }
    }

    protected IEnumerator SelectedPath()
    {
        if (pathToObjective.Length > 1)
        {
            pathToObjective = pathToObjective.Skip(1).ToArray();
            MenuManager.instance.HideBackButton();
            yield return StartCoroutine(FollowPath());
            // StartCoroutine(NextStep());
        }
        // else
        // {
            GetAvailableTargets();
            GetAvailableActions();
            SelectAction();
        // }
    }

    protected void SelectAction()
    {
        if (actions.Count <= 1 && lastState.actions == 0 && lastState.moves == 0)
            StartCoroutine(EndTurn());
        else
        {
            MenuManager.instance.SetupActionMenu(actions);
            MenuManager.instance.ShowBackButton();
            gettingAction = true;
        }
    }

    public void GetActionInput(string action)
    {
        MenuManager.instance.HideActionMenu();
        if (currentObjective == null) { //temp due to weird error
            Debug.Log("Objective Null!");
            currentObjective = new Objective(null, null);
        }
        currentObjective.action = action;
        gettingAction = false;
        if (currentObjective.action != "Wait")
            SelectTarget();
        else
            Act();
    }

    protected void SelectTarget()
    {
        MenuManager.instance.ShowObjects(GetObjects());
        gettingTarget = true;
        Debug.Log("Player waiting for target input");
    }

    private List<InteractableObject> GetObjects()
    {
        List<InteractableObject> objects;
        actableObjects.TryGetValue(currentObjective.action, out objects);
        return objects;
    }

    protected override void SelectItem(String type)
    {
        if (!playable) {
            base.SelectItem(type);
            return;
        }

        MenuManager.instance.ShowPlayerInventory(type, inventory, type == "Weapon" ? GetDistance(currentObjective.target) : 0);
        gettingItem = true;
        attacking = (type == "Weapon");
        Debug.Log("Player waiting for item input");
    }

    public override IEnumerator ChooseItem(HoldableObject item)
    {
        if (looting)
        {
            Storage storage = currentObjective.target.gameObject.GetComponent<Storage>();
            if (storage != null)
            {
                if (storage.items.Contains(item)) {
                    storage.Remove(item);
                    MenuManager.instance.ShowOtherInventory("", inventory, 0, storage.items, storage.name
                        .Replace("(Clone)", "").Replace("1", "").Replace("2", "").Replace("3", "").Replace("4", "")
                        .Replace("5", "").Replace("6", "").Replace("7", "").Replace("8", "").Replace("9", "").Replace("0", ""));
                    Pickup(item);
                } else {
                    Remove(item);
                    storage.items.Add(item);
                    MenuManager.instance.ShowOtherInventory("", inventory, 0, storage.items, storage.name
                        .Replace("(Clone)", "").Replace("1", "").Replace("2", "").Replace("3", "").Replace("4", "")
                        .Replace("5", "").Replace("6", "").Replace("7", "").Replace("8", "").Replace("9", "").Replace("0", ""));
                }
            }
            else
            {
                weightStolen += item.weight;
                Player character = currentObjective.target.gameObject.GetComponent<Player>();
                if (CaughtStealing(character))
                {
                    character.Enemy(this);
                    Back();
                    yield break;
                }
                if (inventory.Contains(item)) {
                    Remove(item);
                    character.Pickup(item);
                }
                else {
                    character.Remove(item);
                    Pickup(item);
                }
                MenuManager.instance.ShowOtherInventory("", inventory, 0, character.inventory, character.name.Replace("(Clone)", ""));
            }
            MenuManager.instance.ShowPlayerInventory("", inventory, 0, inventory, name);
            actionsLeft--;
            UpdateState();
            yield break;
        }
        
        gettingItem = false;
        MenuManager.instance.HideInventories();
        StartCoroutine(base.ChooseItem(item));
    }

    public void PreviewItem(HoldableObject item, Vector3 position)
    {
        if (!attacking)
            return;
        int damage = GetDamage(currentObjective.target, item);
        int hitPercent = HitPercent(currentObjective.target, item);
        int critPercent = CritPercent(item);
        MenuManager.instance.ShowAttackInfo(position, damage, hitPercent, critPercent);
    }

    protected override void Loot(InteractableObject toLoot)
    {
        looting = true;
        GameManager.instance.CameraTarget(toLoot.gameObject);
        Storage storage = toLoot.gameObject.GetComponent<Storage>();
        storage.Open();
        MenuManager.instance.ShowPlayerInventory("", inventory, 0, inventory, name);
        MenuManager.instance.ShowOtherInventory("", inventory, 0, storage.items, toLoot.name
            .Replace("(Clone)", "").Replace("1", "").Replace("2", "").Replace("3", "").Replace("4", "")
            .Replace("5", "").Replace("6", "").Replace("7", "").Replace("8", "").Replace("9", "").Replace("0", ""));
    }

    protected override void Steal(InteractableObject toStealFrom)
    {
        looting = true;
        this.weightStolen = 0;
        GameManager.instance.CameraTarget(toStealFrom.gameObject);
        Player character = toStealFrom.gameObject.GetComponent<Player>();
        MenuManager.instance.ShowPlayerInventory("", inventory, 0, inventory, name);
        MenuManager.instance.ShowOtherInventory("", inventory, 0, character.inventory, toStealFrom.name.Replace("(Clone)", ""));
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Exit")
        {
            Invoke("Restart", 1f);
            enabled = false;
        }

        base.OnTriggerEnter2D(other);
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void CheckIfGameOver()
    {
        if (health <= 0)
        {
            GameManager.instance.GameOver();
        }
    }

    protected override void TalkTo(InteractableObject toTalkTo)
    {
        MenuManager.instance.HideBackButton();
        base.TalkTo(toTalkTo);
    }

    public void Back()
    {
        backButton = true;
        attacking = false;

        if (levelingUp) {
            levelingUp = false;
            MenuManager.instance.HideLevelUp();
            MenuManager.instance.HideBackButton();
            if (levelingUpPlayer.expCarry > 0)
                StartCoroutine(levelingUpPlayer.UpdateExp(expCarry));
            else
                StartCoroutine(NextStep());
            return;
        }

        if (looting)
        {
            looting = false;
            Storage storage = currentObjective.target.gameObject.GetComponent<Storage>();
            if (storage != null)
                storage.Close();
            MenuManager.instance.HideInventories();
            MenuManager.instance.HideBackButton();
            StartCoroutine(NextStep());
            return;
        }
        
        if (gettingMove)
        {
            if (movesLeft == totalMoves && actionsLeft == totalActions && lastState.moves == movesLeft && lastState.actions == actionsLeft && lastState.position == (Vector2)transform.position)
            {
                MenuManager.instance.HideIndicators();
                MenuManager.instance.HideBackButton();
                isTurn = false;
                gettingMove = false;
                StartCoroutine(GameManager.instance.NextTurn());
                return;
            }
            else
            {
                MenuManager.instance.HideIndicators();
                gettingMove = false;
                ResetState();
            }
        }
        else if (gettingAction)
        {
            MenuManager.instance.HideActionMenu();
            gettingAction = false;
        }
        else if (gettingTarget)
        {
            MenuManager.instance.HideIndicators();
            gettingTarget = false;
        }
        else if (gettingItem)
        {
            MenuManager.instance.HideInventories();
            gettingItem = false;
        }
        StartCoroutine(NextStep());
    }

    protected void ResetState()
    {
        health = lastState.health;
        movesLeft = lastState.moves;
        actionsLeft = lastState.actions;
        ErasePosition();
        transform.position = lastState.position;
        UpdatePosition();
        StartCoroutine(UpdateHealthBar(false));
        CheckRoof();
    }

    protected override IEnumerator EndTurn()
    {
        MenuManager.instance.HideBackButton();
        yield return StartCoroutine(base.EndTurn());
    }

    public IEnumerator UpdateExp(int amount)
    {
        Debug.Log("Current Exp: " + experience + ", Exp Gained: " + amount);
        expCarry = Math.Max(0, experience + amount - 100);
        yield return StartCoroutine(MenuManager.instance.UpdateExperience(experience, amount));
        experience = Math.Min(experience + amount, 100);
        if (experience == 100)
            StartCoroutine(LevelUp());
        else
            GameManager.instance.activeCharacter.Back();
    }

    public IEnumerator LevelUp()
    {
        levelingUp = true;
        MenuManager.instance.ShowLevelUp(this);

        int i = 0;
        level++;
        experience = 0;
        yield return StartCoroutine(MenuManager.instance.LevelUpStat(i, level));

        i++;
        if (Random.Range(0,100) <= statUpgradePercent) {
            stats.health++;
            yield return StartCoroutine(MenuManager.instance.LevelUpStat(i, stats.health));
        }
        i++;
        if (Random.Range(0,100) <= statUpgradePercent) {
            stats.agility++;
            yield return StartCoroutine(MenuManager.instance.LevelUpStat(i, stats.agility));
        }
        i++;
        if (Random.Range(0,100) <= statUpgradePercent) {
            stats.strength++;
            yield return StartCoroutine(MenuManager.instance.LevelUpStat(i, stats.strength));
        }
        i++;
        if (Random.Range(0,100) <= statUpgradePercent) {
            stats.magic++;
            yield return StartCoroutine(MenuManager.instance.LevelUpStat(i, stats.magic));
        }
        i++;
        if (Random.Range(0,100) <= statUpgradePercent) {
            stats.defense++;
            yield return StartCoroutine(MenuManager.instance.LevelUpStat(i, stats.defense));
        }
        i++;
        if (Random.Range(0,100) <= statUpgradePercent) {
            stats.resistance++;
            yield return StartCoroutine(MenuManager.instance.LevelUpStat(i, stats.resistance));
        }
        i++;
        if (Random.Range(0,100) <= statUpgradePercent) {
            stats.skill++;
            yield return StartCoroutine(MenuManager.instance.LevelUpStat(i, stats.skill));
        }
        i++;
        if (Random.Range(0,100) <= statUpgradePercent) {
            stats.dexterity++;
            yield return StartCoroutine(MenuManager.instance.LevelUpStat(i, stats.dexterity));
        }

        MenuManager.instance.ShowBackButton();
    }
}
