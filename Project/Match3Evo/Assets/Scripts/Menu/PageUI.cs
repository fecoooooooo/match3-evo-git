using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Match3_Evo
{
    public class PageUI : MonoBehaviour
    {
        [SerializeField] RectTransform[] pages;
        [SerializeField] Text pagingNumber;
        [SerializeField] Button nextButton;
        [SerializeField] Text nextButtonText;
        [SerializeField] Button backButton;
        [SerializeField] int currentPage = -1;

        private void Start()
        {
            NextPage();
        }

        public void NextPage()
        {
            if (currentPage != -1)
            {
                foreach (CanvasGroupFade item in pages[currentPage].GetComponentsInChildren<CanvasGroupFade>())
                    item.Fade(0f);

                StartCoroutine(ClosePreviousPage(currentPage));
            }

            currentPage++;

            if (currentPage < pages.Length)
            {
                pages[currentPage].gameObject.SetActive(true);

                foreach (CanvasGroupFade item in pages[currentPage].GetComponentsInChildren<CanvasGroupFade>())
                    item.Fade(1f);
            }
            else
            {
                currentPage = -1;
                DestroyPageUI();
            }

            pagingNumber.text = (currentPage + 1).ToString() + "/" + pages.Length;
            backButton.gameObject.SetActive(currentPage > 0);
            nextButtonText.text = currentPage == pages.Length - 1 ? "Done" : "Next";
        }

        public void PreviousPage()
        {
            if (currentPage != -1)
            {
                foreach (CanvasGroupFade item in pages[currentPage].GetComponentsInChildren<CanvasGroupFade>())
                    item.Fade(0f);

                StartCoroutine(ClosePreviousPage(currentPage));
            }

            currentPage--;
            pages[currentPage].gameObject.SetActive(true);

            foreach (CanvasGroupFade item in pages[currentPage].GetComponentsInChildren<CanvasGroupFade>())
                item.Fade(1f);

            pagingNumber.text = (currentPage + 1).ToString() + "/" + pages.Length;
            backButton.gameObject.SetActive(currentPage > 0);
            nextButtonText.text = currentPage == pages.Length - 1 ? "Done" : "Next";
        }

        private IEnumerator ClosePreviousPage(int page)
        {
            yield return new WaitForSeconds(1.5f);

            if (currentPage != page)
                pages[page].gameObject.SetActive(false);
        }

        public void DestroyPageUI()
        {
            if (GM.Instance.tutorialGame)
                SceneManager.LoadScene(GM.Instance.menuSceneName);
            else if (GM.Instance.firstLoadUp)
            {
                GM.Instance.firstLoadUp = false;
                GM.boardMng.countDown.Init();
                GM.boardMng.pauseMenu.Hide(true);
                Destroy(gameObject);
            }
            else
                Destroy(gameObject);
        }
    }
}