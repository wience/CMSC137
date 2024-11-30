using System;
using System.Collections;
using MyGame;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class GamePlayPanel : ShowHidable
    {

        [SerializeField] private Text _scoreTxt, _bestTxt;

        private LevelManager LevelManager => LevelManager.Instance;


        private void Update()
        {
            _scoreTxt.text = LevelManager.Score.ToString();
            _bestTxt.text = GameManager.BEST_SCORE.ToString();
        }

        public void OnClickMore()
        {
            UIManager.Instance.MorePanel.Show();
        }
    }

}