using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;


/**
 * A one-off class that creates a singular object responsible for taking care of any dialogue events that come up
 * While this object is on, game functionality should be turned off (either turn things off in this class, or add a condition to the update of player / character / gameManager)
 */

// Notes: need to implement a "fleeing" system where characters try to leave the scene. Just A* but for designated exits.
// Also need to make it so that reason changes enemies into allies
// Maybe you can select item to intimidate with? Doesn't have to be complicated- only guns and knifes work, and only if opponent doesn't have one
// Need to create prefabs for all different NPC types in order to have different portraits

public class DialogueManager : MonoBehaviour
{
    // Data Holders:
    IEnumerable<XElement> items;
    List<XMLData> data = new List<XMLData>();

    // Frame-to-Frame record keeping
    public string conversationType;
    private int currentIndex;
    private int pageNum;
    string currentCharacter;
    string currentDialogue;
    public bool typingInProgress;
    private bool fastForward;
    private bool waitingForPlayerResponse;
    private bool readyToEnd;

    // Object References
    public GameObject player;
    public GameObject talkingNPC;
    public Text dialogueUI;
    public GameObject dialogueBackground;
    public GameObject portraitLeft;
    public GameObject portraitRight;
    public GameObject conversationOptions;
    public GameObject talkingCharacter;
    public GameObject talkingCharacterFrame;

    // Speeds and whatnot
    public float typingSpeed = .05f;
    public float punctuationDelay = .5f;
    private float alphaIncrementRate = .02f;
    private float alphaIncrementAmount = .005f;
    private float postTransitionDelay = .5f;

    // Template responses for the intimidate and reason options:
    private string[] intimidatations = new string[]
    {
        "You need to leave. Now.",
        "I'm not afraid to hurt you. Leave.",
        "Get out of here. And don't look back.",
        "If you stay here, you will get hurt.",
        "You're not going to win this fight. Get out of here."
    };
    private string[] intimidationSuccessResponses = new string[]
    {
        "Aiiieeeeee!",
        "I'll leave! I'll leave!",
        "Don't hurt me please, I'll go!",
        "I didn't realize you were armed, just let me go!"
    };
    private string[] intimidationFailureResponses = new string[]
    {
        "Some tough man you are. I'm not leaving.",
        "Big words for a small man.",
        "I think you're overestimating your position here.",
        "Your overconfidence is humiliating."
    };
    private string[] reasonings = new string[]
    {
        "I'm not the enemy here. We need to team up.",
        "Fighting each other isn't going to achieve anything.",
        "You're being manipulated.",
        "We can make this situation work for us if you trust me."
    };
    private string[] reasoningSuccessResponses = new string[]
    {
        "... You're right.",
        "I see now. Let's work together.",
        "I agree. Lets turn this around."
    };
    private string[] reasoningFailureResponses = new string[]
    {
        "Stop trying to manipulate me.",
        "Words are cheap. Fight like a man.",
        "We aren't friends- raise your fists!",
        "Spewing lies isn't going to save you."
    };

    // End of variables



