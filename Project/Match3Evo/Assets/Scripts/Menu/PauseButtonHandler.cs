using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo
{
    public class PauseButtonHandler : MonoBehaviour
    {
        [SerializeField] Image pauseButtonImage;
        [SerializeField] Sprite pauseImage;
        [SerializeField] Sprite resumeImage;

        bool paused = false;

        private void Start()
        {
            gameObject.SetActive(false);
            GM.boardMng.startGameDelegate += StartGameDelegate;
        }

        public void OnClick()
        {
            if (paused)
            {
                GM.boardMng.OnResumeGame();
                pauseButtonImage.sprite = pauseImage;
            }
            else
            {
                GM.boardMng.OnPauseGame();
                pauseButtonImage.sprite = resumeImage;
            }
            
            paused = !paused;
        }

        public void StartGameDelegate()
        {
            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            GM.boardMng.startGameDelegate -= StartGameDelegate;
        }
    }
}