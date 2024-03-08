namespace HomeTools.ChannelID
{
    public class SceneKeyGenerationException : SceneKeyException
    {
        protected ushort m_InputSceneID;

        public SceneKeyGenerationException(ushort sceneID) => m_InputSceneID = sceneID;

        public ushort SceneID => m_InputSceneID;

        public override string Message => string.Format("Unable to generate scene key for scene ID {0}", m_InputSceneID);
    }
}