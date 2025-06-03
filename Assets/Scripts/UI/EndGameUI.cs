using UnityEngine;

public class EndGameUI : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject losePanel;

    public void ShowEndScreen(bool win)
    {
        winPanel.SetActive(win);
        losePanel.SetActive(!win);
    }
}
