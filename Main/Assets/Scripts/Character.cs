using AStarSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;

public class Character : InteractableObject
{
    public class Objective
    {
        public InteractableObject target;
        public string action;

        public Objective(InteractableObject target, string action)
        {
            this.target = target;
            this.action = action;
        }
    }

    public class State
    {
        public double health;
        public int moves;
        public int actions;
        public Vector2 position;

        public State(double health, int moves, int actions, Vector2 position)
        {
            this.health = health;
            this.moves = moves;
            this.actions = actions;
            this.position = position;
        }
    }

    public class Stats
    {   public int health;
        public int agility;
        public int strength;
        public int magic;
        public int defense;
        public int resistance;
        public int skill;
        public int dexterity;

        public Stats(int min, int max)
        {
            health = Random.Range(min,max);
            strength = Random.Range(min,max);
            magic = Random.Range(min,max);
            defense = Random.Range(min,max);
            resistance = Random.Range(min,max);
            skill = Random.Range(min,max);
            dexterity = Random.Range(min,max);
            agility = Random.Range(min,max);
        }
    }

    public bool playable;
    public bool talkable;
    public Sprite portrait;
    public string name;
    public float moveTime;
    public int totalMoves;
    public int talkingRange;
    public int totalActions;
    public float actionDelay;
    public int level;
    public List<Objective> objectives;
    public List<HoldableObject> inventory;

    protected bool subdued;
    protected Objective currentObjective;
    protected bool isTurn;
    protected bool hasGone;
    protected bool isMoving;
    protected Animator animator;
    private float inverseMoveTime;
    protected Vector2[] pathToObjective;
    protected Dictionary<Vector2, Vector2[]> paths;
    protected Dictionary<String, List<InteractableObject>> actableObjects;
    protected SortedSet<String> actions;
    protected int attackRange;
    protected List<InteractableObject> allies;
    protected List<InteractableObject> enemies;
    protected bool destructive; //will destroy objects in path
    protected int movesLeft;
    protected int actionsLeft;
    protected State lastState;
    protected int weightStolen;
    protected double subduedRatio = 0.25;
    protected Stats stats;
    protected int experience;
    protected int expPerLevelGap = 100;
    protected double statUpgradePercent = 35;
    protected Player levelingUpPlayer;


    // Start is called before the first frame update
    protected override void Start()
    {
        stats = new Stats(5, 15);
        maxHealth = stats.health;

        isTurn = false;
        isMoving = false;
        subdued = false;
        destructive = true; //make it start false unless agitated?

        animator = GetComponent<Animator>();
        inverseMoveTime = 1 / moveTime;
        objectives = new List<Objective>();
        paths = new Dictionary<Vector2, Vector2[]>();
        actableObjects = new Dictionary<String, List<InteractableObject>>();
        actions = new SortedSet<String>();
        allies = new List<InteractableObject>();
        allies.Add(this);
        enemies = new List<InteractableObject>();

        for (int i = 0; i < inventory.Count; i++)
        {
            inventory[i] = Instantiate(inventory[i]);
        }

        base.Start();
    }

    public virtual void StartTurn()
    {
        isTurn = true;
        movesLeft = totalMoves;
        actionsLeft = totalActions;
        CheckSpace();
        UpdateState();
        StartCoroutine(NextStep());
    }

    protected virtual IEnumerator NextStep()
    {
        GameManager.instance.CameraTarget(this.gameObject);

        if (subdued)
        {
            Debug.Log("Subdued! Not Acting.");
            yield return new WaitForSeconds(actionDelay);
            StartCoroutine(EndTurn());
        }

        else
        {
            UpdateObjectives();
            LogObjectives();
            FindPath();
            yield return new WaitForSeconds(moveTime);
            if (pathToObjective.Length > 0)
            {
                yield return StartCoroutine(FollowPath());
                StartCoroutine(NextStep());
            }
            else
            {
                GetAvailableTargets();
                GetAvailableActions();
                Act();
            }
        }
    }

    protected void LogObjectives()
    {
        Debug.Log("Current Objective: " + currentObjective.target + ": " + currentObjective.action);
        String actions = "Objectives Queue: ";
        foreach (Objective objective in objectives)
        {
            actions += objective.target + ": " + objective.action + ", ";
        }
        Debug.Log(actions);
    }

