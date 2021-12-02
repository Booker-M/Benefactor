using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class DialogueSequenceManager : MonoBehaviour
{
    XDocument xmlDoc;
    IEnumerable<XElement> items;
    List<XMLData> data = new List<XMLData>();

    private int currentIndex;
    private int pageNum;

    string currentCharacter;
    string currentDialogue;

    public Text dialogueUI;
    public float typingSpeed = .05f;
    public float punctuationDelay = .5f;
    private float alphaIncrementRate = .02f;
    private float alphaIncrementAmount = .005f;
    private float postTransitionDelay = .5f;

    public bool typingInProgress;
    private bool fastForward;
    private bool transitioning;

    void Start()
    {
        currentIndex = 0;
        pageNum = 0;

        LoadXML();
        StartCoroutine("AssignData");
        typingInProgress = false;
        fastForward = false;

        GameObject.Find("Cover").GetComponent<Image>().color += new Color(0, 0, 0, 1);
        executeNext();
    }

    void LoadXML()
    {
        xmlDoc = XDocument.Load("Assets/Resources/XMLFiles/IntroSequence.xml");
        items = xmlDoc.Descendants("page").Elements();
    }

    IEnumerator AssignData()
    {
        int assignmentIndex = 0;
        bool firstCheck = true;

        foreach (var item in items)
        {
            // Allows us to make large skips in assignment index values without having to process every number inbetween.
            if (firstCheck && Int32.Parse(item.Parent.Attribute("number").Value.ToString()) > assignmentIndex)
            {
                assignmentIndex = Int32.Parse(item.Parent.Attribute("number").Value.ToString());
                firstCheck = false;
            }

            // Handles creation of each individual XML "page"
            if (item.Parent.Attribute("number").Value.ToString() == assignmentIndex.ToString())
            {
                string tempType = item.Parent.Element("type").Value.Trim();
                int tempPageNum = int.Parse(item.Parent.Attribute("number").Value);
                string tempBackdrop = item.Parent.Element("backdrop").Value.Trim();
                string tempSFX = item.Parent.Element("SFX").Value.Trim();
                string tempCharName = item.Parent.Element("character").Value.Trim();
                string tempPortrait = item.Parent.Element("portrait").Value.Trim();
                string tempDialogue = item.Parent.Element("dialogue").Value.Trim();

                data.Add(new XMLData(tempType, tempPageNum, tempBackdrop, tempSFX, tempCharName, tempPortrait, tempDialogue));
                assignmentIndex++;
                firstCheck = true;
            }
        }
        yield return null;
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0) && !typingInProgress && !transitioning) 
        {
            executeNext();
        } else if (Input.GetMouseButtonDown(0))
        {
            fastForward = true;
        }
    }

    private void executeNext()
    {
        Debug.Log("executing page: " + currentIndex);

        switch (data[currentIndex].type)
        {
            case "Dialogue":
                StartCoroutine("readDialogue");
                currentIndex++;
                break;
            case "NewScene":
                GameObject.Find("Backdrop").GetComponent<BackdropManager>().changeBackdrop(data[currentIndex].backdrop);
                GameObject.FindObjectOfType<AmbienceManager>().GetComponent<AudioSource>().volume = 1;
                StartCoroutine("fadeIn");
                break;
            case "FadeOut":
                StartCoroutine("fadeOut");
                StartCoroutine("fadeAmbience");
                break;
            case "SoundEffect":
                GameObject.FindObjectOfType<SFXManager>().PlaySingle(data[currentIndex].SFX);
                currentIndex++;
                executeNext();
                break;
            case "Ambience":
                GameObject.FindObjectOfType<AmbienceManager>().PlaySingle(data[currentIndex].SFX);
                currentIndex++;
                executeNext();
                break;
            case "StopAmbience":
                StartCoroutine("fadeAmbienceMidScene");
                currentIndex++;
                executeNext();
                break;
            case "StartScene":
                beginScene();
                break;
            default:
                Debug.Log("Invalid event type: " + data[currentIndex].type);
                break;
        }
    }

    IEnumerator readDialogue()
    {
        typingInProgress = true;

        dialogueUI.text = "";
        currentDialogue = data[currentIndex].dialogueText;

        GameObject.Find("SpeakingCharacter").GetComponent<Text>().text = data[currentIndex].characterName;
        GameObject.Find("Portrait").GetComponent<PortraitManager>().changePortrait(data[currentIndex].portrait);

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
    }

    IEnumerator fadeAmbience()
    {
        AudioSource source = GameObject.FindObjectOfType<AmbienceManager>().GetComponent<AudioSource>();
        float t = 3;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
            source.volume = t / 3;
            if (data[currentIndex].type != "FadeOut")
                break;
        }
        if (data[currentIndex].type == "FadeOut")
            source.Stop();
        yield break;
    }

    IEnumerator fadeAmbienceMidScene()
    {
        AudioSource source = GameObject.FindObjectOfType<AmbienceManager>().GetComponent<AudioSource>();
        float t = 3;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
            source.volume = t / 3;
        }
        yield break;
    }

    IEnumerator fadeIn()
    {
        transitioning = true;
        Image cover = GameObject.Find("Cover").GetComponent<Image>();

        GameObject.Find("DialogueBackground").GetComponent<Image>().enabled = false;
        GameObject.Find("Portrait").GetComponent<Image>().enabled = false;
        GameObject.Find("SpeakingCharacter").GetComponent<Text>().enabled = false;
        GameObject.Find("Dialogue").GetComponent<Text>().enabled = false;

        while (cover.color.a >= 0)
        {
            cover.color -= new Color(0, 0, 0, alphaIncrementAmount);
            if (fastForward)
            {
                cover.color -= new Color(0, 0, 0, cover.color.a);
                fastForward = false;
            }
            yield return new WaitForSeconds(alphaIncrementRate);
        }

        yield return new WaitForSeconds(postTransitionDelay);

        GameObject.Find("DialogueBackground").GetComponent<Image>().enabled = true;
        GameObject.Find("Portrait").GetComponent<Image>().enabled = true;
        GameObject.Find("SpeakingCharacter").GetComponent<Text>().enabled = true;
        GameObject.Find("Dialogue").GetComponent<Text>().enabled = true;

        transitioning = false;
        currentIndex++;
        executeNext();
    }

    IEnumerator fadeOut()
    {
        transitioning = true;
        Image cover = GameObject.Find("Cover").GetComponent<Image>();

        while (cover.color.a <= 1)
        {
            cover.color += new Color (0, 0, 0, alphaIncrementAmount);
            if (fastForward)
            {
                cover.color += new Color(0, 0, 0, (1 - cover.color.a));
                fastForward = false;
            }
            yield return new WaitForSeconds(alphaIncrementRate);
        }

        yield return new WaitForSeconds(postTransitionDelay);

        transitioning = false;
        currentIndex++;
        executeNext();
    }

    private void beginScene()
    {
        switch (data[currentIndex].backdrop)
        {
            case "Scenario1":
                // Should load Scenario 1, but for right now, just loads test level
                SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
                break;
                
        }
    }

    private void clearFields()
    {
        GameObject.Find("SpeakingCharacter").GetComponent<Text>().text = data[currentIndex].characterName;
        GameObject.Find("Portrait").GetComponent<PortraitManager>().changePortrait(data[currentIndex].portrait);
        dialogueUI.text = "";
    }

}

public class XMLData
{
    public string type;
    public int pageNum;
    public string backdrop;
    public string SFX;
    public string characterName;
    public string portrait;
    public string dialogueText;

    public XMLData(string pageType, int page, string backdropName, string SoundEffect, string charName, string character, string dialogue)
    {
        type = pageType;
        pageNum = page;
        backdrop = backdropName;
        SFX = SoundEffect;
        characterName = charName;
        portrait = character;
        dialogueText = dialogue;
    }
}

