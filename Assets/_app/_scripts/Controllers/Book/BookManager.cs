﻿using UnityEngine;
using UnityEngine.UI;
using EA4S;
using EA4S.Db;
using EA4S.Teacher;
using ModularFramework.Core;
using System;
using System.Text;
using TMPro;
using System.Globalization;

namespace EA4S
{
    public class BookManager : MonoBehaviour
    {
        [Header("Scene Setup")]
        public Music SceneMusic;

        [Header("References")]
        public GameObject ButtonPrefab;
        public GameObject WordsContainer;
        public TextRender ArabicText;
        public TextMeshProUGUI Drawing;

        public LetterObjectView LLText;
        public LetterObjectView LLDrawing;

        void Start()
        {
            GlobalUI.ShowPauseMenu(false);
            AudioManager.I.PlayMusic(SceneMusic);
            SceneTransitioner.Close();

            InitUI();
            Drawing.text = "";
        }

        void InitUI()
        {
            GameObject btnGO;

            //// Words
            foreach (Transform t in WordsContainer.transform) {
                Destroy(t.gameObject);
            }

            foreach (WordData word in AppManager.Instance.DB.GetAllWordData()) {
                btnGO = Instantiate(ButtonPrefab);
                btnGO.transform.SetParent(WordsContainer.transform, false);
                btnGO.GetComponentInChildren<Text>().text = word.Id;
                if (word.Drawing != "") {
                    btnGO.GetComponent<Image>().color = Color.green;
                }
                AddListenerWord(btnGO.GetComponent<Button>(), word);
            }
        }

        void AddListenerWord(Button b, WordData word)
        {
            b.onClick.AddListener(() => PlayWord(word));
        }

        void PlayWord(WordData word)
        {
            Debug.Log("playing word :" + word.Id);
            AudioManager.I.PlayWord(word.Id);
            ArabicText.text = word.Arabic;

            LLText.Lable.text = ArabicAlphabetHelper.PrepareStringForDisplay(word.Arabic);

            if (word.Drawing != "") {
                var drawingChar = ((char)int.Parse(word.Drawing, NumberStyles.HexNumber)).ToString();
                Drawing.text = drawingChar;
                LLDrawing.Lable.text = drawingChar;
                Debug.Log("Drawing: " + word.Drawing);
            } else {
                Drawing.text = "";
                LLDrawing.Lable.text = "";
            }
        }

        public void OpenMap()
        {
            GameManager.Instance.Modules.SceneModule.LoadSceneWithTransition("app_Map");
        }
    }
}