    protected virtual void UpdateObjectives()
    {
        Objective healClosest = new Objective(GetClosest(allies), "Heal");
        if (healClosest.target != null && healClosest.target.IsDamaged() && HasItemType("Medicine") && !HasObjective(healClosest))
        {
            objectives.Prepend(currentObjective);
            currentObjective = healClosest;
        }

        Objective attackClosest = new Objective(GetClosest(enemies), "Attack");
        if (attackClosest.target != null && destructive && !subdued && HasItemType("Weapon") && !HasObjective(attackClosest))
            objectives.Add(attackClosest);

        if (currentObjective == null && objectives.Count > 0)
        {
            currentObjective = objectives[0];
            objectives.Remove(currentObjective);
        }

        if (currentObjective == null || movesLeft <= 0 || (GetDistance(currentObjective.target) <= 1 && actionsLeft == 0))
        {
            if (currentObjective != null && currentObjective.action != "Wait")
                objectives.Prepend(new Objective(currentObjective.target, currentObjective.action));
            currentObjective = new Objective(this, "Wait");
        }
    }

    protected virtual bool HasObjective(Objective toCheck)
    {
        if (currentObjective != null && currentObjective.target == toCheck.target && currentObjective.action == toCheck.action) { return true; }
        foreach (Objective objective in objectives)
            if (objective.target == toCheck.target && objective.action == toCheck.action) { return true; }

        return false;
    }

    protected virtual InteractableObject GetClosest(List<InteractableObject> objects)
    {
        float minDistance = 99999;
        InteractableObject closest = null;

        foreach (InteractableObject o in objects)
        {
            if (o.GetHealth() <= 0)
                continue;
            float distance = GetDistance(o);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = o;
            }
        }

