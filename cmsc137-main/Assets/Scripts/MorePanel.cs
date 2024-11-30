namespace Game
{
    public class MorePanel : ShowHidable
    {
        public void OnClickRestart()
        {
            MyGame.GameManager.LoadGame();
        }
    }
}