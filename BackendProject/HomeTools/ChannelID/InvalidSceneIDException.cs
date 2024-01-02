namespace BackendProject.HomeTools.ChannelID
{
    public class InvalidSceneIDException : SceneKeyException
    {
        protected ushort m_SceneID;
        protected SceneKey m_SceneKey;

        public InvalidSceneIDException(SceneKey sceneKey, ushort sceneID)
        {
            m_SceneID = sceneID;
            m_SceneKey = sceneKey;
        }

        public ushort SceneID => m_SceneID;

        public SceneKey SceneKey => m_SceneKey;

        public override string Message => string.Format("Scene ID {0} is not a valid Home scene ID", m_SceneID);
    }
}