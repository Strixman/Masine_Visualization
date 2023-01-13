using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGameManager : MonoBehaviour
{
    public GameObject startScreen;
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;

    public GameObject settingsScreen;
    public Button saveButton;
    public Button backButton;
    public InputField mqttAddressInput;

    void Awake()
    {
        mqttAddressInput.text = PlayerPrefs.GetString("address", "tcp://127.0.0.1:3030");

        settingsButton.onClick.AddListener(() =>
        {
            if (settingsScreen.activeSelf) return;
            settingsScreen.SetActive(true);
            startScreen.SetActive(false);
        });
        startButton.onClick.AddListener(() =>
        {
            SceneManager.LoadSceneAsync(1);
        });

        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        backButton.onClick.AddListener(() =>
        {
            if (startScreen.activeSelf) return;
            startScreen.SetActive(true);
            settingsScreen.SetActive(false);
        });

        saveButton.onClick.AddListener(() =>
        {
            if (startScreen.activeSelf) return;

            if (mqttAddressInput.text == "" || mqttAddressInput.text == PlayerPrefs.GetString("address", "tcp://127.0.0.1:3030")) return;

            PlayerPrefs.SetString("address", mqttAddressInput.text);

            startScreen.SetActive(true);
            settingsScreen.SetActive(false);
        });

        startScreen.SetActive(true);
        settingsScreen.SetActive(false);
    }
}
