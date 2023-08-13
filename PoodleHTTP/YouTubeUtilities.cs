using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;

namespace PSMultiServer.PoodleHTTP
{
    public class YouTubeUtilities
    {
        /*
         Instructions to get refresh token:
         * https://stackoverflow.com/questions/5850287/youtube-api-single-user-scenario-with-oauth-uploading-videos/8876027#8876027
         * 
         * When getting client_id and client_secret, use installed application, other (this will make the token a long term token)
         */

        private string UploadedVideoId { get; set; }

        private YouTubeService youtube;

        public YouTubeUtilities()
        {
            youtube = BuildService();
        }

        private YouTubeService BuildService()
        {
            ClientSecrets secrets = new ClientSecrets()
            {
                ClientId = ServerConfiguration.CLIENT_ID,
                ClientSecret = ServerConfiguration.CLIENT_SECRET
            };

            var token = new TokenResponse { RefreshToken = ServerConfiguration.REFRESH_TOKEN };
            var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = secrets
                }),
                "user",
                token);

            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = "TestProject"
            });

            return service;
        }

        public string UploadVideo(Stream stream, string title, string desc, string[] tags, string categoryId, bool isPublic)
        {
            var video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = title;
            video.Snippet.Description = desc;
            video.Snippet.Tags = tags;
            video.Snippet.CategoryId = categoryId; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = isPublic ? "public" : "private"; // "private" or "public" or unlisted

            //var videosInsertRequest = youtube.Videos.Insert(video, "snippet,status", stream, "video/*");
            var videosInsertRequest = youtube.Videos.Insert(video, "snippet,status", stream, "video/*");
            videosInsertRequest.ProgressChanged += insertRequest_ProgressChanged;
            videosInsertRequest.ResponseReceived += insertRequest_ResponseReceived;

            videosInsertRequest.Upload();

            return UploadedVideoId;
        }

        public void DeleteVideo(string videoId)
        {
            var videoDeleteRequest = youtube.Videos.Delete(videoId);
            videoDeleteRequest.Execute();
        }

        public void insertRequest_ResponseReceived(Video video)
        {
            UploadedVideoId = video.Id;
            // video.ID gives you the ID of the Youtube video.
            // you can access the video from
            // http://www.youtube.com/watch?v={video.ID}
        }

        public void insertRequest_ProgressChanged(IUploadProgress progress)
        {
            // You can handle several status messages here.
            switch (progress.Status)
            {
                case UploadStatus.Failed:
                    UploadedVideoId = "FAILED";
                    break;
                case UploadStatus.Completed:
                    break;
                default:
                    break;
            }
        }
    }
}
