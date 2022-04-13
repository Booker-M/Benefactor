using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParallaxManager : MonoBehaviour
{
    public SpriteRenderer background;
    public Image[] layers;
    public Image[] layers2;

    public Sprite forestNightBackground;
    public Sprite[] forestNightLayers;

    public Sprite mountainsNightBackground;
    public Sprite[] mountainsNightLayers;

    public float speed = -0.5f;
    public int shootingStarAmount;
    public ShootingStar shootingStar;
    private bool moving;
    private List<ShootingStar> shootingStars;

    private void Start()
    {
        // changeParallax("Disable");
        shootingStars = new List<ShootingStar>();
        for (int i = 0; i < layers.Length; i++) {
            layers[i].GetComponent<RectTransform>().localPosition = new Vector2(0, layers[i].GetComponent<RectTransform>().localPosition.y);
            layers2[i].GetComponent<RectTransform>().localPosition = new Vector2(layers2[i].GetComponent<RectTransform>().rect.width, layers2[i].GetComponent<RectTransform>().localPosition.y);
        }
    }

    private void Update()
    {
        if (moving) {
            for (int i = 0; i < layers.Length; i++) {
                RectTransform transform = layers[i].GetComponent<RectTransform>();
                transform.localPosition = new Vector2(transform.localPosition.x + speed * (i+1) * Time.fixedDeltaTime * 10, transform.localPosition.y);
                if (transform.localPosition.x < transform.rect.width * -1)
                    transform.localPosition = new Vector2(transform.rect.width, transform.localPosition.y);
                if (transform.localPosition.x > transform.rect.width)
                    transform.localPosition = new Vector2(transform.rect.width * -1, transform.localPosition.y);

                layers2[i].GetComponent<RectTransform>().localPosition = new Vector2(transform.localPosition.x + transform.rect.width * (transform.localPosition.x < 0 ? 1 : -1), transform.localPosition.y);
            }
        }
    }

    public void changeParallax(string newParallaxName)
    {
        DespawnStars();
        switch (newParallaxName)
        {
            case "ForestNight":
                background.sprite = forestNightBackground;
                swapLayers(forestNightLayers);
                moving = true;
                break;
            case "MountainsNight":
                background.sprite = mountainsNightBackground;
                swapLayers(mountainsNightLayers);
                SpawnStars();
                moving = true;
                break;
            case "Disable":
                moving = false;
                swapLayers(new Sprite[0]);
                break;
            default:
                Debug.LogError("Invalid parallax name: " + newParallaxName);
                break;
        }
    }

    private void swapLayers(Sprite[] newLayers) {
        for (int i = 0; i < layers.Length; i++) {
            if (i < newLayers.Length) {
                layers[i].sprite = newLayers[i];
                layers[i].enabled = true;
                layers2[i].sprite = newLayers[i];
                layers2[i].enabled = true;
            } else {
                layers[i].enabled = false;
                layers2[i].enabled = false;
            }
        }
    }

    private void SpawnStars() {
        for (int i = 0; i < shootingStarAmount; i++) {
            ShootingStar current = Instantiate(shootingStar);
            shootingStars.Add(current);
            current.transform.parent = transform;
        }
    }

    private void DespawnStars() {
        for (int i = 0; i < shootingStars.Count; i++) {
            Destroy(shootingStars[i]);
        }
        shootingStars = new List<ShootingStar>();
    }

}
