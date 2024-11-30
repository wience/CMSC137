using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;

namespace Game
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GamePlayPanel _gamePlayPanel;
        [SerializeField] private GameOverPanel _gameOverPanel;
        [SerializeField] private MorePanel _morePanel;

        //        public MenuPanel MenuPanel => _menuPanel;
        public GamePlayPanel GamePlayPanel => _gamePlayPanel;

        public GameOverPanel GameOverPanel => _gameOverPanel;

        public MorePanel MorePanel => _morePanel;
        public static UIManager Instance { get; private set; }



        private void Awake()
        {
            Instance = this;
          
        }

        private void OnEnable()
        {
            LevelManager.GameStarted += OnGameStarted;
            LevelManager.GameOver +=OnGameOver;
        }

        private void OnGameOver()
        {
            StartCoroutine(GameOverEnumerator());
        }

        private IEnumerator GameOverEnumerator()
        {
            yield return new WaitForSeconds(1.2f);
            GameOverPanel.Show();
        }

        private void OnDisable()
        {
            LevelManager.GameStarted -= OnGameStarted;
            LevelManager.GameOver -= OnGameOver;
        }




        private void OnGameStarted()
        {
            if (!GamePlayPanel.Showing)
                GamePlayPanel.Show();

        }
    }
}