namespace CryptoSporidium.ChannelID
{
  public class InvalidSceneIDKeyException : SceneKeyException
  {
    protected SceneKey m_SceneIDKey;

    public SceneKey SceneIDKey => m_SceneIDKey;

    public override string Message => string.Format("Invalid Scene ID Key Checksum");

    public InvalidSceneIDKeyException(SceneKey sceneIDKey) => m_SceneIDKey = sceneIDKey;
  }
}