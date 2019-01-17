using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Transfer;
using MindTouch.LambdaSharp;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Tweetinvi;
using Image = Amazon.Rekognition.Model.Image;
using S3Object = Amazon.Rekognition.Model.S3Object;
using TweetinviModels = Tweetinvi.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Fotostatur.ImageAnalyzer {

    public class Function : ALambdaFunction<S3Event, FunctionResponse> {

        //--- Fields ---
        private IAmazonRekognition _rekognitionClient;
        private IAmazonS3 _s3Client;
        private List<FoundCriterias> _foundLabels;
        private string _sourceBucket;
        private string _sourceKey;
        private float _totalScore;
        private int _criteriaFiltered;
        private string _comparingImageKey;
        private string _consumerKey;
        private string _consumerSecret;
        private string _accessToken;
        private string _accessTokenSecret;
        private string _comparingImageBucket;
        private string _headshotFileName;
        private string _filename;
        private float _criteriaThreshold;
        private string _tempResizeFilename;

        //--- Methods ---
        public override Task InitializeAsync(LambdaConfig config) {
            
            // Clients
            _rekognitionClient = new AmazonRekognitionClient();
            _s3Client = new AmazonS3Client();
            
            // social media keys
            // _consumerKey = config.ReadText("TwitterConsumerKey");
            // _consumerSecret = config.ReadText("TwitterConsumerSecret");
            // _accessToken = config.ReadText("TwitterAccessToken");
            // _accessTokenSecret = config.ReadText("TwitterAccessSecret");
            
            // Headshot paths
            _headshotFileName = config.ReadText("HeadshotFileName");
            var headshotS3Path = config.ReadText("HeadshotPhotos").Replace("s3://", "").Split("/");
            _comparingImageBucket = headshotS3Path[0];
            _comparingImageKey = $"{headshotS3Path[1]}/{_headshotFileName}";
            return Task.CompletedTask;
        }

        public override async Task<FunctionResponse> ProcessMessageAsync(S3Event s3Event, ILambdaContext context) {
            LogInfo(JsonConvert.SerializeObject(s3Event));
            _foundLabels = new List<FoundCriterias>();
            _totalScore = 0;
            _criteriaFiltered = 0;
            _criteriaThreshold = 50;
            
            // Get the Bucket name and key from the event
            GetEventInfo(s3Event);
            
            // OBJECT AND SCENE DETECTION
            var detectLabelResponse = await DetectLabels();
            ScoreLabels(detectLabelResponse);

            // TEXT IN IMAGE
            var detectTextResponse = await DetectText();
            ScoreText(detectTextResponse);

            // FACE COMPARISON
            var compareFacesResponse = await CompareFaces();
            ScoreCompare(compareFacesResponse);
           
            // FACIAL ANALYSIS
            var detectFacesResponse = await DetectFaces();
            ScoreFaces(detectFacesResponse);
            
            // Final Score
            float finalScore = 0;
            if (_criteriaFiltered > 0) {
                finalScore = CalculateFinalScore();
            }
            LogInfo($"Final Score: {finalScore}");
            
            // Post if within threshold
            if (finalScore > _criteriaThreshold) {
                // await DownloadS3Image();
                // ResizeImage();
                // UploadImage();
                // TwitterUpload();
            }
            return new FunctionResponse();
        }

        // ########################################
        // ##### S3 EVENT INFO
        // ########################################
        private void GetEventInfo(S3Event s3Event) {
            _sourceBucket = s3Event.Records.First().S3.Bucket.Name;
            _sourceKey = s3Event.Records.First().S3.Object.Key;
            _filename = _sourceKey.Split("/").LastOrDefault();
            _tempResizeFilename = $"/tmp/resize_{_filename}";
        }
        
        // ########################################
        // ##### DETECT LABELS - LEVEL 1
        // ########################################
        public async Task<DetectLabelsResponse> DetectLabels() {
            
            // LEVEL 1: detect labels from the picture
            return new DetectLabelsResponse();
        }

        public void ScoreLabels(DetectLabelsResponse detectLabelsResponse) {
            LogInfo(JsonConvert.SerializeObject(detectLabelsResponse));
            
            // LEVEL 1: determine if photo meets your label criteria
        }
        
        // ########################################
        // ### DETECT TEXT - LEVEL 2
        // ########################################
        private async Task<DetectTextResponse> DetectText() {
            
            // LEVEL 2: detect text in the picture
            return new DetectTextResponse();
        }

        private void ScoreText(DetectTextResponse detectTextResponse) {
            LogInfo(JsonConvert.SerializeObject(detectTextResponse));
            
            // LEVEL 2: make a criteria around detecting text in an image
        }
        
        // ########################################
        // ### Face Compare - LEVEL 3
        // ########################################
        private async Task<CompareFacesResponse> CompareFaces() {
            
            // LEVEL 3: compare face in the picture
            return new CompareFacesResponse();
        }

        private void ScoreCompare(CompareFacesResponse compareFacesResponse) {
            LogInfo(JsonConvert.SerializeObject(compareFacesResponse));
            
            // LEVEL 3: make a criteria around comparing faces
        }

        // ########################################
        // ### DETECT FACES - LEVEL 4
        // ########################################
        public async Task<DetectFacesResponse> DetectFaces() {
            
            // LEVEL 4: detect faces in the picture
            return new DetectFacesResponse();
        }

        public void ScoreFaces(DetectFacesResponse detectFactResponse) {
            LogInfo(JsonConvert.SerializeObject(detectFactResponse));

            // LEVEL 4: choose one or more categories to build criteria from
            // ageRange, beard, boundingBox, eyeglasses, eyesOpen, gender, mouthOpen, mustache, pose, quality, smile, sunglasses
        }

        // ########################################
        // ### PROCESS IMAGE FOR UPLOAD - BOSS
        // ########################################
        private async Task DownloadS3Image() {
            LogInfo("Downloading image");
            
            // BOSS: Download and save image locally from S3
        }
        
        private void ResizeImage() {
            LogInfo("Resize image");
            
            // BOSS: load the image and resize (https://github.com/SixLabors/ImageSharp#api)
        }

        private void TwitterUpload() {
            
            // BOSS: upload to twitter
            try {
                LogInfo("Twitter image");
                var bytes = File.ReadAllBytesAsync("LOCAL FILE PATH").Result;
                Auth.SetUserCredentials(_consumerKey, _consumerSecret, _accessToken, _accessTokenSecret);
                Account.UpdateProfileImage(bytes);
            }
            catch (Exception e) {
                LogError(e);
            }
        }
        
        private void UploadImage() {
            LogInfo("Upload image");
            
            // BOSS (optional): upload processed file to S3
        }
        
        // ########################################
        // ### CALCULATIONS -- OPTIONAL TO USE
        // ########################################
        public void AddTotals(string name, float points) {
            
            // track number of criterias being applied to this image
            _criteriaFiltered += 1;
            
            // track the confidence of each criteria taken from rekognition
            _totalScore += points;
            
            // (optional) keep track of the individual points earned for each criteria 
            _foundLabels.Add(new FoundCriterias {
                Name = name,
                Points = points
            });
        }
        
        private float CalculateFinalScore() {
            LogInfo($"Score Breakdown: {JsonConvert.SerializeObject(_foundLabels)}");
            
            // total confidence divided by the number of criteria filtered
            return _totalScore / _criteriaFiltered;
        }
    }
    
    public class FunctionResponse {

        // this function is intentionally left empty
    }
}