    private void Awake()
    {
        conversationOptions.SetActive(false);
        dialogueBackground.SetActive(false);
        portraitLeft.SetActive(false);
        portraitRight.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && readyToEnd && !typingInProgress)
        {
            GameManager.instance.dialogueInProgress = false;
            dialogueBackground.SetActive(false);
            conversationOptions.SetActive(false);
            portraitLeft.SetActive(false);
            portraitRight.SetActive(false);
        }
        else if (Input.GetMouseButtonDown(0) && !typingInProgress && waitingForPlayerResponse)
        {
            determineResponse();
        }
        else if (Input.GetMouseButtonDown(0) && waitingForPlayerResponse)
        {
            fastForward = true;
        }
    }

    // Only to be used by players
    public void initiateDialogue(GameObject initiator, GameObject responder)
    {
        MenuManager.instance.HidePlayerStats();
        //currentIndex = 0;
        //pageNum = 0;
        conversationType = null;
        fastForward = false;

        player = initiator;
        talkingNPC = responder;

        // Disable Player Functionality should be taken care of in other classes, but might not work fully yet
        GameManager.instance.dialogueInProgress = true;
        readyToEnd = false;
        waitingForPlayerResponse = false;
        conversationType = null;
        dialogueBackground.SetActive(true);
        dialogueUI.text = "";
        // portrait.GetComponent<PortraitManager>().changePortrait("RaskolnikovNeutral");
        // talkingCharacter.GetComponent<Text>().text = "Raskolnikov";
        portraitLeft.GetComponent<Image>().sprite = initiator.GetComponent<Character>().portrait;
        portraitRight.GetComponent<Image>().sprite = responder.GetComponent<Character>().portrait;
        portraitLeft.SetActive(true);
        portraitRight.SetActive(true);
        talkingCharacter.GetComponent<Text>().text = GameManager.instance.activeCharacter.GetComponent<Character>().name;
        conversationOptions.SetActive(true);
    }
   

    // Only used once initiateDialogue has occured
    public void setConversation(string type)
    {
        conversationType = type;
        Debug.Log(type);
        conversationOptions.SetActive(false);

        if (type == "threaten")
        {
            System.Random rand = new System.Random();
            int index = rand.Next(intimidatations.Length);
            string randomThreat = intimidatations[index];
            StartCoroutine(readDialogue(randomThreat, player));
        } else if (type == "reason")
        {
            System.Random rand = new System.Random();
            int index = rand.Next(reasonings.Length);
            string randomReasoning = reasonings[index];
            StartCoroutine(readDialogue(randomReasoning, player));
        } else
        {
            Debug.Log("ERROR- CONVERSATION TYPE DOESN'T EXIST");
        }

        waitingForPlayerResponse = true;
    }

    // To be used for non-player instigated dialogue, if need be
    public void triggerDialogueEvent()
    {

    }


    private void determineResponse()
    {
        if (conversationType == "threaten")
        {
            // Intimidation algorithm here (I think it can stay this simple):
            // Would ideally choose what item to threaten with though
            if (player.GetComponent<Character>().HasItemType("Weapon"))
            {
                // Insert code to set a "fleeing" variable to true here (threat worked)
                talkingNPC.GetComponent<Character>().Subdue();
                System.Random rand = new System.Random();
                int index = rand.Next(intimidationSuccessResponses.Length);
                string randomResponse = intimidationSuccessResponses[index];
                StartCoroutine(readDialogue(randomResponse, talkingNPC));
            } else
            {
                // Insert code to convert neutral to enemy?
                System.Random rand = new System.Random();
                int index = rand.Next(intimidationFailureResponses.Length);
                string randomResponse = intimidationFailureResponses[index];
                StartCoroutine(readDialogue(randomResponse, talkingNPC));
            }
        }

        if (conversationType == "reason")
        {
            // Reasoning algorithm (requires a certain threshold of rational AND reputation)
            if (player.GetComponent<Player>().GetHealth() > player.GetComponent<Player>().maxHealth/2)
            {
                // Insert code to convert enemy into an ally here (reasoning worked)
                System.Random rand = new System.Random();
                int index = rand.Next(reasoningSuccessResponses.Length);
                string randomResponse = reasoningSuccessResponses[index];
                StartCoroutine(readDialogue(randomResponse, talkingNPC));
            } else
            {
                System.Random rand = new System.Random();
                int index = rand.Next(reasoningFailureResponses.Length);
                string randomResponse = reasoningFailureResponses[index];
                StartCoroutine(readDialogue(randomResponse, talkingNPC));
            }
        }

        readyToEnd = true;
    }

    IEnumerator readDialogue(string toDisplay, GameObject speakingCharacter)
    {
        typingInProgress = true;

        dialogueUI.text = "";
        currentDialogue = toDisplay;

        talkingCharacter.GetComponent<Text>().text = speakingCharacter.GetComponent<Character>().name;
        // portrait.GetComponent<Image>().sprite = speakingCharacter.GetComponent<Character>().portrait;

        foreach (char letter in currentDialogue)
        {
            dialogueUI.text += letter;
            float specialCharacterDelay = 0f;

            if (letter == '.' || letter == '?' || letter == '!')
                specialCharacterDelay = punctuationDelay;

            if (fastForward)
            {
                dialogueUI.text = currentDialogue;
                fastForward = false;
                typingInProgress = false;
                yield break;
            }

            yield return new WaitForSeconds(typingSpeed + specialCharacterDelay);
        }

        typingInProgress = false;

        // Sorry for the bad code, it relies on there only being a one line response. I think it'll do though!
        //if (speakingCharacter == talkingNPC)
        //{
        //    readyToEnd = true;
        //}
    }
}














