//using System.Collections;
//using System.Collections.Generic;
//using System.Xml;
//using System.Xml.Serialization;
//using System.IO;
//using System.Xml.Linq;
//using UnityEngine;
//using UnityEngine.UI;
//using System;


///**
// * A one-off class that creates a singular object responsible for taking care of any dialogue events that come up
// * While this object is on, game functionality should be turned off (either turn things off in this class, or add a condition to the update of player / character / gameManager)
// */
//public class DialogueEventManager : MonoBehaviour
//{
//    IEnumerable<XElement> items;
//    List<XMLData> data = new List<XMLData>();

//    private int currentIndex;
//    private int pageNum;

//    string currentCharacter;
//    string currentDialogue;

//    public Text dialogueUI;
//    public float typingSpeed = .05f;
//    public float punctuationDelay = .5f;
//    private float alphaIncrementRate = .02f;
//    private float alphaIncrementAmount = .005f;
//    private float postTransitionDelay = .5f;

//    public bool typingInProgress;
//    private bool fastForward;
//    private bool transitioning;




//    public static void initiateDialogue()
//    {


//        //currentIndex = 0;
//        //pageNum = 0;

//        return;
//    }














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
//        else if (data[currentIndex].type == "NewScene")
//        {
//            GameObject.Find("Backdrop").GetComponent<BackdropManager>().changeBackdrop(data[currentIndex].backdrop);
//            StartCoroutine("fadeIn");
//            // executeNext and currentIndex++ are handled at the end of the fadeIn / fadeOut coroutines
//        }
//        else if (data[currentIndex].type == "FadeOut")
//        {
//            StartCoroutine("fadeOut");
//            // executeNext and currentIndex++ are handled at the end of the fadeIn / fadeOut coroutines
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



//    IEnumerator fadeIn()
//    {
//        transitioning = true;
//        Image cover = GameObject.Find("Cover").GetComponent<Image>();

//        GameObject.Find("DialogueBackground").GetComponent<Image>().enabled = false;
//        GameObject.Find("Portrait").GetComponent<Image>().enabled = false;
//        GameObject.Find("SpeakingCharacter").GetComponent<Text>().enabled = false;
//        GameObject.Find("Dialogue").GetComponent<Text>().enabled = false;

//        while (cover.color.a >= 0)
//        {
//            cover.color -= new Color(0, 0, 0, alphaIncrementAmount);
//            if (fastForward)
//            {
//                cover.color -= new Color(0, 0, 0, cover.color.a);
//                fastForward = false;
//            }
//            yield return new WaitForSeconds(alphaIncrementRate);
//        }

//        yield return new WaitForSeconds(postTransitionDelay);

//        GameObject.Find("DialogueBackground").GetComponent<Image>().enabled = true;
//        GameObject.Find("Portrait").GetComponent<Image>().enabled = true;
//        GameObject.Find("SpeakingCharacter").GetComponent<Text>().enabled = true;
//        GameObject.Find("Dialogue").GetComponent<Text>().enabled = true;

//        transitioning = false;
//        currentIndex++;
//        executeNext();
//    }

//    IEnumerator fadeOut()
//    {
//        transitioning = true;
//        Image cover = GameObject.Find("Cover").GetComponent<Image>();

//        while (cover.color.a <= 1)
//        {
//            cover.color += new Color(0, 0, 0, alphaIncrementAmount);
//            if (fastForward)
//            {
//                cover.color += new Color(0, 0, 0, (1 - cover.color.a));
//                fastForward = false;
//            }
//            yield return new WaitForSeconds(alphaIncrementRate);
//        }

//        yield return new WaitForSeconds(postTransitionDelay);

//        transitioning = false;
//        currentIndex++;
//        executeNext();
//    }

//    private void clearFields()
//    {
//        GameObject.Find("SpeakingCharacter").GetComponent<Text>().text = data[currentIndex].characterName;
//        GameObject.Find("Portrait").GetComponent<PortraitManager>().changePortrait(data[currentIndex].portrait);
//        dialogueUI.text = "";
//    }

//}