        return closest;
    }

    protected void LogPaths()
    {
        String actions = "Available Moves (" + paths.Count + "): ";
        foreach(KeyValuePair<Vector2, Vector2[]> entry in paths)
            actions += entry.Key + ", ";
        Debug.Log(actions);
    }

    virtual protected void FindPath()
    {
        List<HoldableObject> weapons = MenuManager.instance.SortedInventory("Weapon", inventory);
        Astar astar = new Astar(GameManager.instance.Grid);
        Stack<Node> path = astar.FindPath(transform.position, currentObjective.target.transform.position, destructive, totalMoves, weapons);
        int space = 1; //temporarily until adjust for targets you can stand on (interactable  vs holdable)
        pathToObjective = new Vector2[Math.Min(movesLeft, path.Count - space)];
        if (pathToObjective.Length == 0) { return; }
        int i = 0;

        foreach (Node node in path)
        {
            if (node.Weight > 1 || node.Weight == -1) //node.Weight == -1 signifies door
            {
                boxCollider.enabled = false;
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(node.Position, 0.5f);
                boxCollider.enabled = true;
                foreach (Collider2D hitCollider in hitColliders)
                {
                    InteractableObject newTarget = hitCollider.GetComponent<InteractableObject>();
                    if (newTarget != null && newTarget != currentObjective.target)
                    {
                        objectives.Prepend(new Objective(currentObjective.target, currentObjective.action));
                        currentObjective = new Objective(newTarget, newTarget.tag == "Door" ? "Door" : "Attack");
                        Debug.Log("Obstacle: " + currentObjective.target + ": " + currentObjective.action);
                        Array.Resize(ref pathToObjective, i);
                        return;
                    }
                }
            }

            pathToObjective[i] = node.Position;
            i++;
            if (i >= pathToObjective.Length)
                return;
        }
    }

    protected IEnumerator FollowPath()
    {
        isMoving = true;
        ErasePosition();
        string lastState = "";
        foreach (Vector2 coords in pathToObjective)
        {
            animateMovement(coords, lastState);
            yield return StartCoroutine(SmoothMovement(coords));
            CheckSpace();
        }
        UpdatePosition();
        movesLeft -= pathToObjective.Length; //no "- 1" at end
        isMoving = false;
        resetAnimations();
        animator.SetTrigger("idle");
    }

    protected IEnumerator SmoothMovement(Vector2 end)
    {
        StartCoroutine(SoundManager.instance.Walk(moveTime));
        float sqrRemainingDistance = ((Vector2)transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb2D.position, end, inverseMoveTime * Time.fixedDeltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = ((Vector2)transform.position - end).sqrMagnitude;
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
            yield return null;
        }
    }

    protected string animateMovement(Vector2 end, string lastState) {
        string state = null;
        if (transform.position.x < end.x && lastState != "moveRight")
        {
            state = "moveRight";
        }
        else if (transform.position.x > end.x && lastState != "moveLeft")
        {
            state = "moveLeft";
        }
        else if (transform.position.y < end.y && lastState != "moveBack")
        {
            state = "moveBack";
        }
        else if (transform.position.y > end.y && lastState != "moveFront")
        {
            state = "moveFront";
        }
        if (state == null)
        {
            state = lastState;
        } else {
            resetAnimations();
            animator.SetTrigger(state);
        }
        return state;
    }

    protected void resetAnimations() {
        animator.ResetTrigger("moveRight");
        animator.ResetTrigger("moveLeft");
        animator.ResetTrigger("moveBack");
        animator.ResetTrigger("moveFront");
        animator.ResetTrigger("idle");
    }

    virtual protected void GetAvailableTargets()
    {
        actableObjects.Clear();

        if (actionsLeft <= 0)
            return;

        GetAttackRange();
        if (HasItemType("Weapon"))
            GetObjectsToActOn("Attack", attackRange);

        if (HasItemType("Medicine"))
        {
            GetObjectsToActOn("Heal", 1);
            if (IsDamaged())
            {
                List<InteractableObject> objects;
                if (actableObjects.TryGetValue("Heal", out objects) == false)
                {
                    objects = new List<InteractableObject>();
                    actableObjects.Add("Heal", objects);
                }
                objects.Add(this);
            }
        }

        GetObjectsToActOn("Talk", talkingRange);

        GetObjectsToActOn("Door", 1);

        if (HasItemType("Key"))
            GetObjectsToActOn("Unlock", 1);

        GetObjectsToActOn("Lever", 1);

        GetObjectsToActOn("Loot", 1);

        GetObjectsToActOn("Steal", 1);

        GetObjectsToActOn("Trade", 1);
    }

    protected void GetAttackRange()
    {
        attackRange = 1;
        if (HasItemType("Weapon"))
        {
            List<HoldableObject> weapons = MenuManager.instance.SortedInventory("Weapon", inventory);
            foreach (HoldableObject weapon in weapons)
            {
                if (weapon.range > attackRange)
                    attackRange = weapon.range;
            }
        }
    }

    protected void GetObjectsToActOn(String action, int range)
    {
        List<InteractableObject> objects = new List<InteractableObject>();

        boxCollider.enabled = false;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, range);
        boxCollider.enabled = true;
        foreach (var hitCollider in hitColliders)
        {
            InteractableObject hitObject = hitCollider.GetComponent<InteractableObject>();
            if (hitObject != null && GetDistance(hitObject) <= range && GetDistance(hitObject) > 0 && hitObject.GetActions().Contains(action))
            {
                bool safe = true;
                if (hitObject.tag == "Door") //check if door is empty
                {
                    foreach (var hitCollider2 in hitColliders)
                    {
                        if (hitCollider != hitCollider2 && hitCollider2.GetComponent<InteractableObject>() != null && (Vector2)hitCollider.transform.position == (Vector2)hitCollider2.transform.position) {
                            safe = false;
                            break;
                        }
                    }
                }
                else if (hitObject.tag == "Character")
                {
                    if (action == "Heal")
                        safe = allies.Contains(hitObject); //only heal allies
                    else if (action == "Attack")
                        safe = enemies.Contains(hitObject); //only attack enemies
                    else if (action == "Steal")
                        safe = hitCollider.GetComponent<Character>().inventory.Count > 0 && enemies.Contains(hitObject.GetComponent<Player>()); //|| hitObject.GetComponent<Character>().subdued);
                    else if (action == "Trade")
                        safe = hitCollider.GetComponent<Character>().inventory.Count > 0 && allies.Contains(hitObject.GetComponent<Player>()); //|| hitObject.GetComponent<Character>().subdued);
                }
                if (safe)
                    objects.Add(hitObject);
            }
        }

        if (objects.Count > 0)
            actableObjects.Add(action, objects);
    }

    virtual protected void GetAvailableActions()
    {
        actions.Clear();

        foreach (String action in actableObjects.Keys)
            actions.Add(action);

        actions.Add("Wait");
    }

    virtual protected void Act()
    {
        //actionsLeft--;
        //UpdateState();
        List<InteractableObject> objects;
        actableObjects.TryGetValue(currentObjective.action, out objects);

        switch (currentObjective.action)
        {
            case "Attack":
                if (objects != null && objects.Contains(currentObjective.target))
                    SelectItem("Weapon");
                else
                    StartCoroutine(NextStep());
                break;
            case "Heal":
                if (objects != null && objects.Contains(currentObjective.target))
                    SelectItem("Medicine");
                else
                    StartCoroutine(NextStep());
                break;
            case "Talk":
                if (objects != null && objects.Contains(currentObjective.target))
                    TalkTo(currentObjective.target);
                actionsLeft--;
                UpdateState();
                currentObjective = null;
                StartCoroutine(NextStep());
                break;
            case "Door":
                if (objects != null && objects.Contains(currentObjective.target))
                    StartCoroutine(Toggle(currentObjective.target));
                else
                    StartCoroutine(NextStep());
                break;
            case "Unlock":
                if (objects != null && objects.Contains(currentObjective.target))
                    SelectItem("Key");
                else
                    StartCoroutine(NextStep());
                break;
            case "Lever":
                if (objects != null && objects.Contains(currentObjective.target))
                    StartCoroutine(Toggle(currentObjective.target));
                else
                    StartCoroutine(NextStep());
                StartCoroutine(NextStep());
                break;
            case "Loot":
                if (objects != null && objects.Contains(currentObjective.target))
                    Loot(currentObjective.target);
                break;
            case "Steal":
                if (objects != null && objects.Contains(currentObjective.target))
                    Steal(currentObjective.target);
                break;
            case "Trade":
                if (objects != null && objects.Contains(currentObjective.target))
                    Steal(currentObjective.target);
                break;
            case "Wait":
                currentObjective = null;
                StartCoroutine(EndTurn());
                break;
            default:
                throw new Exception("Unknown action");
        }
    }

    protected virtual void SelectItem(String type)
    {
        List<HoldableObject> items = MenuManager.instance.SortedInventory(type, inventory);
        if (items.Count == 0)
        {
            objectives.Prepend(currentObjective);
            StartCoroutine(NextStep());
            return;
        }

        int i = 0;
        if (type == "Weapon")
            while (items[i].range < GetDistance(currentObjective.target))
                i++;
        StartCoroutine(ChooseItem(items[i]));
    }

    public virtual IEnumerator ChooseItem(HoldableObject item)
    {
        MenuManager.instance.HidePlayerStats();
        MenuManager.instance.HideBackButton();
        actionsLeft--;
        UpdateState();
        switch (item.type)
        {
            case "Weapon":
                yield return StartCoroutine(Attack(currentObjective.target, item));
                break;
            case "Medicine":
                yield return StartCoroutine(Heal(currentObjective.target, item));
                StartCoroutine(NextStep());
                break;
            case "Key":
                Unlock(currentObjective.target, item);
                break;
            default:
                break;
        }
    }

    protected virtual IEnumerator Toggle(InteractableObject toToggle)
    {
        GameManager.instance.CameraTarget(toToggle.gameObject);

        Door door = toToggle.gameObject.GetComponent<Door>();
        if (door != null)
            yield return StartCoroutine(door.Toggle());
        else
        {
            Lever lever = toToggle.gameObject.GetComponent<Lever>();
            lever.Toggle();
        }

        actionsLeft--;
        UpdateState();
        currentObjective = null;
        StartCoroutine(NextStep());
    }

    protected virtual void Unlock(InteractableObject toUnlock, HoldableObject key)
    {
        GameManager.instance.CameraTarget(toUnlock.gameObject);

        Door door = toUnlock.gameObject.GetComponent<Door>();
        if (door != null)
        {
            door.Unlock();
            StartCoroutine(NextStep());
        }
        else
        {
            Storage storage = toUnlock.gameObject.GetComponent<Storage>();
            storage.Unlock();
            Loot(toUnlock);
        }

        Remove(key);
    }

    protected virtual void Loot(InteractableObject toLoot)
    {
        GameManager.instance.CameraTarget(toLoot.gameObject);

        Storage storage = toLoot.gameObject.GetComponent<Storage>();
        storage.Open();

        foreach (HoldableObject item in storage.items)
        {
            Pickup(item);
            storage.Remove(item);
        }

        storage.Close();

        currentObjective = null;
        StartCoroutine(NextStep());
    }

    protected virtual void Steal(InteractableObject toStealFrom)
    {
        GameManager.instance.CameraTarget(toStealFrom.gameObject);
        Player character = toStealFrom.gameObject.GetComponent<Player>();
        this.weightStolen = 0;

        foreach (HoldableObject item in character.inventory)
        {
            if (item.power)
                continue;
            weightStolen += item.weight;
            if (CaughtStealing(character))
            {
                character.Enemy(this);
                break;
            }
            Pickup(item);
            character.Remove(item);
        }

        currentObjective = null;
        StartCoroutine(NextStep());
    }

    protected bool CaughtStealing(Player character)
    {
        return character.subdued || allies.Contains(character) ? false : UnityEngine.Random.Range(0, 10) < this.weightStolen;
    }

    protected virtual IEnumerator EndTurn()
    {
        //yield return new WaitForSeconds(actionDelay);

        // Conditionally doesn't end your turn if there is still dialogue in progress
        if (GameManager.instance.dialogueInProgress)
            yield return new WaitUntil(() => GameManager.instance.dialogueInProgress == false);
        else
            yield return new WaitForSeconds(0f);

        CheckSpace(true);
        isTurn = false;
        hasGone = true;
        StartCoroutine(GameManager.instance.NextTurn());
    }

    public bool HasItemType(String type)
    {
        return MenuManager.instance.SortedInventory(type, inventory).Count > 0;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<HoldableObject>() != null)
        {
            Pickup(other.gameObject.GetComponent<HoldableObject>());
        }
    }

    public virtual void Pickup (HoldableObject toPickup, Boolean start = false)
    {
        inventory.Add(toPickup);
        if (!start)
            toPickup.gameObject.SetActive(false);

        UpdateState();
    }

    public void Remove(HoldableObject item)
    {
        inventory.Remove(item);
    }

    protected IEnumerator Attack (InteractableObject toAttack, HoldableObject weapon)
    {
        GameManager.instance.CameraTarget(toAttack.gameObject);

        weapon.uses--;
        if (weapon.uses == 0)
            Remove(weapon);

        bool hit = DoesHit(HitPercent(toAttack, weapon));
        if (hit) {
            int damage = GetDamage(toAttack, weapon);

            if (weapon.swing != null) {
                Vector3 targ = toAttack.transform.position;
                targ.z = 0f;
                Vector3 objectPos = transform.position;
                targ.x = targ.x - objectPos.x;
                targ.y = targ.y - objectPos.y;
                float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
                GameObject swing = Instantiate(weapon.swing, this.transform.position, Quaternion.Euler(new Vector3(0, 0, angle)));
            }

            if (weapon.effect != null) {
                Instantiate(weapon.effect, toAttack.transform);
            }

            yield return StartCoroutine(toAttack.TakeDamage(damage));

            // Check for subdued
            if (toAttack.GetHealth() / toAttack.maxHealth < subduedRatio)
            {
                if (toAttack.gameObject.GetComponent<Character>() != null)
                {
                    Debug.Log("Character Subdued");
                    toAttack.gameObject.GetComponent<Character>().subdued = true;
                }
            }
            else
            {
                Character character = toAttack.gameObject.GetComponent<Character>();
                if (character != null)
                    character.Enemy(this);
            }

            // animator.SetTrigger("enemyAttack");
            if (this.GetComponent<Player>().playable && currentObjective.target.GetComponent<Character>() != null) {
                levelingUpPlayer = this.GetComponent<Player>();
                int expGain = Math.Max(currentObjective.target.GetComponent<Character>().level - level + 1, 1) * expPerLevelGap / ((toAttack.GetHealth() <= 0) ? 1 : 3);
                StartCoroutine(this.GetComponent<Player>().UpdateExp(expGain));
                yield break;
            }
            if (toAttack.GetHealth() <= 0)
                currentObjective = null;
            else if (currentObjective.target.GetComponent<Player>() != null && currentObjective.target.GetComponent<Player>().playable) {
                levelingUpPlayer = currentObjective.target.GetComponent<Player>();
                int expGain = Math.Max(level - currentObjective.target.GetComponent<Character>().level, 1) * expPerLevelGap / 3;
                StartCoroutine(currentObjective.target.GetComponent<Player>().UpdateExp(expGain));
                yield break;
            }
        }
        StartCoroutine(NextStep());
    }

    protected bool DoesHit(int percent) {
        return Random.Range(0, 100) <= percent;
    }

    protected int HitPercent(InteractableObject toAttack, HoldableObject weapon) {
        float chance = 100 - (100 - weapon.accuracy) * GetDistance(toAttack) + (float)stats.skill/10f;
        if (toAttack.gameObject.GetComponent<Character>() != null)
            chance = chance - (float)toAttack.gameObject.GetComponent<Character>().GetStats().agility/10f;
        return Math.Min(100,Math.Max(0,(int)chance));
    }

    protected int GetDamage(InteractableObject toAttack, HoldableObject weapon) {
        int crit = GetCritical(weapon);
        float damage = weapon.amount * (float)stats.strength/50f * crit;
        if (toAttack.gameObject.GetComponent<Character>() != null)
            damage = damage * (100f - (float)toAttack.gameObject.GetComponent<Character>().GetStats().defense)/50f;
        return (int)damage;
    }

    protected int GetCritical(HoldableObject weapon) {
        return DoesHit(CritPercent(weapon)) ? 2 : 1;
    }

    protected int CritPercent(HoldableObject weapon) {
        float chance = Mathf.Sqrt((float)stats.dexterity * weapon.critical);
        return Math.Min(100,Math.Max(0,(int)chance));
    }

    protected virtual IEnumerator Heal(InteractableObject toHeal, HoldableObject medicine)
    {
        GameManager.instance.CameraTarget(toHeal.gameObject);

        yield return StartCoroutine(toHeal.Heal(medicine.amount));

        Remove(medicine);

        Character character = toHeal.gameObject.GetComponent<Character>();
        if (character != null)
            character.Ally(this);

        currentObjective = null; //TEMP
    }

    public void Ally(Character character, bool updateTeam = true)
    {
        enemies.Remove(character);
        allies.Add(character);
        foreach (Character ally in allies)
        {
            if (ally != character && !ally.GetAllies().Contains(character))
                ally.Ally(character, false);
        }
        if (!character.GetAllies().Contains(this))
            character.Ally(this);
    }

    public void Enemy(Character character, bool updateTeam = true)
    {
        allies.Remove(character);
        enemies.Add(character);
        foreach (Character ally in allies)
        {
            if (ally != character && !ally.GetEnemies().Contains(character))
                ally.Enemy(character, false);
        }
        if (!character.GetEnemies().Contains(this))
            character.Enemy(this);
    }

    public List<InteractableObject> GetAllies()
    {
        return allies;
    }

    public List<InteractableObject> GetEnemies()
    {
        return enemies;
    }

    protected virtual void TalkTo(InteractableObject toTalkTo)
    {
        GameObject.Find("DialogueManager").GetComponent<DialogueManager>().initiateDialogue(this.gameObject, toTalkTo.gameObject);
    }

    public override SortedSet<String> GetActions()
    {
        receiveActions = base.GetActions();
        if (talkable)
            receiveActions.Add("Talk");

        // Should be able to steal from allies. Not sure if that part works yet.
        // if (subdued || allies.Contains(GameObject.FindObjectOfType<Player>().GetComponent<InteractableObject>()))
        receiveActions.Add("Steal"); // I do checks in GetActions() for relationships btwn any two characters, not just regarding player
        receiveActions.Add("Trade");

        return receiveActions;
    }

    protected void CheckSpace(bool end = false)
    {
        boxCollider.enabled = false;
        Collider2D hitCollider = Physics2D.OverlapCircle((Vector2)transform.position, 0.1f);
        boxCollider.enabled = true;
        if (!end && hitCollider != null && hitCollider.gameObject.tag == "Damaging")
            StartCoroutine(TakeDamage(hitCollider.gameObject.GetComponent<Damaging>().damagePerTurn));
        CheckRoof();
    }

    protected void CheckRoof() {
        GameManager.instance.CheckRoofs();
    }

    protected void UpdateState()
    {
        lastState = new State(health, movesLeft, actionsLeft, (Vector2)transform.position);
    }

    protected override void UpdatePosition()
    {
        GameManager.instance.UpdateNode(transform.position, false, (float)health); //"false" prevents A* from pathfinding through non-target characters
    }

    protected IEnumerator postActionDelay()
    {
        yield return new WaitForSeconds(actionDelay);
    }

    public bool HasGone() {
        return hasGone;
    }

    public void SetHasGone(bool gone) {
        hasGone = gone;
    }

    public bool IsSubdued() {
        return subdued;
    }

    public void Subdue() {
        subdued = true;
    }

    public Vector2[] GetPathToObjective() {
        return pathToObjective;
    }

    public void SetPathToObjective(Vector2[] path) {
        pathToObjective = path;
    }

    public void SetCurrentObjectiveTarget(InteractableObject target) {
        currentObjective.target = target;
    }

    public Stats GetStats() {
        return stats;
    }
}
