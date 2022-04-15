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
    public XDocument xmlDoc;
    IEnumerable<XElement> items;
    List<XMLData> data = new List<XMLData>();
    public string cutscene;

    private int currentIndex;
    private int pageNum;

    string currentCharacter;
    string currentDialogue;

    public CameraShake cameraShake;
    public Text dialogueUI;
    public float typingSpeed = .05f;
    public float punctuationDelay = .5f;
    private float alphaIncrementRate = .02f;
    private float alphaIncrementAmount = 3f;
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
        xmlDoc = XDocument.Load(Application.dataPath + "/Cutscenes/" + cutscene + ".xml");
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
                string tempBackdrop = item.Parent.Element("backdrop") != null ? item.Parent.Element("backdrop").Value.Trim() : null;
                string tempSFX = item.Parent.Element("SFX") != null ? item.Parent.Element("SFX").Value.Trim() : null;
                string tempCharName = item.Parent.Element("character") != null ? item.Parent.Element("character").Value.Trim() : null;
                string tempPortraitLeft = item.Parent.Element("portraitLeft") != null ? item.Parent.Element("portraitLeft").Value.Trim() : null;
                string tempPortraitRight = item.Parent.Element("portraitRight") != null ? item.Parent.Element("portraitRight").Value.Trim() : null;
                string tempDialogue = item.Parent.Element("dialogue") != null ? item.Parent.Element("dialogue").Value.Trim() : null;
                float tempDuration = item.Parent.Element("duration") != null ? float.Parse(item.Parent.Element("duration").Value) : 0.0f;

                data.Add(new XMLData(tempType, tempPageNum, tempBackdrop, tempSFX, tempCharName, tempPortraitLeft, tempPortraitRight, tempDialogue, tempDuration));
                //Debug.Log(data[assignmentIndex].dialogueText);
                //Debug.Log(assignmentIndex);
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
            case "Menu":
                Menu();
                break;
            case "Dialogue":
                StartCoroutine("readDialogue");
                break;
            case "Pause":
                StartCoroutine("pause");
                break;
            case "Shake":
                StartCoroutine("shake");
                break;
            case "NewScene":
                if (data[currentIndex].backdrop != null) {
                    GameObject.Find("Backdrop").GetComponent<BackdropManager>().changeBackdrop(data[currentIndex].backdrop);
                    GameObject.Find("ParallaxManager").GetComponent<ParallaxManager>().changeParallax("Disable");
                }
                GameObject.FindObjectOfType<AmbienceManager>().GetComponent<AudioSource>().volume = 1;
                StartCoroutine("fadeIn");
                break;
            case "FadeOut":
                StartCoroutine("fadeOut");
                StartCoroutine("fadeAmbience");
                break;
            case "SoundEffect":
                GameObject.FindObjectOfType<SFXManager>().PlaySingle(data[currentIndex].SFX);
                //Debug.Log("Made it");
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
            case "Parallax":
                GameObject.Find("ParallaxManager").GetComponent<ParallaxManager>().changeParallax(data[currentIndex].backdrop);
                currentIndex++;
                executeNext();
                break;
            case "StartScene":
                beginScene();
                break;
            case "HideLeftPortrait":
                hidePortraitLeft();
                currentIndex++;
                executeNext();
                break;
            case "HideRightPortrait":
                hidePortraitRight();
                currentIndex++;
                executeNext();
                break;
            default:
                Debug.Log("Invalid event type: " + data[currentIndex].type);
                break;
        }

        //if (data[currentIndex].type == "Dialogue")
        //{
        //    StartCoroutine("readDialogue");
        //    currentIndex++;
        //}
        //else if (data[currentIndex].type == "NewScene")
        //{
        //    GameObject.Find("Backdrop").GetComponent<BackdropManager>().changeBackdrop(data[currentIndex].backdrop);
        //    StartCoroutine("fadeIn");
        //    // executeNext and currentIndex++ are handled at the end of the fadeIn / fadeOut coroutines
        //}
        //else if (data[currentIndex].type == "FadeOut")
        //{
        //    StartCoroutine("fadeOut");
        //    // executeNext and currentIndex++ are handled at the end of the fadeIn / fadeOut coroutines
        //}
        //else if (data[currentIndex].type == "SoundEffect")
        //{
        //    GameObject.FindObjectOfType<SFXManager>().PlaySingle(data[currentIndex].SFX);
        //    currentIndex++;
        //    executeNext();
        //}
    }

    IEnumerator readDialogue()
    {
        typingInProgress = true;

        dialogueUI.text = "";
        currentDialogue = data[currentIndex].dialogueText;
        showDialogue();

        GameObject.Find("SpeakingCharacter").GetComponent<Text>().text = data[currentIndex].characterName;
        if (data[currentIndex].portraitLeft != null) {
            GameObject.Find("PortraitLeft").GetComponent<PortraitManager>().changePortrait(data[currentIndex].portraitLeft);
            GameObject.Find("PortraitLeft").GetComponent<Image>().color = new Color( 1f, 1f, 1f);
            showPortraitLeft();
            if (GameObject.Find("PortraitRight").GetComponent<Image>().enabled == true) {
                GameObject.Find("PortraitRight").GetComponent<Image>().color = new Color( 0.5f, 0.5f, 0.5f);
                showPortraitRight();
            }

            GameObject.Find("DialogueBackground").transform.localPosition = new Vector2(-50, GameObject.Find("DialogueBackground").transform.localPosition.y);
        }
        if (data[currentIndex].portraitRight != null) {
            GameObject.Find("PortraitRight").GetComponent<PortraitManager>().changePortrait(data[currentIndex].portraitRight);
            GameObject.Find("PortraitRight").GetComponent<Image>().color = new Color( 1f, 1f, 1f);
            showPortraitRight();
            if (GameObject.Find("PortraitLeft").GetComponent<Image>().enabled == true) {
                GameObject.Find("PortraitLeft").GetComponent<Image>().color = new Color( 0.5f, 0.5f, 0.5f);
                showPortraitRight();
            }

            GameObject.Find("DialogueBackground").transform.localPosition = new Vector2(50, GameObject.Find("DialogueBackground").transform.localPosition.y);
        }

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
                currentIndex++;
                typingInProgress = false;
                yield break;
            }

            yield return new WaitForSeconds(typingSpeed + specialCharacterDelay);
        }

        currentIndex++;
        typingInProgress = false;
    }

    private void Menu() {
        transitioning = true;
    }

    IEnumerator pause()
    {
        hideDialogue();
        yield return new WaitForSeconds(data[currentIndex].duration);
        showDialogue();

        currentIndex++;
        executeNext();
    }

    IEnumerator shake()
    {
        hideDialogue();
        yield return StartCoroutine(cameraShake.Shake(data[currentIndex].duration, 0.25f));
        showDialogue();

        currentIndex++;
        executeNext();
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

        hideDialogue();
        hidePortraitLeft();
        hidePortraitRight();

        while (cover.color.a >= 0)
        {
            cover.color -= new Color(0, 0, 0, alphaIncrementAmount*Time.deltaTime);
            if (fastForward)
            {
                cover.color -= new Color(0, 0, 0, cover.color.a);
                fastForward = false;
            }
            yield return new WaitForSeconds(alphaIncrementRate);
        }

        yield return new WaitForSeconds(postTransitionDelay);

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
            cover.color += new Color (0, 0, 0, alphaIncrementAmount*Time.deltaTime);
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
            case "Intro":
                SceneManager.LoadScene("IntroCutscene", LoadSceneMode.Single);
                break;
            case "Scenario1":
                // Should load Scenario 1, but for right now, just loads test level
                SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
                break;
                
        }
    }

    private void clearFields()
    {
        GameObject.Find("SpeakingCharacter").GetComponent<Text>().text = data[currentIndex].characterName;
        GameObject.Find("PortraitLeft").GetComponent<PortraitManager>().changePortrait(data[currentIndex].portraitLeft);
        GameObject.Find("PortraitRight").GetComponent<PortraitManager>().changePortrait(data[currentIndex].portraitRight);
        dialogueUI.text = "";
    }

    private void hideDialogue()
    {
        GameObject.Find("DialogueBackground").GetComponent<Image>().enabled = false;
        GameObject.Find("SpeakerFrame").GetComponent<Image>().enabled = false;
        GameObject.Find("SpeakingCharacter").GetComponent<Text>().enabled = false;
        GameObject.Find("Dialogue").GetComponent<Text>().enabled = false;
    }

    private void hidePortraitLeft()
    {
        GameObject.Find("PortraitLeft").GetComponent<Image>().enabled = false;
    }

    private void hidePortraitRight()
    {
        GameObject.Find("PortraitRight").GetComponent<Image>().enabled = false;
    }

    private void showDialogue()
    {
        GameObject.Find("DialogueBackground").GetComponent<Image>().enabled = true;
        GameObject.Find("SpeakerFrame").GetComponent<Image>().enabled = true;
        GameObject.Find("SpeakingCharacter").GetComponent<Text>().enabled = true;
        GameObject.Find("Dialogue").GetComponent<Text>().enabled = true;
    }

    private void showPortraitLeft()
    {
        GameObject.Find("PortraitLeft").GetComponent<Image>().enabled = true;
    }

    private void showPortraitRight()
    {
        GameObject.Find("PortraitRight").GetComponent<Image>().enabled = true;
    }

}

public class XMLData
{
    public string type;
    public int pageNum;
    public string backdrop;
    public string SFX;
    public string characterName;
    public string portraitLeft;
    public string portraitRight;
    public string dialogueText;
    public float duration;

    public XMLData(string pageType, int page, string backdropName, string SoundEffect, string charName, string characterLeft, string characterRight, string dialogue, float time)
    {
        type = pageType;
        pageNum = page;
        backdrop = backdropName;
        SFX = SoundEffect;
        characterName = charName;
        portraitLeft = characterLeft;
        portraitRight = characterRight;
        dialogueText = dialogue;
        duration = time;
    }
}