//    void Start()
//    {
//        currentIndex = 0;
//        pageNum = 0;

//        LoadXML();
//        StartCoroutine("AssignData");
//        typingInProgress = false;
//        fastForward = false;

//        GameObject.Find("Cover").GetComponent<Image>().color += new Color(0, 0, 0, 1);
//        executeNext();
//    }

//    void LoadXML()
//    {
//        xmlDoc = XDocument.Load("Assets/Resources/XMLFiles/IntroSequence.xml");
//        items = xmlDoc.Descendants("page").Elements();
//    }

//    IEnumerator AssignData()
//    {
//        int assignmentIndex = 0;
//        bool firstCheck = true;

//        foreach (var item in items)
//        {
//            // Allows us to make large skips in assignment index values without having to process every number inbetween.
//            if (firstCheck && Int32.Parse(item.Parent.Attribute("number").Value.ToString()) > assignmentIndex)
//            {
//                assignmentIndex = Int32.Parse(item.Parent.Attribute("number").Value.ToString());
//                firstCheck = false;
//            }

//            // Handles creation of each individual XML "page"
//            if (item.Parent.Attribute("number").Value.ToString() == assignmentIndex.ToString())
//            {
//                string tempType = item.Parent.Element("type").Value.Trim();
//                int tempPageNum = int.Parse(item.Parent.Attribute("number").Value);
//                string tempBackdrop = item.Parent.Element("backdrop").Value.Trim();
//                string tempSFX = item.Parent.Element("SFX").Value.Trim();
//                string tempCharName = item.Parent.Element("character").Value.Trim();
//                string tempPortrait = item.Parent.Element("portrait").Value.Trim();
//                string tempDialogue = item.Parent.Element("dialogue").Value.Trim();

//                data.Add(new XMLData(tempType, tempPageNum, tempBackdrop, tempSFX, tempCharName, tempPortrait, tempDialogue));
//                //Debug.Log(data[assignmentIndex].dialogueText);
//                //Debug.Log(assignmentIndex);
//                assignmentIndex++;
//                firstCheck = true;
//            }
//        }
//        yield return null;
//    }

//    private void Update()
//    {

//        if (Input.GetMouseButtonDown(0) && !typingInProgress && !transitioning)
//        {
//            executeNext();
//        }
//        else if (Input.GetMouseButtonDown(0))
//        {
//            fastForward = true;
//        }
//    }

//    private void executeNext()
//    {
//        Debug.Log("executing page: " + currentIndex);
//        if (data[currentIndex].type == "Dialogue")
//        {
//            StartCoroutine("readDialogue");
//            currentIndex++;
//        }
//        else if (data[currentIndex].type == "SoundEffect")
//        {
//            // TO DO
//            // Also make this whole thing a switch statement please
//            currentIndex++;
//            executeNext();
//        }
//    }

//    IEnumerator readDialogue()
//    {
//        typingInProgress = true;

//        dialogueUI.text = "";
//        currentDialogue = data[currentIndex].dialogueText;

//        GameObject.Find("SpeakingCharacter").GetComponent<Text>().text = data[currentIndex].characterName;
//        GameObject.Find("Portrait").GetComponent<PortraitManager>().changePortrait(data[currentIndex].portrait);

//        foreach (char letter in currentDialogue)
//        {
//            dialogueUI.text += letter;
//            float specialCharacterDelay = 0f;

//            if (letter == '.' || letter == '?' || letter == '!')
//                specialCharacterDelay = punctuationDelay;

//            if (fastForward)
//            {
//                dialogueUI.text = currentDialogue;
//                fastForward = false;
//                typingInProgress = false;
//                yield break;
//            }

//            yield return new WaitForSeconds(typingSpeed + specialCharacterDelay);
//        }

//        typingInProgress = false;
//    }


//    private void clearFields()
//    {
//        GameObject.Find("SpeakingCharacter").GetComponent<Text>().text = data[currentIndex].characterName;
//        GameObject.Find("Portrait").GetComponent<PortraitManager>().changePortrait(data[currentIndex].portrait);
//        dialogueUI.text = "";
//    }

//}