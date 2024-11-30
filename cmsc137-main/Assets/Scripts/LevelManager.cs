using System;
using System.Linq;
using MyGame;
using UnityEngine;

namespace Game
{
    public partial class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }
        public static event Action GameStarted;
        public static event Action GameOver;

        //        public static event Action GameOver;
        //        public static event Action GameStarted;

        [SerializeField] private Board _board;

        [SerializeField] private GamePlayManager _gamePlayManager;

        //        [SerializeField] private LevelCreator _levelCreator;
        private int _score;


        public Board Board => _board;

        public State CurrentState { get; private set; }
        public int Score { get; private set; }

        private void Awake()
        {
            Instance = this;
           
            _gamePlayManager.GameOver+=GamePlayManagerOnGameOver;
            _gamePlayManager.Board.Scored+=BoardOnScored;
            StartTheGame();
        }

        private void BoardOnScored(int score)
        {
            Score += score;
            GameManager.BEST_SCORE = Mathf.Max(GameManager.BEST_SCORE, Score);
        }

        private void StartTheGame()
        {
            _gamePlayManager.Active = true;
            CurrentState = State.Playing;
            GameStarted?.Invoke();
        }

        private void GamePlayManagerOnGameOver()
        {
            CurrentState = State.GameOver;
            GameOver?.Invoke();
        }


        private void Update()
        {
            if (CurrentState == State.GameOver)
                return;
        }


        public enum State
        {
            None,
            Playing,
            GameOver
        }
    }

}