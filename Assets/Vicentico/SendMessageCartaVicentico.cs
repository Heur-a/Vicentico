using UnityEngine;

public class SendMessageCartaVicentico : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public string messageText;
    public Color messageColorInfo = Color.white;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendMessage()
    {
        MessageDisplay.Instance.ShowMessage(messageText,-1f, messageColorInfo);
    }
}
